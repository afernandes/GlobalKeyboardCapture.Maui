namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Transition represented by a keyboard event.
/// </summary>
public enum KeyboardEventType
{
    /// <summary>The key transitioned to the pressed state.</summary>
    KeyDown = 0,

    /// <summary>The key transitioned to the released state.</summary>
    KeyUp = 1
}
