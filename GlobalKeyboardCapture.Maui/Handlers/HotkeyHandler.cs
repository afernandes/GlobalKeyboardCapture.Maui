using System.Runtime.CompilerServices;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Handlers;

public sealed class HotkeyHandler : IKeyHandler
{
    private const int INITIAL_HOTKEY_CAPACITY = 16;
    private const char SEPARATOR = '+';

    private static readonly HashSet<string> EscapeKeyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        "ESC",
        "ESCAPE"
    };

    private static readonly Dictionary<string, Action<KeyEventArgs>> NamedKeySetters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Enter"] = k => k.EnterKey = true,
        ["Return"] = k => k.EnterKey = true,
        ["Tab"] = k => k.TabKey = true,
        ["Backspace"] = k => k.BackspaceKey = true,
        ["Delete"] = k => k.DeleteKey = true,
        ["Del"] = k => k.DeleteKey = true,
        ["Space"] = k => k.SpaceKey = true,
        ["Insert"] = k => k.InsertKey = true,
        ["Ins"] = k => k.InsertKey = true,
        ["Up"] = k => k.UpKey = true,
        ["Down"] = k => k.DownKey = true,
        ["Left"] = k => k.LeftKey = true,
        ["Right"] = k => k.RightKey = true,
        ["Home"] = k => k.HomeKey = true,
        ["End"] = k => k.EndKey = true,
        ["PageUp"] = k => k.PageUpKey = true,
        ["PgUp"] = k => k.PageUpKey = true,
        ["PageDown"] = k => k.PageDownKey = true,
        ["PgDn"] = k => k.PageDownKey = true,
        ["CapsLock"] = k => k.CapsLockKey = true,
        ["NumLock"] = k => k.NumLockKey = true,
        ["ScrollLock"] = k => k.ScrollLockKey = true,
        ["PrintScreen"] = k => k.PrintScreenKey = true,
        ["PrtSc"] = k => k.PrintScreenKey = true,
        ["PauseBreak"] = k => k.PauseBreakKey = true,
        ["Pause"] = k => k.PauseBreakKey = true,
        ["Menu"] = k => k.MenuKey = true,
    };

    // Ordered array of modifiers for consistent normalization
    private static readonly string[] ModifierOrder = ["Ctrl", "Alt", "Shift", "Win"];

    // Common aliases for modifiers
    private static readonly Dictionary<string, string> ModifierAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Control"] = "Ctrl",
        ["Windows"] = "Win"
    };

    private readonly Dictionary<string, Action> _hotkeyActions;

    public HotkeyHandler()
    {
        _hotkeyActions = new Dictionary<string, Action>(
            INITIAL_HOTKEY_CAPACITY,
            StringComparer.OrdinalIgnoreCase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string NormalizeHotkey(string hotkey)
    {
        if (string.IsNullOrWhiteSpace(hotkey)) return string.Empty;

        // Split and trim the parts
        var parts = hotkey.Split(SEPARATOR, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length <= 1) return hotkey;

        // Identify modifiers and main key
        var modifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var mainKey = string.Empty;

        foreach (var part in parts)
        {
            var normalizedPart = ModifierAliases.GetValueOrDefault(part, part);

            if (ModifierOrder.Contains(normalizedPart, StringComparer.OrdinalIgnoreCase))
            {
                modifiers.Add(normalizedPart);
            }
            else
            {
                mainKey = normalizedPart;
            }
        }

        // Build the normalized hotkey string
        var orderedModifiers = ModifierOrder.Where(m => modifiers.Contains(m));

        return string.IsNullOrEmpty(mainKey)
            ? string.Join(SEPARATOR, orderedModifiers)
            : $"{string.Join(SEPARATOR, orderedModifiers)}{SEPARATOR}{mainKey}";
    }

    public void RegisterHotkey(string key, bool requireControl, bool requireAlt, bool requireShift, Action action)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(action);

        var hotKey = new KeyEventArgs
        {
            ControlKey = requireControl,
            AltKey = requireAlt,
            ShiftKey = requireShift
        };

        if (!TryProcessSpecialKey(key, hotKey))
        {
            hotKey.Character = key[0];
        }

        RegisterHotkey(hotKey, action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RegisterHotkey(KeyEventArgs hotKey, Action action)
    {
        ArgumentNullException.ThrowIfNull(hotKey);
        ArgumentNullException.ThrowIfNull(action);
        _hotkeyActions[hotKey.ToString()] = action;
    }

    /// <summary>
    /// Registers a global hotkey with its associated action. The hotkey string is automatically normalized 
    /// to ensure consistent behavior regardless of the order of modifiers.
    /// </summary>
    /// <param name="hotKey">
    /// A string representing the hotkey combination (e.g., "Ctrl+Shift+X", "Alt+Win+Z").
    /// Modifiers can be specified in any order and are automatically normalized.
    /// Supported modifiers: Ctrl (or Control), Alt, Shift, Win (or Windows).
    /// Special keys like function keys (F1-F12), Escape (or Esc) and OEM are also supported.
    /// </param>
    /// <param name="action">
    /// The action to be executed when the hotkey is triggered. This action will be invoked
    /// on the main thread.
    /// </param>
    /// <remarks>
    /// The hotkey string is normalized following these rules:
    /// <list type="bullet">
    /// <item><description>Modifiers are ordered as: Ctrl → Alt → Shift → Win</description></item>
    /// <item><description>Common aliases are supported (e.g., "Control" → "Ctrl", "Windows" → "Win")</description></item>
    /// <item><description>The comparison is case-insensitive</description></item>
    /// <item><description>Extra spaces and empty parts are removed</description></item>
    /// </list>
    /// Examples of equivalent hotkey registrations:
    /// <code>
    /// RegisterHotkey("Shift+Alt+X", action);   // Normalized to "Alt+Shift+X"
    /// RegisterHotkey("Alt+Shift+X", action);   // Already normalized
    /// RegisterHotkey("X+Shift+Alt", action);   // Normalized to "Alt+Shift+X"
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="hotKey"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RegisterHotkey(string hotKey, Action action)
    {
        ArgumentException.ThrowIfNullOrEmpty(hotKey);
        ArgumentNullException.ThrowIfNull(action);

        _hotkeyActions[NormalizeHotkey(hotKey)] = action;
    }

    public void HandleKey(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_hotkeyActions.TryGetValue(key.ToString(), out var action))
        {
            MainThread.BeginInvokeOnMainThread(action);
            key.Handled = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldHandle(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return true;
    }

    private static bool TryProcessSpecialKey(string key, KeyEventArgs hotKey)
    {
        return IsFunctionKey(key, hotKey) || IsEscapeKey(key, hotKey) || IsNamedKey(key, hotKey);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNamedKey(string key, KeyEventArgs hotKey)
    {
        if (!NamedKeySetters.TryGetValue(key, out var setter))
            return false;

        setter(hotKey);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFunctionKey(ReadOnlySpan<char> key, KeyEventArgs hotKey)
    {
        if (key.Length >= 2 && (key[0] == 'F' || key[0] == 'f') && char.IsNumber(key[1]))
        {
            hotKey.FunctionKey = key.ToString();
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEscapeKey(string key, KeyEventArgs hotKey)
    {
        if (!EscapeKeyAliases.Contains(key))
            return false;

        hotKey.EscapeKey = true;
        return true;
    }
}
