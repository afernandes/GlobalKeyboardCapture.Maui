namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>
/// Groups page- or feature-specific handlers under one enable/dispose boundary.
/// </summary>
public interface IKeyboardCaptureScope : IDisposable
{
    string? Name { get; }
    bool IsEnabled { get; set; }
    IDisposable RegisterHandler(IKeyHandler handler, int priority = 0);
}
