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
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Current!.Windows[0].Page!.DisplayAlert(
                        "Global Hotkey Detected",
                        "This hotkey (Alt+Shift+X) is registered globally and works across all pages in the application.\r\n" +
                        "It demonstrates the library's ability to capture keyboard input at the application level.",
                        "Got it!");
                });
            });

            keyHandlerService.RegisterHandler(hotkeyHandler);
        }

        public T GetService<T>()
        {
            if (Current is { Handler.MauiContext: null })
                throw new InvalidOperationException();

            return Current!.Handler!.MauiContext.Services.GetService<T>() ?? throw new InvalidOperationException();
        }
    }
}
