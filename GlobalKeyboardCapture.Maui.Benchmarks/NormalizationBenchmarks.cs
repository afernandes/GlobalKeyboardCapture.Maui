using BenchmarkDotNet.Attributes;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Benchmarks;

[MemoryDiagnoser]
public class NormalizationBenchmarks
{
    private readonly KeyEventArgs _event = new()
    {
        Key = KeyboardKey.F12,
        Modifiers = KeyModifiers.Control | KeyModifiers.Shift
    };

    [Benchmark]
    public string CanonicalEventString() => _event.ToString();

    [Benchmark]
    public KeyGesture ParseCanonicalGesture() => KeyGesture.Parse("Ctrl+Shift+F12");
}
