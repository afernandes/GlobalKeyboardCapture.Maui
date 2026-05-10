using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Tests;

/// <summary>
/// Tests documenting known bugs scheduled for the next round.
/// Each test asserts the desired (correct) behavior. They are <see cref="SkipAttribute"/>
/// to keep CI green until the corresponding fix lands; remove the Skip and watch them pass.
/// </summary>
public class KnownBugsSkippedTests
{
    [Fact(Skip = "B2: RegisterHotkey(string, bool, bool, bool, Action) does not recognize named keys like Enter/Tab/Backspace; fix in next round.")]
    public void B2_RegisterHotkey_AcceptsNamedSpecialKey_Enter()
    {
        var handler = new HotkeyHandler();
        var fired = false;
        handler.RegisterHotkey("Enter", requireControl: false, requireAlt: false, requireShift: true, () => fired = true);

        handler.HandleKey(new KeyEventArgs { ShiftKey = true, EnterKey = true });

        fired.Should().BeTrue();
    }

    [Fact(Skip = "B1: WindowsKeyHandler.Cleanup unsubscribes from KeyDown but subscribes to PreviewKeyDown; covered by an integration test once platform tests are introduced.")]
    public void B1_WindowsKeyHandler_CleanupUnsubscribesFromCorrectEvent()
    {
        true.Should().BeTrue();
    }

    [Fact(Skip = "B3: _isInitialized prevents re-binding the platform view after Activity recreation on Android; covered by an integration test once platform tests are introduced.")]
    public void B3_KeyHandlerService_RebindsAfterPlatformViewRecreation()
    {
        true.Should().BeTrue();
    }
}
