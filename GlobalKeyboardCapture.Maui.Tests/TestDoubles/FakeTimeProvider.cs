using System.Diagnostics;

namespace GlobalKeyboardCapture.Maui.Tests.TestDoubles;

internal sealed class FakeTimeProvider : TimeProvider
{
    private readonly object _lockObject = new();
    private readonly List<FakeTimer> _timers = [];
    private long _timestamp;

    public override long GetTimestamp() => _timestamp;

    public override long TimestampFrequency => Stopwatch.Frequency;

    public override ITimer CreateTimer(
        TimerCallback callback,
        object? state,
        TimeSpan dueTime,
        TimeSpan period)
    {
        ArgumentNullException.ThrowIfNull(callback);
        var timer = new FakeTimer(this, callback, state, dueTime, period);
        lock (_lockObject)
            _timers.Add(timer);
        return timer;
    }

    public void Advance(TimeSpan delta)
    {
        List<(TimerCallback Callback, object? State)> callbacks = [];
        lock (_lockObject)
        {
            _timestamp += (long)(delta.TotalSeconds * TimestampFrequency);
            foreach (var timer in _timers.ToArray())
                timer.CollectDueCallbacks(_timestamp, callbacks);
        }

        foreach (var callback in callbacks)
            callback.Callback(callback.State);
    }

    private void Remove(FakeTimer timer)
    {
        lock (_lockObject)
            _timers.Remove(timer);
    }

    private sealed class FakeTimer : ITimer
    {
        private readonly FakeTimeProvider _owner;
        private readonly TimerCallback _callback;
        private readonly object? _state;
        private long _dueTimestamp;
        private TimeSpan _period;
        private bool _isDisposed;

        public FakeTimer(
            FakeTimeProvider owner,
            TimerCallback callback,
            object? state,
            TimeSpan dueTime,
            TimeSpan period)
        {
            _owner = owner;
            _callback = callback;
            _state = state;
            Change(dueTime, period);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if (_isDisposed)
                return false;
            _period = period;
            _dueTimestamp = dueTime == Timeout.InfiniteTimeSpan
                ? long.MaxValue
                : _owner._timestamp + ToTimestamp(dueTime);
            return true;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            _owner.Remove(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public void CollectDueCallbacks(
            long timestamp,
            List<(TimerCallback Callback, object? State)> callbacks)
        {
            if (_isDisposed || timestamp < _dueTimestamp)
                return;

            callbacks.Add((_callback, _state));
            if (_period == Timeout.InfiniteTimeSpan)
            {
                _dueTimestamp = long.MaxValue;
                return;
            }

            var periodTicks = ToTimestamp(_period);
            do
            {
                _dueTimestamp += periodTicks;
            }
            while (_dueTimestamp <= timestamp);
        }

        private long ToTimestamp(TimeSpan duration) =>
            (long)(duration.TotalSeconds * _owner.TimestampFrequency);
    }
}
