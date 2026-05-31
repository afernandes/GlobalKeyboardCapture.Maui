namespace GlobalKeyboardCapture.Maui.Configuration;
public class KeyHandlerOptions
{
    public int BarcodeTimeout { get; set; } = 100;
    public int MinBarcodeLength { get; set; } = 5;

    /// <summary>
    /// When <c>true</c>, KeyHandlerService stops invoking subsequent handlers as
    /// soon as one sets <see cref="Core.Models.KeyEventArgs.Handled"/> to
    /// <c>true</c>. Defaults to <c>false</c> to preserve the historical
    /// fan-out behavior where every registered handler sees every event
    /// regardless of consumption.
    /// </summary>
    public bool StopOnHandled { get; set; }
}