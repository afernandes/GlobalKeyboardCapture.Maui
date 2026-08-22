using System.Runtime.CompilerServices;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Handlers;

/// <summary>
/// Dispatches normalized key gestures to actions on the MAUI main thread.
/// </summary>
public sealed class HotkeyHandler : IKeyHandler
{
    private const int INITIAL_HOTKEY_CAPACITY = 16;

    private readonly Dictionary<KeyGesture, HotkeyRegistration> _hotkeys = new(INITIAL_HOTKEY_CAPACITY);
    private readonly object _hotkeysLock = new();
    private long _nextRegistrationId;

    /// <summary>
    /// Gets the number of currently registered hotkeys.
    /// </summary>
    public int HotkeyCount
    {
        get
        {
            lock (_hotkeysLock)
            {
                return _hotkeys.Count;
            }
        }
    }

    /// <summary>
    /// Gets a snapshot of currently registered gestures.
    /// </summary>
    public IReadOnlyCollection<KeyGesture> RegisteredHotkeys
    {
        get
        {
            lock (_hotkeysLock)
            {
                return _hotkeys.Keys.ToArray();
            }
        }
    }

    /// <summary>
    /// Registers a key and legacy modifier flags.
    /// </summary>
    public IDisposable RegisterHotkey(
        string key,
        bool requireControl,
        bool requireAlt,
        bool requireShift,
        Action action)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(action);

        var gesture = ParseGesture(key, nameof(key));
        var modifiers = gesture.Modifiers;
        if (requireControl) modifiers |= KeyModifiers.Control;
        if (requireAlt) modifiers |= KeyModifiers.Alt;
        if (requireShift) modifiers |= KeyModifiers.Shift;

        gesture = gesture.Key == KeyboardKey.Character
            ? new KeyGesture(gesture.Character!.Value, modifiers)
            : new KeyGesture(gesture.Key, modifiers);
        return RegisterHotkey(gesture, action);
    }

    /// <summary>
    /// Registers a gesture represented by an existing keyboard event.
    /// </summary>
    public IDisposable RegisterHotkey(KeyEventArgs hotKey, Action action)
    {
        ArgumentNullException.ThrowIfNull(hotKey);
        return RegisterHotkey(KeyGesture.FromEvent(hotKey), action);
    }

    /// <summary>
    /// Registers a textual gesture. Modifier order and supported aliases are normalized.
    /// </summary>
    public IDisposable RegisterHotkey(string hotKey, Action action)
    {
        ArgumentException.ThrowIfNullOrEmpty(hotKey);
        return RegisterHotkey(ParseGesture(hotKey, nameof(hotKey)), action);
    }

    /// <summary>
    /// Registers a typed gesture and returns an idempotent token that removes exactly
    /// that registration when disposed.
    /// </summary>
    public IDisposable RegisterHotkey(KeyGesture hotKey, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (hotKey.Key == KeyboardKey.None)
            throw new ArgumentException("A hotkey must contain a logical key.", nameof(hotKey));

        lock (_hotkeysLock)
        {
            var registration = new HotkeyRegistration(++_nextRegistrationId, action);
            _hotkeys[hotKey] = registration;
            return new HotkeyRegistrationToken(this, hotKey, registration.Id);
        }
    }

    /// <summary>
    /// Removes a textual hotkey registration.
    /// </summary>
    public bool UnregisterHotkey(string hotKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(hotKey);
        return UnregisterHotkey(ParseGesture(hotKey, nameof(hotKey)));
    }

    /// <summary>
    /// Removes a hotkey represented by a keyboard event.
    /// </summary>
    public bool UnregisterHotkey(KeyEventArgs hotKey)
    {
        ArgumentNullException.ThrowIfNull(hotKey);
        return UnregisterHotkey(KeyGesture.FromEvent(hotKey));
    }

    /// <summary>
    /// Removes a typed hotkey.
    /// </summary>
    public bool UnregisterHotkey(KeyGesture hotKey)
    {
        lock (_hotkeysLock)
        {
            return _hotkeys.Remove(hotKey);
        }
    }

    /// <summary>
    /// Removes every registered hotkey.
    /// </summary>
    public void ClearHotkeys()
    {
        lock (_hotkeysLock)
        {
            _hotkeys.Clear();
        }
    }

    /// <inheritdoc />
    public void HandleKey(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (!KeyGesture.TryFromEvent(key, out var gesture))
            return;

        Action? action = null;
        lock (_hotkeysLock)
        {
            if (_hotkeys.TryGetValue(gesture, out var registration))
                action = registration.Action;
        }

        if (action is null)
            return;

        MainThread.BeginInvokeOnMainThread(() => InvokeSafely(action));
        key.Handled = true;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldHandle(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key.EventType == KeyboardEventType.KeyDown;
    }

    private static KeyGesture ParseGesture(string value, string parameterName)
    {
        try
        {
            return KeyGesture.Parse(value);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(exception.Message, parameterName, exception);
        }
    }

    private static void InvokeSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            // User actions run outside input-thread dispatch and must not crash the UI loop.
            System.Diagnostics.Debug.WriteLine($"[GlobalKeyboardCapture] Hotkey action threw: {exception}");
        }
    }

    private void UnregisterHotkey(KeyGesture gesture, long registrationId)
    {
        lock (_hotkeysLock)
        {
            if (_hotkeys.TryGetValue(gesture, out var current) && current.Id == registrationId)
                _hotkeys.Remove(gesture);
        }
    }

    private sealed record HotkeyRegistration(long Id, Action Action);

    private sealed class HotkeyRegistrationToken : IDisposable
    {
        private HotkeyHandler? _owner;
        private readonly KeyGesture _gesture;
        private readonly long _registrationId;

        public HotkeyRegistrationToken(HotkeyHandler owner, KeyGesture gesture, long registrationId)
        {
            _owner = owner;
            _gesture = gesture;
            _registrationId = registrationId;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.UnregisterHotkey(_gesture, _registrationId);
        }
    }
}
