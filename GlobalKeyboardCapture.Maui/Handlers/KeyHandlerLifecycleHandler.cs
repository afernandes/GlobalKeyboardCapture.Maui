using GlobalKeyboardCapture.Maui.Core.Interfaces;

namespace GlobalKeyboardCapture.Maui.Handlers;

public class KeyHandlerLifecycleHandler : ILifecycleHandler
{
    private readonly IKeyHandlerService _keyHandlerService;
    private bool _isInitialized;

    public KeyHandlerLifecycleHandler(IKeyHandlerService keyHandlerService)
    {
        _keyHandlerService = keyHandlerService;
    }

    public void OnStart()
    {
        InitializeIfNeeded();
    }

    public void OnResume()
    {
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (_isInitialized) return;

#if WINDOWS
        var window = Application.Current.Windows.FirstOrDefault()?.Handler.PlatformView as Microsoft.UI.Xaml.Window;
        if (window != null)
        {
            _keyHandlerService.Initialize(window);
            _isInitialized = true;
        }
#elif ANDROID
            var activity = Platform.CurrentActivity;
            if (activity?.Window?.DecorView?.RootView != null)
            {
                _keyHandlerService.Initialize(activity.Window.DecorView.RootView);
                _isInitialized = true;
            }
#endif
    }

    public void OnStop()
    {
        // Opcional: limpar recursos se necessário
    }
}
