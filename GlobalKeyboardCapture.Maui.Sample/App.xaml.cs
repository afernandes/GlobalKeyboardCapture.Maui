using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnStart()
        {
            base.OnStart();
            var keyHandlerService = GetService<IKeyHandlerService>();
            var hotkeyHandler = GetService<HotkeyHandler>();

            // The hotkey string will be automatically normalized to a consistent format.
            // This means you can register hotkeys with modifiers in any order, and they will be standardized to:
            // "Ctrl+Alt+Shift+Win+Key" format. For example:
            // - "Shift+Alt+X" becomes "Alt+Shift+X"
            // - "Alt+Shift+X" becomes "Alt+Shift+X"
            // - "X+Alt+Shift" becomes "Alt+Shift+X"
            // The handler will treat all these variations as the same hotkey.
            hotkeyHandler.RegisterHotkey("Shift+Alt+X", () =>
            {
                _ = MainThread.InvokeOnMainThreadAsync(() =>
                    Current!.Windows[0].Page!.DisplayAlertAsync(
                        "App-wide Hotkey Detected",
                        "This hotkey (Alt+Shift+X) works across all pages while this application is active.",
                        "Got it!"));
            });

            keyHandlerService.RegisterHandler(hotkeyHandler);
        }

        public static T GetService<T>()
        {
            if (Current is { Handler.MauiContext: null })
                throw new InvalidOperationException();

            return Current!.Handler!.MauiContext.Services.GetService<T>() ?? throw new InvalidOperationException();
        }
    }
}
