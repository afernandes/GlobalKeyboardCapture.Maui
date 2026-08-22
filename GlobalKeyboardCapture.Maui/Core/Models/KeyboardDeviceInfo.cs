namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Platform-neutral metadata for the input device that produced an event.
/// </summary>
public sealed record KeyboardDeviceInfo
{
    /// <summary>Creates platform-neutral input-device metadata.</summary>
    /// <param name="id">The native device identifier.</param>
    /// <param name="name">The platform-provided display name.</param>
    /// <param name="isVirtual">Whether the device is virtual.</param>
    /// <param name="isExternal">Whether the device is external.</param>
    /// <param name="descriptor">The stable platform descriptor, when available.</param>
    public KeyboardDeviceInfo(
        int id,
        string? name = null,
        bool isVirtual = false,
        bool isExternal = false,
        string? descriptor = null)
    {
        Id = id;
        Name = name;
        IsVirtual = isVirtual;
        IsExternal = isExternal;
        Descriptor = descriptor;
    }

    /// <summary>Gets the native device identifier.</summary>
    public int Id { get; }

    /// <summary>Gets the platform-provided display name.</summary>
    public string? Name { get; }

    /// <summary>Gets whether the device is virtual.</summary>
    public bool IsVirtual { get; }

    /// <summary>Gets whether the device is external.</summary>
    public bool IsExternal { get; }

    /// <summary>Gets the stable platform descriptor, when available.</summary>
    public string? Descriptor { get; }
}
