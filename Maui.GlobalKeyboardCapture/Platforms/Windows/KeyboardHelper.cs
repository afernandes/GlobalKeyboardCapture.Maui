using Windows.System;

namespace Maui.GlobalKeyboardCapture.Platforms.Windows
{
    internal class KeyboardHelper
    {
        public static char? ToChar(VirtualKey key, bool shift)
        {
            // convert virtual key to char
            if (32 == (int)key)
                return ' ';

            VirtualKey search;

            // look for simple letter
            foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                if (Enum.TryParse<VirtualKey>(letter.ToString(), out search) && search.Equals(key))
                    return (shift) ? letter : letter.ToString()[0];
            }

            // look for simple number
            foreach (var number in "1234567890")
            {
                if (Enum.TryParse<VirtualKey>($"Number{number}", out search) && search.Equals(key))
                    return number;

                if (Enum.TryParse<VirtualKey>($"NumberPad{number}", out search) && search.Equals(key))
                    return number;
            }

            // not found
            return null;
        }

        public static string ToFunction(VirtualKey key)
        {
            VirtualKey search;

            foreach (var letter in new[] { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" })
            {
                if (Enum.TryParse<VirtualKey>(letter.ToString(), out search) && search.Equals(key))
                    return letter;
            }

            return null;
        }
    }
}
