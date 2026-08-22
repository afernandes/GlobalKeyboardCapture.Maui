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
    private readonly TimeProvider _timeProvider;
    private long _lastKeyTimestamp;
    private bool _isOverflowed;
    private bool _isDisposed;

    public event EventHandler<string>? BarcodeScanned;

    public BarcodeHandler(KeyHandlerOptions options, TimeProvider? timeProvider = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions(_options);
        _buffer = new StringBuilder(DEFAULT_BUFFER_CAPACITY);
        _timeProvider = timeProvider ?? TimeProvider.System;
        _lastKeyTimestamp = _timeProvider.GetTimestamp();
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

        var nowTimestamp = _timeProvider.GetTimestamp();
        var elapsedMs = _timeProvider.GetElapsedTime(_lastKeyTimestamp, nowTimestamp).TotalMilliseconds;

        if (elapsedMs >= _options.BarcodeTimeout)
        {
            _buffer.Clear();
            _isOverflowed = false;
        }

        if (key.Character != null)
        {
            if (!_isOverflowed && _buffer.Length >= _options.MaxBarcodeLength)
            {
                _buffer.Clear();
                _isOverflowed = true;
            }

            if (!_isOverflowed)
                _buffer.Append(key.Character);
        }

        _lastKeyTimestamp = nowTimestamp;

        if (key.EnterKey && ProcessBuffer())
            key.Handled = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ProcessBuffer()
    {
        if (_isOverflowed)
        {
            _buffer.Clear();
            _isOverflowed = false;
            return false;
        }

        // Trim BEFORE validating: a whitespace-padded buffer could otherwise pass the
        // length check yet deliver a payload shorter than MinBarcodeLength (even empty).
        var barcode = _buffer.ToString().Trim();
        if (barcode.Length < _options.MinBarcodeLength)
        {
            _buffer.Clear();
            return false;
        }

        OnBarcodeScanned(barcode);
        return true;
    }

    private void OnBarcodeScanned(string barcode)
    {
        try
        {
            BarcodeScanned?.Invoke(this, barcode);
        }
        finally
        {
            // Always clear, even if a subscriber throws — otherwise the next scan
            // would be concatenated to the failed one.
            _buffer.Clear();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(BarcodeHandler));
        }
    }

    private static void ValidateOptions(KeyHandlerOptions options)
    {
        if (options.BarcodeTimeout <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.BarcodeTimeout), "Barcode timeout must be greater than zero.");
        if (options.MinBarcodeLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MinBarcodeLength), "Minimum barcode length must be greater than zero.");
        if (options.MaxBarcodeLength < options.MinBarcodeLength)
            throw new ArgumentOutOfRangeException(nameof(options.MaxBarcodeLength), "Maximum barcode length must be greater than or equal to the minimum length.");
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _buffer.Clear();
        _isOverflowed = false;
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
