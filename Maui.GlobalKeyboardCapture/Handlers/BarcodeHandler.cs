using Maui.GlobalKeyboardCapture.Configuration;
using Maui.GlobalKeyboardCapture.Core.Interfaces;
using System.Text;

namespace Maui.GlobalKeyboardCapture.Handlers;

public class BarcodeHandler : IKeyHandler, IDisposable
{
    private readonly KeyHandlerOptions _options;
    private readonly StringBuilder _buffer = new();
    private DateTime? lastKeyPressed = null;

    public event EventHandler<string> BarcodeScanned;

    public BarcodeHandler(KeyHandlerOptions options)
    {
        _options = options;
    }

    public bool ShouldHandle(Core.Models.KeyEventArgs key)
    {
        return key.NoSpecialKeysPressed && (key.Character != null || key.EnterKey);
    }

    public void HandleKey(Core.Models.KeyEventArgs key)
    {
        var time = (DateTime.Now - lastKeyPressed)?.TotalMilliseconds ?? 0;

        if (time < _options.BarcodeTimeout)
            _buffer.Append(key.Character);
        else
        {
            _buffer.Clear();
            _buffer.Append(key.Character);
        }

        lastKeyPressed = DateTime.Now;

        if (key.EnterKey)
            if (ProcessBuffer())
                key.Handled = true;
    }

    private bool ProcessBuffer()
    {
        var barcode = _buffer.ToString().Trim();
        if (barcode.Length >= _options.MinBarcodeLength)
        {
            BarcodeScanned?.Invoke(this, barcode);
            return true;
        }
        _buffer.Clear();

        return false;
    }

    public void Dispose()
    {
        _buffer.Clear();
    }
}