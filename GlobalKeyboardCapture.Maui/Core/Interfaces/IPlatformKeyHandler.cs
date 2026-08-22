namespace GlobalKeyboardCapture.Maui.Core.Interfaces;
public interface IPlatformKeyHandler
{
    bool SupportsMultiplePlatformViews { get; }
    void Attach(object platformView);
    bool Detach(object platformView);
    void ConfigureHandler(Action<Models.KeyEventArgs> onKeyPressed);
    void ConfigureDiagnostics(Action<Models.KeyboardDiagnosticEventArgs> onDiagnostic)
    {
    }
    void Cleanup();
}
