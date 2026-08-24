using BenchmarkDotNet.Attributes;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Benchmarks;

[MemoryDiagnoser]
public class DispatchBenchmarks
{
    private readonly BenchmarkPlatformKeyHandler _disabledPlatform = new();
    private readonly BenchmarkPlatformKeyHandler _enabledPlatform = new();
    private readonly KeyHandlerService _metricsDisabled;
    private readonly KeyHandlerService _metricsEnabled;
    private readonly KeyEventArgs _event = new() { Character = 'A' };

    public DispatchBenchmarks()
    {
        _metricsDisabled = CreateService(_disabledPlatform, enableMetrics: false);
        _metricsEnabled = CreateService(_enabledPlatform, enableMetrics: true);
    }

    [Benchmark(Baseline = true)]
    public void MetricsDisabled() => _disabledPlatform.Dispatch(_event);

    [Benchmark]
    public void MetricsEnabled() => _enabledPlatform.Dispatch(_event);

    private static KeyHandlerService CreateService(
        BenchmarkPlatformKeyHandler platform,
        bool enableMetrics)
    {
        var service = new KeyHandlerService(
            platform,
            NullLogger<KeyHandlerService>.Instance,
            new KeyHandlerOptions { EnableMetrics = enableMetrics });
        service.RegisterHandler(new NoOpHandler());
        return service;
    }

    private sealed class NoOpHandler : IKeyHandler
    {
        public bool ShouldHandle(KeyEventArgs key) => true;

        public void HandleKey(KeyEventArgs key)
        {
        }
    }
}

internal sealed class BenchmarkPlatformKeyHandler : IPlatformKeyHandler
{
    private Action<KeyEventArgs>? _callback;

    public bool SupportsMultiplePlatformViews => true;

    public void Attach(object platformView)
    {
    }

    public bool Detach(object platformView) => true;

    public void ConfigureHandler(Action<KeyEventArgs> onKeyPressed) => _callback = onKeyPressed;

    public void Cleanup()
    {
    }

    public void Dispatch(KeyEventArgs key) => _callback!(key);
}
