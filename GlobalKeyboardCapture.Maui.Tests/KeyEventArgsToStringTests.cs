using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Tests;

public class KeyEventArgsToStringTests
{
    [Fact]
    public void ModifiersEmittedInCanonicalOrder()
    {
        var key = new KeyEventArgs
        {
            WindowsKey = true,
            ShiftKey = true,
            AltKey = true,
            ControlKey = true,
            Character = 'X'
        };

        key.ToString().Should().Be("Ctrl+Alt+Shift+Win+X");
    }

    [Fact]
    public void OnlyCharacterReturnsCharacter()
    {
        new KeyEventArgs { Character = 'a' }.ToString().Should().Be("a");
    }

    [Fact]
    public void FunctionKeyTakesPrecedenceOverCharacter()
    {
        var key = new KeyEventArgs { FunctionKey = "F5", Character = 'x' };
        key.ToString().Should().Be("F5");
    }

    [Fact]
    public void SpecialKeyEnter()
    {
        new KeyEventArgs { EnterKey = true }.ToString().Should().Be("Enter");
    }

    [Fact]
    public void CtrlPlusEnter()
    {
        new KeyEventArgs { ControlKey = true, EnterKey = true }.ToString().Should().Be("Ctrl+Enter");
    }

    [Fact]
    public void NavigationKeysEmitCorrectly()
    {
        new KeyEventArgs { UpKey = true }.ToString().Should().Be("Up");
        new KeyEventArgs { PageDownKey = true }.ToString().Should().Be("PageDown");
    }

    [Fact]
    public void EscapeKeyEmitsAsEsc()
    {
        new KeyEventArgs { EscapeKey = true }.ToString().Should().Be("Esc");
    }

    [Fact]
    public void EmptyKeyReturnsEmptyStringWhenNoPlatformEvent()
    {
        new KeyEventArgs().ToString().Should().Be(string.Empty);
    }

    [Fact]
    public void OnlyControlPropertyWorks()
    {
        new KeyEventArgs { ControlKey = true }.OnlyControl.Should().BeTrue();
        new KeyEventArgs { ControlKey = true, ShiftKey = true }.OnlyControl.Should().BeFalse();
    }

    [Fact]
    public void NoSpecialKeysPressedWhenNoModifiers()
    {
        new KeyEventArgs { Character = 'a' }.NoSpecialKeysPressed.Should().BeTrue();
        new KeyEventArgs { ControlKey = true }.NoSpecialKeysPressed.Should().BeFalse();
    }

    // Golden-string guards: ToString() is the hotkey lookup contract, so any change to
    // these exact tokens is a breaking change for registered hotkeys (per CLAUDE.md).
    [Fact]
    public void SoloModifierTokens()
    {
        new KeyEventArgs { ControlKey = true }.ToString().Should().Be("Ctrl");
        new KeyEventArgs { AltKey = true }.ToString().Should().Be("Alt");
        new KeyEventArgs { ShiftKey = true }.ToString().Should().Be("Shift");
        new KeyEventArgs { WindowsKey = true }.ToString().Should().Be("Win");
    }

    [Theory]
    [InlineData("Tab")]
    [InlineData("Backspace")]
    [InlineData("Delete")]
    [InlineData("Space")]
    [InlineData("Insert")]
    [InlineData("Up")]
    [InlineData("Down")]
    [InlineData("Left")]
    [InlineData("Right")]
    [InlineData("Home")]
    [InlineData("End")]
    [InlineData("PageUp")]
    [InlineData("PageDown")]
    [InlineData("CapsLock")]
    [InlineData("NumLock")]
    [InlineData("ScrollLock")]
    [InlineData("PrintScreen")]
    [InlineData("PauseBreak")]
    [InlineData("Menu")]
    public void SpecialKeyTokens(string token)
    {
        var key = new KeyEventArgs();
        switch (token)
        {
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
            case "CapsLock": key.CapsLockKey = true; break;
            case "NumLock": key.NumLockKey = true; break;
            case "ScrollLock": key.ScrollLockKey = true; break;
            case "PrintScreen": key.PrintScreenKey = true; break;
            case "PauseBreak": key.PauseBreakKey = true; break;
            case "Menu": key.MenuKey = true; break;
        }
        key.ToString().Should().Be(token);
    }
}
