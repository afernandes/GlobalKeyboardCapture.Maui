using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

/// <summary>
/// Tests for the Round 3 P2 robustness fixes:
///   B7:  KeyHandlerService.HandleKeyPress silently no-ops when disposed
///        (previously threw ObjectDisposedException on the platform input thread).
///   B8:  HotkeyHandler isolates exceptions thrown by user actions.
///   B13: BarcodeHandler.OnBarcodeScanned always clears the buffer, even when
///        a subscriber throws, so the next scan is not contaminated.
///   B14: KeyEventArgs.PlatformEvent is nullable (compile-time check).
/// </summary>
public class P2FixesTests
{
    [Fact]
    public void B7_DispatchAfterDispose_DoesNotThrow_AndIsIgnored()
    {
        var platform = new FakePlatformKeyHandler();
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, new KeyHandlerOptions());
        var recorder = new RecordingKeyHandler();
        svc.RegisterHandler(recorder);

        svc.Dispose();

        Action act = () => platform.Dispatch(new KeyEventArgs { Character = 'X' });

        act.Should().NotThrow();
        recorder.HandledKeys.Should().BeEmpty();
    }

    [Fact]
    public void B8_HotkeyActionThatThrows_DoesNotPropagate()
    {
        var handler = new HotkeyHandler();
        handler.RegisterHotkey("Ctrl+Z", () => throw new InvalidOperationException("boom"));

        var key = new KeyEventArgs { ControlKey = true, Character = 'Z' };
        Action act = () => handler.HandleKey(key);

        act.Should().NotThrow();
        key.Handled.Should().BeTrue();
    }

    [Fact]
    public void B8_FollowingHotkeysStillFireAfterFailingAction()
    {
        var handler = new HotkeyHandler();
        var secondFired = false;
        handler.RegisterHotkey("Ctrl+A", () => throw new InvalidOperationException("boom"));
        handler.RegisterHotkey("Ctrl+B", () => secondFired = true);

        handler.HandleKey(new KeyEventArgs { ControlKey = true, Character = 'A' });
        handler.HandleKey(new KeyEventArgs { ControlKey = true, Character = 'B' });

        secondFired.Should().BeTrue();
    }

    [Fact]
    public void B13_BufferIsClearedEvenWhenSubscriberThrows()
    {
        var options = new KeyHandlerOptions { MinBarcodeLength = 3, BarcodeTimeout = 1000 };
        var time = new FakeTimeProvider();
        var handler = new BarcodeHandler(options, time);

        var scans = new List<string>();
        var shouldThrow = true;
        handler.BarcodeScanned += (_, code) =>
        {
            scans.Add(code);
            if (shouldThrow) throw new InvalidOperationException("boom");
        };

        // First scan: subscriber throws AFTER the value is captured. The buffer
        // must still be cleared so the next scan starts clean.
        foreach (var c in "ABC")
        {
            handler.HandleKey(new KeyEventArgs { Character = c });
            time.Advance(TimeSpan.FromMilliseconds(10));
        }
        Action firstEnter = () => handler.HandleKey(new KeyEventArgs { EnterKey = true });
        firstEnter.Should().Throw<InvalidOperationException>();

        // Second scan: subscriber no longer throws; result must be "XYZ" only,
        // not "ABCXYZ" (which would prove the buffer wasn't cleared).
        shouldThrow = false;
        foreach (var c in "XYZ")
        {
            handler.HandleKey(new KeyEventArgs { Character = c });
            time.Advance(TimeSpan.FromMilliseconds(10));
        }
        handler.HandleKey(new KeyEventArgs { EnterKey = true });

        scans.Should().HaveCount(2);
        scans[0].Should().Be("ABC");
        scans[1].Should().Be("XYZ");
    }

    [Fact]
    public void B14_KeyEventArgs_PlatformEvent_DefaultsToNull()
    {
        var key = new KeyEventArgs();
        key.PlatformEvent.Should().BeNull();
        // Also: ToString must not throw on a fresh instance (defensive path).
        Action act = () => key.ToString();
        act.Should().NotThrow();
    }
}
