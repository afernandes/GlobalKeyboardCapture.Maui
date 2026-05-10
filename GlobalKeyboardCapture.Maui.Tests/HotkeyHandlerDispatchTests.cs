using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Tests;

public class HotkeyHandlerDispatchTests
{
    [Fact]
    public void ShouldHandleAlwaysReturnsTrue()
    {
        var handler = new HotkeyHandler();
        handler.ShouldHandle(new KeyEventArgs()).Should().BeTrue();
    }

    [Fact]
    public void HandleKeyMarksKeyAsHandledWhenMatching()
    {
        var handler = new HotkeyHandler();
        handler.RegisterHotkey("Ctrl+S", () => { });

        var key = new KeyEventArgs { ControlKey = true, Character = 'S' };
        handler.HandleKey(key);

        key.Handled.Should().BeTrue();
    }

    [Fact]
    public void HandleKeyDoesNotMarkHandledWhenNoMatch()
    {
        var handler = new HotkeyHandler();
        handler.RegisterHotkey("Ctrl+S", () => { });

        var key = new KeyEventArgs { ControlKey = true, Character = 'X' };
        handler.HandleKey(key);

        key.Handled.Should().BeFalse();
    }

    [Fact]
    public void RegisterHotkeyOverloadWithKeyEventArgsWorks()
    {
        var handler = new HotkeyHandler();
        var fired = false;
        var registration = new KeyEventArgs { ControlKey = true, AltKey = true, Character = 'P' };
        handler.RegisterHotkey(registration, () => fired = true);

        handler.HandleKey(new KeyEventArgs { ControlKey = true, AltKey = true, Character = 'P' });

        fired.Should().BeTrue();
    }

    [Theory]
    [InlineData("F1")]
    [InlineData("F12")]
    public void RegisterHotkeyAcceptsFunctionKeys(string functionKey)
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey(functionKey, requireControl: false, requireAlt: false, requireShift: false, () => fired = true);

        handler.HandleKey(new KeyEventArgs { FunctionKey = functionKey });

        fired.Should().BeTrue();
    }

    [Theory]
    [InlineData("Esc")]
    [InlineData("ESCAPE")]
    [InlineData("escape")]
    public void RegisterHotkeyAcceptsEscapeAliases(string alias)
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey(alias, requireControl: false, requireAlt: false, requireShift: false, () => fired = true);

        handler.HandleKey(new KeyEventArgs { EscapeKey = true });

        fired.Should().BeTrue();
    }

    [Fact]
    public void HandleKeyThrowsOnNullKey()
    {
        var handler = new HotkeyHandler();
        Action act = () => handler.HandleKey(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
