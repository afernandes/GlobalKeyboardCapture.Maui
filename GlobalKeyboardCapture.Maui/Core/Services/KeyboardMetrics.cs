using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace GlobalKeyboardCapture.Maui.Core.Services;

/// <summary>
/// Publishes the stable meter and instrument names used by opt-in keyboard metrics.
/// Enable collection with <see cref="Configuration.KeyHandlerOptions.EnableMetrics"/>.
/// </summary>
public static class KeyboardMetrics
{
    /// <summary>The <see cref="Meter"/> name exposed to metric listeners.</summary>
    public const string MeterName = "GlobalKeyboardCapture.Maui";

    /// <summary>Counter for normalized events entering the service.</summary>
    public const string EventsReceivedInstrument = "keyboard.events.received";

    /// <summary>Counter for events admitted to handler dispatch.</summary>
    public const string EventsDispatchedInstrument = "keyboard.events.dispatched";

    /// <summary>Counter for events rejected by pipeline policy.</summary>
    public const string EventsIgnoredInstrument = "keyboard.events.ignored";

    /// <summary>Counter for native auto-repeat events entering the service.</summary>
    public const string EventsRepeatedInstrument = "keyboard.events.repeated";

    /// <summary>Counter for handler invocations accepted by <c>ShouldHandle</c>.</summary>
    public const string HandlerInvocationsInstrument = "keyboard.handlers.invocations";

    /// <summary>Histogram of completed handler invocation durations in nanoseconds.</summary>
    public const string HandlerDurationInstrument = "keyboard.handlers.duration";

    /// <summary>Counter for handler or predicate failures isolated by the service.</summary>
    public const string HandlerErrorsInstrument = "keyboard.handlers.errors";
}

internal static class KeyboardMetricsRecorder
{
    private static readonly Meter Meter = new(KeyboardMetrics.MeterName);
    private static readonly Counter<long> EventsReceived =
        Meter.CreateCounter<long>(KeyboardMetrics.EventsReceivedInstrument, "{event}");
    private static readonly Counter<long> EventsDispatched =
        Meter.CreateCounter<long>(KeyboardMetrics.EventsDispatchedInstrument, "{event}");
    private static readonly Counter<long> EventsIgnored =
        Meter.CreateCounter<long>(KeyboardMetrics.EventsIgnoredInstrument, "{event}");
    private static readonly Counter<long> EventsRepeated =
        Meter.CreateCounter<long>(KeyboardMetrics.EventsRepeatedInstrument, "{event}");
    private static readonly Counter<long> HandlerInvocations =
        Meter.CreateCounter<long>(KeyboardMetrics.HandlerInvocationsInstrument, "{invocation}");
    private static readonly Histogram<long> HandlerDuration =
        Meter.CreateHistogram<long>(KeyboardMetrics.HandlerDurationInstrument, "ns");
    private static readonly Counter<long> HandlerErrors =
        Meter.CreateCounter<long>(KeyboardMetrics.HandlerErrorsInstrument, "{error}");

    public static void RecordEventReceived(bool isRepeat)
    {
        EventsReceived.Add(1);
        if (isRepeat)
            EventsRepeated.Add(1);
    }

    public static void RecordEventDispatched() => EventsDispatched.Add(1);

    public static void RecordEventIgnored() => EventsIgnored.Add(1);

    public static void RecordHandlerInvocation() => HandlerInvocations.Add(1);

    public static void RecordHandlerDuration(long startedTimestamp)
    {
        var elapsed = Stopwatch.GetElapsedTime(startedTimestamp);
        HandlerDuration.Record((long)elapsed.TotalNanoseconds);
    }

    public static void RecordHandlerError() => HandlerErrors.Add(1);
}
