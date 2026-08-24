using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Core.Mapping;

internal static class AppleKeyMapper
{
    public static AppleKeyMapping Map(int keyCode, bool shift, bool capsLock)
    {
        if (keyCode is >= 0x04 and <= 0x1D)
        {
            var character = (char)('a' + keyCode - 0x04);
            if (shift ^ capsLock)
                character = char.ToUpperInvariant(character);
            return Character(character);
        }

        if (keyCode is >= 0x1E and <= 0x27)
        {
            const string unshifted = "1234567890";
            const string shifted = "!@#$%^&*()";
            var index = keyCode - 0x1E;
            return Character(shift ? shifted[index] : unshifted[index]);
        }

        if (keyCode is >= 0x3A and <= 0x45)
            return Named((KeyboardKey)((int)KeyboardKey.F1 + keyCode - 0x3A));
        if (keyCode is >= 0x68 and <= 0x6F)
            return Named((KeyboardKey)((int)KeyboardKey.F13 + keyCode - 0x68));

        if (keyCode is >= 0x59 and <= 0x61)
            return Character((char)('1' + keyCode - 0x59), KeyLocation.Numpad);

        return keyCode switch
        {
            0x28 => Named(KeyboardKey.Enter),
            0x29 => Named(KeyboardKey.Escape),
            0x2A => Named(KeyboardKey.Backspace),
            0x2B => Named(KeyboardKey.Tab),
            0x2C => Named(KeyboardKey.Space),
            0x2D => Character(shift ? '_' : '-'),
            0x2E => Character(shift ? '+' : '='),
            0x2F => Character(shift ? '{' : '['),
            0x30 => Character(shift ? '}' : ']'),
            0x31 => Character(shift ? '|' : '\\'),
            0x33 => Character(shift ? ':' : ';'),
            0x34 => Character(shift ? '"' : '\''),
            0x35 => Character(shift ? '~' : '`'),
            0x36 => Character(shift ? '<' : ','),
            0x37 => Character(shift ? '>' : '.'),
            0x38 => Character(shift ? '?' : '/'),
            0x39 => Named(KeyboardKey.CapsLock),
            0x46 => Named(KeyboardKey.PrintScreen),
            0x47 => Named(KeyboardKey.ScrollLock),
            0x48 => Named(KeyboardKey.PauseBreak),
            0x49 => Named(KeyboardKey.Insert),
            0x4A => Named(KeyboardKey.Home),
            0x4B => Named(KeyboardKey.PageUp),
            0x4C => Named(KeyboardKey.Delete),
            0x4D => Named(KeyboardKey.End),
            0x4E => Named(KeyboardKey.PageDown),
            0x4F => Named(KeyboardKey.Right),
            0x50 => Named(KeyboardKey.Left),
            0x51 => Named(KeyboardKey.Down),
            0x52 => Named(KeyboardKey.Up),
            0x53 => Named(KeyboardKey.NumLock, KeyLocation.Numpad),
            0x54 => Character('/', KeyLocation.Numpad),
            0x55 => Character('*', KeyLocation.Numpad),
            0x56 => Character('-', KeyLocation.Numpad),
            0x57 => Character('+', KeyLocation.Numpad),
            0x58 => Named(KeyboardKey.Enter, KeyLocation.Numpad),
            0x62 => Character('0', KeyLocation.Numpad),
            0x63 => Character('.', KeyLocation.Numpad),
            0x65 => Named(KeyboardKey.Menu),
            >= 0xE0 and <= 0xE3 => new AppleKeyMapping(KeyboardKey.None, null, KeyLocation.Left),
            >= 0xE4 and <= 0xE7 => new AppleKeyMapping(KeyboardKey.None, null, KeyLocation.Right),
            _ => new AppleKeyMapping(KeyboardKey.None, null, KeyLocation.Standard)
        };
    }

    private static AppleKeyMapping Character(char character, KeyLocation location = KeyLocation.Standard) =>
        new(KeyboardKey.Character, character, location);

    private static AppleKeyMapping Named(KeyboardKey key, KeyLocation location = KeyLocation.Standard) =>
        new(key, null, location);
}

internal readonly record struct AppleKeyMapping(
    KeyboardKey Key,
    char? Character,
    KeyLocation Location);
