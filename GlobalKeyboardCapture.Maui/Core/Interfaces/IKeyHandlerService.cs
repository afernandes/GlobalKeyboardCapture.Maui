namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

public interface IKeyHandlerService
{
    event EventHandler<Models.KeyboardDiagnosticEventArgs>? DiagnosticEvent;

    bool IsInitialized { get; }
    bool IsCapturing { get; }
    int HandlerCount { get; }
    IReadOnlyList<IKeyHandler> Handlers { get; }

    void Initialize(object platformView);
    IDisposable RegisterHandler(IKeyHandler handler, int priority = 0);
    bool UnregisterHandler(IKeyHandler handler);
    IDisposable SuspendCapture();
    void ResumeCapture();
    IKeyboardCaptureScope CreateScope(string? name = null, bool isEnabled = true);
}
