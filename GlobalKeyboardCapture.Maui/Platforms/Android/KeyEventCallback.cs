using Android.Views;
using Android.Views.Accessibility;
using IWindowCallback = Android.Views.Window.ICallback;
using View = Android.Views.View;

namespace GlobalKeyboardCapture.Maui.Platforms.Android;

public class KeyEventCallback : Java.Lang.Object, IWindowCallback
{
    private readonly AndroidKeyHandler _handler;
    private readonly IWindowCallback _original;

    public KeyEventCallback(AndroidKeyHandler handler, IWindowCallback original)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(original);
        _handler = handler;
        _original = original;
    }

    public bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e is null)
            return false;
        
        var handled = _handler.DispatchKeyEvent(e);
        if (handled) return true;
        return _original.DispatchKeyEvent(e);
    }

    #region Implement other IWindowCallback methods
    public bool DispatchGenericMotionEvent(MotionEvent? e) => _original.DispatchGenericMotionEvent(e) ;
    public bool DispatchKeyShortcutEvent(KeyEvent? e) => _original.DispatchKeyShortcutEvent(e) ;
    public bool DispatchPopulateAccessibilityEvent(AccessibilityEvent? e) => _original.DispatchPopulateAccessibilityEvent(e) ;
    public bool DispatchTouchEvent(MotionEvent? e) => _original.DispatchTouchEvent(e) ;
    public bool DispatchTrackballEvent(MotionEvent? e) => _original.DispatchTrackballEvent(e) ;
    public void OnActionModeFinished(ActionMode? mode) => _original.OnActionModeFinished(mode);
    public void OnActionModeStarted(ActionMode? mode) => _original.OnActionModeStarted(mode);
    public void OnAttachedToWindow() => _original.OnAttachedToWindow();
    public void OnContentChanged() => _original.OnContentChanged();
    public bool OnCreatePanelMenu(int featureId, IMenu menu) => _original.OnCreatePanelMenu(featureId, menu) ;
    public View? OnCreatePanelView(int featureId) => _original.OnCreatePanelView(featureId);
    public void OnDetachedFromWindow() => _original.OnDetachedFromWindow();
    public bool OnMenuItemSelected(int featureId, IMenuItem item) => _original.OnMenuItemSelected(featureId, item);
    public bool OnMenuOpened(int featureId, IMenu menu) => _original.OnMenuOpened(featureId, menu) ;
    public void OnPanelClosed(int featureId, IMenu menu) => _original.OnPanelClosed(featureId, menu);
    public bool OnPreparePanel(int featureId, View? view, IMenu menu) => _original.OnPreparePanel(featureId, view, menu) ;
    public bool OnSearchRequested() => _original.OnSearchRequested() ;
    public bool OnSearchRequested(SearchEvent? searchEvent) => _original.OnSearchRequested(searchEvent) ;
    public void OnWindowAttributesChanged(WindowManagerLayoutParams? attrs) => _original.OnWindowAttributesChanged(attrs);
    public void OnWindowFocusChanged(bool hasFocus) => _original.OnWindowFocusChanged(hasFocus);
    public ActionMode? OnWindowStartingActionMode(ActionMode.ICallback? callback, ActionModeType type) => _original.OnWindowStartingActionMode(callback, type);
    public ActionMode? OnWindowStartingActionMode(ActionMode.ICallback? callback) => _original.OnWindowStartingActionMode(callback);
    #endregion
}
