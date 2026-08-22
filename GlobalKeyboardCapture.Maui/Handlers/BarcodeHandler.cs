using System.Runtime.CompilerServices;
using System.Text;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Handlers;

public sealed class BarcodeHandler : IKeyHandler, IDisposable
{
    private const int DEFAULT_BUFFER_CAPACITY = 50;

    private readonly ScannerState[] _states;
    private readonly TimeProvider _timeProvider;
    private bool _isDisposed;

    /// <summary>Raised with the decoded value when a configured profile accepts a scan.</summary>
    public event EventHandler<string>? BarcodeScanned;

    /// <summary>Raised with decoded value, profile, timing, and device metadata.</summary>
    public event EventHandler<BarcodeScanResult>? ScanCompleted;

    public BarcodeHandler(KeyHandlerOptions options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _timeProvider = timeProvider ?? TimeProvider.System;

        if (options.BarcodeProfiles.Count == 0)
        {
            ValidateLegacyOptions(options);
            _states =
            [
                new ScannerState(new ProfileSnapshot(
                    "Default",
                    TimeSpan.FromMilliseconds(options.BarcodeTimeout),
                    options.MinBarcodeLength,
                    options.MaxBarcodeLength,
                    [KeyboardKey.Enter],
                    [],
                    null,
                    null,
                    false,
                    false,
                    true,
                    true,
                    true,
                    null,
                    null))
            ];
            return;
        }

        _states = new ScannerState[options.BarcodeProfiles.Count];
        for (var i = 0; i < options.BarcodeProfiles.Count; i++)
        {
            var profile = options.BarcodeProfiles[i]
                ?? throw new ArgumentException("Barcode profiles cannot contain null entries.", nameof(options));
            _states[i] = new ScannerState(ProfileSnapshot.Create(profile));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldHandle(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.EventType != KeyboardEventType.KeyDown || !key.NoSpecialKeysPressed)
            return false;

        if (key.Character.HasValue)
            return true;

        var logicalKey = key.Key;
        for (var i = 0; i < _states.Length; i++)
        {
            if (_states[i].Profile.TerminatorKeys.Contains(logicalKey))
                return true;
        }

        return false;
    }

    public void HandleKey(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();

        if (key.EventType != KeyboardEventType.KeyDown || !key.NoSpecialKeysPressed)
            return;

        var timestamp = _timeProvider.GetTimestamp();
        BarcodeScanResult? acceptedScan = null;
        var isTerminator = false;

        for (var i = 0; i < _states.Length; i++)
        {
            var state = _states[i];
            if (!IsApplicable(state.Profile, key))
                continue;

            state.ResetIfExpired(_timeProvider, timestamp);
            state.ResetIfDeviceChanged(key.Device);

            if (!IsTerminator(state.Profile, key))
                continue;

            isTerminator = true;
            acceptedScan ??= TryCompleteScan(state, key);
        }

        if (isTerminator)
        {
            ResetAll();
            if (acceptedScan is not null)
            {
                key.Handled = true;
                OnScanCompleted(acceptedScan);
            }

            return;
        }

        if (!key.Character.HasValue)
            return;

        for (var i = 0; i < _states.Length; i++)
        {
            var state = _states[i];
            if (!IsApplicable(state.Profile, key))
                continue;

            state.ResetIfExpired(_timeProvider, timestamp);
            state.ResetIfDeviceChanged(key.Device);
            state.Append(key.Character.Value, key.Device, timestamp);
        }
    }

    private BarcodeScanResult? TryCompleteScan(ScannerState state, KeyEventArgs terminator)
    {
        if (state.IsOverflowed || state.Buffer.Length == 0)
            return null;

        var profile = state.Profile;
        var rawValue = state.Buffer.ToString();
        var value = profile.TrimWhitespace ? rawValue.Trim() : rawValue;

        var hasPrefix = profile.Prefix is not null
            && value.StartsWith(profile.Prefix, StringComparison.Ordinal);
        if (profile.RequirePrefix && !hasPrefix)
            return null;

        var prefixRemoved = hasPrefix && profile.StripPrefix;
        if (prefixRemoved)
            value = value[profile.Prefix!.Length..];

        var hasSuffix = profile.Suffix is not null
            && value.EndsWith(profile.Suffix, StringComparison.Ordinal);
        if (profile.RequireSuffix && !hasSuffix)
            return null;

        var suffixRemoved = hasSuffix && profile.StripSuffix;
        if (suffixRemoved)
            value = value[..^profile.Suffix!.Length];

        if (profile.TrimWhitespace)
            value = value.Trim();

        if (value.Length < profile.MinLength)
            return null;

        var duration = state.GetDuration(_timeProvider);
        if (profile.MaxAverageInterCharacterDelay.HasValue && state.Buffer.Length > 1)
        {
            var averageTicks = duration.Ticks / (state.Buffer.Length - 1);
            if (TimeSpan.FromTicks(averageTicks) > profile.MaxAverageInterCharacterDelay.Value)
                return null;
        }

        return new BarcodeScanResult
        {
            Value = value,
            RawValue = rawValue,
            ProfileName = profile.Name,
            Duration = duration,
            CharacterCount = state.Buffer.Length,
            Device = state.Device,
            TerminatorKey = terminator.Character.HasValue ? KeyboardKey.None : terminator.Key,
            TerminatorCharacter = terminator.Character,
            PrefixRemoved = prefixRemoved,
            SuffixRemoved = suffixRemoved
        };
    }

    private void OnScanCompleted(BarcodeScanResult result)
    {
        BarcodeScanned?.Invoke(this, result.Value);
        ScanCompleted?.Invoke(this, result);
    }

    private void ResetAll()
    {
        for (var i = 0; i < _states.Length; i++)
            _states[i].Reset();
    }

    private static bool IsApplicable(ProfileSnapshot profile, KeyEventArgs key) =>
        !profile.DeviceId.HasValue || key.Device?.Id == profile.DeviceId.Value;

    private static bool IsTerminator(ProfileSnapshot profile, KeyEventArgs key)
    {
        if (key.Character.HasValue)
            return profile.TerminatorCharacters.Contains(key.Character.Value);

        return profile.TerminatorKeys.Contains(key.Key);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    private static void ValidateLegacyOptions(KeyHandlerOptions options)
    {
        if (options.BarcodeTimeout <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.BarcodeTimeout), "Barcode timeout must be greater than zero.");
        if (options.MinBarcodeLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MinBarcodeLength), "Minimum barcode length must be greater than zero.");
        if (options.MaxBarcodeLength < options.MinBarcodeLength)
            throw new ArgumentOutOfRangeException(nameof(options.MaxBarcodeLength), "Maximum barcode length must be greater than or equal to the minimum length.");
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        ResetAll();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    private sealed class ScannerState(ProfileSnapshot profile)
    {
        public ProfileSnapshot Profile { get; } = profile;
        public StringBuilder Buffer { get; } = new(DEFAULT_BUFFER_CAPACITY);
        public KeyboardDeviceInfo? Device { get; private set; }
        public long FirstTimestamp { get; private set; }
        public long LastTimestamp { get; private set; }
        public bool IsOverflowed { get; private set; }

        public void Append(char character, KeyboardDeviceInfo? device, long timestamp)
        {
            if (IsOverflowed)
            {
                LastTimestamp = timestamp;
                return;
            }

            if (Buffer.Length >= Profile.MaxLength)
            {
                Buffer.Clear();
                IsOverflowed = true;
                LastTimestamp = timestamp;
                return;
            }

            if (Buffer.Length == 0)
            {
                FirstTimestamp = timestamp;
                Device = device;
            }

            Buffer.Append(character);
            LastTimestamp = timestamp;
        }

        public void ResetIfExpired(TimeProvider timeProvider, long timestamp)
        {
            if ((Buffer.Length > 0 || IsOverflowed)
                && timeProvider.GetElapsedTime(LastTimestamp, timestamp) >= Profile.InterCharacterTimeout)
            {
                Reset();
            }
        }

        public void ResetIfDeviceChanged(KeyboardDeviceInfo? device)
        {
            if ((Buffer.Length > 0 || IsOverflowed) && Device?.Id != device?.Id)
                Reset();
        }

        public TimeSpan GetDuration(TimeProvider timeProvider) =>
            Buffer.Length > 1
                ? timeProvider.GetElapsedTime(FirstTimestamp, LastTimestamp)
                : TimeSpan.Zero;

        public void Reset()
        {
            Buffer.Clear();
            Device = null;
            FirstTimestamp = 0;
            LastTimestamp = 0;
            IsOverflowed = false;
        }
    }

    private sealed record ProfileSnapshot(
        string Name,
        TimeSpan InterCharacterTimeout,
        int MinLength,
        int MaxLength,
        HashSet<KeyboardKey> TerminatorKeys,
        HashSet<char> TerminatorCharacters,
        string? Prefix,
        string? Suffix,
        bool RequirePrefix,
        bool RequireSuffix,
        bool StripPrefix,
        bool StripSuffix,
        bool TrimWhitespace,
        TimeSpan? MaxAverageInterCharacterDelay,
        int? DeviceId)
    {
        public static ProfileSnapshot Create(BarcodeScannerProfile profile)
        {
            if (profile.InterCharacterTimeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(profile.InterCharacterTimeout));
            if (profile.MinLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(profile.MinLength));
            if (profile.MaxLength < profile.MinLength)
                throw new ArgumentOutOfRangeException(nameof(profile.MaxLength));
            if (profile.TerminatorKeys.Count == 0 && profile.TerminatorCharacters.Count == 0)
                throw new ArgumentException("A barcode scanner profile requires at least one terminator.", nameof(profile));
            if (profile.RequirePrefix && string.IsNullOrEmpty(profile.Prefix))
                throw new ArgumentException("A required barcode prefix cannot be empty.", nameof(profile));
            if (profile.RequireSuffix && string.IsNullOrEmpty(profile.Suffix))
                throw new ArgumentException("A required barcode suffix cannot be empty.", nameof(profile));
            if (profile.MaxAverageInterCharacterDelay <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(profile.MaxAverageInterCharacterDelay));

            return new ProfileSnapshot(
                profile.Name,
                profile.InterCharacterTimeout,
                profile.MinLength,
                profile.MaxLength,
                new HashSet<KeyboardKey>(profile.TerminatorKeys),
                new HashSet<char>(profile.TerminatorCharacters),
                profile.Prefix,
                profile.Suffix,
                profile.RequirePrefix,
                profile.RequireSuffix,
                profile.StripPrefix,
                profile.StripSuffix,
                profile.TrimWhitespace,
                profile.MaxAverageInterCharacterDelay,
                profile.DeviceId);
        }
    }
}
