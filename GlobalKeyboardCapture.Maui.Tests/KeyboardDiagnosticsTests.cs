using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class KeyboardDiagnosticsTests
{
    [Fact]
    public void EnabledDiagnosticsExposeNormalizedAndNativeMetadata()
    {
        var platform = new FakePlatformKeyHandler();
        var service = new KeyHandlerService(
            platform,
            NullLogger<KeyHandlerService>.Instance,
            new KeyHandlerOptions { EnableDiagnostics = true });
        KeyboardDiagnosticEventArgs? observed = null;
        service.DiagnosticEvent += (_, diagnostic) => observed = diagnostic;

        platform.Dispatch(new KeyEventArgs
        {
            Key = KeyboardKey.F1,
            Platform = KeyboardPlatform.Android,
            NativeKeyCode = 131,
            NativeScanCode = 59,
            Device = new KeyboardDeviceInfo(7, "USB Keyboard", isVirtual: false, isExternal: true)
        });

        observed.Should().NotBeNull();
        observed!.Stage.Should().Be(KeyboardDiagnosticStage.Dispatched);
        observed.NormalizedKey.Should().Be("F1");
        observed.Platform.Should().Be(KeyboardPlatform.Android);
        observed.NativeKeyCode.Should().Be(131);
        observed.Device?.Name.Should().Be("USB Keyboard");
    }

    [Fact]
    public void DisabledDiagnosticsHaveNoSubscriberOrLoggingCost()
    {
        var platform = new FakePlatformKeyHandler();
        var service = new KeyHandlerService(
            platform,
            NullLogger<KeyHandlerService>.Instance,
            new KeyHandlerOptions());
        var raised = false;
        service.DiagnosticEvent += (_, _) => raised = true;

        platform.Dispatch(new KeyEventArgs { Character = 'A' });

        raised.Should().BeFalse();
    }
}
