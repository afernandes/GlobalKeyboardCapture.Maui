using GlobalKeyboardCapture.Maui.Core.Interfaces;
using Microsoft.Maui.LifecycleEvents;

namespace GlobalKeyboardCapture.Maui.Configuration;

/// <summary>Provides MAUI lifecycle integration for keyboard capture.</summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Adds platform lifecycle callbacks that attach and detach native keyboard sources.
    /// </summary>
    /// <param name="builder">The MAUI application builder.</param>
    /// <returns>The same builder instance.</returns>
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
                .OnResume(activity => ResolveLifecycleHandler()?.OnPlatformViewCreated(activity))
                .OnDestroy(activity => ResolveLifecycleHandler()?.OnPlatformViewDestroyed(activity)));
#elif IOS || MACCATALYST
            events.AddiOS(ios => ios
                .FinishedLaunching((application, launchOptions) =>
                {
                    ResolveLifecycleHandler()?.OnPlatformViewCreated(application);
                    return true;
                })
                .WillTerminate(application =>
                    ResolveLifecycleHandler()?.OnPlatformViewDestroyed(application)));
#endif
        });

        return builder;
    }

    private static ILifecycleHandler? ResolveLifecycleHandler() =>
        IPlatformApplication.Current?.Services.GetService<ILifecycleHandler>()
        ?? Application.Current?.Handler?.MauiContext?.Services.GetService<ILifecycleHandler>();
}
