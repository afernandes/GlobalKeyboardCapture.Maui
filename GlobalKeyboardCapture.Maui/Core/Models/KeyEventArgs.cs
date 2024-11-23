using System.Runtime.CompilerServices;
using System.Text;

namespace GlobalKeyboardCapture.Maui.Core.Models;

public sealed class KeyEventArgs
{
    // Constants for string builder initial capacity
    private const int INITIAL_TOSTRING_CAPACITY = 32;

    // Platform-specific event
    public object PlatformEvent { get; set; }
    public bool Handled { get; set; }

    // Characters and Function
    public char? Character { get; set; }
    public string? FunctionKey { get; set; }

    // Modifier Keys
    public bool ControlKey { get; set; }
    public bool AltKey { get; set; }
    public bool ShiftKey { get; set; }
    public bool WindowsKey { get; set; }

    // Navigation Keys
    public bool UpKey { get; set; }
    public bool DownKey { get; set; }
    public bool LeftKey { get; set; }
    public bool RightKey { get; set; }
    public bool HomeKey { get; set; }
    public bool EndKey { get; set; }
    public bool PageUpKey { get; set; }
    public bool PageDownKey { get; set; }

    // Editing Keys
    public bool EnterKey { get; set; }
    public bool TabKey { get; set; }
    public bool BackspaceKey { get; set; }
    public bool DeleteKey { get; set; }
    public bool EscapeKey { get; set; }
    public bool SpaceKey { get; set; }
    public bool InsertKey { get; set; }

    // System Keys
    public bool CapsLockKey { get; set; }
    public bool NumLockKey { get; set; }
    public bool ScrollLockKey { get; set; }
    public bool PrintScreenKey { get; set; }
    public bool PauseBreakKey { get; set; }
    public bool MenuKey { get; set; }

    public bool OnlyWindows
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    }

    public bool OnlyAlt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && AltKey && !ControlKey && !ShiftKey;
    }

    public bool OnlyControl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && ControlKey && !ShiftKey;
    }

    public bool OnlyShift
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && !ControlKey && ShiftKey;
    }

    public bool NoSpecialKeysPressed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    }

    public bool AnySpecialKeyPressed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => WindowsKey || AltKey || ControlKey || ShiftKey;
    }

    // ToString implementation optimized
    public override string ToString()
    {
        var list = new StringBuilder(INITIAL_TOSTRING_CAPACITY);
        var needsSeparator = false;

        // Helper method to append with separator
        void AppendWithSeparator(string value)
        {
            if (needsSeparator)
                list.Append('+');
            list.Append(value);
            needsSeparator = true;
        }

        // Modifiers
        if (ControlKey) AppendWithSeparator("Ctrl");
        if (AltKey) AppendWithSeparator("Alt");
        if (ShiftKey) AppendWithSeparator("Shift");
        if (WindowsKey) AppendWithSeparator("Win");

        // Function or Character
        if (FunctionKey != null)
            AppendWithSeparator(FunctionKey);
        else if (Character.HasValue)
            AppendWithSeparator(Character.Value.ToString());

        // Special Keys
        if (EnterKey) AppendWithSeparator("Enter");
        if (TabKey) AppendWithSeparator("Tab");
        if (BackspaceKey) AppendWithSeparator("Backspace");
        if (DeleteKey) AppendWithSeparator("Delete");
        if (EscapeKey) AppendWithSeparator("Esc");
        if (SpaceKey) AppendWithSeparator("Space");
        if (InsertKey) AppendWithSeparator("Insert");

        // Navigation
        if (UpKey) AppendWithSeparator("Up");
        if (DownKey) AppendWithSeparator("Down");
        if (LeftKey) AppendWithSeparator("Left");
        if (RightKey) AppendWithSeparator("Right");
        if (HomeKey) AppendWithSeparator("Home");
        if (EndKey) AppendWithSeparator("End");
        if (PageUpKey) AppendWithSeparator("PageUp");
        if (PageDownKey) AppendWithSeparator("PageDown");

        // System
        if (CapsLockKey) AppendWithSeparator("CapsLock");
        if (NumLockKey) AppendWithSeparator("NumLock");
        if (ScrollLockKey) AppendWithSeparator("ScrollLock");
        if (PrintScreenKey) AppendWithSeparator("PrintScreen");
        if (PauseBreakKey) AppendWithSeparator("PauseBreak");
        if (MenuKey) AppendWithSeparator("Menu");

        if (list.Length > 0)
            return list.ToString();

        // Platform-specific handling
#if WINDOWS
        if (PlatformEvent is Microsoft.UI.Xaml.Input.KeyRoutedEventArgs keyArgs)
        {
            return $"OEM{keyArgs.Key}";
        }
#elif ANDROID
        if (PlatformEvent is Android.Views.KeyEvent keyEvent)
        {
            var unicodeChar = (char)keyEvent.UnicodeChar;
            return !char.IsControl(unicodeChar) 
                ? unicodeChar.ToString() 
                : keyEvent.KeyCode.ToString();
        }
#endif
        return PlatformEvent?.ToString() ?? string.Empty;
    }
}
