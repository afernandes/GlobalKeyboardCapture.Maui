using System.Runtime.CompilerServices;
using System.Text;

namespace GlobalKeyboardCapture.Maui.Core.Models;

public sealed class KeyEventArgs
{
    // Constants for string builder initial capacity
    private const int INITIAL_TOSTRING_CAPACITY = 32;
    private KeyboardKey _typedKey;

    // Platform-specific event
    public object? PlatformEvent { get; set; }
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

    // Characters and Function
    public char? Character { get; set; }
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

    // Modifier Keys
    public bool ControlKey { get; set; }
    public bool AltKey { get; set; }
    public bool ShiftKey { get; set; }
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

    // Navigation Keys
    public bool UpKey { get; set; }
    public bool DownKey { get; set; }
    public bool LeftKey { get; set; }
    public bool RightKey { get; set; }
    public bool HomeKey { get; set; }
    public bool EndKey { get; set; }
    public bool PageUpKey { get; set; }
    public bool PageDownKey { get; set; }

    // Editing Keys
    public bool EnterKey { get; set; }
    public bool TabKey { get; set; }
    public bool BackspaceKey { get; set; }
    public bool DeleteKey { get; set; }
    public bool EscapeKey { get; set; }
    public bool SpaceKey { get; set; }
    public bool InsertKey { get; set; }

    // System Keys
    public bool CapsLockKey { get; set; }
    public bool NumLockKey { get; set; }
    public bool ScrollLockKey { get; set; }
    public bool PrintScreenKey { get; set; }
    public bool PauseBreakKey { get; set; }
    public bool MenuKey { get; set; }

    public bool OnlyWindows
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    }

    public bool OnlyAlt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && AltKey && !ControlKey && !ShiftKey;
    }

    public bool OnlyControl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && ControlKey && !ShiftKey;
    }

    public bool OnlyShift
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && !ControlKey && ShiftKey;
    }

    public bool NoSpecialKeysPressed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !WindowsKey && !AltKey && !ControlKey && !ShiftKey;
    }

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

    // ToString implementation optimized
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
