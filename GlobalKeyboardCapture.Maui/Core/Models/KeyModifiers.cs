namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Modifier keys active for a keyboard event or gesture.
/// </summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Control = 1 << 0,
    Alt = 1 << 1,
    Shift = 1 << 2,
    Meta = 1 << 3,
    All = Control | Alt | Shift | Meta
}
