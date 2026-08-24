namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Describes immutable platform and input-device criteria used to route keyboard events.
/// Every configured criterion must match for an event to be accepted.
/// </summary>
public sealed record KeyboardDeviceFilter
{
    /// <summary>Creates an immutable keyboard-event routing filter.</summary>
    /// <param name="deviceId">The exact native device identifier to accept.</param>
    /// <param name="isVirtual">The required virtual-device state.</param>
    /// <param name="isExternal">The required external-device state.</param>
    /// <param name="descriptor">The exact platform descriptor, compared without case.</param>
    /// <param name="platform">The required originating platform.</param>
    public KeyboardDeviceFilter(
        int? deviceId = null,
        bool? isVirtual = null,
        bool? isExternal = null,
        string? descriptor = null,
        KeyboardPlatform? platform = null)
    {
        DeviceId = deviceId;
        IsVirtual = isVirtual;
        IsExternal = isExternal;
        Descriptor = descriptor;
        Platform = platform;
    }

    /// <summary>Gets the exact native device identifier to accept.</summary>
    public int? DeviceId { get; }

    /// <summary>Gets the required virtual-device state.</summary>
    public bool? IsVirtual { get; }

    /// <summary>Gets the required external-device state.</summary>
    public bool? IsExternal { get; }

    /// <summary>Gets the exact platform descriptor to accept.</summary>
    public string? Descriptor { get; }

    /// <summary>Gets the required originating platform.</summary>
    public KeyboardPlatform? Platform { get; }

    /// <summary>Determines whether a normalized keyboard event satisfies this filter.</summary>
    /// <param name="key">The normalized keyboard event to inspect.</param>
    /// <returns><see langword="true"/> when every configured criterion matches.</returns>
    public bool Matches(KeyEventArgs key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (Platform.HasValue && key.Platform != Platform.Value)
            return false;

        if (!HasDeviceCriteria())
            return true;

        var device = key.Device;
        if (device is null)
            return false;

        return (!DeviceId.HasValue || device.Id == DeviceId.Value)
            && (!IsVirtual.HasValue || device.IsVirtual == IsVirtual.Value)
            && (!IsExternal.HasValue || device.IsExternal == IsExternal.Value)
            && (Descriptor is null
                || string.Equals(device.Descriptor, Descriptor, StringComparison.OrdinalIgnoreCase));
    }

    private bool HasDeviceCriteria() =>
        DeviceId.HasValue || IsVirtual.HasValue || IsExternal.HasValue || Descriptor is not null;
}
