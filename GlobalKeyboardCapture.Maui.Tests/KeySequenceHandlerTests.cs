using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeySequenceHandlerTests
{
    [Fact]
    public void OptionsDefaultToBackwardCompatibleOverlapPolicy()
    {
        new Configuration.KeyHandlerOptions().SequenceOverlapPolicy.Should()
            .Be(SequenceOverlapPolicy.ExecuteImmediately);
    }

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

    [Fact]
    public void SharedInitialPrefixSelectsTheMatchingBranch()
    {
        var handler = new KeySequenceHandler(new FakeTimeProvider());
        var comments = 0;
        var uncomment = 0;
        handler.RegisterSequence(["Ctrl+K", "Ctrl+C"], () => comments++);
        handler.RegisterSequence(["Ctrl+K", "Ctrl+U"], () => uncomment++);

        handler.HandleKey(Key("K", control: true));
        handler.HandleKey(Key("U", control: true));

        comments.Should().Be(0);
        uncomment.Should().Be(1);
    }

    [Fact]
    public void ExecuteImmediatelyPreservesShortAndLongSequenceActions()
    {
        var handler = new KeySequenceHandler(
            new FakeTimeProvider(),
            SequenceOverlapPolicy.ExecuteImmediately);
        var shortInvocations = 0;
        var longInvocations = 0;
        handler.RegisterSequence(["A", "B"], () => shortInvocations++);
        handler.RegisterSequence(["A", "B", "C"], () => longInvocations++);

        handler.HandleKey(Key("A"));
        handler.HandleKey(Key("B"));
        handler.HandleKey(Key("C"));

        shortInvocations.Should().Be(1);
        longInvocations.Should().Be(1);
    }

    [Fact]
    public void PreferLongestSuppressesShortActionWhenLongSequenceCompletes()
    {
        var handler = new KeySequenceHandler(
            new FakeTimeProvider(),
            SequenceOverlapPolicy.PreferLongest);
        var shortInvocations = 0;
        var longInvocations = 0;
        handler.RegisterSequence(["A", "B"], () => shortInvocations++);
        handler.RegisterSequence(["A", "B", "C"], () => longInvocations++);

        handler.HandleKey(Key("A"));
        handler.HandleKey(Key("B"));

        shortInvocations.Should().Be(0);
        handler.GetProgressSnapshot().Should().ContainSingle(progress => progress.IsCompletionPending);

        handler.HandleKey(Key("C"));

        shortInvocations.Should().Be(0);
        longInvocations.Should().Be(1);
        handler.GetProgressSnapshot().Should().OnlyContain(progress => progress.MatchedGestureCount == 0);
    }

    [Fact]
    public void PreferLongestInvokesShortActionWhenLongSequenceDiverges()
    {
        var handler = new KeySequenceHandler(
            new FakeTimeProvider(),
            SequenceOverlapPolicy.PreferLongest);
        var invocations = 0;
        handler.RegisterSequence(["A", "B"], () => invocations++);
        handler.RegisterSequence(["A", "B", "C"], () => { });

        handler.HandleKey(Key("A"));
        handler.HandleKey(Key("B"));
        handler.HandleKey(Key("X"));

        invocations.Should().Be(1);
    }

    [Fact]
    public void PreferLongestInvokesShortActionWhenDisambiguationTimesOut()
    {
        var time = new FakeTimeProvider();
        var handler = new KeySequenceHandler(time, SequenceOverlapPolicy.PreferLongest);
        var invocations = 0;
        handler.RegisterSequence(
            ["A", "B"],
            () => invocations++,
            TimeSpan.FromMilliseconds(100));
        handler.RegisterSequence(
            ["A", "B", "C"],
            () => { },
            TimeSpan.FromMilliseconds(100));

        handler.HandleKey(Key("A"));
        handler.HandleKey(Key("B"));
        time.Advance(TimeSpan.FromMilliseconds(101));

        invocations.Should().Be(1);
        handler.GetProgressSnapshot().Should().OnlyContain(progress =>
            progress.MatchedGestureCount == 0 && !progress.IsCompletionPending);
    }

    [Fact]
    public void RejectAmbiguousRejectsPrefixInEitherRegistrationOrder()
    {
        var first = new KeySequenceHandler(
            new FakeTimeProvider(),
            SequenceOverlapPolicy.RejectAmbiguous);
        first.RegisterSequence(["A", "B"], () => { });

        var forward = () => first.RegisterSequence(["A", "B", "C"], () => { });

        forward.Should().Throw<InvalidOperationException>();

        var second = new KeySequenceHandler(
            new FakeTimeProvider(),
            SequenceOverlapPolicy.RejectAmbiguous);
        second.RegisterSequence(["A", "B", "C"], () => { });

        var reverse = () => second.RegisterSequence(["A", "B"], () => { });

        reverse.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CancelPendingSequencesResetsProgressAndCancelsDeferredActions()
    {
        var time = new FakeTimeProvider();
        var handler = new KeySequenceHandler(time, SequenceOverlapPolicy.PreferLongest);
        var invocations = 0;
        handler.RegisterSequence(["A", "B"], () => invocations++, TimeSpan.FromMilliseconds(100));
        handler.RegisterSequence(["A", "B", "C"], () => { }, TimeSpan.FromMilliseconds(100));
        handler.HandleKey(Key("A"));
        handler.HandleKey(Key("B"));

        handler.CancelPendingSequences().Should().BeTrue();
        handler.CancelPendingSequences().Should().BeFalse();
        time.Advance(TimeSpan.FromMilliseconds(101));

        invocations.Should().Be(0);
        handler.GetProgressSnapshot().Should().OnlyContain(progress =>
            progress.MatchedGestureCount == 0 && !progress.IsCompletionPending);
    }

    [Fact]
    public void ProgressChangedExposesDetachedSnapshot()
    {
        var handler = new KeySequenceHandler(new FakeTimeProvider());
        handler.RegisterSequence(["A", "B"], () => { });
        var notifications = 0;
        handler.ProgressChanged += (_, _) => notifications++;

        handler.HandleKey(Key("A"));

        var snapshot = handler.GetProgressSnapshot().Should().ContainSingle().Which;
        snapshot.Sequence.Should().Be("A, B");
        snapshot.MatchedGestureCount.Should().Be(1);
        snapshot.GestureCount.Should().Be(2);
        snapshot.IsCompletionPending.Should().BeFalse();
        notifications.Should().Be(1);

        handler.CancelPendingSequences();
        snapshot.MatchedGestureCount.Should().Be(1);
        notifications.Should().Be(2);
    }

    private static KeyEventArgs Key(string value, bool control = false) => new()
    {
        Character = value[0],
        ControlKey = control
    };
}
