using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Sample;

public partial class MainPage : ContentPage
{
    private readonly IKeyHandlerService _keyHandlerService;
    private readonly KeyDisplayHandler _keyDisplayHandler;
    private readonly BarcodeHandler _barcodeHandler;
    private readonly HotkeyHandler _hotkeyHandler;

    private int _counter = 0;

    public MainPage(
        IKeyHandlerService keyHandlerService,
        BarcodeHandler barcodeHandler,
        HotkeyHandler hotkeyHandler)
    {
        InitializeComponent();

        _keyHandlerService = keyHandlerService;
        _barcodeHandler = barcodeHandler;
        _hotkeyHandler = hotkeyHandler;

        _keyDisplayHandler = new KeyDisplayHandler();

        SetupHandlers();
    }

    private void SetupHandlers()
    {
        _hotkeyHandler.RegisterHotkey("F2", ToggleEditMode);
        _hotkeyHandler.RegisterHotkey("Ctrl+S", SaveChanges);
        _hotkeyHandler.RegisterHotkey("ESC", CancelOperation);
        _hotkeyHandler.RegisterHotkey("E", false, false, true, RaiseTestButton); // Shift+E
        _hotkeyHandler.RegisterHotkey("VolumeUp", VolumeControlAction);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Key Display Handler
        _keyDisplayHandler.KeyPressed += OnKeyPressed;
        _keyHandlerService.RegisterHandler(_keyDisplayHandler);

        // Barcode Handler
        _barcodeHandler.BarcodeScanned += OnBarcodeScanned;
        _keyHandlerService.RegisterHandler(_barcodeHandler);

        // Hotkey Handler
        _keyHandlerService.RegisterHandler(_hotkeyHandler);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _keyHandlerService.UnregisterHandler(_keyDisplayHandler);
        _keyHandlerService.UnregisterHandler(_barcodeHandler);
        _keyHandlerService.UnregisterHandler(_hotkeyHandler);
        _keyDisplayHandler.KeyPressed -= OnKeyPressed;
        _barcodeHandler.BarcodeScanned -= OnBarcodeScanned;
    }


    private void OnKeyPressed(object? sender, string keyDisplay)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            KeyPressedLabel.Text = keyDisplay;
        });
    }

    private void OnBarcodeScanned(object? sender, string barcode)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ScannedCode.Text = barcode;
            Message.Text = $"Barcode scanned at {DateTime.Now:HH:mm:ss}";
        });
    }

    private void ToggleEditMode()
    {
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CounterBtn.IsEnabled = !CounterBtn.IsEnabled;
            Message.Text = $"Edit mode {(CounterBtn.IsEnabled ? "enabled" : "disabled")}";
        });
    }

    private void SaveChanges()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Message.Text = $"Changes saved at {DateTime.Now:HH:mm:ss}";
        });
    }

    private void CancelOperation()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Message.Text = "Operation cancelled";
        });
    }

    private void VolumeControlAction()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Message.Text = $"Volume control action triggered at {DateTime.Now:HH:mm:ss}";
        });
    }

    private void RaiseTestButton()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (CounterBtn.IsEnabled)
            {
                RaiseTest();
                Message.Text = $"Button activated by hotkey at {DateTime.Now:HH:mm:ss}";
            }
            else
            {
                Message.Text = $"Button not is enabled at {DateTime.Now:HH:mm:ss}";
            }
        });
    }
    
    private void OnCounterClicked(object sender, EventArgs e)
    {
        RaiseTest();
    }

    private void RaiseTest()
    {
        _counter++;
        CounterBtn.Text = $"Clicked {_counter} {(_counter == 1 ? "time" : "times")}";
    }
}
