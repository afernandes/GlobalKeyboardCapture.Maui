namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>
/// Groups page- or feature-specific handlers under one enable/dispose boundary.
/// </summary>
public interface IKeyboardCaptureScope : IDisposable
{
    /// <summary>Gets the optional diagnostic name of this scope.</summary>
    string? Name { get; }

    /// <summary>Gets the immutable platform and device filter shared by this scope.</summary>
    Models.KeyboardDeviceFilter? DeviceFilter { get; }

    /// <summary>Gets or sets whether handlers in this scope participate in dispatch.</summary>
    bool IsEnabled { get; set; }

    /// <summary>Registers a handler owned by this scope.</summary>
    /// <param name="handler">The handler to register.</param>
    /// <param name="priority">Dispatch priority; higher values run first.</param>
    /// <returns>An idempotent token that removes this registration.</returns>
    IDisposable RegisterHandler(IKeyHandler handler, int priority = 0);

    /// <summary>Registers a handler with additional platform and device criteria.</summary>
    /// <param name="handler">The handler to register.</param>
    /// <param name="deviceFilter">The criteria combined with the scope filter.</param>
    /// <param name="priority">Dispatch priority; higher values run first.</param>
    /// <returns>An idempotent token that removes this registration.</returns>
    IDisposable RegisterHandler(
        IKeyHandler handler,
        Models.KeyboardDeviceFilter deviceFilter,
        int priority = 0);
}
