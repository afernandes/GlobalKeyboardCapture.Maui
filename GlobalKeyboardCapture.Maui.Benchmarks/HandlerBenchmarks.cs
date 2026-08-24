using BenchmarkDotNet.Attributes;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Benchmarks;

[MemoryDiagnoser]
public class HandlerBenchmarks
{
    private readonly HotkeyHandler _hotkeyHandler = new();
    private readonly BarcodeHandler _barcodeHandler = new(new KeyHandlerOptions());
    private readonly KeyEventArgs _hotkey = new()
    {
        Character = 'K',
        ControlKey = true
    };
    private readonly KeyEventArgs _scannerCharacter = new() { Character = '1' };

    public HandlerBenchmarks()
    {
        _hotkeyHandler.RegisterHotkey("Ctrl+K", static () => { });
    }

    [Benchmark]
    public void HotkeyLookup() => _hotkeyHandler.HandleKey(_hotkey);

    [Benchmark]
    public void ScannerBuffering() => _barcodeHandler.HandleKey(_scannerCharacter);
}
