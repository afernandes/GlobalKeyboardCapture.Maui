using Maui.GlobalKeyboardCapture.Core.Interfaces;
using Maui.GlobalKeyboardCapture.Handlers;

namespace Maui.GlobalKeyboardCapture.Sample
{
    public partial class MainPage : ContentPage
    {
        private readonly IKeyHandlerService _keyHandlerService;
        private readonly BarcodeHandler _barcodeHandler;
        private readonly HotkeyHandler _hotkeyHandler;

        int count = 0;

        public MainPage(
            IKeyHandlerService keyHandlerService,
            BarcodeHandler barcodeHandler,
            HotkeyHandler hotkeyHandler)
        {
            InitializeComponent();

            _keyHandlerService = keyHandlerService;
            _barcodeHandler = barcodeHandler;
            _hotkeyHandler = hotkeyHandler;

            // Configura handlers
            SetupHandlers();
        }

        private void SetupHandlers()
        {
            _barcodeHandler.BarcodeScanned += OnBarcodeScanned;

            _hotkeyHandler.RegisterHotkey("F2", false, false, false, EnableEditMode);
            _hotkeyHandler.RegisterHotkey("S", true, false, false, SaveCurrentItem);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _keyHandlerService.RegisterHandler(_barcodeHandler);
            _keyHandlerService.RegisterHandler(_hotkeyHandler);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _keyHandlerService.UnregisterHandler(_barcodeHandler);
            _keyHandlerService.UnregisterHandler(_hotkeyHandler);
        }


        private async void SaveCurrentItem()
        {
            Message.Text = "Ctrl+S fired!";
            await Task.Delay(3000);
            Message.Text = string.Empty;
        }

        private void EnableEditMode()
        {
            CounterBtn.IsEnabled = !CounterBtn.IsEnabled;
        }

        private void OnBarcodeScanned(object? sender, string barcode)
        {
            ScannedCode.Text = barcode;
        }
        
        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
