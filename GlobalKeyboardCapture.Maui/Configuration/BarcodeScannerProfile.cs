using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Configuration;

/// <summary>
/// Describes how a keyboard-wedge barcode scanner frames and delivers a scan.
/// </summary>
public sealed class BarcodeScannerProfile
{
    /// <summary>
    /// Creates a scanner profile with <see cref="KeyboardKey.Enter"/> as its terminator.
    /// </summary>
    public BarcodeScannerProfile(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        TerminatorKeys.Add(KeyboardKey.Enter);
    }

    /// <summary>Gets the profile name reported with completed scans.</summary>
    public string Name { get; }

    /// <summary>Gets or sets the maximum delay allowed between consecutive characters.</summary>
    public TimeSpan InterCharacterTimeout { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Gets or sets the minimum decoded payload length.</summary>
    public int MinLength { get; set; } = 5;

    /// <summary>Gets or sets the maximum raw scan length.</summary>
    public int MaxLength { get; set; } = 4096;

    /// <summary>Gets the logical keys that terminate a scan.</summary>
    public ISet<KeyboardKey> TerminatorKeys { get; } = new HashSet<KeyboardKey>();

    /// <summary>Gets the control or printable characters that terminate a scan.</summary>
    public ISet<char> TerminatorCharacters { get; } = new HashSet<char>();

    /// <summary>Gets or sets an optional framing prefix.</summary>
    public string? Prefix { get; set; }

    /// <summary>Gets or sets an optional framing suffix.</summary>
    public string? Suffix { get; set; }

    /// <summary>Gets or sets whether input without <see cref="Prefix"/> is rejected.</summary>
    public bool RequirePrefix { get; set; }

    /// <summary>Gets or sets whether input without <see cref="Suffix"/> is rejected.</summary>
    public bool RequireSuffix { get; set; }

    /// <summary>Gets or sets whether a matched prefix is removed from the decoded value.</summary>
    public bool StripPrefix { get; set; } = true;

    /// <summary>Gets or sets whether a matched suffix is removed from the decoded value.</summary>
    public bool StripSuffix { get; set; } = true;

    /// <summary>Gets or sets whether surrounding whitespace is removed before validation.</summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum average delay between characters. When set, slower
    /// streams are rejected as likely human typing.
    /// </summary>
    public TimeSpan? MaxAverageInterCharacterDelay { get; set; }

    /// <summary>Gets or sets an optional native input-device identifier filter.</summary>
    public int? DeviceId { get; set; }
}
