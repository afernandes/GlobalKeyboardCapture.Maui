using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>
/// Registers operating-system hotkeys that can activate while the application is not focused.
/// </summary>
public interface IGlobalHotkeyService
{
    /// <summary>Gets whether the current platform implements operating-system global hotkeys.</summary>
    bool IsSupported { get; }

    /// <summary>Gets whether the service is attached to a native window.</summary>
    bool IsAttached { get; }

    /// <summary>Gets the number of registered operating-system hotkeys.</summary>
    int HotkeyCount { get; }

    /// <summary>Registers a typed global hotkey and returns an idempotent removal token.</summary>
    IDisposable RegisterHotkey(KeyGesture gesture, Action action, bool suppressRepeat = true);

    /// <summary>Registers a textual global hotkey and returns an idempotent removal token.</summary>
    IDisposable RegisterHotkey(string gesture, Action action, bool suppressRepeat = true);

    /// <summary>Removes a typed global hotkey.</summary>
    bool UnregisterHotkey(KeyGesture gesture);

    /// <summary>Removes all global hotkeys.</summary>
    void ClearHotkeys();
}
