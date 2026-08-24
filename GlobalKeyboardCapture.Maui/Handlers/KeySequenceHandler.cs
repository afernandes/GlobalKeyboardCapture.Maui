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
    private readonly SequenceOverlapPolicy _overlapPolicy;
    private long _nextRegistrationId;
    private bool _isDisposed;

    /// <summary>Creates a sequence handler using the system clock.</summary>
    public KeySequenceHandler()
        : this(TimeProvider.System, SequenceOverlapPolicy.ExecuteImmediately)
    {
    }

    /// <summary>Creates a sequence handler using the system clock and an overlap policy.</summary>
    /// <param name="overlapPolicy">How strict-prefix sequence overlaps are resolved.</param>
    public KeySequenceHandler(SequenceOverlapPolicy overlapPolicy)
        : this(TimeProvider.System, overlapPolicy)
    {
    }

    /// <summary>Creates a sequence handler using an explicit clock.</summary>
    /// <param name="timeProvider">The clock used to enforce sequence timeouts.</param>
    public KeySequenceHandler(TimeProvider timeProvider)
        : this(timeProvider, SequenceOverlapPolicy.ExecuteImmediately)
    {
    }

    /// <summary>Creates a sequence handler using an explicit clock and overlap policy.</summary>
    /// <param name="timeProvider">The clock used to enforce sequence timeouts.</param>
    /// <param name="overlapPolicy">How strict-prefix sequence overlaps are resolved.</param>
    public KeySequenceHandler(TimeProvider timeProvider, SequenceOverlapPolicy overlapPolicy)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        if (!Enum.IsDefined(overlapPolicy))
            throw new ArgumentOutOfRangeException(nameof(overlapPolicy));
        _overlapPolicy = overlapPolicy;
    }

    /// <summary>Occurs after sequence progress changes. Subscribers run outside the state lock.</summary>
    public event EventHandler? ProgressChanged;

    /// <summary>Gets how strict-prefix sequence overlaps are resolved.</summary>
    public SequenceOverlapPolicy OverlapPolicy => _overlapPolicy;

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
        var progressChanged = false;
        IDisposable token;
        lock (_lockObject)
        {
            ThrowIfDisposed();
            if (_overlapPolicy == SequenceOverlapPolicy.RejectAmbiguous)
                ThrowIfAmbiguous(key, gestures);

            if (_sequences.TryGetValue(key, out var previous))
            {
                progressChanged = HasProgress(previous);
                ClearProgress(previous);
            }

            var registration = new SequenceRegistration(
                ++_nextRegistrationId,
                key,
                gestures,
                action,
                effectiveTimeout);
            _sequences[key] = registration;
            token = new SequenceRegistrationToken(this, key, registration.Id);
        }

        if (progressChanged)
            RaiseProgressChanged();
        return token;
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
        var progressChanged = false;
        bool removed;
        lock (_lockObject)
        {
            var key = CreateSequenceKey(sequence);
            SequenceRegistration? registration = null;
            removed = !_isDisposed && _sequences.Remove(key, out registration);
            if (registration is not null)
            {
                progressChanged = HasProgress(registration);
                ClearProgress(registration);
            }
        }

        if (progressChanged)
            RaiseProgressChanged();
        return removed;
    }

    /// <summary>Removes every registered key sequence.</summary>
    public void ClearSequences()
    {
        var progressChanged = false;
        lock (_lockObject)
        {
            ThrowIfDisposed();
            foreach (var registration in _sequences.Values)
            {
                progressChanged |= HasProgress(registration);
                ClearProgress(registration);
            }
            _sequences.Clear();
        }

        if (progressChanged)
            RaiseProgressChanged();
    }

    /// <summary>Clears all partial and deferred sequence progress without removing registrations.</summary>
    /// <returns><see langword="true"/> when at least one sequence had pending progress.</returns>
    public bool CancelPendingSequences()
    {
        var changed = false;
        lock (_lockObject)
        {
            ThrowIfDisposed();
            foreach (var registration in _sequences.Values)
            {
                changed |= HasProgress(registration);
                ClearProgress(registration);
            }
        }

        if (changed)
            RaiseProgressChanged();
        return changed;
    }

    /// <summary>Gets a detached snapshot of every registered sequence's current progress.</summary>
    /// <returns>Progress entries in registration order.</returns>
    public IReadOnlyList<KeySequenceProgress> GetProgressSnapshot()
    {
        lock (_lockObject)
        {
            ThrowIfDisposed();
            var snapshot = new KeySequenceProgress[_sequences.Count];
            var index = 0;
            foreach (var registration in _sequences.Values)
            {
                snapshot[index++] = new KeySequenceProgress(
                    string.Join(", ", registration.Sequence),
                    registration.IsCompletionPending
                        ? registration.Sequence.Length
                        : registration.Progress,
                    registration.Sequence.Length,
                    registration.IsCompletionPending,
                    registration.Timeout);
            }
            return snapshot;
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
        List<SequenceRegistration>? completedRegistrations = null;
        var progressChanged = false;
        lock (_lockObject)
        {
            foreach (var registration in _sequences.Values)
            {
                var previousProgress = registration.Progress;
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
                        completedRegistrations ??= [];
                        completedRegistrations.Add(registration);
                    }
                }
                else
                {
                    registration.Progress = registration.Sequence[0] == gesture ? 1 : 0;
                    if (registration.Progress > 0)
                        registration.LastTimestamp = timestamp;
                }

                progressChanged |= previousProgress != registration.Progress;
            }

            if (completedRegistrations is not null)
            {
                foreach (var completed in completedRegistrations)
                {
                    completed.Progress = 0;
                    if (_overlapPolicy == SequenceOverlapPolicy.PreferLongest
                        && HasActiveLongerCandidate(completed))
                    {
                        SetPendingCompletion(completed);
                        progressChanged = true;
                        continue;
                    }

                    if (_overlapPolicy == SequenceOverlapPolicy.PreferLongest)
                        progressChanged |= CancelPendingPrefixes(completed.Sequence);

                    completedActions ??= [];
                    completedActions.Add(completed.Action);
                }
            }

            if (_overlapPolicy == SequenceOverlapPolicy.PreferLongest)
            {
                foreach (var registration in _sequences.Values)
                {
                    if (!registration.IsCompletionPending || HasActiveLongerCandidate(registration))
                        continue;

                    TakePendingCompletion(registration, ref completedActions);
                    progressChanged = true;
                }
            }
        }

        if (progressChanged)
            RaiseProgressChanged();

        if (completedActions is not null)
        {
            key.Handled = true;
            foreach (var action in completedActions)
                MainThread.BeginInvokeOnMainThread(() => InvokeSafely(action));
        }
    }

    private void ThrowIfAmbiguous(string replacementKey, KeyGesture[] sequence)
    {
        foreach (var registration in _sequences.Values)
        {
            if (registration.Key == replacementKey)
                continue;
            if (IsStrictPrefix(sequence, registration.Sequence)
                || IsStrictPrefix(registration.Sequence, sequence))
            {
                throw new InvalidOperationException(
                    $"The sequence '{string.Join(", ", sequence)}' overlaps " +
                    $"with '{string.Join(", ", registration.Sequence)}'.");
            }
        }
    }

    private bool HasActiveLongerCandidate(SequenceRegistration shorter)
    {
        foreach (var candidate in _sequences.Values)
        {
            if (ReferenceEquals(candidate, shorter)
                || candidate.Progress < shorter.Sequence.Length
                || !IsStrictPrefix(shorter.Sequence, candidate.Sequence))
            {
                continue;
            }
            return true;
        }
        return false;
    }

    private static bool IsStrictPrefix(
        IReadOnlyList<KeyGesture> prefix,
        IReadOnlyList<KeyGesture> sequence)
    {
        if (prefix.Count >= sequence.Count)
            return false;
        for (var index = 0; index < prefix.Count; index++)
        {
            if (prefix[index] != sequence[index])
                return false;
        }
        return true;
    }

    private void SetPendingCompletion(SequenceRegistration registration)
    {
        registration.PendingTimer?.Dispose();
        registration.IsCompletionPending = true;
        var state = new PendingCompletionState(this, registration);
        registration.PendingTimer = _timeProvider.CreateTimer(
            static timerState =>
            {
                var pending = (PendingCompletionState)timerState!;
                pending.Owner.CompletePendingAfterTimeout(pending.Registration);
            },
            state,
            registration.Timeout,
            Timeout.InfiniteTimeSpan);
    }

    private void CompletePendingAfterTimeout(SequenceRegistration registration)
    {
        Action? action = null;
        ITimer? elapsedTimer = null;
        var changed = false;
        lock (_lockObject)
        {
            if (_isDisposed
                || !_sequences.TryGetValue(registration.Key, out var current)
                || current.Id != registration.Id
                || !current.IsCompletionPending)
            {
                return;
            }

            current.IsCompletionPending = false;
            elapsedTimer = current.PendingTimer;
            current.PendingTimer = null;
            action = current.Action;
            changed = true;

            foreach (var candidate in _sequences.Values)
            {
                if (!IsStrictPrefix(current.Sequence, candidate.Sequence))
                    continue;
                changed |= HasProgress(candidate);
                ClearProgress(candidate);
            }
        }

        elapsedTimer?.Dispose();
        if (changed)
            RaiseProgressChanged();
        MainThread.BeginInvokeOnMainThread(() => InvokeSafely(action));
    }

    private bool CancelPendingPrefixes(IReadOnlyList<KeyGesture> completedSequence)
    {
        var changed = false;
        foreach (var registration in _sequences.Values)
        {
            if (!registration.IsCompletionPending
                || !IsStrictPrefix(registration.Sequence, completedSequence))
            {
                continue;
            }

            ClearProgress(registration);
            changed = true;
        }
        return changed;
    }

    private static void TakePendingCompletion(
        SequenceRegistration registration,
        ref List<Action>? completedActions)
    {
        registration.IsCompletionPending = false;
        registration.PendingTimer?.Dispose();
        registration.PendingTimer = null;
        completedActions ??= [];
        completedActions.Add(registration.Action);
    }

    private static bool HasProgress(SequenceRegistration registration) =>
        registration.Progress > 0 || registration.IsCompletionPending;

    private static void ClearProgress(SequenceRegistration registration)
    {
        registration.Progress = 0;
        registration.IsCompletionPending = false;
        registration.PendingTimer?.Dispose();
        registration.PendingTimer = null;
    }

    private void RaiseProgressChanged()
    {
        var subscriber = ProgressChanged;
        if (subscriber is null)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                subscriber(this, EventArgs.Empty);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[GlobalKeyboardCapture] Sequence progress subscriber threw: {exception}");
            }
        });
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
        var progressChanged = false;
        lock (_lockObject)
        {
            if (_sequences.TryGetValue(key, out var current) && current.Id == registrationId)
            {
                progressChanged = HasProgress(current);
                ClearProgress(current);
                _sequences.Remove(key);
            }
        }
        if (progressChanged)
            RaiseProgressChanged();
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
            foreach (var registration in _sequences.Values)
                ClearProgress(registration);
            _sequences.Clear();
            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private sealed class SequenceRegistration(
        long id,
        string key,
        KeyGesture[] sequence,
        Action action,
        TimeSpan timeout)
    {
        public long Id { get; } = id;
        public string Key { get; } = key;
        public KeyGesture[] Sequence { get; } = sequence;
        public Action Action { get; } = action;
        public TimeSpan Timeout { get; } = timeout;
        public int Progress { get; set; }
        public long LastTimestamp { get; set; }
        public bool IsCompletionPending { get; set; }
        public ITimer? PendingTimer { get; set; }
    }

    private sealed record PendingCompletionState(
        KeySequenceHandler Owner,
        SequenceRegistration Registration);

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
