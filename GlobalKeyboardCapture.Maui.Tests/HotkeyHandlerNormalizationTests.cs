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

    [Theory]
    [InlineData("Escape", "Esc")]
    [InlineData("ESCAPE", "Esc")]
    [InlineData("Ctrl+Escape", "Ctrl+Esc")]
    [InlineData("Ctrl+Return", "Ctrl+Enter")]
    [InlineData("Shift+Del", "Shift+Delete")]
    [InlineData("Alt+Ins", "Alt+Insert")]
    [InlineData("Ctrl+PgUp", "Ctrl+PageUp")]
    [InlineData("Ctrl+PgDn", "Ctrl+PageDown")]
    [InlineData("PrtSc", "PrintScreen")]
    [InlineData("Pause", "PauseBreak")]
    public void CanonicalizesMainKeyAliasesToToStringForm(string input, string canonical)
    {
        // The string API must resolve key aliases to the same names KeyEventArgs.ToString()
        // emits, otherwise the dispatch lookup (which uses ToString()) never matches.
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey(input, () => fired = true);

        handler.HandleKey(KeyForHotkey(canonical));

        fired.Should().BeTrue();
    }

    [Fact]
    public void UnregisterHotkey_String_HonorsMainKeyAlias()
    {
        var handler = new HotkeyHandler();
        handler.RegisterHotkey(new Core.Models.KeyEventArgs { ControlKey = true, EscapeKey = true }, () => { });

        // "Ctrl+Escape" must normalize to the same slot as the registered "Ctrl+Esc".
        handler.UnregisterHotkey("Ctrl+Escape").Should().BeTrue();
        handler.UnregisterHotkey("Ctrl+Esc").Should().BeFalse();
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
                case "Backspace": key.BackspaceKey = true; break;
                case "Delete": key.DeleteKey = true; break;
                case "Insert": key.InsertKey = true; break;
                case "Space": key.SpaceKey = true; break;
                case "PageUp": key.PageUpKey = true; break;
                case "PageDown": key.PageDownKey = true; break;
                case "PrintScreen": key.PrintScreenKey = true; break;
                case "PauseBreak": key.PauseBreakKey = true; break;
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
