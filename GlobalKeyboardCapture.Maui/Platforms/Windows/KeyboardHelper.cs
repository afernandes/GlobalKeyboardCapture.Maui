using System.Runtime.CompilerServices;
using Windows.System;

namespace GlobalKeyboardCapture.Maui.Platforms.Windows;

internal static class KeyboardHelper
{
    private static readonly Dictionary<VirtualKey, char> SpecialKeys;
    private static readonly Dictionary<VirtualKey, string> FunctionKeys;

    static KeyboardHelper()
    {
        // Initialize special keys mapping
        SpecialKeys = new Dictionary<VirtualKey, char>(50)
        {
            // Math operators
            { VirtualKey.Add, '+' },
            { VirtualKey.Subtract, '-' },
            { VirtualKey.Multiply, '*' },
            { VirtualKey.Divide, '/' },

            // Punctuation
            { VirtualKey.Decimal, '.' },
            { VirtualKey.Separator, ',' },

            // OEM Specific Keys
            { (VirtualKey)194, '.' },  // Decimal point on numpad
            { (VirtualKey)186, ';' },  // OEM 1 (Semicolon)
            { (VirtualKey)187, '=' },  // OEM Plus
            { (VirtualKey)188, ',' },  // OEM Comma
            { (VirtualKey)189, '-' },  // OEM Minus
            { (VirtualKey)190, '.' },  // OEM Period
            { (VirtualKey)191, '/' },  // OEM 2 (Slash)
            { (VirtualKey)192, '`' },  // OEM 3 (Grave)
            { (VirtualKey)219, '[' },  // OEM 4 (Left bracket)
            { (VirtualKey)220, '\\' }, // OEM 5 (Backslash)
            { (VirtualKey)221, ']' },  // OEM 6 (Right bracket)
            { (VirtualKey)222, '\'' }  // OEM 7 (Single quote)
        };

        // Initialize function keys mapping (F1-F24)
        FunctionKeys = new Dictionary<VirtualKey, string>(24);
        for (int i = 1; i <= 24; i++)
        {
            var key = (VirtualKey)((int)VirtualKey.F1 + (i - 1));
            FunctionKeys[key] = $"F{i}";
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char? ToChar(VirtualKey key)
    {
        // Check letters (A-Z)
        if (key >= VirtualKey.A && key <= VirtualKey.Z)
            return (char)('A' + (key - VirtualKey.A));

        // Check main numbers (0-9)
        if (key >= VirtualKey.Number0 && key <= VirtualKey.Number9)
            return (char)('0' + (key - VirtualKey.Number0));

        // Check numpad numbers (0-9)
        if (key >= VirtualKey.NumberPad0 && key <= VirtualKey.NumberPad9)
            return (char)('0' + (key - VirtualKey.NumberPad0));

        // Check special keys
        return SpecialKeys.TryGetValue(key, out char value) ? value : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ToFunction(VirtualKey key)
    {
        return FunctionKeys.GetValueOrDefault(key);
    }
}
