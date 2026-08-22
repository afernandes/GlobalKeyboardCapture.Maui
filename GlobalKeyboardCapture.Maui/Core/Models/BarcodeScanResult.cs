namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Contains the decoded value and capture metadata for a completed barcode scan.
/// </summary>
public sealed record BarcodeScanResult
{
    /// <summary>Gets the decoded value after configured transformations.</summary>
    public required string Value { get; init; }

    /// <summary>Gets the raw characters captured before transformations.</summary>
    public required string RawValue { get; init; }

    /// <summary>Gets the scanner profile that accepted the scan.</summary>
    public required string ProfileName { get; init; }

    /// <summary>Gets the elapsed time from the first to the last captured character.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Gets the number of captured characters before transformations.</summary>
    public int CharacterCount { get; init; }

    /// <summary>Gets metadata for the device that produced the scan, when available.</summary>
    public KeyboardDeviceInfo? Device { get; init; }

    /// <summary>Gets the logical key used to terminate the scan, when applicable.</summary>
    public KeyboardKey TerminatorKey { get; init; }

    /// <summary>Gets the character used to terminate the scan, when applicable.</summary>
    public char? TerminatorCharacter { get; init; }

    /// <summary>Gets whether the configured prefix was removed.</summary>
    public bool PrefixRemoved { get; init; }

    /// <summary>Gets whether the configured suffix was removed.</summary>
    public bool SuffixRemoved { get; init; }
}
