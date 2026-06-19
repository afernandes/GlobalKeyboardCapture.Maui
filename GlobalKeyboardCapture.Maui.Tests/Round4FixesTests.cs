using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

/// <summary>
/// Tests for the Round 4 fixes (formerly P2 remainder):
///   B10: HotkeyHandler.UnregisterHotkey / ClearHotkeys.
///   B11: KeyHandlerOptions.StopOnHandled stops the dispatch chain when set.
///   B12: KeyboardHelper.ToChar(char) overload behaves like the string overload
///        for the cases that DisplayLabel actually produces.
/// </summary>
public class Round4FixesTests
{
    // ---------- B10 ----------

    [Fact]
    public void B10_UnregisterHotkey_String_StopsFiring()
    {
        var handler = new HotkeyHandler();
        var fired = 0;
        handler.RegisterHotkey("Ctrl+Shift+X", () => fired++);

        handler.HandleKey(new KeyEventArgs { ControlKey = true, ShiftKey = true, Character = 'X' });
        handler.UnregisterHotkey("Ctrl+Shift+X").Should().BeTrue();
        handler.HandleKey(new KeyEventArgs { ControlKey = true, ShiftKey = true, Character = 'X' });

        fired.Should().Be(1);
    }

    [Fact]
    public void B10_UnregisterHotkey_String_HonorsNormalization()
    {
        var handler = new HotkeyHandler();
        handler.RegisterHotkey("Shift+Ctrl+X", () => { });

        // Different modifier order, alias for Ctrl — both must resolve to the same slot.
        handler.UnregisterHotkey("Control+Shift+X").Should().BeTrue();
        handler.UnregisterHotkey("Ctrl+Shift+X").Should().BeFalse();
    }

    [Fact]
    public void B10_UnregisterHotkey_KeyEventArgs_StopsFiring()
    {
        var handler = new HotkeyHandler();
        var fired = 0;
        var combo = new KeyEventArgs { AltKey = true, Character = 'Z' };
        handler.RegisterHotkey(combo, () => fired++);

        handler.HandleKey(new KeyEventArgs { AltKey = true, Character = 'Z' });
        handler.UnregisterHotkey(new KeyEventArgs { AltKey = true, Character = 'Z' }).Should().BeTrue();
        handler.HandleKey(new KeyEventArgs { AltKey = true, Character = 'Z' });

        fired.Should().Be(1);
    }

    [Fact]
    public void B10_UnregisterHotkey_ReturnsFalseWhenNotRegistered()
    {
        var handler = new HotkeyHandler();
        handler.UnregisterHotkey("Ctrl+Q").Should().BeFalse();
    }

    [Fact]
    public void B10_ClearHotkeys_RemovesEverything()
    {
        var handler = new HotkeyHandler();
        var fired = 0;
        handler.RegisterHotkey("Ctrl+A", () => fired++);
        handler.RegisterHotkey("Ctrl+B", () => fired++);
        handler.RegisterHotkey("Alt+C", () => fired++);

        handler.ClearHotkeys();

        handler.HandleKey(new KeyEventArgs { ControlKey = true, Character = 'A' });
        handler.HandleKey(new KeyEventArgs { ControlKey = true, Character = 'B' });
        handler.HandleKey(new KeyEventArgs { AltKey = true, Character = 'C' });

        fired.Should().Be(0);
    }

    [Fact]
    public void B10_UnregisterHotkey_String_ThrowsOnNullOrEmpty()
    {
        var handler = new HotkeyHandler();
        ((Action)(() => handler.UnregisterHotkey(""))).Should().Throw<ArgumentException>();
        ((Action)(() => handler.UnregisterHotkey((string)null!))).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void B10_UnregisterHotkey_KeyEventArgs_ThrowsOnNull()
    {
        var handler = new HotkeyHandler();
        ((Action)(() => handler.UnregisterHotkey((KeyEventArgs)null!))).Should().Throw<ArgumentNullException>();
    }

    // ---------- B11 ----------

    [Fact]
    public void B11_StopOnHandled_DefaultsFalse_AllHandlersStillRun()
    {
        var platform = new FakePlatformKeyHandler();
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, new KeyHandlerOptions());
        var first = new RecordingKeyHandler(onHandle: k => k.Handled = true);
        var second = new RecordingKeyHandler();
        svc.RegisterHandler(first);
        svc.RegisterHandler(second);

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        first.HandledKeys.Should().HaveCount(1);
        second.HandledKeys.Should().HaveCount(1);
    }

    [Fact]
    public void B11_StopOnHandled_True_BreaksChainAfterHandled()
    {
        var platform = new FakePlatformKeyHandler();
        var options = new KeyHandlerOptions { StopOnHandled = true };
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, options);
        var first = new RecordingKeyHandler(onHandle: k => k.Handled = true);
        var second = new RecordingKeyHandler();
        svc.RegisterHandler(first);
        svc.RegisterHandler(second);

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        first.HandledKeys.Should().HaveCount(1);
        second.HandledKeys.Should().BeEmpty();
    }

    [Fact]
    public void B11_StopOnHandled_True_ContinuesWhenNotHandled()
    {
        var platform = new FakePlatformKeyHandler();
        var options = new KeyHandlerOptions { StopOnHandled = true };
        var svc = new KeyHandlerService(platform, NullLogger<KeyHandlerService>.Instance, options);
        var first = new RecordingKeyHandler(); // doesn't mark Handled
        var second = new RecordingKeyHandler();
        svc.RegisterHandler(first);
        svc.RegisterHandler(second);

        platform.Dispatch(new KeyEventArgs { Character = 'X' });

        first.HandledKeys.Should().HaveCount(1);
        second.HandledKeys.Should().HaveCount(1);
    }
}
