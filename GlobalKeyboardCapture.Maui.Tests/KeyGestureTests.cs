using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeyGestureTests
{
    [Theory]
    [InlineData("Ctrl+Alt+Shift+Win+F12", "Ctrl+Alt+Shift+Win+F12")]
    [InlineData("windows+control+escape", "Ctrl+Win+Esc")]
    [InlineData("Shift+Return", "Shift+Enter")]
    [InlineData("Ctrl+PgDn", "Ctrl+PageDown")]
    [InlineData("+", "+")]
    [InlineData("Ctrl++", "Ctrl++")]
    [InlineData("Ctrl+Plus", "Ctrl++")]
    public void ParseProducesCanonicalGesture(string input, string canonical)
    {
        var gesture = KeyGesture.Parse(input);

        gesture.ToString().Should().Be(canonical);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ctrl")]
    [InlineData("Ctrl+A+B")]
    [InlineData("F0")]
    [InlineData("F25")]
    [InlineData("UnknownKey")]
    public void ParseRejectsInvalidGestures(string input)
    {
        var act = () => KeyGesture.Parse(input);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void CharacterGesturesAreCaseInsensitive()
    {
        KeyGesture.Parse("Ctrl+a").Should().Be(KeyGesture.Parse("CTRL+A"));
    }

    [Fact]
    public void FromEventUsesTypedKeyAndModifiers()
    {
        var key = new KeyEventArgs
        {
            ControlKey = true,
            ShiftKey = true,
            PageDownKey = true
        };

        KeyGesture.FromEvent(key).Should().Be(
            new KeyGesture(KeyboardKey.PageDown, KeyModifiers.Control | KeyModifiers.Shift));
    }

    [Fact]
    public void KeyEventExposesTypedProjectionWithoutBreakingLegacyProperties()
    {
        var key = new KeyEventArgs
        {
            Key = KeyboardKey.Enter,
            Modifiers = KeyModifiers.Control | KeyModifiers.Alt
        };

        key.EnterKey.Should().BeTrue();
        key.ControlKey.Should().BeTrue();
        key.AltKey.Should().BeTrue();
        key.ToString().Should().Be("Ctrl+Alt+Enter");
    }
}
