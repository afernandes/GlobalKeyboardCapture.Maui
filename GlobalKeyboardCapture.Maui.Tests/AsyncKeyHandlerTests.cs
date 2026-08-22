using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class AsyncKeyHandlerTests
{
    [Fact]
    public async Task AsyncHandlerRunsOutsideDispatchWithSafeEventSnapshot()
    {
        var platform = new FakePlatformKeyHandler();
        using var service = new KeyHandlerService(
            platform,
            NullLogger<KeyHandlerService>.Instance,
            new KeyHandlerOptions());
        var handler = new BlockingAsyncHandler();
        service.RegisterHandler(handler);
        var nativeEvent = new object();
        var key = new KeyEventArgs { Character = 'A', PlatformEvent = nativeEvent };

        platform.Dispatch(key);
        await handler.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        handler.Received.Should().NotBeNull();
        handler.Received.Should().NotBeSameAs(key);
        handler.Received!.Character.Should().Be('A');
        handler.Received.PlatformEvent.Should().BeNull();
        key.Handled.Should().BeFalse("async handlers cannot alter native propagation after dispatch returns");

        handler.Release.TrySetResult();
        await handler.Completed.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DisposingServiceCancelsRunningAsyncHandlers()
    {
        var platform = new FakePlatformKeyHandler();
        var service = new KeyHandlerService(
            platform,
            NullLogger<KeyHandlerService>.Instance,
            new KeyHandlerOptions());
        var handler = new BlockingAsyncHandler();
        service.RegisterHandler(handler);

        platform.Dispatch(new KeyEventArgs { Character = 'A' });
        await handler.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        service.Dispose();

        await handler.Cancelled.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
    }

    private sealed class BlockingAsyncHandler : IAsyncKeyHandler
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Cancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public KeyEventArgs? Received { get; private set; }

        public bool ShouldHandle(KeyEventArgs key) => true;

        public async ValueTask HandleKeyAsync(KeyEventArgs key, CancellationToken cancellationToken)
        {
            Received = key;
            Started.TrySetResult();
            try
            {
                await Release.Task.WaitAsync(cancellationToken);
                Completed.TrySetResult();
            }
            catch (OperationCanceledException)
            {
                Cancelled.TrySetResult();
            }
        }
    }
}
