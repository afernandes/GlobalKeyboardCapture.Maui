namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

public interface IKeyHandlerService
{
    bool IsInitialized { get; }
    int HandlerCount { get; }
    IReadOnlyList<IKeyHandler> Handlers { get; }

    void Initialize(object platformView);
    IDisposable RegisterHandler(IKeyHandler handler, int priority = 0);
    bool UnregisterHandler(IKeyHandler handler);
}
