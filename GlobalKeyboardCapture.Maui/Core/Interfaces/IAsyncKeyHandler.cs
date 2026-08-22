using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>
/// Handles a keyboard-event snapshot asynchronously without blocking the native input thread.
/// </summary>
/// <remarks>
/// Asynchronous handlers are fire-and-observe. Changes to <see cref="KeyEventArgs.Handled"/>
/// cannot affect native propagation because dispatch has already returned.
/// </remarks>
public interface IAsyncKeyHandler : IKeyHandler
{
    /// <summary>Processes an event snapshot with cancellation tied to the handler service.</summary>
    ValueTask HandleKeyAsync(KeyEventArgs key, CancellationToken cancellationToken);

    void IKeyHandler.HandleKey(KeyEventArgs key)
    {
    }
}
