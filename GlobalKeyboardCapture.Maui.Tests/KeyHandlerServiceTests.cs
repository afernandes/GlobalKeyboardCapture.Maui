using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public class KeyHandlerServiceTests
{
    private static (KeyHandlerService svc, FakePlatformKeyHandler platform) Build(
        KeyHandlerOptions? options = null)
    {
        var platform = new FakePlatformKeyHandler();
        var logger = NullLogger<KeyHandlerService>.Instance;
        return (new KeyHandlerService(platform, logger, options ?? new KeyHandlerOptions()), platform);
    }

    [Fact]
    public void ConstructorWiresPlatformCallback()
    {
        var (_, platform) = Build();
        var key = new KeyEventArgs();
        Action act = () => platform.Dispatch(key);
        act.Should().NotThrow();
    }

    [Fact]
    public void RegisteredHandlerReceivesDispatchedKey()
    {
        var (svc, platform) = Build();
        var recorder = new RecordingKeyHandler();
        svc.RegisterHandler(recorder);

        var key = new KeyEventArgs { Character = 'A' };
        platform.Dispatch(key);

        recorder.HandledKeys.Should().ContainSingle().Which.Should().BeSameAs(key);
    }

    [Fact]
    public void HandlersWithShouldHandleFalseAreSkipped()
    {
        var (svc, platform) = Build();
        var recorder = new RecordingKeyHandler(shouldHandle: _ => false);
        svc.RegisterHandler(recorder);

        platform.Dispatch(new KeyEventArgs());

        recorder.HandledKeys.Should().BeEmpty();
    }

    [Fact]
    public void ExceptionInOneHandlerDoesNotStopOthers()
    {
        var (svc, platform) = Build();
        var first = new RecordingKeyHandler(onHandle: _ => throw new InvalidOperationException("boom"));
        var second = new RecordingKeyHandler();
        svc.RegisterHandler(first);
        svc.RegisterHandler(second);

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        second.HandledKeys.Should().HaveCount(1);
    }

    [Fact]
    public void UnregisterStopsDelivery()
    {
        var (svc, platform) = Build();
        var recorder = new RecordingKeyHandler();
        svc.RegisterHandler(recorder);
        svc.UnregisterHandler(recorder);

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        recorder.HandledKeys.Should().BeEmpty();
    }

    [Fact]
    public void InitializeForwardsToPlatformHandler()
    {
        var (svc, platform) = Build();
        svc.Initialize(new object());

        platform.Initialized.Should().BeTrue();
        platform.InitializeCallCount.Should().Be(1);
    }

    [Fact]
    public void InitializeIsIdempotentForSamePlatformView()
    {
        var (svc, platform) = Build();
        var view = new object();
        svc.Initialize(view);
        svc.Initialize(view);

        platform.InitializeCallCount.Should().Be(1);
    }

    [Fact]
    public void InitializeRebindsWhenPlatformViewChanges()
    {
        var (svc, platform) = Build();
        svc.Initialize(new object());
        svc.Initialize(new object());

        platform.InitializeCallCount.Should().Be(2);
    }

    [Fact]
    public void DisposeCleansUpPlatform()
    {
        var (svc, platform) = Build();
        svc.Dispose();
        platform.CleanedUp.Should().BeTrue();
    }

    [Fact]
    public void OperationsAfterDisposeThrow()
    {
        var (svc, _) = Build();
        svc.Dispose();

        ((Action)(() => svc.Initialize(new object()))).Should().Throw<ObjectDisposedException>();
        ((Action)(() => svc.RegisterHandler(new RecordingKeyHandler()))).Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void RegisterDuringDispatchDoesNotCorruptIteration()
    {
        var (svc, platform) = Build();
        var late = new RecordingKeyHandler();
        var early = new RecordingKeyHandler(onHandle: _ => svc.RegisterHandler(late));
        svc.RegisterHandler(early);

        platform.Dispatch(new KeyEventArgs { Character = 'A' });

        early.HandledKeys.Should().HaveCount(1);
        late.HandledKeys.Should().BeEmpty();

        platform.Dispatch(new KeyEventArgs { Character = 'B' });

        early.HandledKeys.Should().HaveCount(2);
        late.HandledKeys.Should().HaveCount(1);
    }
}
