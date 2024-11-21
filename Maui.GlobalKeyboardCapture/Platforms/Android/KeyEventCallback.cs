using Android.Views;
using Android.Views.Accessibility;
using IWindowCallback = Android.Views.Window.ICallback;
using View = Android.Views.View;

namespace Maui.GlobalKeyboardCapture.Platforms.Android;

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
