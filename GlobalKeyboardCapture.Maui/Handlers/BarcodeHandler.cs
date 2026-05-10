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
    private bool _isDisposed;

    public event EventHandler<string>? BarcodeScanned;

    public BarcodeHandler(KeyHandlerOptions options, TimeProvider? timeProvider = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
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
            _buffer.Clear();

        if (key.Character != null)
            _buffer.Append(key.Character);

        _lastKeyTimestamp = nowTimestamp;

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
