namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

public interface IKeyHandlerService
{
    void Initialize(object platformView);
    void RegisterHandler(IKeyHandler handler);
    void UnregisterHandler(IKeyHandler handler);
}
