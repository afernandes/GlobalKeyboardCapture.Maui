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

        public static char? ToChar(string key, bool shift)
        {
            // convert virtual key to char
            if (32 == key[0])
                return ' ';

            // look for simple letter
            foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                if (key.Equals(letter.ToString(), StringComparison.OrdinalIgnoreCase))
                    return (shift) ? letter : letter.ToString()[0];
            }

            // look for simple number
            foreach (var number in "1234567890")
            {
                if (key.Equals(number.ToString(), StringComparison.OrdinalIgnoreCase))
                    return number;

                if (key.Equals("Num" + number))
                    return number;
            }

            // not found
            return null;
        }
    }
}
