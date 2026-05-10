using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Tests;

/// <summary>
/// Tests for the three P0 fixes:
///   B1: WindowsKeyHandler.Cleanup must unsubscribe from PreviewKeyDown (not KeyDown).
///   B2: RegisterHotkey(string, bool, bool, bool, Action) must recognize named keys
///       like Enter/Tab/Backspace, not only function keys and Escape.
///   B3: KeyHandlerService must rebind when the platform view changes (e.g. Android
///       Activity recreation on configuration change).
/// </summary>
public class P0FixesTests
{
    [Fact]
    public void B2_RegisterHotkey_AcceptsEnterAsNamedKey()
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey("Enter", requireControl: false, requireAlt: false, requireShift: true, () => fired = true);

        handler.HandleKey(new KeyEventArgs { ShiftKey = true, EnterKey = true });

        fired.Should().BeTrue();
    }

    [Theory]
    [InlineData("Tab", false, false, false)]
    [InlineData("Backspace", true, false, false)]
    [InlineData("Delete", false, true, false)]
    [InlineData("Space", false, false, true)]
    [InlineData("Up", false, false, false)]
    [InlineData("PageDown", true, false, false)]
    [InlineData("Home", false, false, false)]
    public void B2_RegisterHotkey_AcceptsAllNamedKeys(string keyName, bool ctrl, bool alt, bool shift)
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey(keyName, ctrl, alt, shift, () => fired = true);

        var key = new KeyEventArgs { ControlKey = ctrl, AltKey = alt, ShiftKey = shift };
        SetNamedKey(key, keyName);
        handler.HandleKey(key);

        fired.Should().BeTrue();
    }

    [Theory]
    [InlineData("Return", "Enter")]
    [InlineData("Del", "Delete")]
    [InlineData("PgUp", "PageUp")]
    [InlineData("PgDn", "PageDown")]
    [InlineData("Pause", "PauseBreak")]
    public void B2_RegisterHotkey_AcceptsAliases(string alias, string canonical)
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey(alias, requireControl: false, requireAlt: false, requireShift: false, () => fired = true);

        var key = new KeyEventArgs();
        SetNamedKey(key, canonical);
        handler.HandleKey(key);

        fired.Should().BeTrue();
    }

    private static void SetNamedKey(KeyEventArgs key, string name)
    {
        switch (name)
        {
            case "Enter": key.EnterKey = true; break;
            case "Tab": key.TabKey = true; break;
            case "Backspace": key.BackspaceKey = true; break;
            case "Delete": key.DeleteKey = true; break;
            case "Space": key.SpaceKey = true; break;
            case "Insert": key.InsertKey = true; break;
            case "Up": key.UpKey = true; break;
            case "Down": key.DownKey = true; break;
            case "Left": key.LeftKey = true; break;
            case "Right": key.RightKey = true; break;
            case "Home": key.HomeKey = true; break;
            case "End": key.EndKey = true; break;
            case "PageUp": key.PageUpKey = true; break;
            case "PageDown": key.PageDownKey = true; break;
            case "PauseBreak": key.PauseBreakKey = true; break;
            default: throw new ArgumentException($"Unknown named key: {name}");
        }
    }
}
