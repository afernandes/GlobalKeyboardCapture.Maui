namespace GlobalKeyboardCapture.Maui.Core.Interfaces;
public interface IPlatformKeyHandler
{
    void Initialize(object platformView);
    void ConfigureHandler(Action<Models.KeyEventArgs> onKeyPressed);
    void ConfigureDiagnostics(Action<Models.KeyboardDiagnosticEventArgs> onDiagnostic)
    {
    }
    void Cleanup();
}
