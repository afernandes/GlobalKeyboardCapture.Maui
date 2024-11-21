using Windows.System;

namespace GlobalKeyboardCapture.Maui.Platforms.Windows
{
    internal class KeyboardHelper
    {
        public static char? ToChar(VirtualKey key)
        {
            // Letras (A-Z)
            if (key >= VirtualKey.A && key <= VirtualKey.Z)
                return (char)('A' + (key - VirtualKey.A));

            // Números principais (0-9)
            if (key >= VirtualKey.Number0 && key <= VirtualKey.Number9)
                return (char)('0' + (key - VirtualKey.Number0));

            // Números do teclado numérico (0-9)
            if (key >= VirtualKey.NumberPad0 && key <= VirtualKey.NumberPad9)
                return (char)('0' + (key - VirtualKey.NumberPad0));

            // Operadores e caracteres especiais
            return key switch
            {
                VirtualKey.Space => ' ',

                // Operadores matemáticos
                VirtualKey.Add => '+',
                VirtualKey.Subtract => '-',
                VirtualKey.Multiply => '*',
                VirtualKey.Divide => '/',

                // Pontuação
                VirtualKey.Decimal => '.',
                VirtualKey.Separator => ',',

                // OEM Specific Keys
                (VirtualKey)194 => '.', // Represents the decimal point on the numpad
                (VirtualKey)186 => ';', // OEM 1 (Semicolon)
                (VirtualKey)187 => '=', // OEM Plus (Equal sign)
                (VirtualKey)188 => ',', // OEM Comma (Comma)
                (VirtualKey)189 => '-', // OEM Minus (Minus sign)
                (VirtualKey)190 => '.', // OEM Period (Period)
                (VirtualKey)191 => '/', // OEM 2 (Slash)
                (VirtualKey)192 => '`', // OEM 3 (Grave accent)
                (VirtualKey)219 => '[', // OEM 4 (Left bracket)
                (VirtualKey)220 => '\\', // OEM 5 (Backslash)
                (VirtualKey)221 => ']', // OEM 6 (Right bracket)
                (VirtualKey)222 => '\'', // OEM 7 (Single quote)

                _ => null
            };
        }

        public static string? ToFunction(VirtualKey key)
        {
            return key switch
            {
                VirtualKey.F1 => "F1",
                VirtualKey.F2 => "F2",
                VirtualKey.F3 => "F3",
                VirtualKey.F4 => "F4",
                VirtualKey.F5 => "F5",
                VirtualKey.F6 => "F6",
                VirtualKey.F7 => "F7",
                VirtualKey.F8 => "F8",
                VirtualKey.F9 => "F9",
                VirtualKey.F10 => "F10",
                VirtualKey.F11 => "F11",
                VirtualKey.F12 => "F12",
                VirtualKey.F13 => "F13",
                VirtualKey.F14 => "F14",
                VirtualKey.F15 => "F15",
                VirtualKey.F16 => "F16",
                VirtualKey.F17 => "F17",
                VirtualKey.F18 => "F18",
                VirtualKey.F19 => "F19",
                VirtualKey.F20 => "F20",
                VirtualKey.F21 => "F21",
                VirtualKey.F22 => "F22",
                VirtualKey.F23 => "F23",
                VirtualKey.F24 => "F24",
                _ => null
            };
        }
    }
}
