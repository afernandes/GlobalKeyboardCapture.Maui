using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class GlobalHotkeyServiceTests
{
    [Fact]
    public void NoOpPlatformHandlerAcceptsSharedMauiProgramLifecycleCalls()
    {
        var handler = new NoOpPlatformKeyHandler();
        var view = new object();

        var act = () =>
        {
            handler.ConfigureHandler(_ => { });
            handler.Attach(view);
            handler.Detach(view).Should().BeTrue();
            handler.Cleanup();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void UnsupportedServiceReportsCapabilityAndFailsExplicitly()
    {
        var service = new UnsupportedGlobalHotkeyService();

        service.IsSupported.Should().BeFalse();
        service.IsAttached.Should().BeFalse();
        service.HotkeyCount.Should().Be(0);
        var act = () => service.RegisterHotkey(
            new KeyGesture('A', KeyModifiers.Control),
            () => { });

        act.Should().Throw<PlatformNotSupportedException>();
    }

    [Fact]
    public void UnsupportedServiceStringRegistrationStillValidatesGesture()
    {
        var service = new UnsupportedGlobalHotkeyService();

        ((Action)(() => service.RegisterHotkey("invalid gesture", () => { })))
            .Should().Throw<FormatException>();
    }
}
