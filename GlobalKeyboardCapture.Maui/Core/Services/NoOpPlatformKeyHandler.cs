using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Core.Services;

internal sealed class NoOpPlatformKeyHandler : IPlatformKeyHandler
{
    public bool SupportsMultiplePlatformViews => true;

    public void Attach(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);
    }

    public bool Detach(object platformView) => platformView is not null;

    public void ConfigureHandler(Action<KeyEventArgs> onKeyPressed)
    {
        ArgumentNullException.ThrowIfNull(onKeyPressed);
    }

    public void Cleanup()
    {
    }
}
