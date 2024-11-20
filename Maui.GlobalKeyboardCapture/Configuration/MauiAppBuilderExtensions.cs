using Maui.GlobalKeyboardCapture.Core.Interfaces;
using Microsoft.Maui.LifecycleEvents;

namespace Maui.GlobalKeyboardCapture.Configuration;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseKeyboardHandling(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(windows => windows
                .OnLaunched((application, args) =>
                {
                    var handler = Application.Current.Handler.MauiContext.Services.GetService<ILifecycleHandler>();
                    handler?.OnStart();
                }));
#elif ANDROID
            events.AddAndroid(android => android
                .OnResume(activity =>
                {
                    var handler = Application.Current.Handler.MauiContext.Services.GetService<ILifecycleHandler>();
                    handler?.OnResume();
                })
                .OnCreate((activity, bundle) =>
                {
                    var handler = Application.Current.Handler.MauiContext.Services.GetService<ILifecycleHandler>();
                    handler?.OnStart();
                }));
#endif
        });

        return builder;
    }
}