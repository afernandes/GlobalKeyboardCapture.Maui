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
    private Microsoft.UI.Xaml.Window _window;
    private Action<KeyEventArgs> _onKeyPressed;

    Func<VirtualKey, CoreVirtualKeyStates> GetKeyState;
    
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
        _window = platformView as Microsoft.UI.Xaml.Window;
        if (_window?.Content != null)
        {
            _window.Content.PreviewKeyDown += OnKeyDown;
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

        //System.Diagnostics.Debug.WriteLine($"Key: {args.Key}, Char: {character}, Function: {functionKey}, " +
        //    $"Modifiers: Ctrl={keyEvent.ControlKey}, Alt={keyEvent.AltKey}, Shift={keyEvent.ShiftKey}, Win={keyEvent.WindowsKey}");

        _onKeyPressed?.Invoke(keyEvent);

        if (keyEvent.Handled)
            args.Handled = true;
    }

    public void Cleanup()
    {
        if (_window?.Content != null)
        {
            _window.Content.KeyDown -= OnKeyDown;
        }
    }
}
