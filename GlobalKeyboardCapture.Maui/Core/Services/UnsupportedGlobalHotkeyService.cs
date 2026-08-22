using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Core.Services;

internal sealed class UnsupportedGlobalHotkeyService : IGlobalHotkeyService
{
    public bool IsSupported => false;
    public bool IsAttached => false;
    public int HotkeyCount => 0;

    public IDisposable RegisterHotkey(KeyGesture gesture, Action action, bool suppressRepeat = true)
    {
        ArgumentNullException.ThrowIfNull(action);
        throw new PlatformNotSupportedException(
            "Operating-system global hotkeys are currently supported only on Windows.");
    }

    public IDisposable RegisterHotkey(string gesture, Action action, bool suppressRepeat = true)
    {
        ArgumentNullException.ThrowIfNull(action);
        return RegisterHotkey(KeyGesture.Parse(gesture), action, suppressRepeat);
    }

    public bool UnregisterHotkey(KeyGesture gesture) => false;

    public void ClearHotkeys()
    {
    }
}
