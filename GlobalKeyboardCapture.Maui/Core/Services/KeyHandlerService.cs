using System.Runtime.CompilerServices;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using Microsoft.Extensions.Logging;

namespace GlobalKeyboardCapture.Maui.Core.Services;

/// <inheritdoc cref="IKeyHandlerService"/>
public sealed class KeyHandlerService : IKeyHandlerService, IDisposable
{
    private const int INITIAL_HANDLERS_CAPACITY = 8;

    private readonly object _lockObject = new();
    // List (not HashSet) so dispatch honors registration order — required for
    // StopOnHandled, which makes handler order observable.
    private readonly List<HandlerRegistration> _handlers;
    private readonly IPlatformKeyHandler _platformHandler;
    private readonly ILogger<KeyHandlerService> _logger;
    private readonly KeyHandlerOptions _options;
    private readonly CancellationTokenSource _asyncHandlerCancellation = new();
    private readonly CancellationToken _asyncHandlerCancellationToken;
    private readonly HashSet<long> _activeSuspensions = [];
    private readonly Dictionary<object, PlatformViewRegistration> _platformViews =
        new(ReferenceEqualityComparer.Instance);
    private long _nextRegistrationId;
    private long _nextPlatformViewRegistrationId;
    private long _nextSuspensionId;
    private bool _isDisposed;

    /// <inheritdoc/>
    public event EventHandler<KeyboardDiagnosticEventArgs>? DiagnosticEvent;

    /// <inheritdoc/>
    public bool IsInitialized
    {
        get
        {
            lock (_lockObject)
            {
                return !_isDisposed && _platformViews.Count > 0;
            }
        }
    }

    /// <inheritdoc/>
    public int PlatformViewCount
    {
        get
        {
            lock (_lockObject)
            {
                return _platformViews.Count;
            }
        }
    }

