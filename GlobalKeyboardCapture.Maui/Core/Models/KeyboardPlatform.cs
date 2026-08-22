namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Platform that produced a keyboard event.
/// </summary>
public enum KeyboardPlatform
{
    /// <summary>The originating platform is unavailable.</summary>
    Unknown = 0,

    /// <summary>Microsoft Windows through WinUI.</summary>
    Windows,

    /// <summary>Android.</summary>
    Android,

    /// <summary>Apple iOS.</summary>
    iOS,

    /// <summary>Apple Mac Catalyst.</summary>
    MacCatalyst
}
