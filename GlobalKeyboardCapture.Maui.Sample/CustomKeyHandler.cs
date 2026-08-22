using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Sample;

/// <summary>
/// Sample handler that exposes every captured key to the diagnostics page.
/// </summary>
public sealed class KeyDisplayHandler : IKeyHandler
{
    /// <summary>
    /// Raised when a key is captured.
    /// </summary>
    public event EventHandler<KeyEventArgs>? KeyPressed;

    public bool ShouldHandle(KeyEventArgs key) => true;

    public void HandleKey(KeyEventArgs key)
    {
#if ANDROID
        Android.Util.Log.Info(
            "GKC.Integration",
            $"Key={key};NativeKeyCode={key.NativeKeyCode};Location={key.Location};EventType={key.EventType}");
#endif
        KeyPressed?.Invoke(this, key.CreateSnapshot());
    }
}

