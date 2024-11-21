namespace Maui.GlobalKeyboardCapture.Platforms.Android
{
    internal class KeyboardHelper
    {
        public static string ToFunction(string key)
        {
            foreach (var letter in new[] { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" })
            {
                if (key.Equals(letter, StringComparison.OrdinalIgnoreCase))
                    return letter;
            }

            return null;
        }

        public static char? ToChar(string key)
        {
            key = key?.Trim();
            if (string.IsNullOrEmpty(key)) return null;

            // Espaço
            if (32 == key[0] || key.Equals("Space", StringComparison.OrdinalIgnoreCase))
                return ' ';

            // Letras
            foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                if (key.Equals(letter.ToString(), StringComparison.OrdinalIgnoreCase))
                    return letter;
            }

            // Números e NumPad
            foreach (var number in "1234567890")
            {
                if (key.Equals(number.ToString(), StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("Num" + number) ||
                    key.Equals("Numpad" + number))
                    return number;
            }

            // Operadores matemáticos e pontuação
            switch (key.ToUpper())
            {
                // Operadores matemáticos
                case "PERIOD":
                case "NUMPAD_DOT":
                case ".": return '.';
                case "PLUS":
                case "NUMPAD_ADD":
                case "+": return '+';
                case "MINUS":
                case "NUMPAD_SUBTRACT":
                case "-": return '-';
                case "MULTIPLY":
                case "NUMPAD_MULTIPLY":
                case "*": return '*';
                case "DIVIDE":
                case "NUMPAD_DIVIDE":
                case "SLASH":
                case "/": return '/';

                // Símbolos e pontuação
                case "EQUALS":
                case "=": return '=';
                case "COMMA":
                case ",": return ',';
                case "SEMICOLON":
                case ";": return ';';
                case "COLON":
                case ":": return ':';
                case "LEFT_BRACKET":
                case "[": return '[';
                case "RIGHT_BRACKET":
                case "]": return ']';
                case "BACKSLASH":
                case "\\": return '\\';
                case "APOSTROPHE":
                case "'": return '\'';
                case "QUOTE":
                case "\"": return '"';
                case "GRAVE":
                case "`": return '`';
                case "TILDE":
                case "~": return '~';

                // Símbolos especiais
                case "AT":
                case "@": return '@';
                case "POUND":
                case "#": return '#';
                case "DOLLAR":
                case "$": return '$';
                case "PERCENT":
                case "%": return '%';
                case "CARET":
                case "^": return '^';
                case "AMPERSAND":
                case "&": return '&';
                case "UNDERSCORE":
                case "_": return '_';
                case "PIPE":
                case "|": return '|';
                case "EXCLAMATION":
                case "!": return '!';
                case "QUESTION":
                case "?": return '?';
                case "PARENTHESIS_LEFT":
                case "(": return '(';
                case "PARENTHESIS_RIGHT":
                case ")": return ')';
                case "BRACE_LEFT":
                case "{": return '{';
                case "BRACE_RIGHT":
                case "}": return '}';
            }

            return null;
        }
    }
}
