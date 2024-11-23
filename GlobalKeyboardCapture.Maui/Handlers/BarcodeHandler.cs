using System.Runtime.CompilerServices;
using System.Text;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Handlers;

public sealed class BarcodeHandler : IKeyHandler, IDisposable
{
    private const int DEFAULT_BUFFER_CAPACITY = 50;

    private readonly KeyHandlerOptions _options;
    private readonly StringBuilder _buffer;
    private DateTime _lastKeyPressed;
    private bool _isDisposed;

    public event EventHandler<string>? BarcodeScanned;

    public BarcodeHandler(KeyHandlerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _buffer = new StringBuilder(DEFAULT_BUFFER_CAPACITY);
        _lastKeyPressed = DateTime.Now;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldHandle(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key.NoSpecialKeysPressed && (key.Character != null || key.EnterKey);
    }

    public void HandleKey(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();

        var now = DateTime.Now;
        var timeSinceLastKey = (now - _lastKeyPressed).TotalMilliseconds;

        if (timeSinceLastKey >= _options.BarcodeTimeout)
            _buffer.Clear();

        if (key.Character != null)
            _buffer.Append(key.Character);

        _lastKeyPressed = now;

        if (key.EnterKey && ProcessBuffer())
            key.Handled = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ProcessBuffer()
    {
        if (_buffer.Length < _options.MinBarcodeLength)
        {
            _buffer.Clear();
            return false;
        }

        OnBarcodeScanned(_buffer.ToString().Trim());
        return true;
    }

    private void OnBarcodeScanned(string barcode)
    {
        BarcodeScanned?.Invoke(this, barcode);
        _buffer.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(BarcodeHandler));
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _buffer.Clear();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
