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

    // Canonical aliases for the main (non-modifier) key, mirroring the names that
    // KeyEventArgs.ToString() emits. Only aliases whose characters differ from the
    // canonical form need mapping (case is handled by the OrdinalIgnoreCase dictionary).
    // Keep this in sync with KeyEventArgs.ToString() and NamedKeySetters.
    private static readonly Dictionary<string, string> KeyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Escape"] = "Esc",
        ["Return"] = "Enter",
        ["Del"] = "Delete",
        ["Ins"] = "Insert",
        ["PgUp"] = "PageUp",
        ["PgDn"] = "PageDown",
        ["PrtSc"] = "PrintScreen",
        ["Pause"] = "PauseBreak"
    };

    private readonly Dictionary<string, Action> _hotkeyActions;
    private readonly object _hotkeysLock = new();

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
        if (parts.Length == 0) return string.Empty;
        // A lone key still needs canonicalization (e.g. "Escape" -> "Esc") so the
        // string API resolves to the same slot KeyEventArgs.ToString() produces.
        if (parts.Length == 1) return CanonicalizeKey(parts[0]);

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
                mainKey = CanonicalizeKey(normalizedPart);
            }
        }

        // Build the normalized hotkey string
        var orderedModifiers = ModifierOrder.Where(m => modifiers.Contains(m));

        return string.IsNullOrEmpty(mainKey)
            ? string.Join(SEPARATOR, orderedModifiers)
            : $"{string.Join(SEPARATOR, orderedModifiers)}{SEPARATOR}{mainKey}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CanonicalizeKey(string key) => KeyAliases.GetValueOrDefault(key, key);

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

    public void RegisterHotkey(KeyEventArgs hotKey, Action action)
    {
        ArgumentNullException.ThrowIfNull(hotKey);
        ArgumentNullException.ThrowIfNull(action);
        lock (_hotkeysLock)
        {
            _hotkeyActions[hotKey.ToString()] = action;
        }
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
    public void RegisterHotkey(string hotKey, Action action)
    {
        ArgumentException.ThrowIfNullOrEmpty(hotKey);
        ArgumentNullException.ThrowIfNull(action);

        lock (_hotkeysLock)
        {
            _hotkeyActions[NormalizeHotkey(hotKey)] = action;
        }
    }

    /// <summary>
    /// Removes a hotkey previously registered via the string overload.
    /// The lookup uses the same normalization, so modifier order and aliases are
    /// honored ("Control+Shift+X" matches a registration of "Shift+Ctrl+X").
    /// </summary>
    /// <returns><c>true</c> if a hotkey was removed; <c>false</c> if no match was registered.</returns>
    public bool UnregisterHotkey(string hotKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(hotKey);
        lock (_hotkeysLock)
        {
            return _hotkeyActions.Remove(NormalizeHotkey(hotKey));
        }
    }

    /// <summary>
    /// Removes a hotkey previously registered via the <see cref="KeyEventArgs"/> overload.
    /// </summary>
    /// <returns><c>true</c> if a hotkey was removed; <c>false</c> if no match was registered.</returns>
    public bool UnregisterHotkey(KeyEventArgs hotKey)
    {
        ArgumentNullException.ThrowIfNull(hotKey);
        lock (_hotkeysLock)
        {
            return _hotkeyActions.Remove(hotKey.ToString());
        }
    }

    /// <summary>
    /// Removes every registered hotkey.
    /// </summary>
    public void ClearHotkeys()
    {
        lock (_hotkeysLock)
        {
            _hotkeyActions.Clear();
        }
    }

    public void HandleKey(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);

        Action? action;
        lock (_hotkeysLock)
        {
            _hotkeyActions.TryGetValue(key.ToString(), out action);
        }

        if (action is not null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    // Hotkey actions are user code; isolate exceptions so a misbehaving
                    // handler does not crash the UI thread. Service ILogger is not
                    // reachable from here, so trace and continue.
                    System.Diagnostics.Debug.WriteLine($"[GlobalKeyboardCapture] Hotkey action threw: {ex}");
                }
            });
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