    /// <inheritdoc/>
    public int HandlerCount
    {
        get
        {
            lock (_lockObject)
            {
                return _handlers.Count;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsCapturing
    {
        get
        {
            lock (_lockObject)
            {
                return !_isDisposed && _activeSuspensions.Count == 0;
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IKeyHandler> Handlers
    {
        get
        {
            lock (_lockObject)
            {
                var handlers = new IKeyHandler[_handlers.Count];
                for (var index = 0; index < handlers.Length; index++)
                    handlers[index] = _handlers[index].Handler;
                return handlers;
            }
        }
    }

    /// <summary>Creates a service with default options.</summary>
    /// <param name="platformHandler">The native keyboard adapter.</param>
    /// <param name="logger">The service logger.</param>
    public KeyHandlerService(
        IPlatformKeyHandler platformHandler,
        ILogger<KeyHandlerService> logger)
        : this(platformHandler, logger, new KeyHandlerOptions())
    {
    }

    /// <summary>Creates a service with explicit options.</summary>
    /// <param name="platformHandler">The native keyboard adapter.</param>
    /// <param name="logger">The service logger.</param>
    /// <param name="options">Capture and dispatch options.</param>
    public KeyHandlerService(
        IPlatformKeyHandler platformHandler,
        ILogger<KeyHandlerService> logger,
        KeyHandlerOptions options)
    {
        _platformHandler = platformHandler ?? throw new ArgumentNullException(nameof(platformHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _asyncHandlerCancellationToken = _asyncHandlerCancellation.Token;
        _handlers = new List<HandlerRegistration>(INITIAL_HANDLERS_CAPACITY);
        _platformHandler.ConfigureHandler(HandleKeyPress);
        if (_options.EnableDiagnostics)
            _platformHandler.ConfigureDiagnostics(ReportDiagnostic);
    }

    /// <inheritdoc/>
    public void Initialize(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);
        ThrowIfDisposed();

        lock (_lockObject)
        {
            ThrowIfDisposed();
            if (_platformViews.TryGetValue(platformView, out var existing))
            {
                existing.IsPermanent = true;
                return;
            }

            try
            {
                PrepareForPlatformViewAttachment();
                _platformHandler.Attach(platformView);
                _platformViews.Add(
                    platformView,
                    new PlatformViewRegistration(
                        ++_nextPlatformViewRegistrationId,
                        platformView,
                        isPermanent: true,
                        leaseCount: 0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize platform handler");
                throw;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleKeyPress(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);

        HandlerRegistration[] currentHandlers;
        var isSuspended = false;
        lock (_lockObject)
        {
            // Silent early-return: this runs on the platform input thread and may be
            // invoked for events already in flight after Dispose. Throwing here would
            // crash the platform input pipeline.
            if (_isDisposed) return;
            isSuspended = _activeSuspensions.Count > 0;
            currentHandlers = isSuspended ? [] : _handlers.ToArray();
        }

        if (isSuspended)
        {
            if (_options.EnableDiagnostics)
            {
                ReportDiagnostic(KeyboardDiagnosticEventArgs.FromKeyEvent(
                    key,
                    KeyboardDiagnosticStage.Ignored,
                    "Capture is suspended."));
            }
            return;
        }

        if (key.EventType == KeyboardEventType.KeyUp && !_options.CaptureKeyUp)
        {
            ReportIgnoredEvent(key, "Key-up events are disabled.");
            return;
        }

        if (key.IsRepeat && !_options.AllowKeyRepeat)
        {
            ReportIgnoredEvent(key, "Auto-repeat events are disabled.");
            return;
        }

        if (_options.EnableDiagnostics)
            ReportDiagnostic(KeyboardDiagnosticEventArgs.FromKeyEvent(key));

        var stopOnHandled = _options.StopOnHandled;
        foreach (var registration in currentHandlers)
        {
            if (registration.Scope is { IsEnabled: false })
                continue;

            var handler = registration.Handler;
            try
            {
                if (handler.ShouldHandle(key))
                {
                    if (handler is IAsyncKeyHandler asyncHandler)
                    {
                        var snapshot = key.CreateSnapshot();
                        _ = Task.Run(() => ExecuteAsyncHandler(
                            asyncHandler,
                            snapshot,
                            _asyncHandlerCancellationToken));
                    }
                    else
                    {
                        handler.HandleKey(key);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler failed to process key event");
            }

            if (stopOnHandled && key.Handled) break;
        }
    }

    /// <inheritdoc/>
    public IDisposable RegisterHandler(IKeyHandler handler, int priority = 0)
        => RegisterHandler(handler, priority, scope: null);

    private IDisposable RegisterHandler(IKeyHandler handler, int priority, CaptureScope? scope)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            ThrowIfDisposed();

            var existing = _handlers.Find(registration =>
                ReferenceEquals(registration.Handler, handler) && ReferenceEquals(registration.Scope, scope));
            if (existing is not null)
                return new HandlerRegistrationToken(this, existing.Id);

            var registration = new HandlerRegistration(++_nextRegistrationId, handler, priority, scope);
            var insertionIndex = _handlers.FindIndex(item => item.Priority < priority);
            if (insertionIndex < 0)
                _handlers.Add(registration);
            else
                _handlers.Insert(insertionIndex, registration);

            return new HandlerRegistrationToken(this, registration.Id);
        }
    }

    private void ReportIgnoredEvent(KeyEventArgs key, string reason)
    {
        if (_options.EnableDiagnostics)
        {
            ReportDiagnostic(KeyboardDiagnosticEventArgs.FromKeyEvent(
                key,
                KeyboardDiagnosticStage.Ignored,
                reason));
        }
    }

    private async Task ExecuteAsyncHandler(
        IAsyncKeyHandler handler,
        KeyEventArgs snapshot,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleKeyAsync(snapshot, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Asynchronous keyboard handler canceled during service shutdown");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Asynchronous handler failed to process key event");
        }
    }

    /// <inheritdoc/>
    public IDisposable AttachPlatformView(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);

        lock (_lockObject)
        {
            ThrowIfDisposed();

            if (_platformViews.TryGetValue(platformView, out var existing))
            {
                existing.LeaseCount++;
                return new PlatformViewLease(this, platformView, existing.Id);
            }

            try
            {
                PrepareForPlatformViewAttachment();
                _platformHandler.Attach(platformView);
                var registration = new PlatformViewRegistration(
                    ++_nextPlatformViewRegistrationId,
                    platformView,
                    isPermanent: false,
                    leaseCount: 1);
                _platformViews.Add(platformView, registration);
                return new PlatformViewLease(this, platformView, registration.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to attach platform handler");
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public bool DetachPlatformView(object platformView)
    {
        if (platformView is null)
            return false;

        lock (_lockObject)
        {
            if (_isDisposed || !_platformViews.Remove(platformView))
                return false;

            try
            {
                _platformHandler.Detach(platformView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to detach platform handler");
                throw;
            }

            return true;
        }
    }

    private void PrepareForPlatformViewAttachment()
    {
        if (_platformHandler.SupportsMultiplePlatformViews || _platformViews.Count == 0)
            return;

        foreach (var registration in _platformViews.Values)
        {
            try
            {
                _platformHandler.Detach(registration.PlatformView);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to detach a previous platform view during rebind");
            }
        }

        _platformViews.Clear();
    }

    private void ReleasePlatformView(object platformView, long registrationId)
    {
        lock (_lockObject)
        {
            if (_isDisposed
                || !_platformViews.TryGetValue(platformView, out var registration)
                || registration.Id != registrationId)
            {
                return;
            }

            if (registration.LeaseCount > 0)
                registration.LeaseCount--;

            if (registration.IsPermanent || registration.LeaseCount > 0)
                return;

            _platformViews.Remove(platformView);
            try
            {
                _platformHandler.Detach(platformView);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to release platform-view attachment");
            }
        }
    }

    /// <inheritdoc/>
    public bool UnregisterHandler(IKeyHandler handler)
    {
        if (handler == null) return false;

        lock (_lockObject)
        {
            if (_isDisposed) return false;
            return _handlers.RemoveAll(registration => ReferenceEquals(registration.Handler, handler)) > 0;
        }
    }

    /// <inheritdoc/>
    public IDisposable SuspendCapture()
    {
        lock (_lockObject)
        {
            ThrowIfDisposed();
            var suspensionId = ++_nextSuspensionId;
            _activeSuspensions.Add(suspensionId);
            return new SuspensionToken(this, suspensionId);
        }
    }

    /// <inheritdoc/>
    public void ResumeCapture()
    {
        lock (_lockObject)
        {
            ThrowIfDisposed();
            _activeSuspensions.Clear();
        }
    }

    /// <inheritdoc/>
    public IKeyboardCaptureScope CreateScope(string? name = null, bool isEnabled = true)
    {
        lock (_lockObject)
        {
            ThrowIfDisposed();
            return new CaptureScope(this, name, isEnabled);
        }
    }

    private void UnregisterHandler(long registrationId)
    {
        lock (_lockObject)
        {
            if (_isDisposed) return;
            _handlers.RemoveAll(registration => registration.Id == registrationId);
        }
    }

    private void ResumeCapture(long suspensionId)
    {
        lock (_lockObject)
        {
            if (_isDisposed) return;
            _activeSuspensions.Remove(suspensionId);
        }
    }

    private void ReportDiagnostic(KeyboardDiagnosticEventArgs diagnostic)
    {
        if (!_options.EnableDiagnostics)
            return;

        _logger.LogInformation(
            "Keyboard event {Stage}: Platform={Platform}, Key={NormalizedKey}, NativeKeyCode={NativeKeyCode}, ScanCode={ScanCode}, Action={Action}, Flags={Flags}, Repeat={RepeatCount}, DeviceId={DeviceId}, Reason={Reason}",
            diagnostic.Stage,
            diagnostic.Platform,
            diagnostic.NormalizedKey,
            diagnostic.NativeKeyCode,
            diagnostic.NativeScanCode,
            diagnostic.NativeAction,
            diagnostic.NativeFlags,
            diagnostic.RepeatCount,
            diagnostic.Device?.Id,
            diagnostic.Reason);

        try
        {
            DiagnosticEvent?.Invoke(this, diagnostic);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "A keyboard diagnostic subscriber failed");
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(KeyHandlerService));
        }
    }

    #region IDisposable Implementation

    /// <summary>Stops native capture and releases all registrations and attachments.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        var shouldDispose = false;
        lock (_lockObject)
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            shouldDispose = disposing;
            _handlers.Clear();
            _activeSuspensions.Clear();
            _platformViews.Clear();
        }

        if (!shouldDispose)
            return;

        _asyncHandlerCancellation.Cancel();
        try
        {
            _platformHandler.Cleanup();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during service disposal");
        }
        _asyncHandlerCancellation.Dispose();
    }

    #endregion

    private sealed record HandlerRegistration(long Id, IKeyHandler Handler, int Priority, CaptureScope? Scope);

    private sealed class PlatformViewRegistration(
        long id,
        object platformView,
        bool isPermanent,
        int leaseCount)
    {
        public long Id { get; } = id;
        public object PlatformView { get; } = platformView;
        public bool IsPermanent { get; set; } = isPermanent;
        public int LeaseCount { get; set; } = leaseCount;
    }

    private sealed class PlatformViewLease : IDisposable
    {
        private KeyHandlerService? _owner;
        private readonly object _platformView;
        private readonly long _registrationId;

        public PlatformViewLease(KeyHandlerService owner, object platformView, long registrationId)
        {
            _owner = owner;
            _platformView = platformView;
            _registrationId = registrationId;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.ReleasePlatformView(_platformView, _registrationId);
        }
    }

    private sealed class HandlerRegistrationToken : IDisposable
    {
        private KeyHandlerService? _owner;
        private readonly long _registrationId;

        public HandlerRegistrationToken(KeyHandlerService owner, long registrationId)
        {
            _owner = owner;
            _registrationId = registrationId;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.UnregisterHandler(_registrationId);
        }
    }

    private sealed class SuspensionToken : IDisposable
    {
        private KeyHandlerService? _owner;
        private readonly long _suspensionId;

        public SuspensionToken(KeyHandlerService owner, long suspensionId)
        {
            _owner = owner;
            _suspensionId = suspensionId;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.ResumeCapture(_suspensionId);
        }
    }

    private sealed class CaptureScope : IKeyboardCaptureScope
    {
        private readonly object _scopeLock = new();
        private readonly KeyHandlerService _owner;
        private readonly List<IDisposable> _registrations = [];
        private int _isEnabled;
        private bool _isDisposed;

        public CaptureScope(KeyHandlerService owner, string? name, bool isEnabled)
        {
            _owner = owner;
            Name = name;
            _isEnabled = isEnabled ? 1 : 0;
        }

        public string? Name { get; }

        public bool IsEnabled
        {
            get => Volatile.Read(ref _isEnabled) != 0;
            set
            {
                lock (_scopeLock)
                {
                    ThrowIfDisposed();
                    Volatile.Write(ref _isEnabled, value ? 1 : 0);
                }
            }
        }

        public IDisposable RegisterHandler(IKeyHandler handler, int priority = 0)
        {
            lock (_scopeLock)
            {
                ThrowIfDisposed();
                var registration = _owner.RegisterHandler(handler, priority, this);
                _registrations.Add(registration);
                return registration;
            }
        }

        public void Dispose()
        {
            IDisposable[] registrations;
            lock (_scopeLock)
            {
                if (_isDisposed) return;
                _isDisposed = true;
                Volatile.Write(ref _isEnabled, 0);
                registrations = _registrations.ToArray();
                _registrations.Clear();
            }

            foreach (var registration in registrations)
                registration.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(IKeyboardCaptureScope));
        }
    }
}
