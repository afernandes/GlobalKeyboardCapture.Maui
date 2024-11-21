using Android.App;
using Android.Content.PM;
using Android.Views;
using GlobalKeyboardCapture.Maui;
using GlobalKeyboardCapture.Maui.Core.Interfaces;

namespace GlobalKeyboardCapture.MauiSample
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, WindowSoftInputMode = SoftInput.StateVisible | SoftInput.AdjustResize)]
    public class MainActivity : MauiAppCompatActivity
    {

        //public override bool DispatchKeyEvent(KeyEvent e)
        //{
        //    var keyHandlerService = IPlatformApplication.Current.Services.GetService<IPlatformKeyHandler>();
        //    if (keyHandlerService is AndroidKeyHandler androidHandler)
        //    {
        //        var handled = androidHandler.DispatchKeyEvent(e);
        //        if (handled) return true;
        //    }

        //    return base.DispatchKeyEvent(e);
        //}
    }
}
