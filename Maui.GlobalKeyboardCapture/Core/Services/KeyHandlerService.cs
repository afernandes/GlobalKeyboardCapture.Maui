using Maui.GlobalKeyboardCapture.Core.Interfaces;

namespace Maui.GlobalKeyboardCapture.Core.Services;

public class KeyHandlerService : IKeyHandlerService, IDisposable
{
    private readonly List<IKeyHandler> _handlers = new();
    private readonly IPlatformKeyHandler _platformHandler;

    public KeyHandlerService(IPlatformKeyHandler platformHandler)
    {
        _platformHandler = platformHandler;
        _platformHandler.ConfigureHandler(HandleKeyPress);
    }

    public void Initialize(object platformView)
    {
        _platformHandler?.Initialize(platformView);
    }

    private void HandleKeyPress(Core.Models.KeyEventArgs key)
    {
        foreach (var handler in _handlers.ToList())
        {
            if (handler.ShouldHandle(key))
            {
                handler.HandleKey(key);
            }
        }
    }

    public void RegisterHandler(IKeyHandler handler)
    {
        if (!_handlers.Contains(handler))
        {
            _handlers.Add(handler);
        }
    }

    public void UnregisterHandler(IKeyHandler handler)
    {
        _handlers.Remove(handler);
    }

    public void Dispose()
    {
        _platformHandler?.Cleanup();
        _handlers.Clear();
    }
}
