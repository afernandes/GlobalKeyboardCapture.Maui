#if IOS || MACCATALYST
using Foundation;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using UIKit;

namespace GlobalKeyboardCapture.Maui.Sample;

internal static class AppleKeyboardLayoutCollector
{
    public static void Record(NSSet<UIPress> presses)
    {
        var services = IPlatformApplication.Current?.Services;
        var cache = services?.GetService<KeyboardLayoutTranslationCache>();
        if (cache is null)
            return;

#if IOS
        const KeyboardPlatform platform = KeyboardPlatform.iOS;
#else
        const KeyboardPlatform platform = KeyboardPlatform.MacCatalyst;
#endif

        foreach (var press in presses)
        {
            var key = press.Key;
            if (key is null)
                continue;

            var flags = key.ModifierFlags;
            var modifiers = KeyModifiers.None;
            if (flags.HasFlag(UIKeyModifierFlags.Control))
                modifiers |= KeyModifiers.Control;
            if (flags.HasFlag(UIKeyModifierFlags.Alternate))
                modifiers |= KeyModifiers.Alt;
            if (flags.HasFlag(UIKeyModifierFlags.Shift))
                modifiers |= KeyModifiers.Shift;
            if (flags.HasFlag(UIKeyModifierFlags.Command))
                modifiers |= KeyModifiers.Meta;

            var context = new KeyboardLayoutTranslationContext(
                platform,
                checked((int)key.KeyCode),
                modifiers,
                flags.HasFlag(UIKeyModifierFlags.AlphaShift));
            cache.SetTranslation(in context, key.Characters);
        }
    }
}
#endif
