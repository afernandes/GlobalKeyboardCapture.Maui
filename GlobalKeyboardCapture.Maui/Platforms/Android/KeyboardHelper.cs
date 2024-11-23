using System.Runtime.CompilerServices;

namespace GlobalKeyboardCapture.Maui.Platforms.Android;

internal static class KeyboardHelper
{
    private static readonly HashSet<string> FKeys = new(["F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"], StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<char> SingleCharKeys;
    private static readonly Dictionary<string, char> KeyMap;

    static KeyboardHelper()
    {
        // Initialize HashSet of unique characters (for quick checks)
        SingleCharKeys = new HashSet<char>()
            {
                ' ' // Space
            };

        // Add letters (A-Z)
        for (char c = 'A'; c <= 'Z'; c++)
            SingleCharKeys.Add(c);

        // Add numbers (0-9)
        for (char c = '0'; c <= '9'; c++)
            SingleCharKeys.Add(c);

        // Initialize dictionary with adequate initial capacity
        KeyMap = new Dictionary<string, char>(100, StringComparer.OrdinalIgnoreCase);

        // Map space
        KeyMap["SPACE"] = ' ';

        // Map numbers and their variations
        for (char n = '0'; n <= '9'; n++)
        {
            KeyMap[$"NUM{n}"] = n;
            KeyMap[$"NUMPAD{n}"] = n;
        }

        // Map math operators and their variations (using tuple for clarity)
        var mathOperators = new[]
        {
                (".", new[] { "PERIOD", "NUMPAD_DOT" }),
                ("+", new[] { "PLUS", "NUMPAD_ADD" }),
                ("-", new[] { "MINUS", "NUMPAD_SUBTRACT" }),
                ("*", new[] { "MULTIPLY", "NUMPAD_MULTIPLY" }),
                ("/", new[] { "DIVIDE", "NUMPAD_DIVIDE", "SLASH" })
            };

        foreach (var (symbol, variants) in mathOperators)
        {
            KeyMap[symbol] = symbol[0];
            foreach (var variant in variants)
                KeyMap[variant] = symbol[0];
        }

        // Map symbols and punctuation
        var symbolMappings = new[]
        {
                ("EQUALS", '='),
                ("COMMA", ','),
                ("SEMICOLON", ';'),
                ("COLON", ':'),
                ("LEFT_BRACKET", '['),
                ("RIGHT_BRACKET", ']'),
                ("BACKSLASH", '\\'),
                ("APOSTROPHE", '\''),
                ("QUOTE", '"'),
                ("GRAVE", '`'),
                ("TILDE", '~'),
                ("AT", '@'),
                ("POUND", '#'),
                ("DOLLAR", '$'),
                ("PERCENT", '%'),
                ("CARET", '^'),
                ("AMPERSAND", '&'),
                ("UNDERSCORE", '_'),
                ("PIPE", '|'),
                ("EXCLAMATION", '!'),
                ("QUESTION", '?'),
                ("PARENTHESIS_LEFT", '('),
                ("PARENTHESIS_RIGHT", ')'),
                ("BRACE_LEFT", '{'),
                ("BRACE_RIGHT", '}')
            };

        // Add symbols and their characters
        foreach (var (word, symbol) in symbolMappings)
        {
            KeyMap[word] = symbol;
            SingleCharKeys.Add(symbol);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ToFunction(string key)
    {
        return FKeys.Contains(key) ? key : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char? ToChar(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        return ProcessKey(key.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char? ProcessKey(ReadOnlySpan<char> key)
    {
        // Remove whitespace
        key = key.Trim();
        if (key.IsEmpty) return null;

        // Check single character (most common case)
        if (key.Length == 1 && SingleCharKeys.Contains(char.ToUpperInvariant(key[0])))
            return char.ToUpperInvariant(key[0]);

        // Search in key mappings dictionary
        return KeyMap.TryGetValue(key.ToString(), out char value) ? value : null;
    }
}

