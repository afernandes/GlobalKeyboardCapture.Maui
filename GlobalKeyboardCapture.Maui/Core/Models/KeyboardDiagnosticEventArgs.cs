namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Processing stage represented by a diagnostic keyboard event.
/// </summary>
public enum KeyboardDiagnosticStage
{
    /// <summary>The normalized event entered the handler pipeline.</summary>
    Dispatched = 0,

    /// <summary>The native or normalized event was deliberately ignored.</summary>
    Ignored = 1
}

/// <summary>
/// Structured diagnostic information for a native keyboard event.
/// </summary>
public sealed class KeyboardDiagnosticEventArgs : EventArgs
{
    /// <summary>Creates structured diagnostics for one native keyboard event.</summary>
    /// <param name="stage">The processing stage.</param>
    /// <param name="platform">The originating platform.</param>
    /// <param name="normalizedKey">The normalized key string, when available.</param>
    /// <param name="nativeKeyCode">The native logical key code.</param>
    /// <param name="nativeScanCode">The native hardware scan code.</param>
    /// <param name="nativeAction">The native action or transition.</param>
    /// <param name="nativeFlags">Formatted native flags.</param>
    /// <param name="repeatCount">The native repeat count.</param>
    /// <param name="device">Originating device metadata.</param>
    /// <param name="reason">The dispatch or rejection reason.</param>
    /// <param name="timestamp">The event timestamp, or UTC now when omitted.</param>
    public KeyboardDiagnosticEventArgs(
        KeyboardDiagnosticStage stage,
        KeyboardPlatform platform,
        string? normalizedKey,
        int nativeKeyCode = 0,
        int nativeScanCode = 0,
        string? nativeAction = null,
        string? nativeFlags = null,
        int repeatCount = 0,
        KeyboardDeviceInfo? device = null,
        string? reason = null,
        DateTimeOffset? timestamp = null)
    {
        Stage = stage;
        Platform = platform;
        NormalizedKey = normalizedKey;
        NativeKeyCode = nativeKeyCode;
        NativeScanCode = nativeScanCode;
        NativeAction = nativeAction;
        NativeFlags = nativeFlags;
        RepeatCount = repeatCount;
        Device = device;
        Reason = reason;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    /// <summary>Gets the UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Gets the processing stage.</summary>
    public KeyboardDiagnosticStage Stage { get; }

    /// <summary>Gets the originating platform.</summary>
    public KeyboardPlatform Platform { get; }

    /// <summary>Gets the normalized key string, when available.</summary>
    public string? NormalizedKey { get; }

    /// <summary>Gets the native logical key code.</summary>
    public int NativeKeyCode { get; }

    /// <summary>Gets the native hardware scan code.</summary>
    public int NativeScanCode { get; }

    /// <summary>Gets the native action or transition.</summary>
    public string? NativeAction { get; }

    /// <summary>Gets formatted native flags.</summary>
    public string? NativeFlags { get; }

    /// <summary>Gets the native repeat count.</summary>
    public int RepeatCount { get; }

    /// <summary>Gets metadata for the originating input device.</summary>
    public KeyboardDeviceInfo? Device { get; }

    /// <summary>Gets the dispatch or rejection reason.</summary>
    public string? Reason { get; }

    internal static KeyboardDiagnosticEventArgs FromKeyEvent(
        KeyEventArgs keyEvent,
        KeyboardDiagnosticStage stage = KeyboardDiagnosticStage.Dispatched,
        string? reason = null) =>
        new(
            stage,
            keyEvent.Platform,
            keyEvent.ToString(),
            keyEvent.NativeKeyCode,
            keyEvent.NativeScanCode,
            keyEvent.EventType.ToString(),
            keyEvent.NativeFlags,
            keyEvent.RepeatCount,
            keyEvent.Device,
            reason);
}
