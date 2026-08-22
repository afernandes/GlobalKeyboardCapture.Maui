namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Modifier keys active for a keyboard event or gesture.
/// </summary>
[Flags]
public enum KeyModifiers
{
    /// <summary>No modifier is active.</summary>
    None = 0,

    /// <summary>The Control modifier.</summary>
    Control = 1 << 0,

    /// <summary>The Alt or Option modifier.</summary>
    Alt = 1 << 1,

    /// <summary>The Shift modifier.</summary>
    Shift = 1 << 2,

    /// <summary>The Windows, Command, or Meta modifier.</summary>
    Meta = 1 << 3,

    /// <summary>All supported modifiers.</summary>
    All = Control | Alt | Shift | Meta
}
