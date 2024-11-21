# GlobalKeyboardCapture.Maui

A powerful .NET MAUI library for global keyboard capture with strong support for barcode scanners. Provides system-wide key interception, hotkeys management.

[![NuGet](https://img.shields.io/nuget/v/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![Downloads](https://img.shields.io/nuget/dt/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)

![GlobalKeyboardCapture.Maui Demo](print.png)

*Demo application showing key capture, barcode scanning, and hotkeys functionality*

## Features

- 🔑 Global keyboard capture
- 📊 Advanced keyboard input processing
- 🏷️ Built-in barcode scanner support
- ⌨️ Customizable hotkeys system
- 📱 Cross-platform (Windows & Android)
- 🎛️ Highly configurable
- 🧩 Easy to integrate
- 🔧 Built for .NET MAUI

## Common Use Cases

- Barcode scanner integration
- Global hotkeys and shortcuts
- System-wide keyboard monitoring
- Custom keyboard input handling
- Input automation
- Multi-mode keyboard capture

## Installation

```bash
dotnet add package GlobalKeyboardCapture.Maui
```

Or via the NuGet Package Manager:

```
Install-Package GlobalKeyboardCapture.Maui
```

## Quick Start

1. Register the service in your `MauiProgram.cs`:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .UseKeyboardHandling();

    builder.Services.AddKeyboardHandling(options =>
    {
        // Barcode specific settings
        options.BarcodeTimeout = 150;
        options.MinBarcodeLength = 8;
    });

    return builder.Build();
}
```

2. Basic usage example:

```csharp
public partial class MainPage : ContentPage
{
    private readonly IKeyHandlerService _keyHandlerService;
    private readonly BarcodeHandler _barcodeHandler;
    private readonly HotkeyHandler _hotkeyHandler;

    public MainPage(
        IKeyHandlerService keyHandlerService, 
        BarcodeHandler barcodeHandler,
        HotkeyHandler hotkeyHandler)
    {
        InitializeComponent();
        
        _keyHandlerService = keyHandlerService;
        _barcodeHandler = barcodeHandler;
        _hotkeyHandler = hotkeyHandler;
        
        SetupHandlers();
    }

    private void SetupHandlers()
    {
        // Setup barcode handling
        _barcodeHandler.BarcodeScanned += (sender, input) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProcessInput(input);
            });
        };

        // Setup global hotkeys
        _hotkeyHandler.RegisterHotkey("F2", () =>
        {
            EnableEditMode();
        });

        // Register handlers
        _keyHandlerService.RegisterHandler(_barcodeHandler);           
        _keyHandlerService.RegisterHandler(_hotkeyHandler);
    }
}
```

## Advanced Usage

### Custom Key Handler

Create your own key handler for specific needs:

```csharp
public class CustomKeyHandler : IKeyHandler
{
    public bool ShouldHandle(string key) => true;

    public void HandleKey(string key)
    {
        // Your custom key handling logic
    }
}
```

### Global Hotkeys

```csharp
// Single key hotkeys
_hotkeyHandler.RegisterHotkey("F2", EnableEditMode);
_hotkeyHandler.RegisterHotkey("ESC", CancelOperation);

// Modifier key combinations
_hotkeyHandler.RegisterHotkey("Shift+S", SaveAction);
_hotkeyHandler.RegisterHotkey("Ctrl+Alt+P", PrintAction);
_hotkeyHandler.RegisterHotkey("Ctrl+Alt+Shift+P", PrintAction);

//Special keys
_hotkeyHandler.RegisterHotkey("VolumeUp", VolumeControlAction); //Android Volume Up
_hotkeyHandler.RegisterHotkey("OEM173", VolumeControlAction); //OEM 173
```

### Barcode Scanner Mode

```csharp
_barcodeHandler.BarcodeScanned += (sender, input) =>
{    
    ProcessProduct(input); 
};
```

## Key Features

### Global Key Capture
- Capture keyboard input regardless of focus
- Works with all UI controls
- System-wide key interception

### Input Processing
- Configurable input timeout
- Input validation and filtering
- Custom processing rules

### Platform Support
- Windows desktop applications
- Android mobile applications
- Consistent API across platforms

## API Reference

### Core Services

- `IKeyHandlerService`: Main service for global keyboard handling
- `BarcodeHandler`: Specialized handler for barcode input scenarios
- `HotkeyHandler`: Global hotkeys management

### Interfaces

- `IKeyHandler`: Base interface for custom handlers
- `ILifecycleHandler`: Application lifecycle management

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Anderson Fernandes do Nascimento

## Support

If you encounter any issues or need help, please [open an issue](https://github.com/afernandes/GlobalKeyboardCapture.Maui/issues).
