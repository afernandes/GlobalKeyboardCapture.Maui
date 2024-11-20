using Maui.GlobalKeyboardCapture.Core.Interfaces;

namespace Maui.GlobalKeyboardCapture.Handlers;
public class HotkeyHandler : IKeyHandler
{
    private readonly Dictionary<string, Action> _hotkeyActions = new();
    public void RegisterHotkey(string key, bool requireControl, bool requireAlt, bool requireShift, Action action)
    {
        var hotKey = new Core.Models.KeyEventArgs
        {
            ControlKey = requireControl,
            AltKey = requireAlt,
            ShiftKey = requireShift
        };

        if (key.Length == 2 && key[0] == 'F' && char.IsNumber(key[1]))
            hotKey.FunctionKey = key;
        else
            hotKey.Character = key[0];

        RegisterHotkey(hotKey,action);
    }

    public void RegisterHotkey(Core.Models.KeyEventArgs hotKey, Action action)
    {
        _hotkeyActions[hotKey.ToString()] = action;
    }

    public bool ShouldHandle(Core.Models.KeyEventArgs key) => true;

    public void HandleKey(Core.Models.KeyEventArgs key)
    {
        if (_hotkeyActions.TryGetValue(key.ToString(), out var action))
        {
            MainThread.BeginInvokeOnMainThread(action);
            key.Handled = true;
        }
    }
}