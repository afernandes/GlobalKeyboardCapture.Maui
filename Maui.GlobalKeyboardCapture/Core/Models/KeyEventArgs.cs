namespace Maui.GlobalKeyboardCapture.Core.Models;

public class KeyEventArgs
{
    public object PlatformEvent { get; set; }
    public bool Handled { get; set; } = false;
    public bool EnterKey { get; set; }
    public bool AltKey { get; set; }
    public bool ControlKey { get; set; }
    public bool ShiftKey { get; set; }
    public char? Character { get; set; }
    public string FunctionKey { get; set; }
    public bool WindowsKey { get; internal set; }
    public bool DownKey { get; set; }
    public bool UpKey { get; set; }
    public bool LeftKey { get; set; }
    public bool RightKey { get; set; }

    public bool OnlyWindows => this.WindowsKey && !this.AltKey && !this.ControlKey && !this.ShiftKey;
    public bool OnlyAlt => !this.WindowsKey && this.AltKey && !this.ControlKey && !this.ShiftKey;
    public bool OnlyControl => !this.WindowsKey && !this.AltKey && this.ControlKey && !this.ShiftKey;
    public bool OnlyShift => !this.WindowsKey && !this.AltKey && !this.ControlKey && this.ShiftKey;
    public bool NoSpecialKeysPressed => !this.WindowsKey && !this.AltKey && !this.ControlKey && !this.ShiftKey;
    public bool AnySpecialKeyPressed => this.WindowsKey || this.AltKey || this.ControlKey || this.ShiftKey;

    public override string ToString()
    {
        var list = new List<string>();

        if (ControlKey)
            list.Add("Ctrl");
        if (AltKey)
            list.Add("Alt");
        if (ShiftKey)
            list.Add("Shift");
        if (WindowsKey)
            list.Add("Win");
        if (FunctionKey != null)
            list.Add(FunctionKey);
        if (Character != null)
            list.Add(Character.ToString());
        if (EnterKey)
            list.Add("Enter");
        if (DownKey)
            list.Add("Down");
        if (UpKey)
            list.Add("Up");
        if (LeftKey)
            list.Add("Left");
        if (RightKey)
            list.Add("Right");

        return (list.Any() ? string.Join('+', list) : PlatformEvent?.ToString()) ?? string.Empty;
    }
}
