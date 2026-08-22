using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Handlers;

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
        services.AddTransient<KeySequenceHandler>();

#if WINDOWS
        services.AddSingleton<IPlatformKeyHandler, WindowsKeyHandler>();
        services.AddSingleton<WindowsGlobalHotkeyService>();
        services.AddSingleton<IGlobalHotkeyService>(provider => provider.GetRequiredService<WindowsGlobalHotkeyService>());
        services.AddSingleton<IPlatformViewLifecycleSink>(provider => provider.GetRequiredService<WindowsGlobalHotkeyService>());
#elif ANDROID
        services.AddSingleton<IPlatformKeyHandler, AndroidKeyHandler>();
        services.AddSingleton<IGlobalHotkeyService, UnsupportedGlobalHotkeyService>();
#elif IOS || MACCATALYST
        services.AddSingleton<IPlatformKeyHandler, AppleKeyHandler>();
        services.AddSingleton<IGlobalHotkeyService, UnsupportedGlobalHotkeyService>();
#else
        services.AddSingleton<IPlatformKeyHandler, NoOpPlatformKeyHandler>();
        services.AddSingleton<IGlobalHotkeyService, UnsupportedGlobalHotkeyService>();
#endif

        services.AddSingleton<ILifecycleHandler>(provider => new KeyHandlerLifecycleHandler(
            provider.GetRequiredService<IKeyHandlerService>(),
            provider.GetServices<IPlatformViewLifecycleSink>()));

        return services;
    }
}
