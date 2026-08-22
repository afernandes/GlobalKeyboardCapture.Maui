using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Tests.TestDoubles;

internal sealed class FakePlatformKeyHandler(bool supportsMultiplePlatformViews = true) : IPlatformKeyHandler
{
    private Action<KeyEventArgs>? _callback;

    public bool SupportsMultiplePlatformViews { get; } = supportsMultiplePlatformViews;
    public bool Initialized => AttachedViews.Count > 0;
    public bool CleanedUp { get; private set; }
    public int InitializeCallCount { get; private set; }
    public int DetachCallCount { get; private set; }
    public List<object> AttachedViews { get; } = [];
    public List<object> DetachedViews { get; } = [];

    public void Attach(object platformView)
    {
        InitializeCallCount++;
        if (!AttachedViews.Any(view => ReferenceEquals(view, platformView)))
            AttachedViews.Add(platformView);
    }

    public bool Detach(object platformView)
    {
        var index = AttachedViews.FindIndex(view => ReferenceEquals(view, platformView));
        if (index < 0)
            return false;

        AttachedViews.RemoveAt(index);
        DetachedViews.Add(platformView);
        DetachCallCount++;
        return true;
    }

    public void ConfigureHandler(Action<KeyEventArgs> onKeyPressed)
    {
        _callback = onKeyPressed;
    }

    public void Cleanup()
    {
        CleanedUp = true;
        AttachedViews.Clear();
    }

    public void Dispatch(KeyEventArgs key) => _callback?.Invoke(key);
}
