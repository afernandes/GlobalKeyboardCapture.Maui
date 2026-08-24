namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Provides allocation-free physical-key and modifier data to an optional keyboard-layout translator.
/// </summary>
public readonly record struct KeyboardLayoutTranslationContext
{
    /// <summary>Creates a keyboard-layout translation request.</summary>
    /// <param name="platform">The originating platform.</param>
    /// <param name="nativeKeyCode">The native physical key code.</param>
    /// <param name="modifiers">The active normalized modifiers.</param>
    /// <param name="capsLock">Whether Caps Lock is active.</param>
    public KeyboardLayoutTranslationContext(
        KeyboardPlatform platform,
        int nativeKeyCode,
        KeyModifiers modifiers,
        bool capsLock)
    {
        Platform = platform;
        NativeKeyCode = nativeKeyCode;
        Modifiers = modifiers;
        CapsLock = capsLock;
    }

    /// <summary>Gets the originating platform.</summary>
    public KeyboardPlatform Platform { get; }

    /// <summary>Gets the native physical key code.</summary>
    public int NativeKeyCode { get; }

    /// <summary>Gets the active normalized modifiers.</summary>
    public KeyModifiers Modifiers { get; }

    /// <summary>Gets whether Caps Lock is active.</summary>
    public bool CapsLock { get; }
}
