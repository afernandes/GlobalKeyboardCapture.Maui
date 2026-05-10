using GlobalKeyboardCapture.Maui.Core.Interfaces;

namespace GlobalKeyboardCapture.Maui.Handlers;

public class KeyHandlerLifecycleHandler : ILifecycleHandler
{
    private readonly IKeyHandlerService _keyHandlerService;

    public KeyHandlerLifecycleHandler(IKeyHandlerService keyHandlerService)
    {
        _keyHandlerService = keyHandlerService;
    }

    public void OnStart()
    {
        BindCurrentPlatformView();
    }

    public void OnResume()
    {
        BindCurrentPlatformView();
    }

    private void BindCurrentPlatformView()
    {
#if WINDOWS
        var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (window != null)
        {
            _keyHandlerService.Initialize(window);
        }
#elif ANDROID
        var activity = Platform.CurrentActivity;
        if (activity?.Window?.DecorView?.RootView != null)
        {
            _keyHandlerService.Initialize(activity.Window.DecorView.RootView);
        }
#endif
    }

    public void OnStop()
    {
    }
}
