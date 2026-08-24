using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Sample;

public partial class App : Application
{
    private readonly IDisposable _appHandlerRegistration;
    private readonly IDisposable _appHotkeyRegistration;

    public App(IKeyHandlerService keyHandlerService, HotkeyHandler hotkeyHandler)
    {
        InitializeComponent();

        _appHotkeyRegistration = hotkeyHandler.RegisterHotkey("Shift+Alt+X", () =>
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = Current?.Windows.FirstOrDefault()?.Page;
                if (page is not null)
                {
                    await page.DisplayAlertAsync(
                        "App-wide hotkey detected",
                        "Alt+Shift+X works across pages while this application is active.",
                        "Got it!");
                }
            }));
        _appHandlerRegistration = keyHandlerService.RegisterHandler(hotkeyHandler, priority: -100);
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new AppShell());
}
