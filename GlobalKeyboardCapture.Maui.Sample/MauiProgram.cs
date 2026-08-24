using GlobalKeyboardCapture.Maui.Configuration;
using Microsoft.Extensions.Logging;

namespace GlobalKeyboardCapture.Maui.Sample
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
                    fonts.AddFont("FontAwesome6Brands-Regular-400.otf", "FABrands");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<MainPage>();

            var layoutTranslationCache =
                new global::GlobalKeyboardCapture.Maui.Core.Services.KeyboardLayoutTranslationCache();
            builder.Services.AddSingleton(layoutTranslationCache);

            builder.Services.AddKeyboardHandling(options =>
            {
                options.KeyboardLayoutTranslator = layoutTranslationCache;
                options.SequenceOverlapPolicy =
                    global::GlobalKeyboardCapture.Maui.Core.Models.SequenceOverlapPolicy.PreferLongest;
                options.EnableDiagnostics = true;
                options.CaptureKeyUp = true;
                options.AllowKeyRepeat = false;
                options.BarcodeProfiles.Add(new BarcodeScannerProfile("Retail")
                {
                    InterCharacterTimeout = TimeSpan.FromMilliseconds(150),
                    MaxAverageInterCharacterDelay = TimeSpan.FromMilliseconds(80),
                    MinLength = 5,
                    MaxLength = 128
                });
            });

            return builder.Build();
        }
    }
}
