namespace GlobalKeyboardCapture.Maui.Core.Models;

public class KeyEventArgs
{
    public object PlatformEvent { get; set; }
    public bool Handled { get; set; } = false;

    // Caracteres e Função
    public char? Character { get; set; }
    public string? FunctionKey { get; set; }

    // Teclas Modificadoras
    public bool ControlKey { get; set; }
    public bool AltKey { get; set; }
    public bool ShiftKey { get; set; }
    public bool WindowsKey { get; set; }

    // Teclas de Navegação
    public bool UpKey { get; set; }
    public bool DownKey { get; set; }
    public bool LeftKey { get; set; }
    public bool RightKey { get; set; }
    public bool HomeKey { get; set; }
    public bool EndKey { get; set; }
    public bool PageUpKey { get; set; }
    public bool PageDownKey { get; set; }

    // Teclas de Edição
    public bool EnterKey { get; set; }
    public bool TabKey { get; set; }
    public bool BackspaceKey { get; set; }
    public bool DeleteKey { get; set; }
    public bool EscapeKey { get; set; }
    public bool SpaceKey { get; set; }
    public bool InsertKey { get; set; }

    // Teclas de Sistema
    public bool CapsLockKey { get; set; }
    public bool NumLockKey { get; set; }
    public bool ScrollLockKey { get; set; }
    public bool PrintScreenKey { get; set; }
    public bool PauseBreakKey { get; set; }
    public bool MenuKey { get; set; }

    // Helpers
    public bool OnlyWindows => WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    public bool OnlyAlt => !WindowsKey && AltKey && !ControlKey && !ShiftKey;
    public bool OnlyControl => !WindowsKey && !AltKey && ControlKey && !ShiftKey;
    public bool OnlyShift => !WindowsKey && !AltKey && !ControlKey && ShiftKey;
    public bool NoSpecialKeysPressed => !WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    public bool AnySpecialKeyPressed => WindowsKey || AltKey || ControlKey || ShiftKey;

    // ToString implementation
    public override string ToString()
    {
        var list = new List<string>();

        // Modificadores
        if (ControlKey) list.Add("Ctrl");
        if (AltKey) list.Add("Alt");
        if (ShiftKey) list.Add("Shift");
        if (WindowsKey) list.Add("Win");

        // Função ou Caractere
        if (FunctionKey != null)
            list.Add(FunctionKey);
        else
        {
            var charString = Character?.ToString();
            if (!string.IsNullOrWhiteSpace(charString))
                list.Add(charString);
        }

        // Teclas Especiais
        if (EnterKey) list.Add("Enter");
        if (TabKey) list.Add("Tab");
        if (BackspaceKey) list.Add("Backspace");
        if (DeleteKey) list.Add("Delete");
        if (EscapeKey) list.Add("Esc");
        if (SpaceKey) list.Add("Space");
        if (InsertKey) list.Add("Insert");

        // Navegação
        if (UpKey) list.Add("Up");
        if (DownKey) list.Add("Down");
        if (LeftKey) list.Add("Left");
        if (RightKey) list.Add("Right");
        if (HomeKey) list.Add("Home");
        if (EndKey) list.Add("End");
        if (PageUpKey) list.Add("PageUp");
        if (PageDownKey) list.Add("PageDown");

        // Sistema
        if (CapsLockKey) list.Add("CapsLock");
        if (NumLockKey) list.Add("NumLock");
        if (ScrollLockKey) list.Add("ScrollLock");
        if (PrintScreenKey) list.Add("PrintScreen");
        if (PauseBreakKey) list.Add("PauseBreak");
        if (MenuKey) list.Add("Menu");

        if (list.Count > 0)
            return string.Join('+', list);

#if WINDOWS
        if (PlatformEvent is Microsoft.UI.Xaml.Input.KeyRoutedEventArgs keyArgs)
        {
            var key = keyArgs.Key.ToString();
            return $"OEM{key}";
        }
#elif ANDROID
        if (PlatformEvent is Android.Views.KeyEvent keyEvent)
        {
            var unicodeChar = (char)keyEvent.UnicodeChar;
            
            if (!char.IsControl(unicodeChar))
                return unicodeChar.ToString();
            
            return $"{keyEvent.KeyCode}";
        }
#endif
        return PlatformEvent?.ToString() ?? string.Empty;
    }
}
