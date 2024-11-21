using Android.Views;
using Android.Views.Accessibility;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Platforms.Android;
using Activity = Android.App.Activity;
using IWindowCallback = Android.Views.Window.ICallback;
using View = Android.Views.View;

namespace GlobalKeyboardCapture.Maui;

public class AndroidKeyHandler : IPlatformKeyHandler
{
    private Action<Core.Models.KeyEventArgs> _onKeyPressed;

    private Activity _activity;
    private IWindowCallback _originalDispatcher;
    private bool _isInitialized;

    public void ConfigureHandler(Action<Core.Models.KeyEventArgs> onKeyPressed)
    {
        _onKeyPressed = onKeyPressed;
    }

    public bool DispatchKeyEvent(KeyEvent e)
    {
        if (e.Action == KeyEventActions.Down && e.Flags == KeyEventFlags.FromSystem)
        {
            var keyEvent = new Core.Models.KeyEventArgs
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

            var character = KeyboardHelper.ToChar(e.DisplayLabel.ToString());
            var functionKey = (character == null) ? KeyboardHelper.ToFunction(e.DisplayLabel.ToString()) : null;

            keyEvent.Character = character;
            keyEvent.FunctionKey = functionKey;

            _onKeyPressed?.Invoke(keyEvent);
            return keyEvent.Handled;
        }

        return false;
    }

    public void Initialize(object platformView)
    {
        if (_isInitialized) return;

        _activity = Platform.CurrentActivity;
        if (_activity?.Window != null)
        {
            // Install global event dispatcher
            _originalDispatcher = _activity.Window.Callback;
            _activity.Window.Callback = new KeyEventCallback(this, _activity.Window.Callback);
            _isInitialized = true;
        }
    }

    public void Cleanup()
    {
        if (_activity?.Window != null && _originalDispatcher != null)
        {
            _activity.Window.Callback = _originalDispatcher;
            _originalDispatcher = null;
        }
        _activity = null;
        _isInitialized = false;
    }
}
