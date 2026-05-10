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
}
