using Maui.GlobalKeyboardCapture.Configuration;
using Maui.GlobalKeyboardCaptureSample;
using Microsoft.Extensions.Logging;

namespace Maui.GlobalKeyboardCapture.Sample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseKeyboardHandling()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<MainPage>();

            // Adiciona o serviço de teclado com configurações personalizadas
            builder.Services.AddKeyboardHandling(options =>
            {
                options.BarcodeTimeout = 150;
                options.MinBarcodeLength = 5;
            });

            return builder.Build();
        }
    }
}
