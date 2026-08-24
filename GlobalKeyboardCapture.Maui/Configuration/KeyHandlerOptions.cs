namespace GlobalKeyboardCapture.Maui.Configuration;
/// <summary>
/// Configures keyboard dispatch, diagnostics, repeat handling, and barcode scanning.
/// </summary>
public class KeyHandlerOptions
{
    /// <summary>
    /// Gets explicitly configured keyboard-wedge scanner profiles. When empty, the
    /// legacy barcode timeout and length properties define a compatible default profile.
    /// </summary>
    public IList<BarcodeScannerProfile> BarcodeProfiles { get; } = new List<BarcodeScannerProfile>();

    /// <summary>
    /// Gets or sets the legacy default scanner timeout in milliseconds.
    /// Ignored when <see cref="BarcodeProfiles"/> contains profiles.
    /// </summary>
    public int BarcodeTimeout { get; set; } = 100;

    /// <summary>
    /// Gets or sets the legacy default minimum barcode length.
    /// Ignored when <see cref="BarcodeProfiles"/> contains profiles.
    /// </summary>
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
    /// Enables key-release events. Disabled by default to preserve the historical
    /// key-down-only dispatch contract.
    /// </summary>
    public bool CaptureKeyUp { get; set; }

    /// <summary>
    /// Enables native auto-repeat key-down events. Disabled by default so held hotkeys
    /// and scanner input do not fire repeatedly.
    /// </summary>
    public bool AllowKeyRepeat { get; set; }

    /// <summary>
    /// Enables structured native/normalized keyboard diagnostics. Disabled by default
    /// so the input hot path performs no diagnostic string formatting.
    /// </summary>
    public bool EnableDiagnostics { get; set; }
}
