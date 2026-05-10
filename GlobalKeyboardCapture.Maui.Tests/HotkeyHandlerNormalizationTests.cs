using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Tests;

public class HotkeyHandlerNormalizationTests
{
    [Theory]
    [InlineData("Ctrl+Alt+Shift+X", "Ctrl+Alt+Shift+X")]
    [InlineData("X+Shift+Alt+Ctrl", "Ctrl+Alt+Shift+X")]
    [InlineData("Shift+Ctrl+X", "Ctrl+Shift+X")]
    [InlineData("Win+Alt+Z", "Alt+Win+Z")]
    public void RegistersInCanonicalModifierOrder(string input, string canonical)
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey(input, () => fired = true);

        handler.HandleKey(KeyForHotkey(canonical));

        fired.Should().BeTrue();
    }

    [Theory]
    [InlineData("Control+X", "Ctrl+X")]
    [InlineData("Windows+Z", "Win+Z")]
    [InlineData("CONTROL+windows+x", "Ctrl+Win+X")]
    public void AcceptsAliasesCaseInsensitive(string input, string canonical)
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey(input, () => fired = true);

        handler.HandleKey(KeyForHotkey(canonical));

        fired.Should().BeTrue();
    }

    [Fact]
    public void EmptyHotkeyStringThrows()
    {
        var handler = new HotkeyHandler();
        Action act = () => handler.RegisterHotkey("", () => { });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullActionThrows()
    {
        var handler = new HotkeyHandler();
        Action act = () => handler.RegisterHotkey("Ctrl+X", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ReRegisteringSameHotkeyOverridesAction()
    {
        var handler = new HotkeyHandler();
        var firstFired = 0;
        var secondFired = 0;

        handler.RegisterHotkey("Ctrl+S", () => firstFired++);
        handler.RegisterHotkey("Ctrl+S", () => secondFired++);

        handler.HandleKey(KeyForHotkey("Ctrl+S"));

        firstFired.Should().Be(0);
        secondFired.Should().Be(1);
    }

    private static Core.Models.KeyEventArgs KeyForHotkey(string hotkey)
    {
        var key = new Core.Models.KeyEventArgs();
        var parts = hotkey.Split('+');
        foreach (var part in parts)
        {
            switch (part)
            {
                case "Ctrl": key.ControlKey = true; break;
                case "Alt": key.AltKey = true; break;
                case "Shift": key.ShiftKey = true; break;
                case "Win": key.WindowsKey = true; break;
                case "Esc": key.EscapeKey = true; break;
                case "Enter": key.EnterKey = true; break;
                case "Tab": key.TabKey = true; break;
                default:
                    if (part.Length >= 2 && (part[0] == 'F' || part[0] == 'f') && char.IsDigit(part[1]))
                        key.FunctionKey = part;
                    else if (part.Length == 1)
                        key.Character = part[0];
                    break;
            }
        }
        return key;
    }
}
