namespace GlobalKeyboardCapture.Maui.Core.Mapping;

internal static class PlatformKeyMapper
{
    private static readonly Dictionary<string, char> AndroidNamedCharacters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["PERIOD"] = '.',
            ["NUMPAD_DOT"] = '.',
            ["PLUS"] = '+',
            ["NUMPAD_ADD"] = '+',
            ["MINUS"] = '-',
            ["NUMPAD_SUBTRACT"] = '-',
            ["MULTIPLY"] = '*',
            ["NUMPAD_MULTIPLY"] = '*',
            ["DIVIDE"] = '/',
            ["NUMPAD_DIVIDE"] = '/',
            ["SLASH"] = '/',
            ["EQUALS"] = '=',
            ["COMMA"] = ',',
            ["SEMICOLON"] = ';',
            ["COLON"] = ':',
            ["LEFT_BRACKET"] = '[',
            ["RIGHT_BRACKET"] = ']',
            ["BACKSLASH"] = '\\',
            ["APOSTROPHE"] = '\'',
            ["QUOTE"] = '"',
            ["GRAVE"] = '`',
            ["TILDE"] = '~',
            ["AT"] = '@',
            ["POUND"] = '#',
            ["DOLLAR"] = '$',
            ["PERCENT"] = '%',
            ["CARET"] = '^',
            ["AMPERSAND"] = '&',
            ["UNDERSCORE"] = '_',
            ["PIPE"] = '|',
            ["EXCLAMATION"] = '!',
            ["QUESTION"] = '?',
            ["PARENTHESIS_LEFT"] = '(',
            ["PARENTHESIS_RIGHT"] = ')',
            ["BRACE_LEFT"] = '{',
            ["BRACE_RIGHT"] = '}'
        };

    private static readonly Dictionary<int, char> WindowsSpecialCharacters = new()
    {
        [107] = '+',
        [109] = '-',
        [106] = '*',
        [111] = '/',
        [110] = '.',
        [108] = ',',
        [194] = '.',
        [186] = ';',
        [187] = '=',
        [188] = ',',
        [189] = '-',
        [190] = '.',
        [191] = '/',
        [192] = '`',
        [219] = '[',
        [220] = '\\',
        [221] = ']',
        [222] = '\''
    };

    public static char? MapAndroidDisplayLabel(char key)
    {
        if (key == '\0' || char.IsWhiteSpace(key))
            return null;

        var upper = char.ToUpperInvariant(key);
        return char.IsAsciiLetterOrDigit(upper) || IsSupportedPunctuation(upper)
            ? upper
            : null;
    }

    public static char? MapAndroidDisplayLabel(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var value = key.Trim();
        if (value.Length == 1)
            return MapAndroidDisplayLabel(value[0]);
        if (value.Length is 4 or 7 && value.StartsWith("NUM", StringComparison.OrdinalIgnoreCase))
        {
            var last = value[^1];
            if (char.IsAsciiDigit(last)
                && (value.Length == 4 || value.StartsWith("NUMPAD", StringComparison.OrdinalIgnoreCase)))
            {
                return last;
            }
        }

        return AndroidNamedCharacters.TryGetValue(value, out var mappedValue)
            ? mappedValue
            : null;
    }

    public static char? MapWindowsVirtualKey(int virtualKey)
    {
        if (virtualKey is >= 65 and <= 90)
            return (char)virtualKey;
        if (virtualKey is >= 48 and <= 57)
            return (char)virtualKey;
        if (virtualKey is >= 96 and <= 105)
            return (char)('0' + virtualKey - 96);
        return WindowsSpecialCharacters.TryGetValue(virtualKey, out var mappedValue)
            ? mappedValue
            : null;
    }

    public static string? MapWindowsFunctionKey(int virtualKey) =>
        virtualKey is >= 112 and <= 135
            ? $"F{virtualKey - 111}"
            : null;

    private static bool IsSupportedPunctuation(char value) => value is
        '.' or '+' or '-' or '*' or '/' or '=' or ',' or ';' or ':'
        or '[' or ']' or '\\' or '\'' or '"' or '`' or '~' or '@'
        or '#' or '$' or '%' or '^' or '&' or '_' or '|' or '!'
        or '?' or '(' or ')' or '{' or '}';
}
