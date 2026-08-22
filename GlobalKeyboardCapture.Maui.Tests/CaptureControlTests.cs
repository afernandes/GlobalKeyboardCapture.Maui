using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class CaptureControlTests
{
    [Fact]
    public void SuspensionTokensAreNestedAndIdempotent()
    {
        var platform = new FakePlatformKeyHandler();
        var service = CreateService(platform);
        var handler = new RecordingKeyHandler();
        service.RegisterHandler(handler);

        var first = service.SuspendCapture();
        var second = service.SuspendCapture();
        platform.Dispatch(new KeyEventArgs { Character = 'A' });
        first.Dispose();
        first.Dispose();
        platform.Dispatch(new KeyEventArgs { Character = 'B' });
        second.Dispose();
        platform.Dispatch(new KeyEventArgs { Character = 'C' });

        handler.HandledKeys.Select(key => key.Character).Should().Equal('C');
        service.IsCapturing.Should().BeTrue();
    }

    [Fact]
    public void ResumeCaptureClearsOutstandingSuspensions()
    {
        var platform = new FakePlatformKeyHandler();
        var service = CreateService(platform);
        var handler = new RecordingKeyHandler();
        service.RegisterHandler(handler);
        using var suspension = service.SuspendCapture();

        service.ResumeCapture();
        platform.Dispatch(new KeyEventArgs { Character = 'A' });

        handler.HandledKeys.Should().HaveCount(1);
        service.IsCapturing.Should().BeTrue();
    }

    [Fact]
    public void CaptureScopeCanBeDisabledReenabledAndDisposed()
    {
        var platform = new FakePlatformKeyHandler();
        var service = CreateService(platform);
        var handler = new RecordingKeyHandler();
        var scope = service.CreateScope("page");
        scope.RegisterHandler(handler);

        scope.IsEnabled = false;
        platform.Dispatch(new KeyEventArgs { Character = 'A' });
        scope.IsEnabled = true;
        platform.Dispatch(new KeyEventArgs { Character = 'B' });
        scope.Dispose();
        platform.Dispatch(new KeyEventArgs { Character = 'C' });

        handler.HandledKeys.Select(key => key.Character).Should().Equal('B');
        service.HandlerCount.Should().Be(0);
    }

    private static KeyHandlerService CreateService(FakePlatformKeyHandler platform) =>
        new(platform, NullLogger<KeyHandlerService>.Instance, new KeyHandlerOptions());
}
