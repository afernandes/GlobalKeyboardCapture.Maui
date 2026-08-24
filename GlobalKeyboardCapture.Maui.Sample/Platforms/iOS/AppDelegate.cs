using Foundation;

using UIKit;

namespace GlobalKeyboardCapture.Maui.Sample;

[Register("AppDelegate")]
public sealed class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        AppleKeyboardLayoutCollector.Record(presses);
        base.PressesBegan(presses, evt);
    }
}
