namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Physical location of a key when the platform can identify it.
/// </summary>
public enum KeyLocation
{
    /// <summary>The physical key location is unavailable.</summary>
    Unknown = 0,

    /// <summary>The standard section of the keyboard.</summary>
    Standard,

    /// <summary>The left-hand version of a modifier key.</summary>
    Left,

    /// <summary>The right-hand version of a modifier key.</summary>
    Right,

    /// <summary>The numeric keypad.</summary>
    Numpad
}
