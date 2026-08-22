namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Processing stage represented by a diagnostic keyboard event.
/// </summary>
public enum KeyboardDiagnosticStage
{
    Dispatched = 0,
    Ignored = 1
}

/// <summary>
/// Structured diagnostic information for a native keyboard event.
/// </summary>
public sealed class KeyboardDiagnosticEventArgs : EventArgs
{
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

    public DateTimeOffset Timestamp { get; }
    public KeyboardDiagnosticStage Stage { get; }
    public KeyboardPlatform Platform { get; }
    public string? NormalizedKey { get; }
    public int NativeKeyCode { get; }
    public int NativeScanCode { get; }
    public string? NativeAction { get; }
    public string? NativeFlags { get; }
    public int RepeatCount { get; }
    public KeyboardDeviceInfo? Device { get; }
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
