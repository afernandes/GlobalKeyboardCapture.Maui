using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Core.Services;

/// <summary>
/// Stores bounded, thread-safe translations captured from native layout-aware events such as
/// Apple <c>UIKey.Characters</c>, and serves them to the physical keyboard pipeline.
/// </summary>
public sealed class KeyboardLayoutTranslationCache : IKeyboardLayoutTranslator
{
    private const int MAX_ENTRIES = 256;

    private readonly object _lockObject = new();
    private readonly Dictionary<KeyboardLayoutTranslationContext, char> _translations = [];

    /// <summary>Gets the number of cached physical-key and modifier combinations.</summary>
    public int Count
    {
        get
        {
            lock (_lockObject)
                return _translations.Count;
        }
    }

    /// <summary>
    /// Records one translated printable UTF-16 character. Invalid, control, surrogate, or
    /// multi-character text removes the existing entry for the same context.
    /// </summary>
    /// <param name="context">The physical-key and modifier context.</param>
    /// <param name="text">Layout-aware text supplied by the operating system.</param>
    /// <returns><see langword="true"/> when a printable character was stored.</returns>
    public bool SetTranslation(
        in KeyboardLayoutTranslationContext context,
        string? text)
    {
        if (text is not { Length: 1 }
            || char.IsControl(text[0])
            || char.IsSurrogate(text[0]))
        {
            lock (_lockObject)
                _translations.Remove(context);
            return false;
        }

        lock (_lockObject)
        {
            if (_translations.Count >= MAX_ENTRIES && !_translations.ContainsKey(context))
                _translations.Clear();
            _translations[context] = text[0];
        }
        return true;
    }

    /// <inheritdoc/>
    public bool TryTranslate(
        in KeyboardLayoutTranslationContext context,
        out char character)
    {
        lock (_lockObject)
            return _translations.TryGetValue(context, out character);
    }

    /// <summary>Removes every cached translation, for example after a layout change.</summary>
    public void Clear()
    {
        lock (_lockObject)
            _translations.Clear();
    }
}
