using GlobalKeyboardCapture.Maui.Core.Mapping;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class PlatformKeyMapperTests
{
    [Theory]
    [InlineData('a', 'A')]
    [InlineData('9', '9')]
    [InlineData('+', '+')]
    [InlineData(' ', null)]
    [InlineData('\0', null)]
    public void MapsAndroidDisplayCharacters(char input, char? expected)
    {
        PlatformKeyMapper.MapAndroidDisplayLabel(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("NUM0", '0')]
    [InlineData("NUMPAD9", '9')]
    [InlineData("NUMPAD_ADD", '+')]
    [InlineData("LEFT_BRACKET", '[')]
    [InlineData("QUESTION", '?')]
    [InlineData("unknown", null)]
    public void MapsAndroidNamedDisplayLabels(string input, char? expected)
    {
        PlatformKeyMapper.MapAndroidDisplayLabel(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(65, 'A')]
    [InlineData(90, 'Z')]
    [InlineData(48, '0')]
    [InlineData(105, '9')]
    [InlineData(186, ';')]
    [InlineData(222, '\'')]
    [InlineData(1, null)]
    public void MapsWindowsVirtualKeys(int input, char? expected)
    {
        PlatformKeyMapper.MapWindowsVirtualKey(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(112, "F1")]
    [InlineData(123, "F12")]
    [InlineData(135, "F24")]
    [InlineData(136, null)]
    public void MapsWindowsFunctionKeys(int input, string? expected)
    {
        PlatformKeyMapper.MapWindowsFunctionKey(input).Should().Be(expected);
    }
}
