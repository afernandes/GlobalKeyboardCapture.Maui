namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>
/// Groups page- or feature-specific handlers under one enable/dispose boundary.
/// </summary>
public interface IKeyboardCaptureScope : IDisposable
{
    /// <summary>Gets the optional diagnostic name of this scope.</summary>
    string? Name { get; }

    /// <summary>Gets or sets whether handlers in this scope participate in dispatch.</summary>
    bool IsEnabled { get; set; }

    /// <summary>Registers a handler owned by this scope.</summary>
    /// <param name="handler">The handler to register.</param>
    /// <param name="priority">Dispatch priority; higher values run first.</param>
    /// <returns>An idempotent token that removes this registration.</returns>
    IDisposable RegisterHandler(IKeyHandler handler, int priority = 0);
}
