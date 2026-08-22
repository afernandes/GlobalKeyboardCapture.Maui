namespace GlobalKeyboardCapture.Maui.Core.Models;

/// <summary>
/// Platform-neutral metadata for the input device that produced an event.
/// </summary>
public sealed record KeyboardDeviceInfo
{
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

    public int Id { get; }
    public string? Name { get; }
    public bool IsVirtual { get; }
    public bool IsExternal { get; }
    public string? Descriptor { get; }
}
