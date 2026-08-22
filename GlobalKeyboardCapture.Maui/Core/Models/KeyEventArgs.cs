using System.Runtime.CompilerServices;
using System.Text;

namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Represents one platform-neutral keyboard transition and its native metadata.
/// </summary>
public sealed class KeyEventArgs
{
    // Constants for string builder initial capacity
    private const int INITIAL_TOSTRING_CAPACITY = 32;
    private KeyboardKey _typedKey;

    /// <summary>
    /// Gets or sets the native event object. Its lifetime normally ends when synchronous
    /// dispatch returns; use <see cref="CreateSnapshot"/> for deferred work.
    /// </summary>
    public object? PlatformEvent { get; set; }

    /// <summary>Gets or sets whether the event was consumed by the application pipeline.</summary>
    public bool Handled { get; set; }

    /// <summary>Gets or sets the platform that produced this event.</summary>
    public KeyboardPlatform Platform { get; set; }

    /// <summary>Gets or sets whether this event represents a key press or release.</summary>
    public KeyboardEventType EventType { get; set; } = KeyboardEventType.KeyDown;

    /// <summary>Gets or sets the physical key location, when known.</summary>
    public KeyLocation Location { get; set; }

    /// <summary>Gets or sets the native platform key code.</summary>
    public int NativeKeyCode { get; set; }

    /// <summary>Gets or sets the native hardware scan code.</summary>
    public int NativeScanCode { get; set; }

    /// <summary>Gets or sets native platform flags used for diagnostics.</summary>
    public string? NativeFlags { get; set; }

    /// <summary>Gets or sets the native repeat count.</summary>
    public int RepeatCount { get; set; }

    /// <summary>Gets whether this is an auto-repeat event.</summary>
    public bool IsRepeat => RepeatCount > 0;

    /// <summary>Gets or sets metadata about the originating input device.</summary>
    public KeyboardDeviceInfo? Device { get; set; }

    /// <summary>Gets or sets the printable character represented by this event.</summary>
    public char? Character { get; set; }

    /// <summary>Gets or sets the legacy canonical function-key name.</summary>
    public string? FunctionKey { get; set; }

    /// <summary>
    /// Gets or sets the platform-neutral logical key. Legacy key properties remain
    /// synchronized when this property is assigned.
    /// </summary>
    public KeyboardKey Key
    {
        get => ResolveKeyboardKey();
        set => ApplyKeyboardKey(value);
    }

    /// <summary>Gets or sets whether Control is active.</summary>
    public bool ControlKey { get; set; }

    /// <summary>Gets or sets whether Alt or Option is active.</summary>
    public bool AltKey { get; set; }

    /// <summary>Gets or sets whether Shift is active.</summary>
    public bool ShiftKey { get; set; }

    /// <summary>Gets or sets whether Windows, Command, or Meta is active.</summary>
    public bool WindowsKey { get; set; }

    /// <summary>
    /// Gets or sets the active modifiers as flags.
    /// </summary>
    public KeyModifiers Modifiers
    {
        get
        {
            var modifiers = KeyModifiers.None;
            if (ControlKey) modifiers |= KeyModifiers.Control;
            if (AltKey) modifiers |= KeyModifiers.Alt;
            if (ShiftKey) modifiers |= KeyModifiers.Shift;
            if (WindowsKey) modifiers |= KeyModifiers.Meta;
            return modifiers;
        }
        set
        {
            if ((value & ~KeyModifiers.All) != 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            ControlKey = (value & KeyModifiers.Control) != 0;
            AltKey = (value & KeyModifiers.Alt) != 0;
            ShiftKey = (value & KeyModifiers.Shift) != 0;
            WindowsKey = (value & KeyModifiers.Meta) != 0;
        }
    }

    /// <summary>Gets or sets whether the Up arrow is represented.</summary>
    public bool UpKey { get; set; }

    /// <summary>Gets or sets whether the Down arrow is represented.</summary>
    public bool DownKey { get; set; }

    /// <summary>Gets or sets whether the Left arrow is represented.</summary>
    public bool LeftKey { get; set; }

    /// <summary>Gets or sets whether the Right arrow is represented.</summary>
    public bool RightKey { get; set; }

    /// <summary>Gets or sets whether Home is represented.</summary>
    public bool HomeKey { get; set; }

    /// <summary>Gets or sets whether End is represented.</summary>
    public bool EndKey { get; set; }

    /// <summary>Gets or sets whether Page Up is represented.</summary>
    public bool PageUpKey { get; set; }

    /// <summary>Gets or sets whether Page Down is represented.</summary>
    public bool PageDownKey { get; set; }

    /// <summary>Gets or sets whether Enter or Return is represented.</summary>
    public bool EnterKey { get; set; }

    /// <summary>Gets or sets whether Tab is represented.</summary>
    public bool TabKey { get; set; }

    /// <summary>Gets or sets whether Backspace is represented.</summary>
    public bool BackspaceKey { get; set; }

    /// <summary>Gets or sets whether forward Delete is represented.</summary>
    public bool DeleteKey { get; set; }

    /// <summary>Gets or sets whether Escape is represented.</summary>
    public bool EscapeKey { get; set; }

    /// <summary>Gets or sets whether Space is represented.</summary>
    public bool SpaceKey { get; set; }

    /// <summary>Gets or sets whether Insert is represented.</summary>
    public bool InsertKey { get; set; }

    /// <summary>Gets or sets whether Caps Lock is represented.</summary>
    public bool CapsLockKey { get; set; }

    /// <summary>Gets or sets whether Num Lock is represented.</summary>
    public bool NumLockKey { get; set; }

    /// <summary>Gets or sets whether Scroll Lock is represented.</summary>
    public bool ScrollLockKey { get; set; }

    /// <summary>Gets or sets whether Print Screen is represented.</summary>
    public bool PrintScreenKey { get; set; }

    /// <summary>Gets or sets whether Pause or Break is represented.</summary>
    public bool PauseBreakKey { get; set; }

    /// <summary>Gets or sets whether the context-menu key is represented.</summary>
    public bool MenuKey { get; set; }

    /// <summary>Gets whether Meta is the only active modifier.</summary>
    public bool OnlyWindows
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    }

