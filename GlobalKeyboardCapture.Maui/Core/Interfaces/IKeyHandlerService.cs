namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>Coordinates native keyboard sources and the ordered handler pipeline.</summary>
public interface IKeyHandlerService
{
    /// <summary>Occurs when diagnostics are enabled and a native event is dispatched or ignored.</summary>
    event EventHandler<Models.KeyboardDiagnosticEventArgs>? DiagnosticEvent;

    /// <summary>Gets whether at least one native platform view is attached.</summary>
    bool IsInitialized { get; }

    /// <summary>Gets whether capture is initialized and has no active suspension tokens.</summary>
    bool IsCapturing { get; }

    /// <summary>Gets the number of attached native platform views.</summary>
    int PlatformViewCount { get; }

    /// <summary>Gets the number of registered handlers.</summary>
    int HandlerCount { get; }

    /// <summary>Gets an ordered snapshot of registered handlers.</summary>
    IReadOnlyList<IKeyHandler> Handlers { get; }

    /// <summary>Permanently attaches a native platform view for compatibility with 1.x consumers.</summary>
    /// <param name="platformView">The native activity, application, or window.</param>
    void Initialize(object platformView);

    /// <summary>Attaches a native platform view with reference-counted lifetime.</summary>
    /// <param name="platformView">The native activity, application, or window.</param>
    /// <returns>An idempotent token that releases the attachment.</returns>
    IDisposable AttachPlatformView(object platformView);

    /// <summary>Detaches a native platform view regardless of outstanding leases.</summary>
    /// <param name="platformView">The platform view to detach.</param>
    /// <returns><see langword="true"/> when an attachment was removed.</returns>
    bool DetachPlatformView(object platformView);

    /// <summary>Registers a handler in deterministic priority and registration order.</summary>
    /// <param name="handler">The handler to register.</param>
    /// <param name="priority">Dispatch priority; higher values run first.</param>
    /// <returns>An idempotent token that removes exactly this registration.</returns>
    IDisposable RegisterHandler(IKeyHandler handler, int priority = 0);

    /// <summary>Registers a handler for events that satisfy an immutable device filter.</summary>
    /// <param name="handler">The handler to register.</param>
    /// <param name="deviceFilter">The platform and device criteria applied before dispatch.</param>
    /// <param name="priority">Dispatch priority; higher values run first.</param>
    /// <returns>An idempotent token that removes exactly this registration.</returns>
    IDisposable RegisterHandler(
        IKeyHandler handler,
        Models.KeyboardDeviceFilter deviceFilter,
        int priority = 0);

    /// <summary>Removes all registrations for a handler instance.</summary>
    /// <param name="handler">The handler instance to remove.</param>
    /// <returns><see langword="true"/> when at least one registration was removed.</returns>
    bool UnregisterHandler(IKeyHandler handler);

    /// <summary>Suspends dispatch until the returned token is disposed.</summary>
    /// <returns>An idempotent capture-suspension token.</returns>
    IDisposable SuspendCapture();

    /// <summary>Clears all active suspensions created by <see cref="SuspendCapture"/>.</summary>
    void ResumeCapture();

    /// <summary>Creates a handler group with a shared enable and disposal boundary.</summary>
    /// <param name="name">An optional diagnostic scope name.</param>
    /// <param name="isEnabled">Whether the scope starts enabled.</param>
    /// <returns>The new capture scope.</returns>
    IKeyboardCaptureScope CreateScope(string? name = null, bool isEnabled = true);

    /// <summary>Creates a handler group restricted by an immutable device filter.</summary>
    /// <param name="deviceFilter">The platform and device criteria shared by the scope.</param>
    /// <param name="name">An optional diagnostic scope name.</param>
    /// <param name="isEnabled">Whether the scope starts enabled.</param>
    /// <returns>The new capture scope.</returns>
    IKeyboardCaptureScope CreateScope(
        Models.KeyboardDeviceFilter deviceFilter,
        string? name = null,
        bool isEnabled = true);
}
