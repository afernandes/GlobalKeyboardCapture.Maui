using Windows.System;
using Windows.UI.Core;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Platforms.Windows;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using KeyEventArgs = GlobalKeyboardCapture.Maui.Core.Models.KeyEventArgs;

namespace GlobalKeyboardCapture.Maui;

public class WindowsKeyHandler : IPlatformKeyHandler
{
    private Microsoft.UI.Xaml.Window? _window;
    private Microsoft.UI.Xaml.UIElement? _subscribedContent;
    private Action<KeyEventArgs>? _onKeyPressed;

    readonly Func<VirtualKey, CoreVirtualKeyStates> GetKeyState;
    
    public WindowsKeyHandler()
    {
        GetKeyState = InputKeyboardSource.GetKeyStateForCurrentThread;
    }

    public void ConfigureHandler(Action<KeyEventArgs> onKeyPressed)
    {
        _onKeyPressed = onKeyPressed;
    }

    public void Initialize(object platformView)
    {
        // Detach from any previously bound window/content first.
        Unsubscribe();

        _window = platformView as Microsoft.UI.Xaml.Window;
        if (_window == null)
            return;

        // During OnLaunched the MAUI root content is frequently not attached yet, so a
        // one-shot "subscribe if Content != null" silently captures nothing. Try now and,
        // if Content isn't ready, retry on each activation until it is.
        if (!TrySubscribe())
            _window.Activated += OnWindowActivated;
    }

    private void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        if (TrySubscribe() && _window != null)
            _window.Activated -= OnWindowActivated;
    }

    private bool TrySubscribe()
    {
        var content = _window?.Content;
        if (content == null)
            return false;
        if (ReferenceEquals(content, _subscribedContent))
            return true;

        // Move the subscription to the exact current content element.
        if (_subscribedContent != null)
            _subscribedContent.PreviewKeyDown -= OnKeyDown;
        content.PreviewKeyDown += OnKeyDown;
        _subscribedContent = content;
        return true;
    }

    private void Unsubscribe()
    {
        if (_window != null)
            _window.Activated -= OnWindowActivated;
        if (_subscribedContent != null)
        {
            _subscribedContent.PreviewKeyDown -= OnKeyDown;
            _subscribedContent = null;
        }
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs args)
    {
        if (args.Handled)
            return;

        var keyEvent = new Core.Models.KeyEventArgs
        {
            // Modifiers
            ControlKey = (GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down,
            AltKey = (GetKeyState(VirtualKey.Menu) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down,
            ShiftKey = (GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down,
            WindowsKey = ((GetKeyState(VirtualKey.LeftWindows) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down) ||
                         ((GetKeyState(VirtualKey.RightWindows) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down),

            // Navigation
            UpKey = args.Key == VirtualKey.Up,
            DownKey = args.Key == VirtualKey.Down,
            LeftKey = args.Key == VirtualKey.Left,
            RightKey = args.Key == VirtualKey.Right,
            HomeKey = args.Key == VirtualKey.Home,
            EndKey = args.Key == VirtualKey.End,
            PageUpKey = args.Key == VirtualKey.PageUp,
            PageDownKey = args.Key == VirtualKey.PageDown,

            // Edition
            EnterKey = args.Key == VirtualKey.Enter,
            TabKey = args.Key == VirtualKey.Tab,
            BackspaceKey = args.Key == VirtualKey.Back,
            DeleteKey = args.Key == VirtualKey.Delete,
            EscapeKey = args.Key == VirtualKey.Escape,
            SpaceKey = args.Key == VirtualKey.Space,
            InsertKey = args.Key == VirtualKey.Insert,

            // System
            CapsLockKey = args.Key == VirtualKey.CapitalLock,
            NumLockKey = args.Key == VirtualKey.NumberKeyLock,
            ScrollLockKey = args.Key == VirtualKey.Scroll,
            PrintScreenKey = args.Key == VirtualKey.Print,
            PauseBreakKey = args.Key == VirtualKey.Pause,
            MenuKey = args.Key == VirtualKey.Application,

            PlatformEvent = args
        };

        var character = KeyboardHelper.ToChar(args.Key);
        var functionKey = character == null ? KeyboardHelper.ToFunction(args.Key) : null;

        keyEvent.Character = character;
        keyEvent.FunctionKey = functionKey;
        

        _onKeyPressed?.Invoke(keyEvent);

        if (keyEvent.Handled)
            args.Handled = true;
    }

    public void Cleanup()
    {
        Unsubscribe();
        _window = null;
    }
}
