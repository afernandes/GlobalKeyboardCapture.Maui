using GlobalKeyboardCapture.Maui.Core.Interfaces;
using Microsoft.Maui.LifecycleEvents;

namespace GlobalKeyboardCapture.Maui.Configuration;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseKeyboardHandling(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(windows => windows
                .OnWindowCreated(window => ResolveLifecycleHandler()?.OnPlatformViewCreated(window))
                .OnClosed((window, args) => ResolveLifecycleHandler()?.OnPlatformViewDestroyed(window)));
#elif ANDROID
            events.AddAndroid(android => android
                .OnCreate((activity, bundle) => ResolveLifecycleHandler()?.OnPlatformViewCreated(activity))
                .OnDestroy(activity => ResolveLifecycleHandler()?.OnPlatformViewDestroyed(activity)));
#endif
        });

        return builder;
    }

    private static ILifecycleHandler? ResolveLifecycleHandler() =>
        IPlatformApplication.Current?.Services.GetService<ILifecycleHandler>()
        ?? Application.Current?.Handler?.MauiContext?.Services.GetService<ILifecycleHandler>();
}
