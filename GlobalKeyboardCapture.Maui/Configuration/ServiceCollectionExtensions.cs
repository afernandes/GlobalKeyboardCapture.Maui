using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui;

namespace GlobalKeyboardCapture.Maui.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeyboardHandling(
        this IServiceCollection services,
        Action<KeyHandlerOptions>? configure = null)
    {
        var options = new KeyHandlerOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IKeyHandlerService, KeyHandlerService>();
        services.AddTransient<BarcodeHandler>();
        services.AddTransient<HotkeyHandler>();
        services.AddSingleton<ILifecycleHandler, KeyHandlerLifecycleHandler>();

#if WINDOWS
        services.AddSingleton<IPlatformKeyHandler, WindowsKeyHandler>();
#elif ANDROID
        services.AddSingleton<IPlatformKeyHandler, AndroidKeyHandler>();
#endif

        return services;
    }
}
