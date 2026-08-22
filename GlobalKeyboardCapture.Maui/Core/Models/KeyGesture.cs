using System.Text;

namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// A normalized, platform-neutral key and modifier combination.
/// </summary>
public readonly record struct KeyGesture
{
    private const int INITIAL_STRING_CAPACITY = 32;

    private static readonly Dictionary<string, KeyModifiers> ModifierNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ctrl"] = KeyModifiers.Control,
        ["Control"] = KeyModifiers.Control,
        ["Alt"] = KeyModifiers.Alt,
        ["Shift"] = KeyModifiers.Shift,
        ["Win"] = KeyModifiers.Meta,
        ["Windows"] = KeyModifiers.Meta,
        ["Meta"] = KeyModifiers.Meta,
        ["Command"] = KeyModifiers.Meta,
        ["Cmd"] = KeyModifiers.Meta
    };

    private static readonly Dictionary<string, KeyboardKey> NamedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Enter"] = KeyboardKey.Enter,
        ["Return"] = KeyboardKey.Enter,
        ["Tab"] = KeyboardKey.Tab,
        ["Backspace"] = KeyboardKey.Backspace,
        ["Delete"] = KeyboardKey.Delete,
        ["Del"] = KeyboardKey.Delete,
        ["Escape"] = KeyboardKey.Escape,
        ["Esc"] = KeyboardKey.Escape,
        ["Space"] = KeyboardKey.Space,
        ["Spacebar"] = KeyboardKey.Space,
        ["Insert"] = KeyboardKey.Insert,
        ["Ins"] = KeyboardKey.Insert,
        ["Up"] = KeyboardKey.Up,
        ["ArrowUp"] = KeyboardKey.Up,
        ["Down"] = KeyboardKey.Down,
        ["ArrowDown"] = KeyboardKey.Down,
        ["Left"] = KeyboardKey.Left,
        ["ArrowLeft"] = KeyboardKey.Left,
        ["Right"] = KeyboardKey.Right,
        ["ArrowRight"] = KeyboardKey.Right,
        ["Home"] = KeyboardKey.Home,
        ["End"] = KeyboardKey.End,
        ["PageUp"] = KeyboardKey.PageUp,
        ["PgUp"] = KeyboardKey.PageUp,
        ["PageDown"] = KeyboardKey.PageDown,
        ["PgDn"] = KeyboardKey.PageDown,
        ["CapsLock"] = KeyboardKey.CapsLock,
        ["NumLock"] = KeyboardKey.NumLock,
        ["ScrollLock"] = KeyboardKey.ScrollLock,
        ["PrintScreen"] = KeyboardKey.PrintScreen,
        ["PrtSc"] = KeyboardKey.PrintScreen,
        ["PauseBreak"] = KeyboardKey.PauseBreak,
        ["Pause"] = KeyboardKey.PauseBreak,
        ["Menu"] = KeyboardKey.Menu,
        ["VolumeUp"] = KeyboardKey.VolumeUp,
        ["VolumeDown"] = KeyboardKey.VolumeDown,
        ["VolumeMute"] = KeyboardKey.VolumeMute,
        ["Mute"] = KeyboardKey.VolumeMute,
        ["Back"] = KeyboardKey.Back,
        ["Forward"] = KeyboardKey.Forward,
        ["Search"] = KeyboardKey.Search,
        ["MediaPlayPause"] = KeyboardKey.MediaPlayPause,
        ["MediaStop"] = KeyboardKey.MediaStop,
        ["MediaNext"] = KeyboardKey.MediaNext,
        ["MediaPrevious"] = KeyboardKey.MediaPrevious
    };

    /// <summary>Gets the platform-neutral logical key.</summary>
    public KeyboardKey Key { get; }

    /// <summary>Gets the printable character when <see cref="Key"/> is <see cref="KeyboardKey.Character"/>.</summary>
    public char? Character { get; }

    /// <summary>Gets the required modifier flags.</summary>
    public KeyModifiers Modifiers { get; }

    /// <summary>Creates a printable-character gesture.</summary>
    /// <param name="character">The printable character.</param>
    /// <param name="modifiers">The required modifiers.</param>
    public KeyGesture(char character, KeyModifiers modifiers = KeyModifiers.None)
    {
        ValidateModifiers(modifiers);
        if (char.IsControl(character))
            throw new ArgumentOutOfRangeException(nameof(character), "A gesture character cannot be a control character.");

        if (character == ' ')
        {
            Key = KeyboardKey.Space;
            Character = null;
        }
        else
        {
            Key = KeyboardKey.Character;
            Character = char.ToUpperInvariant(character);
        }

        Modifiers = modifiers;
    }

    /// <summary>Creates a named-key gesture.</summary>
    /// <param name="key">A named logical key other than <see cref="KeyboardKey.None"/> or <see cref="KeyboardKey.Character"/>.</param>
    /// <param name="modifiers">The required modifiers.</param>
    public KeyGesture(KeyboardKey key, KeyModifiers modifiers = KeyModifiers.None)
    {
        ValidateModifiers(modifiers);
        if (key is KeyboardKey.None or KeyboardKey.Character)
            throw new ArgumentOutOfRangeException(nameof(key), "Use the character constructor for character gestures.");

        Key = key;
        Character = null;
        Modifiers = modifiers;
    }

    /// <summary>Parses a textual gesture using canonical names and supported aliases.</summary>
    /// <param name="value">The gesture text, such as <c>Ctrl+S</c>.</param>
    /// <returns>The parsed gesture.</returns>
    public static KeyGesture Parse(string value)
    {
        if (!TryParse(value, out var gesture))
            throw new FormatException($"'{value}' is not a valid key gesture.");

        return gesture;
    }

    /// <summary>Attempts to parse a textual gesture.</summary>
    /// <param name="value">The gesture text.</param>
    /// <param name="gesture">The parsed gesture when successful.</param>
    /// <returns><see langword="true"/> when the text is valid.</returns>
    public static bool TryParse(string? value, out KeyGesture gesture)
    {
        gesture = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        if (text == "+")
        {
            gesture = new KeyGesture('+');
            return true;
        }

        var plusKey = text.EndsWith("++", StringComparison.Ordinal);
        var tokenText = plusKey ? text[..^2] : text;
        var tokens = tokenText.Split('+', StringSplitOptions.TrimEntries);
        if (tokens.Length == 0 || tokens.Any(string.IsNullOrEmpty))
            return false;

        var modifiers = KeyModifiers.None;
        KeyboardKey mainKey = KeyboardKey.None;
        char? character = plusKey ? '+' : null;

        foreach (var token in tokens)
        {
            if (ModifierNames.TryGetValue(token, out var modifier))
            {
                modifiers |= modifier;
                continue;
            }

            if (mainKey != KeyboardKey.None || character.HasValue)
                return false;

            if (!TryParseMainKey(token, out mainKey, out character))
                return false;
        }

        if (mainKey == KeyboardKey.None && !character.HasValue)
            return false;

        gesture = character.HasValue
            ? new KeyGesture(character.Value, modifiers)
            : new KeyGesture(mainKey, modifiers);
        return true;
    }

    /// <summary>Creates a gesture from a normalized keyboard event.</summary>
    /// <param name="keyEvent">The source event.</param>
    /// <returns>The equivalent gesture.</returns>
    public static KeyGesture FromEvent(KeyEventArgs keyEvent)
    {
        ArgumentNullException.ThrowIfNull(keyEvent);
        if (!TryFromEvent(keyEvent, out var gesture))
            throw new InvalidOperationException("The keyboard event does not contain a logical key.");

        return gesture;
    }

    /// <summary>Attempts to create a gesture from a normalized keyboard event.</summary>
    /// <param name="keyEvent">The source event.</param>
    /// <param name="gesture">The equivalent gesture when successful.</param>
    /// <returns><see langword="true"/> when the event contains a logical key.</returns>
    public static bool TryFromEvent(KeyEventArgs? keyEvent, out KeyGesture gesture)
    {
        gesture = default;
        if (keyEvent is null)
            return false;

        var key = keyEvent.Key;
        if (key == KeyboardKey.None)
            return false;

        gesture = key == KeyboardKey.Character && keyEvent.Character.HasValue
            ? new KeyGesture(keyEvent.Character.Value, keyEvent.Modifiers)
            : new KeyGesture(key, keyEvent.Modifiers);
        return true;
    }

    /// <summary>Determines whether a normalized event equals this gesture.</summary>
    /// <param name="keyEvent">The event to compare.</param>
    /// <returns><see langword="true"/> when key and modifiers match.</returns>
    public bool Matches(KeyEventArgs keyEvent) =>
        TryFromEvent(keyEvent, out var gesture) && Equals(gesture);

    /// <summary>Returns the canonical <c>Ctrl+Alt+Shift+Win+Key</c> representation.</summary>
    /// <returns>The canonical gesture string.</returns>
    public override string ToString()
    {
        if (Key == KeyboardKey.None)
            return string.Empty;

        var result = new StringBuilder(INITIAL_STRING_CAPACITY);
        var modifiers = Modifiers;
        AppendModifier(result, KeyModifiers.Control, "Ctrl");
        AppendModifier(result, KeyModifiers.Alt, "Alt");
        AppendModifier(result, KeyModifiers.Shift, "Shift");
        AppendModifier(result, KeyModifiers.Meta, "Win");

        if (result.Length > 0)
            result.Append('+');
        result.Append(GetCanonicalKeyName(Key, Character));
        return result.ToString();

        void AppendModifier(StringBuilder builder, KeyModifiers modifier, string name)
        {
            if ((modifiers & modifier) == 0)
                return;
            if (builder.Length > 0)
                builder.Append('+');
            builder.Append(name);
        }
    }

    internal static bool TryParseFunctionKey(ReadOnlySpan<char> token, out KeyboardKey key)
    {
        key = KeyboardKey.None;
        if (token.Length is not (2 or 3) || (token[0] != 'F' && token[0] != 'f'))
            return false;
        if (!int.TryParse(token[1..], out var number) || number is < 1 or > 24)
            return false;

        key = (KeyboardKey)((int)KeyboardKey.F1 + number - 1);
        return true;
    }

    internal static string GetCanonicalKeyName(KeyboardKey key, char? character = null)
    {
        if (key == KeyboardKey.Character)
            return character?.ToString() ?? string.Empty;
        if (key is >= KeyboardKey.F1 and <= KeyboardKey.F24)
            return $"F{(int)key - (int)KeyboardKey.F1 + 1}";

        return key switch
        {
            KeyboardKey.Enter => "Enter",
            KeyboardKey.Tab => "Tab",
            KeyboardKey.Backspace => "Backspace",
            KeyboardKey.Delete => "Delete",
            KeyboardKey.Escape => "Esc",
            KeyboardKey.Space => "Space",
            KeyboardKey.Insert => "Insert",
            KeyboardKey.Up => "Up",
            KeyboardKey.Down => "Down",
            KeyboardKey.Left => "Left",
            KeyboardKey.Right => "Right",
            KeyboardKey.Home => "Home",
            KeyboardKey.End => "End",
            KeyboardKey.PageUp => "PageUp",
            KeyboardKey.PageDown => "PageDown",
            KeyboardKey.CapsLock => "CapsLock",
            KeyboardKey.NumLock => "NumLock",
            KeyboardKey.ScrollLock => "ScrollLock",
            KeyboardKey.PrintScreen => "PrintScreen",
            KeyboardKey.PauseBreak => "PauseBreak",
            KeyboardKey.Menu => "Menu",
            KeyboardKey.VolumeUp => "VolumeUp",
            KeyboardKey.VolumeDown => "VolumeDown",
            KeyboardKey.VolumeMute => "VolumeMute",
            KeyboardKey.Back => "Back",
            KeyboardKey.Forward => "Forward",
            KeyboardKey.Search => "Search",
            KeyboardKey.MediaPlayPause => "MediaPlayPause",
            KeyboardKey.MediaStop => "MediaStop",
            KeyboardKey.MediaNext => "MediaNext",
            KeyboardKey.MediaPrevious => "MediaPrevious",
            _ => string.Empty
        };
    }

    private static bool TryParseMainKey(string token, out KeyboardKey key, out char? character)
    {
        character = null;
        if (NamedKeys.TryGetValue(token, out key))
            return true;
        if (TryParseFunctionKey(token, out key))
            return true;
        if (token.Equals("Plus", StringComparison.OrdinalIgnoreCase)
            || token.Equals("OemPlus", StringComparison.OrdinalIgnoreCase)
            || token.Equals("Add", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyboardKey.Character;
            character = '+';
            return true;
        }
        if (token.Equals("Minus", StringComparison.OrdinalIgnoreCase)
            || token.Equals("OemMinus", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyboardKey.Character;
            character = '-';
            return true;
        }
        if (token.Length == 1 && !char.IsControl(token[0]))
        {
            key = KeyboardKey.Character;
            character = token[0];
            return true;
        }

        key = KeyboardKey.None;
        return false;
    }

    private static void ValidateModifiers(KeyModifiers modifiers)
    {
        if ((modifiers & ~KeyModifiers.All) != 0)
            throw new ArgumentOutOfRangeException(nameof(modifiers));
    }
}
