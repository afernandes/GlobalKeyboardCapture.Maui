using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Handlers;

/// <summary>
/// Matches ordered key-gesture sequences such as Ctrl+K followed by Ctrl+C.
/// </summary>
public sealed class KeySequenceHandler : IKeyHandler, IDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);

    private readonly object _lockObject = new();
    private readonly Dictionary<string, SequenceRegistration> _sequences = [];
    private readonly TimeProvider _timeProvider;
    private long _nextRegistrationId;
    private bool _isDisposed;

    /// <summary>Creates a sequence handler using the system clock.</summary>
    public KeySequenceHandler()
        : this(TimeProvider.System)
    {
    }

    /// <summary>Creates a sequence handler using an explicit clock.</summary>
    /// <param name="timeProvider">The clock used to enforce sequence timeouts.</param>
    public KeySequenceHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>Gets the number of registered key sequences.</summary>
    public int SequenceCount
    {
        get
        {
            lock (_lockObject)
                return _sequences.Count;
        }
    }

    /// <summary>Registers a typed sequence and returns an idempotent removal token.</summary>
    public IDisposable RegisterSequence(
        IReadOnlyList<KeyGesture> sequence,
        Action action,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        ArgumentNullException.ThrowIfNull(action);
        if (sequence.Count < 2)
            throw new ArgumentException("A key sequence requires at least two gestures.", nameof(sequence));

        var effectiveTimeout = timeout ?? DefaultTimeout;
        if (effectiveTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var gestures = new KeyGesture[sequence.Count];
        for (var i = 0; i < gestures.Length; i++)
        {
            if (sequence[i].Key == KeyboardKey.None)
                throw new ArgumentException("A key sequence cannot contain an empty gesture.", nameof(sequence));
            gestures[i] = sequence[i];
        }

        var key = CreateSequenceKey(gestures);
        lock (_lockObject)
        {
            ThrowIfDisposed();
            var registration = new SequenceRegistration(
                ++_nextRegistrationId,
                gestures,
                action,
                effectiveTimeout);
            _sequences[key] = registration;
            return new SequenceRegistrationToken(this, key, registration.Id);
        }
    }

    /// <summary>Registers a textual sequence and returns an idempotent removal token.</summary>
    public IDisposable RegisterSequence(
        IReadOnlyList<string> sequence,
        Action action,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        var gestures = new KeyGesture[sequence.Count];
        for (var i = 0; i < gestures.Length; i++)
            gestures[i] = KeyGesture.Parse(sequence[i]);
        return RegisterSequence(gestures, action, timeout);
    }

    /// <summary>Removes a typed key sequence.</summary>
    public bool UnregisterSequence(IReadOnlyList<KeyGesture> sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        lock (_lockObject)
            return !_isDisposed && _sequences.Remove(CreateSequenceKey(sequence));
    }

    /// <summary>Removes every registered key sequence.</summary>
    public void ClearSequences()
    {
        lock (_lockObject)
        {
            ThrowIfDisposed();
            _sequences.Clear();
        }
    }

    /// <inheritdoc />
    public bool ShouldHandle(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.EventType != KeyboardEventType.KeyDown || key.IsRepeat)
            return false;

        lock (_lockObject)
            return !_isDisposed && _sequences.Count > 0 && KeyGesture.TryFromEvent(key, out _);
    }

    /// <inheritdoc />
    public void HandleKey(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();
        if (key.EventType != KeyboardEventType.KeyDown
            || key.IsRepeat
            || !KeyGesture.TryFromEvent(key, out var gesture))
        {
            return;
        }

        var timestamp = _timeProvider.GetTimestamp();
        List<Action>? completedActions = null;
        lock (_lockObject)
        {
            foreach (var registration in _sequences.Values)
            {
                if (registration.Progress > 0
                    && _timeProvider.GetElapsedTime(registration.LastTimestamp, timestamp) >= registration.Timeout)
                {
                    registration.Progress = 0;
                }

                if (registration.Sequence[registration.Progress] == gesture)
                {
                    registration.Progress++;
                    registration.LastTimestamp = timestamp;
                    if (registration.Progress == registration.Sequence.Length)
                    {
                        registration.Progress = 0;
                        completedActions ??= [];
                        completedActions.Add(registration.Action);
                    }
                    continue;
                }

                registration.Progress = registration.Sequence[0] == gesture ? 1 : 0;
                if (registration.Progress > 0)
                    registration.LastTimestamp = timestamp;
            }
        }

        if (completedActions is null)
            return;

        key.Handled = true;
        foreach (var action in completedActions)
            MainThread.BeginInvokeOnMainThread(() => InvokeSafely(action));
    }

    private static string CreateSequenceKey(IReadOnlyList<KeyGesture> sequence)
    {
        if (sequence.Count == 0)
            return string.Empty;

        return string.Join('\u001F', sequence);
    }

    private static void InvokeSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[GlobalKeyboardCapture] Key-sequence action threw: {exception}");
        }
    }

    private void UnregisterSequence(string key, long registrationId)
    {
        lock (_lockObject)
        {
            if (_sequences.TryGetValue(key, out var current) && current.Id == registrationId)
                _sequences.Remove(key);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    /// <summary>Clears every sequence and prevents further registration.</summary>
    public void Dispose()
    {
        lock (_lockObject)
        {
            if (_isDisposed)
                return;
            _sequences.Clear();
            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private sealed class SequenceRegistration(
        long id,
        KeyGesture[] sequence,
        Action action,
        TimeSpan timeout)
    {
        public long Id { get; } = id;
        public KeyGesture[] Sequence { get; } = sequence;
        public Action Action { get; } = action;
        public TimeSpan Timeout { get; } = timeout;
        public int Progress { get; set; }
        public long LastTimestamp { get; set; }
    }

    private sealed class SequenceRegistrationToken : IDisposable
    {
        private KeySequenceHandler? _owner;
        private readonly string _key;
        private readonly long _registrationId;

        public SequenceRegistrationToken(KeySequenceHandler owner, string key, long registrationId)
        {
            _owner = owner;
            _key = key;
            _registrationId = registrationId;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.UnregisterSequence(_key, _registrationId);
        }
    }
}
