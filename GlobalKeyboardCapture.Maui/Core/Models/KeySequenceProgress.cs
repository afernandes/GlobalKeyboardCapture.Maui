namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>Represents a detached, immutable snapshot of one registered sequence's progress.</summary>
public sealed record KeySequenceProgress
{
    /// <summary>Creates a detached sequence-progress snapshot.</summary>
    /// <param name="sequence">The canonical comma-separated sequence.</param>
    /// <param name="matchedGestureCount">The number of gestures currently matched.</param>
    /// <param name="gestureCount">The total number of gestures in the sequence.</param>
    /// <param name="isCompletionPending">Whether a completed prefix awaits disambiguation.</param>
    /// <param name="timeout">The configured sequence timeout.</param>
    public KeySequenceProgress(
        string sequence,
        int matchedGestureCount,
        int gestureCount,
        bool isCompletionPending,
        TimeSpan timeout)
    {
        Sequence = sequence;
        MatchedGestureCount = matchedGestureCount;
        GestureCount = gestureCount;
        IsCompletionPending = isCompletionPending;
        Timeout = timeout;
    }

    /// <summary>Gets the canonical comma-separated sequence.</summary>
    public string Sequence { get; }

    /// <summary>Gets the number of gestures currently matched.</summary>
    public int MatchedGestureCount { get; }

    /// <summary>Gets the total number of gestures in the sequence.</summary>
    public int GestureCount { get; }

    /// <summary>Gets whether a completed prefix awaits disambiguation.</summary>
    public bool IsCompletionPending { get; }

    /// <summary>Gets the configured sequence timeout.</summary>
    public TimeSpan Timeout { get; }
}
