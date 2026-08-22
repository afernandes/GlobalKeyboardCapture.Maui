using System.Runtime.CompilerServices;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using Microsoft.Extensions.Logging;

namespace GlobalKeyboardCapture.Maui.Core.Services;

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
    private readonly HashSet<long> _activeSuspensions = [];
    private object? _boundPlatformView;
    private long _nextRegistrationId;
    private long _nextSuspensionId;
    private bool _isDisposed;

    public event EventHandler<KeyboardDiagnosticEventArgs>? DiagnosticEvent;

    public bool IsInitialized
    {
        get
        {
            lock (_lockObject)
            {
                return !_isDisposed && _boundPlatformView is not null;
            }
        }
    }

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

    public KeyHandlerService(
        IPlatformKeyHandler platformHandler,
        ILogger<KeyHandlerService> logger)
        : this(platformHandler, logger, new KeyHandlerOptions())
    {
    }

    public KeyHandlerService(
        IPlatformKeyHandler platformHandler,
        ILogger<KeyHandlerService> logger,
        KeyHandlerOptions options)
    {
        _platformHandler = platformHandler ?? throw new ArgumentNullException(nameof(platformHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _handlers = new List<HandlerRegistration>(INITIAL_HANDLERS_CAPACITY);
        _platformHandler.ConfigureHandler(HandleKeyPress);
        if (_options.EnableDiagnostics)
            _platformHandler.ConfigureDiagnostics(ReportDiagnostic);
    }

    public void Initialize(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);
        ThrowIfDisposed();

        lock (_lockObject)
        {
            if (ReferenceEquals(_boundPlatformView, platformView)) return;

            try
            {
                _platformHandler.Initialize(platformView);
                _boundPlatformView = platformView;
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
                if (handler?.ShouldHandle(key) == true)
                {
                    handler.HandleKey(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler failed to process key event");
            }

            if (stopOnHandled && key.Handled) break;
        }
    }

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

    public bool UnregisterHandler(IKeyHandler handler)
    {
        if (handler == null) return false;

        lock (_lockObject)
        {
            if (_isDisposed) return false;
            return _handlers.RemoveAll(registration => ReferenceEquals(registration.Handler, handler)) > 0;
        }
    }

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

    public void ResumeCapture()
    {
        lock (_lockObject)
        {
            ThrowIfDisposed();
            _activeSuspensions.Clear();
        }
    }

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        lock (_lockObject)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                try
                {
                    _platformHandler?.Cleanup();
                    _handlers.Clear();
                    _activeSuspensions.Clear();
                    _boundPlatformView = null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during service disposal");
                }
            }

            _isDisposed = true;
        }
    }

    #endregion

    private sealed record HandlerRegistration(long Id, IKeyHandler Handler, int Priority, CaptureScope? Scope);

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
