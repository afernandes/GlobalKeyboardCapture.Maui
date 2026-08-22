# GlobalKeyboardCapture.Maui

A .NET MAUI library for application-wide keyboard capture with strong support for keyboard-wedge barcode scanners and hotkey management. Events are captured while one of the app's windows is active; operating-system-wide hotkeys are a separate, Windows-specific capability.

[![NuGet](https://img.shields.io/nuget/v/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![NuGet](https://img.shields.io/nuget/dt/GlobalKeyboardCapture.Maui.svg?label=Nuget&maxAge=60)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg?maxAge=60)](https://raw.githubusercontent.com/afernandes/GlobalKeyboardCapture.Maui/master/LICENSE)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui?ref=badge_shield)
[![.NET Support](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=afernandes_GlobalKeyboardCapture.Maui&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=afernandes_GlobalKeyboardCapture.Maui)

![GlobalKeyboardCapture.Maui Demo](https://raw.githubusercontent.com/afernandes/Maui.GlobalKeyboardCapture/refs/heads/main/Print.png)

*Demo application showing key capture, barcode scanning, and hotkeys functionality*

## Features

- 🔑 Application-wide keyboard capture
- 📊 Advanced keyboard input processing
- 🏷️ Built-in barcode scanner support
- ⌨️ Customizable hotkeys system
- 📱 Cross-platform (Windows & Android)
- 🎛️ Highly configurable
- 🧩 Easy to integrate
- 🔧 Built for .NET MAUI

## Common Use Cases

- Barcode scanner integration
- App-wide hotkeys and shortcuts
- Keyboard monitoring inside the active application window
- Custom keyboard input handling
- Input automation
- Multi-mode keyboard capture

## Full key support:
- Standard keys (A-Z, 0-9)
- Function keys (F1-F24 on Windows, F1-F12 on Android)
- Modifier keys (Ctrl, Alt, Shift)
- Windows OEM keys (;, /, [, ], etc)
- Android special keys (Volume, Back, Menu)
- Navigation keys
- Special characters

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
        options.MaxBarcodeLength = 4096;  // upper bound on the scan buffer (anti-runaway)

        // When true, stops invoking later handlers once one sets KeyEventArgs.Handled.
        // Default is false (every registered handler sees every key).
        options.StopOnHandled = false;
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
    public bool ShouldHandle(KeyEventArgs key) => true;

    public void HandleKey(KeyEventArgs key)
    {
        // key.ToString() yields the normalized combo, e.g. "Ctrl+S", "F2", "Enter".
        var combo = key.ToString();

        // Your custom key handling logic.
        // Set key.Handled = true to stop the key from propagating further.
    }
}
```
### Global Hotkeys

The library features a smart hotkey normalization system that allows flexible registration of keyboard shortcuts:

```csharp
// Single key hotkeys
_hotkeyHandler.RegisterHotkey("F2", EnableEditMode);
_hotkeyHandler.RegisterHotkey("ESC", CancelOperation);

// All these registrations trigger the same hotkey (Ctrl+Alt+Shift+P)
_hotkeyHandler.RegisterHotkey("Ctrl+Alt+Shift+P", PrintAction);
_hotkeyHandler.RegisterHotkey("Shift+Ctrl+Alt+P", PrintAction);
_hotkeyHandler.RegisterHotkey("Alt+Shift+Control+P", PrintAction);

// Supports common aliases
_hotkeyHandler.RegisterHotkey("Control+Alt+P", PrintAction);  // "Control" is normalized to "Ctrl"
_hotkeyHandler.RegisterHotkey("Windows+Shift+X", ActionX);    // "Windows" is normalized to "Win"

// Case-insensitive handling
_hotkeyHandler.RegisterHotkey("CTRL+ALT+P", PrintAction);
_hotkeyHandler.RegisterHotkey("ctrl+alt+p", PrintAction);

//Special keys
_hotkeyHandler.RegisterHotkey("VolumeUp", VolumeControlAction); //Android Volume Up
_hotkeyHandler.RegisterHotkey("OEM173", VolumeControlAction); //OEM 173
```

The hotkey system provides:
- Automatic normalization of modifier order (Ctrl → Alt → Shift → Win)
- Case-insensitive handling
- Common alias support (e.g., "Control" → "Ctrl")
- Consistent behavior regardless of registration order
- High-performance implementation with minimal allocations

Registered hotkeys can be removed individually or all at once:

```csharp
_hotkeyHandler.UnregisterHotkey("Ctrl+Alt+Shift+P"); // by string (same normalization)
_hotkeyHandler.UnregisterHotkey(comboKeyEventArgs);  // by KeyEventArgs
_hotkeyHandler.ClearHotkeys();                        // remove every registered hotkey
```

### Barcode Scanner Mode

```csharp
_barcodeHandler.BarcodeScanned += (sender, input) =>
{    
    ProcessProduct(input); 
};
```

### Performance Optimizations

The library is optimized for performance and efficiency:

- **Minimal Allocations**: Uses modern .NET features to minimize garbage collection pressure
- **Fast Lookups**: Optimized dictionary implementations for hotkey matching
- **Efficient String Handling**: Smart string comparison and normalization
- **Aggressive Inlining**: Critical paths are optimized for speed
- **Memory Efficiency**: Careful management of memory allocations in hot paths

## Key Features

### Application-wide Key Capture
- Capture keyboard input regardless of focus
- Works with all UI controls
- Key interception inside the active application window

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


[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui?ref=badge_large)

## Author

Anderson Fernandes do Nascimento

## Support

If you encounter any issues or need help, please [open an issue](https://github.com/afernandes/GlobalKeyboardCapture.Maui/issues).
