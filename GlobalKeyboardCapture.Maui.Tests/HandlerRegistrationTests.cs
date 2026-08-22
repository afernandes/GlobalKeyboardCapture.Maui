using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class HandlerRegistrationTests
{
    [Fact]
    public void HigherPriorityHandlersRunFirstAndTiesKeepRegistrationOrder()
    {
        var platform = new FakePlatformKeyHandler();
        var service = CreateService(platform);
        var order = new List<string>();

        service.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("normal")));
        service.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("high-a")), priority: 100);
        service.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("high-b")), priority: 100);

        platform.Dispatch(new KeyEventArgs { Character = 'A' });

        order.Should().Equal("high-a", "high-b", "normal");
    }

    [Fact]
    public void RegistrationTokenUnregistersExactlyOnce()
    {
        var platform = new FakePlatformKeyHandler();
        var service = CreateService(platform);
        var handler = new RecordingKeyHandler();
        var registration = service.RegisterHandler(handler);

        registration.Dispose();
        registration.Dispose();
        platform.Dispatch(new KeyEventArgs { Character = 'A' });

        handler.HandledKeys.Should().BeEmpty();
        service.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void ServiceExposesSnapshotState()
    {
        var platform = new FakePlatformKeyHandler();
        var service = CreateService(platform);
        var handler = new RecordingKeyHandler();

        service.IsInitialized.Should().BeFalse();
        service.RegisterHandler(handler);
        service.Initialize(new object());

        service.IsInitialized.Should().BeTrue();
        service.HandlerCount.Should().Be(1);
        service.Handlers.Should().Equal(handler);
    }

    private static KeyHandlerService CreateService(FakePlatformKeyHandler platform) =>
        new(platform, NullLogger<KeyHandlerService>.Instance, new KeyHandlerOptions());
}
