using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeyboardLayoutTranslationCacheTests
{
    [Fact]
    public void RecordsAndResolvesExactLayoutContext()
    {
        var cache = new KeyboardLayoutTranslationCache();
        var context = Context(KeyModifiers.Shift);

        cache.SetTranslation(in context, "Ç").Should().BeTrue();

        cache.TryTranslate(in context, out var character).Should().BeTrue();
        character.Should().Be('Ç');
        cache.TryTranslate(Context(KeyModifiers.None), out _).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("\n")]
    public void InvalidTextRemovesExistingTranslation(string? text)
    {
        var cache = new KeyboardLayoutTranslationCache();
        var context = Context(KeyModifiers.None);
        cache.SetTranslation(in context, "ç");

        cache.SetTranslation(in context, text).Should().BeFalse();

        cache.TryTranslate(in context, out _).Should().BeFalse();
    }

    [Fact]
    public void ClearRemovesAllTranslations()
    {
        var cache = new KeyboardLayoutTranslationCache();
        var context = Context(KeyModifiers.None);
        cache.SetTranslation(in context, "ç");

        cache.Clear();

        cache.Count.Should().Be(0);
        cache.TryTranslate(in context, out _).Should().BeFalse();
    }

    private static KeyboardLayoutTranslationContext Context(KeyModifiers modifiers) => new(
        KeyboardPlatform.iOS,
        nativeKeyCode: 0x33,
        modifiers,
        capsLock: false);
}
