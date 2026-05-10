using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;

namespace GlobalKeyboardCapture.Maui.Tests;

public class BarcodeHandlerTests
{
    private static (BarcodeHandler handler, FakeTimeProvider time, List<string> scanned) Build(
        int minLength = 5,
        int timeoutMs = 100)
    {
        var options = new KeyHandlerOptions { MinBarcodeLength = minLength, BarcodeTimeout = timeoutMs };
        var time = new FakeTimeProvider();
        var handler = new BarcodeHandler(options, time);
        var scanned = new List<string>();
        handler.BarcodeScanned += (_, code) => scanned.Add(code);
        return (handler, time, scanned);
    }

    [Fact]
    public void ShouldHandleAcceptsCharactersWithoutModifiers()
    {
        var (handler, _, _) = Build();
        handler.ShouldHandle(new KeyEventArgs { Character = 'A' }).Should().BeTrue();
    }

    [Fact]
    public void ShouldHandleAcceptsEnterWithoutModifiers()
    {
        var (handler, _, _) = Build();
        handler.ShouldHandle(new KeyEventArgs { EnterKey = true }).Should().BeTrue();
    }

    [Fact]
    public void ShouldHandleRejectsKeysWithModifiers()
    {
        var (handler, _, _) = Build();
        handler.ShouldHandle(new KeyEventArgs { Character = 'A', ControlKey = true }).Should().BeFalse();
    }

    [Fact]
    public void AccumulatesAndScansOnEnter()
    {
        var (handler, time, scanned) = Build(minLength: 5);

        foreach (var c in "12345")
        {
            handler.HandleKey(new KeyEventArgs { Character = c });
            time.Advance(TimeSpan.FromMilliseconds(10));
        }

        var enter = new KeyEventArgs { EnterKey = true };
        handler.HandleKey(enter);

        scanned.Should().ContainSingle().Which.Should().Be("12345");
        enter.Handled.Should().BeTrue();
    }

    [Fact]
    public void ShortBufferDoesNotEmitAndDoesNotMarkEnterHandled()
    {
        var (handler, time, scanned) = Build(minLength: 5);

        foreach (var c in "123")
        {
            handler.HandleKey(new KeyEventArgs { Character = c });
            time.Advance(TimeSpan.FromMilliseconds(10));
        }

        var enter = new KeyEventArgs { EnterKey = true };
        handler.HandleKey(enter);

        scanned.Should().BeEmpty();
        enter.Handled.Should().BeFalse();
    }

    [Fact]
    public void TimeoutClearsBufferBeforeNextKey()
    {
        var (handler, time, scanned) = Build(minLength: 4, timeoutMs: 100);

        handler.HandleKey(new KeyEventArgs { Character = 'A' });
        handler.HandleKey(new KeyEventArgs { Character = 'B' });
        time.Advance(TimeSpan.FromMilliseconds(500));
        handler.HandleKey(new KeyEventArgs { Character = 'C' });
        handler.HandleKey(new KeyEventArgs { Character = 'D' });
        handler.HandleKey(new KeyEventArgs { Character = 'E' });
        handler.HandleKey(new KeyEventArgs { Character = 'F' });
        handler.HandleKey(new KeyEventArgs { EnterKey = true });

        scanned.Should().ContainSingle().Which.Should().Be("CDEF");
    }

    [Fact]
    public void DisposeMakesHandleKeyThrow()
    {
        var (handler, _, _) = Build();
        handler.Dispose();

        Action act = () => handler.HandleKey(new KeyEventArgs { Character = 'X' });
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void DisposeIsIdempotent()
    {
        var (handler, _, _) = Build();
        handler.Dispose();
        Action act = () => handler.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void DefaultConstructorUsesSystemTimeProvider()
    {
        var options = new KeyHandlerOptions { MinBarcodeLength = 1 };
        var handler = new BarcodeHandler(options);
        handler.Should().NotBeNull();
    }
}
