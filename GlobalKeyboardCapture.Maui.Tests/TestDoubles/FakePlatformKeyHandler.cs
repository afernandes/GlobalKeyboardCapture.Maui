using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Tests.TestDoubles;

internal sealed class FakePlatformKeyHandler : IPlatformKeyHandler
{
    private Action<KeyEventArgs>? _callback;

    public bool Initialized { get; private set; }
    public bool CleanedUp { get; private set; }
    public int InitializeCallCount { get; private set; }

    public void Initialize(object platformView)
    {
        Initialized = true;
        InitializeCallCount++;
    }

    public void ConfigureHandler(Action<KeyEventArgs> onKeyPressed)
    {
        _callback = onKeyPressed;
    }

    public void Cleanup()
    {
        CleanedUp = true;
    }

    public void Dispatch(KeyEventArgs key) => _callback?.Invoke(key);
}
