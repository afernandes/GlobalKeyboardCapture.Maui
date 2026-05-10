using System.Diagnostics;

namespace GlobalKeyboardCapture.Maui.Tests.TestDoubles;

internal sealed class FakeTimeProvider : TimeProvider
{
    private long _timestamp;

    public override long GetTimestamp() => _timestamp;

    public override long TimestampFrequency => Stopwatch.Frequency;

    public void Advance(TimeSpan delta)
    {
        _timestamp += (long)(delta.TotalSeconds * TimestampFrequency);
    }
}
