using System.Runtime.CompilerServices;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using Microsoft.Extensions.Logging;

namespace GlobalKeyboardCapture.Maui.Core.Services;

public sealed class KeyHandlerService : IKeyHandlerService, IDisposable
{
    private const int INITIAL_HANDLERS_CAPACITY = 8;

    private readonly object _lockObject = new();
    private readonly HashSet<IKeyHandler> _handlers;
    private readonly IPlatformKeyHandler _platformHandler;
    private readonly ILogger<KeyHandlerService> _logger;
    private bool _isInitialized;
    private bool _isDisposed;

    public KeyHandlerService(
        IPlatformKeyHandler platformHandler,
        ILogger<KeyHandlerService> logger)
    {
        _platformHandler = platformHandler ?? throw new ArgumentNullException(nameof(platformHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _handlers = new HashSet<IKeyHandler>(INITIAL_HANDLERS_CAPACITY);
        _platformHandler.ConfigureHandler(HandleKeyPress);
    }

    public void Initialize(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);
        ThrowIfDisposed();

        lock (_lockObject)
        {
            if (_isInitialized) return;

            try
            {
                _platformHandler.Initialize(platformView);
                _isInitialized = true;
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
            ThrowIfDisposed();
            currentHandlers = _handlers.ToArray();
        }

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
        }
    }

    public void RegisterHandler(IKeyHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            ThrowIfDisposed();
            _handlers.Add(handler);
        }
    }

    public void UnregisterHandler(IKeyHandler handler)
    {
        if (handler == null) return;

        lock (_lockObject)
        {
            if (_isDisposed) return;
            _handlers.Remove(handler);
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
}
