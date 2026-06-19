using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

/// <summary>
/// Verifies KeyHandlerService dispatches handlers in registration order. With a
/// HashSet the handler that consumed an event under StopOnHandled was
/// implementation-dependent; a List makes ordering deterministic, and the
/// Contains guard keeps registration idempotent.
/// </summary>
public class DispatchOrderTests
{
    [Fact]
    public void Dispatch_InvokesHandlersInRegistrationOrder()
    {
        var platform = new FakePlatformKeyHandler();
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, new KeyHandlerOptions());

        var order = new List<string>();
        svc.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("a")));
        svc.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("b")));
        svc.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("c")));

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        order.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void StopOnHandled_StopsAtFirstRegisteredHandlerThatHandles()
    {
        var platform = new FakePlatformKeyHandler();
        var options = new KeyHandlerOptions { StopOnHandled = true };
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, options);

        var order = new List<string>();
        svc.RegisterHandler(new RecordingKeyHandler(onHandle: k => { order.Add("a"); k.Handled = true; }));
        svc.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("b")));
        svc.RegisterHandler(new RecordingKeyHandler(onHandle: _ => order.Add("c")));

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        order.Should().Equal("a");
    }

    [Fact]
    public void RegisterHandler_IsIdempotent()
    {
        var platform = new FakePlatformKeyHandler();
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, new KeyHandlerOptions());

        var handler = new RecordingKeyHandler();
        svc.RegisterHandler(handler);
        svc.RegisterHandler(handler); // duplicate registration must not double-dispatch

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        handler.HandledKeys.Should().HaveCount(1);
    }

    [Fact]
    public void Dispatch_AfterUnregisterAndReregister_UsesAppendOrder()
    {
        // Register a, b, c; unregister b; register b again. A List appends the
        // re-added handler, so dispatch order is a, c, b. A HashSet would reuse b's
        // freed slot and iterate a, b, c, so this asserts the ordered-List behavior
        // the fix depends on and would fail on the old HashSet implementation.
        var platform = new FakePlatformKeyHandler();
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, new KeyHandlerOptions());

        var order = new List<string>();
        var a = new RecordingKeyHandler(onHandle: _ => order.Add("a"));
        var b = new RecordingKeyHandler(onHandle: _ => order.Add("b"));
        var c = new RecordingKeyHandler(onHandle: _ => order.Add("c"));
        svc.RegisterHandler(a);
        svc.RegisterHandler(b);
        svc.RegisterHandler(c);
        svc.UnregisterHandler(b);
        svc.RegisterHandler(b);

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        order.Should().Equal("a", "c", "b");
    }
}
