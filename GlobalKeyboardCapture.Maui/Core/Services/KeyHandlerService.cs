using GlobalKeyboardCapture.Maui.Core.Interfaces;

namespace GlobalKeyboardCapture.Maui.Core.Services;

public class KeyHandlerService : IKeyHandlerService, IDisposable
{
    private readonly HashSet<IKeyHandler> _handlers = [];
    private readonly IPlatformKeyHandler _platformHandler;

    public KeyHandlerService(IPlatformKeyHandler platformHandler)
    {
        _platformHandler = platformHandler;
        _platformHandler.ConfigureHandler(HandleKeyPress);
    }

    public void Initialize(object platformView)
    {
        _platformHandler.Initialize(platformView);
    }

    private void HandleKeyPress(Core.Models.KeyEventArgs key)
    {
        foreach (var handler in _handlers)
        {
            if (handler.ShouldHandle(key))
            {
                handler.HandleKey(key);
            }
        }
    }

    public void RegisterHandler(IKeyHandler handler)
    {
        _handlers.Add(handler);
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
