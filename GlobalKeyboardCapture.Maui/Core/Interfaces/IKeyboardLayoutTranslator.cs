using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>
/// Optionally translates a physical key through the operating system's active keyboard layout.
/// Implementations must be thread-safe and should avoid blocking the native input callback.
/// </summary>
public interface IKeyboardLayoutTranslator
{
    /// <summary>Attempts to obtain one printable UTF-16 character for a physical key.</summary>
    /// <param name="context">Physical key, platform, and modifier state.</param>
    /// <param name="character">The translated printable character when successful.</param>
    /// <returns><see langword="true"/> when a character is available.</returns>
    bool TryTranslate(
        in KeyboardLayoutTranslationContext context,
        out char character);
}
