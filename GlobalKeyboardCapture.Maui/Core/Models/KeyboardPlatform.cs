namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Platform that produced a keyboard event.
/// </summary>
public enum KeyboardPlatform
{
    Unknown = 0,
    Windows,
    Android,
    iOS,
    MacCatalyst
}
