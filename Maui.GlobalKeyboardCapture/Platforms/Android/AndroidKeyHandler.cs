using Android.Views;
using Android.Views.Accessibility;
using Maui.GlobalKeyboardCapture.Core.Interfaces;
using Maui.GlobalKeyboardCapture.Platforms.Android;
using Activity = Android.App.Activity;
using IWindowCallback = Android.Views.Window.ICallback;
using View = Android.Views.View;

namespace Maui.GlobalKeyboardCapture;

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
            var enter = e.KeyCode == Keycode.Enter;
            var alt = e.IsAltPressed;
            var shift = e.IsShiftPressed;
            var control = e.IsCtrlPressed;
            var windows = e.KeyCode == Keycode.Window;
            var character = KeyboardHelper.ToChar(e.DisplayLabel.ToString(), shift);
            var functionKey = (character == null) ? KeyboardHelper.ToFunction(e.DisplayLabel.ToString()) : null;
            var down = false;
            var up = false;
            var left = false;
            var right = false;

            var keyDown = new Core.Models.KeyEventArgs
            {
                EnterKey = enter,
                ShiftKey = shift,
                ControlKey = control,
                WindowsKey = windows,
                AltKey = alt,
                Character = character,
                FunctionKey = functionKey,
                PlatformEvent = e,
                DownKey = down,
                UpKey = up,
                LeftKey = left,
                RightKey = right
            };

            _onKeyPressed?.Invoke(keyDown);
            return keyDown.Handled;
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
            _activity.Window.Callback = _originalDispatcher as IWindowCallback;
            _originalDispatcher = null;
        }
        _activity = null;
        _isInitialized = false;
    }
}

public class KeyEventCallback : Java.Lang.Object, IWindowCallback
{
    private readonly AndroidKeyHandler _handler;
    private readonly IWindowCallback _original;

    public KeyEventCallback(AndroidKeyHandler handler, IWindowCallback original)
    {
        _handler = handler;
        _original = original;
    }

    public bool DispatchKeyEvent(KeyEvent e)
    {
        var handled = _handler.DispatchKeyEvent(e);
        if (handled) return true;
        return _original?.DispatchKeyEvent(e) ?? false;
    }

    #region Implement other IWindowCallback methods
    public bool DispatchGenericMotionEvent(MotionEvent e) => _original?.DispatchGenericMotionEvent(e) ?? false;
    public bool DispatchKeyShortcutEvent(KeyEvent e) => _original?.DispatchKeyShortcutEvent(e) ?? false;
    public bool DispatchPopulateAccessibilityEvent(AccessibilityEvent e) => _original?.DispatchPopulateAccessibilityEvent(e) ?? false;
    public bool DispatchTouchEvent(MotionEvent e) => _original?.DispatchTouchEvent(e) ?? false;
    public bool DispatchTrackballEvent(MotionEvent e) => _original?.DispatchTrackballEvent(e) ?? false;
    public void OnActionModeFinished(ActionMode mode) => _original?.OnActionModeFinished(mode);
    public void OnActionModeStarted(ActionMode mode) => _original?.OnActionModeStarted(mode);
    public void OnAttachedToWindow() => _original?.OnAttachedToWindow();
    public void OnContentChanged() => _original?.OnContentChanged();
    public bool OnCreatePanelMenu(int featureId, IMenu menu) => _original?.OnCreatePanelMenu(featureId, menu) ?? false;
    public View OnCreatePanelView(int featureId) => _original?.OnCreatePanelView(featureId);
    public void OnDetachedFromWindow() => _original?.OnDetachedFromWindow();
    public bool OnMenuItemSelected(int featureId, IMenuItem item) => _original?.OnMenuItemSelected(featureId, item) ?? false;
    public bool OnMenuOpened(int featureId, IMenu menu) => _original?.OnMenuOpened(featureId, menu) ?? false;
    public void OnPanelClosed(int featureId, IMenu menu) => _original?.OnPanelClosed(featureId, menu);
    public bool OnPreparePanel(int featureId, View view, IMenu menu) => _original?.OnPreparePanel(featureId, view, menu) ?? false;
    public bool OnSearchRequested() => _original?.OnSearchRequested() ?? false;
    public bool OnSearchRequested(SearchEvent searchEvent) => _original?.OnSearchRequested(searchEvent) ?? false;
    public void OnWindowAttributesChanged(WindowManagerLayoutParams attrs) => _original?.OnWindowAttributesChanged(attrs);
    public void OnWindowFocusChanged(bool hasFocus) => _original?.OnWindowFocusChanged(hasFocus);
    public ActionMode? OnWindowStartingActionMode(ActionMode.ICallback? callback, ActionModeType type) => _original?.OnWindowStartingActionMode(callback, type);
    public ActionMode OnWindowStartingActionMode(ActionMode.ICallback callback) => _original?.OnWindowStartingActionMode(callback);
    #endregion
}
