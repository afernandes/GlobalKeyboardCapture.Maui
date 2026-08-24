using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeyboardDeviceRoutingTests
{
    [Fact]
    public void FilterMatchesEveryConfiguredCriterion()
    {
        var filter = new KeyboardDeviceFilter(
            deviceId: 42,
            isVirtual: false,
            isExternal: true,
            descriptor: "usb:scanner",
            platform: KeyboardPlatform.Android);
        var matching = Event(
            KeyboardPlatform.Android,
            new KeyboardDeviceInfo(42, "Scanner", isExternal: true, descriptor: "USB:SCANNER"));

        filter.Matches(matching).Should().BeTrue();
        filter.Matches(matching.WithDevice(id: 7)).Should().BeFalse();
        filter.Matches(Event(KeyboardPlatform.Windows, matching.Device)).Should().BeFalse();
        filter.Matches(Event(KeyboardPlatform.Android, device: null)).Should().BeFalse();
    }

    [Fact]
    public void PlatformOnlyFilterAcceptsEventWithoutDeviceMetadata()
    {
        var filter = new KeyboardDeviceFilter(platform: KeyboardPlatform.iOS);

        filter.Matches(Event(KeyboardPlatform.iOS, device: null)).Should().BeTrue();
        filter.Matches(Event(KeyboardPlatform.Android, device: null)).Should().BeFalse();
    }

    [Fact]
    public void RegistrationFilterRoutesOnlyMatchingDevice()
    {
        var (service, platform) = Build();
        var handler = new RecordingKeyHandler();
        service.RegisterHandler(handler, new KeyboardDeviceFilter(deviceId: 42));

        platform.Dispatch(Event(KeyboardPlatform.Android, new KeyboardDeviceInfo(7)));
        platform.Dispatch(Event(KeyboardPlatform.Android, new KeyboardDeviceInfo(42)));

        handler.HandledKeys.Should().ContainSingle()
            .Which.Device!.Id.Should().Be(42);
    }

    [Fact]
    public void ScopeAndRegistrationFiltersMustBothMatch()
    {
        var (service, platform) = Build();
        var handler = new RecordingKeyHandler();
        using var scope = service.CreateScope(
            new KeyboardDeviceFilter(platform: KeyboardPlatform.Android),
            "scanner");
        scope.RegisterHandler(handler, new KeyboardDeviceFilter(isExternal: true));

        platform.Dispatch(Event(
            KeyboardPlatform.Android,
            new KeyboardDeviceInfo(1, isExternal: false)));
        platform.Dispatch(Event(
            KeyboardPlatform.Windows,
            new KeyboardDeviceInfo(2, isExternal: true)));
        platform.Dispatch(Event(
            KeyboardPlatform.Android,
            new KeyboardDeviceInfo(3, isExternal: true)));

        handler.HandledKeys.Should().ContainSingle()
            .Which.Device!.Id.Should().Be(3);
        scope.DeviceFilter.Should().NotBeNull();
    }

    [Fact]
    public void SameHandlerCanBeRegisteredForDifferentDevices()
    {
        var (service, platform) = Build();
        var handler = new RecordingKeyHandler();
        service.RegisterHandler(handler, new KeyboardDeviceFilter(deviceId: 10));
        service.RegisterHandler(handler, new KeyboardDeviceFilter(deviceId: 20));

        platform.Dispatch(Event(KeyboardPlatform.Android, new KeyboardDeviceInfo(10)));
        platform.Dispatch(Event(KeyboardPlatform.Android, new KeyboardDeviceInfo(20)));

        service.HandlerCount.Should().Be(2);
        handler.HandledKeys.Select(key => key.Device!.Id).Should().Equal(10, 20);
    }

    private static (KeyHandlerService Service, FakePlatformKeyHandler Platform) Build()
    {
        var platform = new FakePlatformKeyHandler();
        var service = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance);
        return (service, platform);
    }

    private static KeyEventArgs Event(KeyboardPlatform platform, KeyboardDeviceInfo? device) => new()
    {
        Character = 'A',
        Platform = platform,
        Device = device
    };
}

internal static class KeyboardDeviceRoutingTestExtensions
{
    public static KeyEventArgs WithDevice(this KeyEventArgs key, int id) => new()
    {
        Character = key.Character,
        Platform = key.Platform,
        Device = new KeyboardDeviceInfo(
            id,
            key.Device?.Name,
            key.Device?.IsVirtual ?? false,
            key.Device?.IsExternal ?? false,
            key.Device?.Descriptor)
    };
}
