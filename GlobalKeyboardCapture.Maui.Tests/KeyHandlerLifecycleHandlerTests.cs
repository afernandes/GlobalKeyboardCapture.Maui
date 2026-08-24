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

    [Fact]
    public void CreatedObserverCanSynchronouslyDestroyTheTrackedPlatformView()
    {
        var platform = new FakePlatformKeyHandler();
        using var service = new Core.Services.KeyHandlerService(
            platform,
            NullLogger<Core.Services.KeyHandlerService>.Instance);
        var sink = new ReentrantLifecycleSink();
        using var lifecycle = new KeyHandlerLifecycleHandler(service, [sink]);
        var view = new object();
        sink.OnCreated = lifecycle.OnPlatformViewDestroyed;

        lifecycle.OnPlatformViewCreated(view);

        platform.InitializeCallCount.Should().Be(1);
        platform.DetachCallCount.Should().Be(1);
        service.PlatformViewCount.Should().Be(0);
    }

    private sealed class ReentrantLifecycleSink : Core.Interfaces.IPlatformViewLifecycleSink
    {
        public Action<object>? OnCreated { get; set; }

        public void OnPlatformViewCreated(object platformView) => OnCreated?.Invoke(platformView);

        public void OnPlatformViewDestroyed(object platformView)
        {
        }
    }
}