    /// <summary>Gets whether Alt is the only active modifier.</summary>
    public bool OnlyAlt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && AltKey && !ControlKey && !ShiftKey;
    }

    /// <summary>Gets whether Control is the only active modifier.</summary>
    public bool OnlyControl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && ControlKey && !ShiftKey;
    }

    /// <summary>Gets whether Shift is the only active modifier.</summary>
    public bool OnlyShift
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && !ControlKey && ShiftKey;
    }

    /// <summary>Gets whether no modifier is active.</summary>
    public bool NoSpecialKeysPressed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    }

    /// <summary>Gets whether at least one modifier is active.</summary>
    public bool AnySpecialKeyPressed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => WindowsKey || AltKey || ControlKey || ShiftKey;
    }

    /// <summary>
    /// Creates a detached event snapshot. Native event objects are excluded by default
    /// because their lifetime generally ends when the platform callback returns.
    /// </summary>
    public KeyEventArgs CreateSnapshot(bool includePlatformEvent = false)
    {
        var snapshot = (KeyEventArgs)MemberwiseClone();
        if (!includePlatformEvent)
            snapshot.PlatformEvent = null;
        return snapshot;
    }

    /// <summary>
    /// Returns the canonical legacy lookup key with modifiers ordered as
    /// <c>Ctrl+Alt+Shift+Win</c>.
    /// </summary>
    /// <returns>The normalized key representation.</returns>
    public override string ToString()
    {
        var list = new StringBuilder(INITIAL_TOSTRING_CAPACITY);
        var needsSeparator = false;

        // Helper method to append with separator
        void AppendWithSeparator(string value)
        {
            if (needsSeparator)
                list.Append('+');
            list.Append(value);
            needsSeparator = true;
        }

        // Modifiers
        if (ControlKey) AppendWithSeparator("Ctrl");
        if (AltKey) AppendWithSeparator("Alt");
        if (ShiftKey) AppendWithSeparator("Shift");
        if (WindowsKey) AppendWithSeparator("Win");

        // Function or Character
        if (FunctionKey != null)
            AppendWithSeparator(FunctionKey);
        else if (Character.HasValue)
            AppendWithSeparator(Character.Value.ToString());

        // Special Keys
        if (EnterKey) AppendWithSeparator("Enter");
        if (TabKey) AppendWithSeparator("Tab");
        if (BackspaceKey) AppendWithSeparator("Backspace");
        if (DeleteKey) AppendWithSeparator("Delete");
        if (EscapeKey) AppendWithSeparator("Esc");
        if (SpaceKey) AppendWithSeparator("Space");
        if (InsertKey) AppendWithSeparator("Insert");

        // Navigation
        if (UpKey) AppendWithSeparator("Up");
        if (DownKey) AppendWithSeparator("Down");
        if (LeftKey) AppendWithSeparator("Left");
        if (RightKey) AppendWithSeparator("Right");
        if (HomeKey) AppendWithSeparator("Home");
        if (EndKey) AppendWithSeparator("End");
        if (PageUpKey) AppendWithSeparator("PageUp");
        if (PageDownKey) AppendWithSeparator("PageDown");

        // System
        if (CapsLockKey) AppendWithSeparator("CapsLock");
        if (NumLockKey) AppendWithSeparator("NumLock");
        if (ScrollLockKey) AppendWithSeparator("ScrollLock");
        if (PrintScreenKey) AppendWithSeparator("PrintScreen");
        if (PauseBreakKey) AppendWithSeparator("PauseBreak");
        if (MenuKey) AppendWithSeparator("Menu");

        if (!HasLegacyLogicalKey() && _typedKey != KeyboardKey.None)
            AppendWithSeparator(KeyGesture.GetCanonicalKeyName(_typedKey));

        if (list.Length > 0)
            return list.ToString();

        // Platform-specific handling
#if WINDOWS
        if (PlatformEvent is Microsoft.UI.Xaml.Input.KeyRoutedEventArgs keyArgs)
        {
            return $"OEM{keyArgs.Key}";
        }
#elif ANDROID
        if (PlatformEvent is Android.Views.KeyEvent keyEvent)
        {
            var unicodeChar = (char)keyEvent.UnicodeChar;
            return !char.IsControl(unicodeChar)
                ? unicodeChar.ToString()
                : keyEvent.KeyCode.ToString();
        }
#endif
        return PlatformEvent?.ToString() ?? string.Empty;
    }

    private KeyboardKey ResolveKeyboardKey()
    {
        if (FunctionKey is not null && KeyGesture.TryParseFunctionKey(FunctionKey, out var functionKey))
            return functionKey;
        if (Character.HasValue)
            return KeyboardKey.Character;
        if (EnterKey) return KeyboardKey.Enter;
        if (TabKey) return KeyboardKey.Tab;
        if (BackspaceKey) return KeyboardKey.Backspace;
        if (DeleteKey) return KeyboardKey.Delete;
        if (EscapeKey) return KeyboardKey.Escape;
        if (SpaceKey) return KeyboardKey.Space;
        if (InsertKey) return KeyboardKey.Insert;
        if (UpKey) return KeyboardKey.Up;
        if (DownKey) return KeyboardKey.Down;
        if (LeftKey) return KeyboardKey.Left;
        if (RightKey) return KeyboardKey.Right;
        if (HomeKey) return KeyboardKey.Home;
        if (EndKey) return KeyboardKey.End;
        if (PageUpKey) return KeyboardKey.PageUp;
        if (PageDownKey) return KeyboardKey.PageDown;
        if (CapsLockKey) return KeyboardKey.CapsLock;
        if (NumLockKey) return KeyboardKey.NumLock;
        if (ScrollLockKey) return KeyboardKey.ScrollLock;
        if (PrintScreenKey) return KeyboardKey.PrintScreen;
        if (PauseBreakKey) return KeyboardKey.PauseBreak;
        if (MenuKey) return KeyboardKey.Menu;
        return _typedKey;
    }

    private void ApplyKeyboardKey(KeyboardKey key)
    {
        ClearLogicalKey();
        _typedKey = key;

        if (key is >= KeyboardKey.F1 and <= KeyboardKey.F24)
        {
            FunctionKey = KeyGesture.GetCanonicalKeyName(key);
            return;
        }

        switch (key)
        {
            case KeyboardKey.None:
                break;
            case KeyboardKey.Character:
                throw new ArgumentException("Assign Character when setting a character key.", nameof(key));
            case KeyboardKey.Enter: EnterKey = true; break;
            case KeyboardKey.Tab: TabKey = true; break;
            case KeyboardKey.Backspace: BackspaceKey = true; break;
            case KeyboardKey.Delete: DeleteKey = true; break;
            case KeyboardKey.Escape: EscapeKey = true; break;
            case KeyboardKey.Space: SpaceKey = true; break;
            case KeyboardKey.Insert: InsertKey = true; break;
            case KeyboardKey.Up: UpKey = true; break;
            case KeyboardKey.Down: DownKey = true; break;
            case KeyboardKey.Left: LeftKey = true; break;
            case KeyboardKey.Right: RightKey = true; break;
            case KeyboardKey.Home: HomeKey = true; break;
            case KeyboardKey.End: EndKey = true; break;
            case KeyboardKey.PageUp: PageUpKey = true; break;
            case KeyboardKey.PageDown: PageDownKey = true; break;
            case KeyboardKey.CapsLock: CapsLockKey = true; break;
            case KeyboardKey.NumLock: NumLockKey = true; break;
            case KeyboardKey.ScrollLock: ScrollLockKey = true; break;
            case KeyboardKey.PrintScreen: PrintScreenKey = true; break;
            case KeyboardKey.PauseBreak: PauseBreakKey = true; break;
            case KeyboardKey.Menu: MenuKey = true; break;
        }
    }

    private bool HasLegacyLogicalKey() =>
        FunctionKey is not null
        || Character.HasValue
        || EnterKey
        || TabKey
        || BackspaceKey
        || DeleteKey
        || EscapeKey
        || SpaceKey
        || InsertKey
        || UpKey
        || DownKey
        || LeftKey
        || RightKey
        || HomeKey
        || EndKey
        || PageUpKey
        || PageDownKey
        || CapsLockKey
        || NumLockKey
        || ScrollLockKey
        || PrintScreenKey
        || PauseBreakKey
        || MenuKey;

    private void ClearLogicalKey()
    {
        Character = null;
        FunctionKey = null;
        EnterKey = false;
        TabKey = false;
        BackspaceKey = false;
        DeleteKey = false;
        EscapeKey = false;
        SpaceKey = false;
        InsertKey = false;
        UpKey = false;
        DownKey = false;
        LeftKey = false;
        RightKey = false;
        HomeKey = false;
        EndKey = false;
        PageUpKey = false;
        PageDownKey = false;
        CapsLockKey = false;
        NumLockKey = false;
        ScrollLockKey = false;
        PrintScreenKey = false;
        PauseBreakKey = false;
        MenuKey = false;
    }
}
