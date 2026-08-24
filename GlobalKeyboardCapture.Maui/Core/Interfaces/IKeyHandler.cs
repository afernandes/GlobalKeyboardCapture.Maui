namespace GlobalKeyboardCapture.Maui.Core.Interfaces;
/// <summary>Defines a synchronous consumer in the keyboard dispatch pipeline.</summary>
public interface IKeyHandler
{
    /// <summary>Processes a keyboard event selected by <see cref="ShouldHandle"/>.</summary>
    /// <param name="key">The normalized keyboard event.</param>
    void HandleKey(Models.KeyEventArgs key);

    /// <summary>Determines whether this handler should receive a keyboard event.</summary>
    /// <param name="key">The normalized keyboard event.</param>
    /// <returns><see langword="true"/> when <see cref="HandleKey"/> should be called.</returns>
    bool ShouldHandle(Models.KeyEventArgs key);
}
