using System.Runtime.CompilerServices;
using Android.Views;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Platforms.Android;
using Activity = Android.App.Activity;
using IWindowCallback = Android.Views.Window.ICallback;

namespace GlobalKeyboardCapture.Maui;

public sealed class AndroidKeyHandler : IPlatformKeyHandler, IDisposable
{
    private const KeyEventActions KEY_ACTION_DOWN = KeyEventActions.Down;
    private const KeyEventFlags KEY_FLAGS_FROM_SYSTEM = KeyEventFlags.FromSystem;

    private readonly object _lockObject = new();
    private Action<KeyEventArgs>? _onKeyPressed;
    private Activity? _activity;
    private IWindowCallback? _originalDispatcher;
    private bool _isInitialized;
    private bool _isDisposed;

    public void ConfigureHandler(Action<Core.Models.KeyEventArgs> onKeyPressed)
    {
        ArgumentNullException.ThrowIfNull(onKeyPressed);
        _onKeyPressed = onKeyPressed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DispatchKeyEvent(KeyEvent e)
    {
        ArgumentNullException.ThrowIfNull(e);
        ThrowIfDisposed();

        if (e.Action != KEY_ACTION_DOWN || e.Flags != KEY_FLAGS_FROM_SYSTEM)
            return false;

        var keyEvent = CreateKeyEventArgs(e);
        ProcessKeyEvent(keyEvent, e);

        return keyEvent.Handled;
    }

    public void Initialize(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);
        ThrowIfDisposed();

        lock (_lockObject)
        {
            if (_isInitialized) return;

            _activity = Platform.CurrentActivity
                ?? throw new InvalidOperationException("Current activity is null");

            if (_activity?.Window == null)
                throw new InvalidOperationException("Activity window is null");

            // Install global event dispatcher
            _originalDispatcher = _activity.Window.Callback;
            _activity.Window.Callback = new KeyEventCallback(this, _activity.Window.Callback!);
            _isInitialized = true;
        }
    }

    public void Cleanup()
    {
        Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static KeyEventArgs CreateKeyEventArgs(KeyEvent e)
    {
        return new KeyEventArgs
        {
            // Modifiers
            ControlKey = e.IsCtrlPressed,
            AltKey = e.IsAltPressed,
            ShiftKey = e.IsShiftPressed,
            WindowsKey = e.KeyCode == Keycode.Window,

            // Navigation
            UpKey = e.KeyCode == Keycode.DpadUp,
            DownKey = e.KeyCode == Keycode.DpadDown,
            LeftKey = e.KeyCode == Keycode.DpadLeft,
            RightKey = e.KeyCode == Keycode.DpadRight,
            HomeKey = e.KeyCode == Keycode.MoveHome,
            EndKey = e.KeyCode == Keycode.MoveEnd,
            PageUpKey = e.KeyCode == Keycode.PageUp,
            PageDownKey = e.KeyCode == Keycode.PageDown,

            // Edition
            EnterKey = e.KeyCode == Keycode.Enter,
            TabKey = e.KeyCode == Keycode.Tab,
            BackspaceKey = e.KeyCode == Keycode.Del,
            DeleteKey = e.KeyCode == Keycode.ForwardDel,
            EscapeKey = e.KeyCode == Keycode.Escape,
            SpaceKey = e.KeyCode == Keycode.Space,
            InsertKey = e.KeyCode == Keycode.Insert,

            // System
            CapsLockKey = e.KeyCode == Keycode.CapsLock,
            NumLockKey = e.KeyCode == Keycode.NumLock,
            ScrollLockKey = e.KeyCode == Keycode.ScrollLock,
            MenuKey = e.KeyCode == Keycode.Menu,
            PlatformEvent = e
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessKeyEvent(KeyEventArgs keyEvent, KeyEvent e)
    {
        var displayLabel = e.DisplayLabel.ToString();
        keyEvent.Character = KeyboardHelper.ToChar(displayLabel);

        if (keyEvent.Character == null)
        {
            keyEvent.FunctionKey = KeyboardHelper.ToFunction(displayLabel);
        }

        _onKeyPressed?.Invoke(keyEvent);
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(AndroidKeyHandler));
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        lock (_lockObject)
        {
            if (_isDisposed) return;

            if (_activity?.Window != null && _originalDispatcher != null)
            {
                _activity.Window.Callback = _originalDispatcher;
            }

            _originalDispatcher = null;
            _activity = null;
            _isInitialized = false;
            _isDisposed = true;
        }
    }
}
