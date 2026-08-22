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
    private object? _boundPlatformView;
    private long _nextRegistrationId;
    private bool _isDisposed;

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

        IKeyHandler[] currentHandlers;
        lock (_lockObject)
        {
            // Silent early-return: this runs on the platform input thread and may be
            // invoked for events already in flight after Dispose. Throwing here would
            // crash the platform input pipeline.
            if (_isDisposed) return;
            currentHandlers = new IKeyHandler[_handlers.Count];
            for (var index = 0; index < currentHandlers.Length; index++)
                currentHandlers[index] = _handlers[index].Handler;
        }

        var stopOnHandled = _options.StopOnHandled;
        foreach (var handler in currentHandlers)
        {
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
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            ThrowIfDisposed();

            var existing = _handlers.Find(registration => ReferenceEquals(registration.Handler, handler));
            if (existing is not null)
                return new HandlerRegistrationToken(this, existing.Id);

            var registration = new HandlerRegistration(++_nextRegistrationId, handler, priority);
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
            var index = _handlers.FindIndex(registration => ReferenceEquals(registration.Handler, handler));
            if (index < 0)
                return false;
            _handlers.RemoveAt(index);
            return true;
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

    private sealed record HandlerRegistration(long Id, IKeyHandler Handler, int Priority);

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
}
