using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeySequenceHandlerTests
{
    [Fact]
    public void RegisteredSequenceInvokesActionAfterLastGesture()
    {
        var time = new FakeTimeProvider();
        var handler = new KeySequenceHandler(time);
        var invocations = 0;
        handler.RegisterSequence(
            [KeyGesture.Parse("Ctrl+K"), KeyGesture.Parse("Ctrl+C")],
            () => invocations++);

        handler.HandleKey(Key("K", control: true));
        var final = Key("C", control: true);
        handler.HandleKey(final);

        invocations.Should().Be(1);
        final.Handled.Should().BeTrue();
    }

    [Fact]
    public void SequenceExpiresAfterConfiguredTimeout()
    {
        var time = new FakeTimeProvider();
        var handler = new KeySequenceHandler(time);
        var invocations = 0;
        handler.RegisterSequence(
            [KeyGesture.Parse("A"), KeyGesture.Parse("B")],
            () => invocations++,
            TimeSpan.FromMilliseconds(100));

        handler.HandleKey(Key("A"));
        time.Advance(TimeSpan.FromMilliseconds(101));
        handler.HandleKey(Key("B"));

        invocations.Should().Be(0);
    }

    [Fact]
    public void RegistrationTokenCannotRemoveReplacementSequence()
    {
        var handler = new KeySequenceHandler(new FakeTimeProvider());
        var oldInvocations = 0;
        var newInvocations = 0;
        var oldRegistration = handler.RegisterSequence(
            [KeyGesture.Parse("A"), KeyGesture.Parse("B")],
            () => oldInvocations++);
        handler.RegisterSequence(
            [KeyGesture.Parse("A"), KeyGesture.Parse("B")],
            () => newInvocations++);

        oldRegistration.Dispose();
        handler.HandleKey(Key("A"));
        handler.HandleKey(Key("B"));

        oldInvocations.Should().Be(0);
        newInvocations.Should().Be(1);
    }

    [Fact]
    public void KeyUpAndRepeatDoNotAdvanceSequences()
    {
        var handler = new KeySequenceHandler(new FakeTimeProvider());
        var invocations = 0;
        handler.RegisterSequence(
            [KeyGesture.Parse("A"), KeyGesture.Parse("B")],
            () => invocations++);

        handler.HandleKey(new KeyEventArgs { Character = 'A', EventType = KeyboardEventType.KeyUp });
        handler.HandleKey(new KeyEventArgs { Character = 'A', RepeatCount = 1 });
        handler.HandleKey(Key("B"));

        invocations.Should().Be(0);
    }

    private static KeyEventArgs Key(string value, bool control = false) => new()
    {
        Character = value[0],
        ControlKey = control
    };
}
