namespace GlobalKeyboardCapture.Maui.Configuration;
public class KeyHandlerOptions
{
    public int BarcodeTimeout { get; set; } = 100;
    public int MinBarcodeLength { get; set; } = 5;

    /// <summary>
    /// Upper bound on the in-progress scan buffer. Guards against an unterminated fast
    /// key stream growing memory without bound on the input thread; when the buffer
    /// reaches this size it is reset. Generous by default so legitimate long payloads
    /// (e.g. 2D/QR codes) are not truncated.
    /// </summary>
    public int MaxBarcodeLength { get; set; } = 4096;

    /// <summary>
    /// When <c>true</c>, KeyHandlerService stops invoking subsequent handlers as
    /// soon as one sets <see cref="Core.Models.KeyEventArgs.Handled"/> to
    /// <c>true</c>. Defaults to <c>false</c> to preserve the historical
    /// fan-out behavior where every registered handler sees every event
    /// regardless of consumption.
    /// </summary>
    public bool StopOnHandled { get; set; }

    /// <summary>
    /// Enables structured native/normalized keyboard diagnostics. Disabled by default
    /// so the input hot path performs no diagnostic string formatting.
    /// </summary>
    public bool EnableDiagnostics { get; set; }
}
