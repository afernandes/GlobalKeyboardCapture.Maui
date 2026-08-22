using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class HotkeyRegistrationTests
{
    [Fact]
    public void TypedGestureRegistrationDispatchesAndCanBeDisposed()
    {
        var handler = new HotkeyHandler();
        var fired = 0;
        var registration = handler.RegisterHotkey(
            new KeyGesture('S', KeyModifiers.Control),
            () => fired++);

        handler.HandleKey(new KeyEventArgs { Character = 's', ControlKey = true });
        registration.Dispose();
        handler.HandleKey(new KeyEventArgs { Character = 's', ControlKey = true });

        fired.Should().Be(1);
        handler.HotkeyCount.Should().Be(0);
    }

    [Fact]
    public void DisposingReplacedRegistrationDoesNotRemoveReplacement()
    {
        var handler = new HotkeyHandler();
        var fired = 0;
        var oldRegistration = handler.RegisterHotkey("Ctrl+R", () => fired += 10);
        handler.RegisterHotkey("Ctrl+R", () => fired++);

        oldRegistration.Dispose();
        handler.HandleKey(new KeyEventArgs { Character = 'R', ControlKey = true });

        fired.Should().Be(1);
        handler.HotkeyCount.Should().Be(1);
    }
}
