namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Platform-neutral logical keys understood by the capture pipeline.
/// </summary>
public enum KeyboardKey
{
    /// <summary>No logical key.</summary>
    None = 0,

    /// <summary>A printable character stored in <see cref="KeyEventArgs.Character"/>.</summary>
    Character,

    /// <summary>The Enter or Return key.</summary>
    Enter,

    /// <summary>The Tab key.</summary>
    Tab,

    /// <summary>The Backspace key.</summary>
    Backspace,

    /// <summary>The forward Delete key.</summary>
    Delete,

    /// <summary>The Escape key.</summary>
    Escape,

    /// <summary>The Space key.</summary>
    Space,

    /// <summary>The Insert key.</summary>
    Insert,

    /// <summary>The Up arrow key.</summary>
    Up,

    /// <summary>The Down arrow key.</summary>
    Down,

    /// <summary>The Left arrow key.</summary>
    Left,

    /// <summary>The Right arrow key.</summary>
    Right,

    /// <summary>The Home key.</summary>
    Home,

    /// <summary>The End key.</summary>
    End,

    /// <summary>The Page Up key.</summary>
    PageUp,

    /// <summary>The Page Down key.</summary>
    PageDown,

    /// <summary>The Caps Lock key.</summary>
    CapsLock,

    /// <summary>The Num Lock key.</summary>
    NumLock,

    /// <summary>The Scroll Lock key.</summary>
    ScrollLock,

    /// <summary>The Print Screen key.</summary>
    PrintScreen,

    /// <summary>The Pause or Break key.</summary>
    PauseBreak,

    /// <summary>The context-menu key.</summary>
    Menu,

    /// <summary>The F1 function key.</summary>
    F1,

    /// <summary>The F2 function key.</summary>
    F2,

    /// <summary>The F3 function key.</summary>
    F3,

    /// <summary>The F4 function key.</summary>
    F4,

    /// <summary>The F5 function key.</summary>
    F5,

    /// <summary>The F6 function key.</summary>
    F6,

    /// <summary>The F7 function key.</summary>
    F7,

    /// <summary>The F8 function key.</summary>
    F8,

    /// <summary>The F9 function key.</summary>
    F9,

    /// <summary>The F10 function key.</summary>
    F10,

    /// <summary>The F11 function key.</summary>
    F11,

    /// <summary>The F12 function key.</summary>
    F12,

    /// <summary>The F13 function key.</summary>
    F13,

    /// <summary>The F14 function key.</summary>
    F14,

    /// <summary>The F15 function key.</summary>
    F15,

    /// <summary>The F16 function key.</summary>
    F16,

    /// <summary>The F17 function key.</summary>
    F17,

    /// <summary>The F18 function key.</summary>
    F18,

    /// <summary>The F19 function key.</summary>
    F19,

    /// <summary>The F20 function key.</summary>
    F20,

    /// <summary>The F21 function key.</summary>
    F21,

    /// <summary>The F22 function key.</summary>
    F22,

    /// <summary>The F23 function key.</summary>
    F23,

    /// <summary>The F24 function key.</summary>
    F24,

    /// <summary>The volume-up key.</summary>
    VolumeUp,

    /// <summary>The volume-down key.</summary>
    VolumeDown,

    /// <summary>The volume-mute key.</summary>
    VolumeMute,

    /// <summary>The navigation Back key.</summary>
    Back,

    /// <summary>The navigation Forward key.</summary>
    Forward,

    /// <summary>The Search key.</summary>
    Search,

    /// <summary>The media play/pause key.</summary>
    MediaPlayPause,

    /// <summary>The media stop key.</summary>
    MediaStop,

    /// <summary>The next-track media key.</summary>
    MediaNext,

    /// <summary>The previous-track media key.</summary>
    MediaPrevious
}
