namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>
/// Coordinates platform-view attachment leases with native application lifecycle events.
/// </summary>
public interface ILifecycleHandler : IDisposable
{
    /// <summary>Attaches keyboard capture to a newly created native platform view.</summary>
    void OnPlatformViewCreated(object platformView);

    /// <summary>Releases keyboard capture from a destroyed native platform view.</summary>
    void OnPlatformViewDestroyed(object platformView);
}
