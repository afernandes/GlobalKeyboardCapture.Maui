using GlobalKeyboardCapture.Maui.Core.Mapping;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class AppleKeyMapperTests
{
    [Theory]
    [InlineData(0x04, false, false, 'a')]
    [InlineData(0x04, true, false, 'A')]
    [InlineData(0x04, false, true, 'A')]
    [InlineData(0x04, true, true, 'a')]
    [InlineData(0x1E, false, false, '1')]
    [InlineData(0x1E, true, false, '!')]
    [InlineData(0x2D, false, false, '-')]
    [InlineData(0x2D, true, false, '_')]
    [InlineData(0x57, false, false, '+')]
    public void MapsHidCharacters(int keyCode, bool shift, bool capsLock, char expected)
    {
        var mapping = AppleKeyMapper.Map(keyCode, shift, capsLock);

        mapping.Key.Should().Be(KeyboardKey.Character);
        mapping.Character.Should().Be(expected);
    }

    [Theory]
    [InlineData(0x28, KeyboardKey.Enter, KeyLocation.Standard)]
    [InlineData(0x58, KeyboardKey.Enter, KeyLocation.Numpad)]
    [InlineData(0x2B, KeyboardKey.Tab, KeyLocation.Standard)]
    [InlineData(0x3A, KeyboardKey.F1, KeyLocation.Standard)]
    [InlineData(0x6F, KeyboardKey.F20, KeyLocation.Standard)]
    [InlineData(0x52, KeyboardKey.Up, KeyLocation.Standard)]
    public void MapsHidNamedKeys(int keyCode, KeyboardKey expected, KeyLocation location)
    {
        var mapping = AppleKeyMapper.Map(keyCode, shift: false, capsLock: false);

        mapping.Key.Should().Be(expected);
        mapping.Location.Should().Be(location);
        mapping.Character.Should().BeNull();
    }

    [Theory]
    [InlineData(0x2F, '´')]
    [InlineData(0x31, '#')]
    [InlineData(0x35, '§')]
    public void LayoutTranslationOverridesPrintableHidFallback(int keyCode, char translated)
    {
        var mapping = AppleKeyMapper.Map(
            keyCode,
            shift: false,
            capsLock: false,
            translated);

        mapping.Key.Should().Be(KeyboardKey.Character);
        mapping.Character.Should().Be(translated);
    }

    [Fact]
    public void LayoutTranslationCannotReplaceNamedKey()
    {
        var mapping = AppleKeyMapper.Map(0x3A, shift: false, capsLock: false, translatedCharacter: 'x');

        mapping.Key.Should().Be(KeyboardKey.F1);
        mapping.Character.Should().BeNull();
    }

    [Fact]
    public void InvalidLayoutTranslationFallsBackToHidMap()
    {
        var mapping = AppleKeyMapper.Map(0x2F, shift: false, capsLock: false, translatedCharacter: '\n');

        mapping.Character.Should().Be('[');
    }

    [Fact]
    public void TranslationContextIsAllocationFreeValueData()
    {
        var context = new KeyboardLayoutTranslationContext(
            KeyboardPlatform.iOS,
            nativeKeyCode: 0x2F,
            KeyModifiers.Shift,
            capsLock: false);

        context.Platform.Should().Be(KeyboardPlatform.iOS);
        context.NativeKeyCode.Should().Be(0x2F);
        context.Modifiers.Should().Be(KeyModifiers.Shift);
    }
}
