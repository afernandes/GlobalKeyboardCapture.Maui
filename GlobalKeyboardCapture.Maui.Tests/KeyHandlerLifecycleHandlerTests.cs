using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeyHandlerLifecycleHandlerTests
{
    [Fact]
    public void CreatedAndDestroyedPlatformViewsOwnOneAttachmentLease()
    {
        var platform = new FakePlatformKeyHandler();
        using var service = new Core.Services.KeyHandlerService(
            platform,
            NullLogger<Core.Services.KeyHandlerService>.Instance);
        using var lifecycle = new KeyHandlerLifecycleHandler(service);
        var view = new object();

        lifecycle.OnPlatformViewCreated(view);
        lifecycle.OnPlatformViewCreated(view);
        lifecycle.OnPlatformViewDestroyed(view);

        platform.InitializeCallCount.Should().Be(1);
        platform.DetachCallCount.Should().Be(1);
        service.PlatformViewCount.Should().Be(0);
    }

    [Fact]
    public void DisposeReleasesEveryAttachedPlatformView()
    {
        var platform = new FakePlatformKeyHandler();
        using var service = new Core.Services.KeyHandlerService(
            platform,
            NullLogger<Core.Services.KeyHandlerService>.Instance);
        var lifecycle = new KeyHandlerLifecycleHandler(service);

        lifecycle.OnPlatformViewCreated(new object());
        lifecycle.OnPlatformViewCreated(new object());
        lifecycle.Dispose();

        platform.DetachCallCount.Should().Be(2);
        service.PlatformViewCount.Should().Be(0);
    }
}
