namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>Defines how registered key sequences with strict prefix overlap are resolved.</summary>
public enum SequenceOverlapPolicy
{
    /// <summary>
    /// Executes a shorter sequence immediately and still allows a longer sequence with the
    /// same prefix to complete later. This preserves the original library behavior.
    /// </summary>
    ExecuteImmediately = 0,

    /// <summary>
    /// Defers a completed shorter sequence while a longer matching sequence remains possible.
    /// The shorter action runs when the longer branch diverges or its disambiguation timeout elapses.
    /// </summary>
    PreferLongest,

    /// <summary>Rejects registration when either sequence is a strict prefix of the other.</summary>
    RejectAmbiguous
}
