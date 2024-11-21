using System.Globalization;
using GlobalKeyboardCapture.Maui.Core.Interfaces;

namespace GlobalKeyboardCapture.Maui.Handlers;
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

        if(!CheckFunction(key, hotKey) 
           && !CheckEsc(key, hotKey))
            hotKey.Character = key[0];

        RegisterHotkey(hotKey,action);
    }

    public void RegisterHotkey(Core.Models.KeyEventArgs hotKey, Action action)
    {
        RegisterHotkey(hotKey.ToString(), action);
    }

    public void RegisterHotkey(string hotKey, Action action)
    {
        _hotkeyActions[hotKey] = action;
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

    private bool CheckFunction(string key, Core.Models.KeyEventArgs hotKey)
    {
        if (key.Length == 2 && key[0] == 'F' && char.IsNumber(key[1]))
        {
            hotKey.FunctionKey = key;
            return true;
        }

        return false;
    }

    private bool CheckEsc(string key, Core.Models.KeyEventArgs hotKey)
    {
        if (key.Equals("ESC", StringComparison.OrdinalIgnoreCase)
            || key.Equals("Escape", StringComparison.OrdinalIgnoreCase))
        {
            hotKey.EscapeKey = true;
            return true;
        }

        return false;
    }
}
