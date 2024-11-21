using Maui.GlobalKeyboardCapture.Core.Interfaces;
using Maui.GlobalKeyboardCapture.Handlers;

namespace Maui.GlobalKeyboardCapture.Sample;

public partial class MainPage : ContentPage
{
    private readonly IKeyHandlerService _keyHandlerService;
    private readonly KeyDisplayHandler _keyDisplayHandler;
    private readonly BarcodeHandler _barcodeHandler;
    private readonly HotkeyHandler _hotkeyHandler;

    private int _counter = 0;
    private bool _isEditMode = false;

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
        _hotkeyHandler.RegisterHotkey("F2", false, false, false, ToggleEditMode);
        _hotkeyHandler.RegisterHotkey("Ctrl+S", SaveChanges);
        _hotkeyHandler.RegisterHotkey("ESC", false, false, false, CancelOperation);
        _hotkeyHandler.RegisterHotkey("E", false, false, true, EnableSpecialMode); // Shift+E
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


    private void OnKeyPressed(object sender, string keyDisplay)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            KeyPressedLabel.Text = keyDisplay;
        });
    }

    private void OnBarcodeScanned(object sender, string barcode)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ScannedCode.Text = barcode;
            Message.Text = $"Barcode scanned at {DateTime.Now:HH:mm:ss}";
        });
    }

    private void ToggleEditMode()
    {
        _isEditMode = !_isEditMode;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CounterBtn.IsEnabled = _isEditMode;
            Message.Text = $"Edit mode {(_isEditMode ? "enabled" : "disabled")}";
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

    private void EnableSpecialMode()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Message.Text = $"Special mode activated at {DateTime.Now:HH:mm:ss}";
        });
    }

    private void VolumeControlAction()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Message.Text = $"Volume control action triggered at {DateTime.Now:HH:mm:ss}";
        });
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        _counter++;
        CounterBtn.Text = $"Clicked {_counter} {(_counter == 1 ? "time" : "times")}";
    }

    
}
