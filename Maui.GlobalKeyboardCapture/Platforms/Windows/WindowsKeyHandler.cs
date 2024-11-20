using Windows.System;
using Windows.UI.Core;
using Maui.GlobalKeyboardCapture.Core.Interfaces;
using Maui.GlobalKeyboardCapture.Platforms.Windows;
using Microsoft.UI.Input;
using KeyEventArgs = Maui.GlobalKeyboardCapture.Core.Models.KeyEventArgs;

namespace Maui.GlobalKeyboardCapture;

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

    private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs args)
    {
        if (args.Handled)
            return;

        var enter = ((GetKeyState(VirtualKey.Enter) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down);
        var alt = (GetKeyState(VirtualKey.Menu) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        var shift = (GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        var control = (GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        var windows = ((GetKeyState(VirtualKey.LeftWindows) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down) || ((GetKeyState(VirtualKey.RightWindows) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down);
        var character = KeyboardHelper.ToChar(args.Key, shift);
        var functionKey = (character == null) ? KeyboardHelper.ToFunction(args.Key) : null;
        var down = (GetKeyState(VirtualKey.Down) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        var up = (GetKeyState(VirtualKey.Up) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        var left = (GetKeyState(VirtualKey.Left) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        var right = (GetKeyState(VirtualKey.Right) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        var keyDown = new KeyEventArgs
        {
            EnterKey = enter,
            ShiftKey = shift,
            ControlKey = control,
            WindowsKey = windows,
            AltKey = alt,
            Character = character,
            FunctionKey = functionKey,
            PlatformEvent = args,
            DownKey = down,
            UpKey = up,
            LeftKey = left,
            RightKey = right
        };

        _onKeyPressed?.Invoke(keyDown);

        if (keyDown.Handled)
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
