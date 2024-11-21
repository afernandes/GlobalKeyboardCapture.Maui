namespace GlobalKeyboardCapture.Maui.Core.Interfaces;
public interface IKeyHandler
{
    void HandleKey(Models.KeyEventArgs key);
    bool ShouldHandle(Models.KeyEventArgs key);
}