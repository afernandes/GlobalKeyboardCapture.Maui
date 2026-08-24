using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeyboardMetricsTests
{
    [Fact]
    public void MetricsAreDisabledByDefault()
    {
        new KeyHandlerOptions().EnableMetrics.Should().BeFalse();
    }

    [Fact]
    public void EnabledMetricsRecordPipelineAndHandlerActivity()
    {
        var measurements = new ConcurrentDictionary<string, long>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, currentListener) =>
        {
            if (instrument.Meter.Name == KeyboardMetrics.MeterName)
                currentListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            measurements.AddOrUpdate(instrument.Name, measurement, (_, current) => current + measurement));
        listener.Start();

        var platform = new FakePlatformKeyHandler();
        using var service = new KeyHandlerService(
            platform,
            NullLogger<KeyHandlerService>.Instance,
            new KeyHandlerOptions { EnableMetrics = true });
        service.RegisterHandler(new RecordingKeyHandler());

        platform.Dispatch(new KeyEventArgs { Character = 'A' });

        measurements[KeyboardMetrics.EventsReceivedInstrument].Should().Be(1);
        measurements[KeyboardMetrics.EventsDispatchedInstrument].Should().Be(1);
        measurements[KeyboardMetrics.HandlerInvocationsInstrument].Should().Be(1);
        measurements.Should().ContainKey(KeyboardMetrics.HandlerDurationInstrument);
    }

    [Fact]
    public void EnabledMetricsRecordIgnoredAndRepeatEvents()
    {
        var measurements = new ConcurrentDictionary<string, long>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, currentListener) =>
        {
            if (instrument.Meter.Name == KeyboardMetrics.MeterName)
                currentListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            measurements.AddOrUpdate(instrument.Name, measurement, (_, current) => current + measurement));
        listener.Start();

        var platform = new FakePlatformKeyHandler();
        using var service = new KeyHandlerService(
            platform,
            NullLogger<KeyHandlerService>.Instance,
            new KeyHandlerOptions { EnableMetrics = true });

        platform.Dispatch(new KeyEventArgs { Character = 'A', RepeatCount = 1 });

        measurements[KeyboardMetrics.EventsReceivedInstrument].Should().Be(1);
        measurements[KeyboardMetrics.EventsRepeatedInstrument].Should().Be(1);
        measurements[KeyboardMetrics.EventsIgnoredInstrument].Should().Be(1);
        measurements.Should().NotContainKey(KeyboardMetrics.EventsDispatchedInstrument);
    }
}
