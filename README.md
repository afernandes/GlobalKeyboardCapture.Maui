# GlobalKeyboardCapture.Maui

Application-wide physical-keyboard capture for .NET MAUI, with typed hotkeys, keyboard-wedge barcode scanning, key sequences, diagnostics, and Windows operating-system global hotkeys.

[![NuGet](https://img.shields.io/nuget/v/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![NuGet downloads](https://img.shields.io/nuget/dt/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_MAUI-10.0-512BD4)](https://dotnet.microsoft.com/apps/maui)

> **Application-wide is not OS-global.** Normal handlers capture keys while one of the application's windows is active. `IGlobalHotkeyService` is a separate Windows-only feature that can activate while another application has focus.

## Supported versions and platforms

| Package line | .NET MAUI | Android | Windows | iOS | Mac Catalyst | Policy |
|---|---:|:---:|:---:|:---:|:---:|---|
| 2.x | 10 | ✅ | ✅ | ✅ | ✅ | Active feature development |
| 1.x | 8 | ✅ | ✅ | — | — | Compatibility maintenance on `codex/net8-maintenance-1.x` |

.NET MAUI 8 workloads are outside Microsoft's supported lifecycle. The 1.x branch remains available for compatibility, but cannot receive platform security fixes that Microsoft no longer publishes. New platforms and features are developed on 2.x.

Minimum platform versions for 2.x are Android 5.0 (API 21), Windows 10 build 17763, iOS 15, and Mac Catalyst 15.

## Highlights

- Deterministic, priority-ordered handler pipeline with disposable registrations.
- Typed `KeyboardKey`, `KeyModifiers`, and `KeyGesture` APIs plus compatible string aliases.
- Scanner profiles with device filters, timing limits, prefixes, suffixes, custom terminators, and bounded buffers.
- Capture scopes and nest-safe suspension tokens for pages, dialogs, and focused text controls.
- Structured native diagnostics and input-device metadata.
- Optional key-up and auto-repeat capture.
- Non-blocking asynchronous handlers with cancellation and event snapshots.
- Ordered key sequences such as `Ctrl+K`, then `Ctrl+C`.
- Multiple-window lifecycle handling on supported platforms.
- True Windows global hotkeys through `RegisterHotKey` / `WM_HOTKEY`.
- Trimming and AOT analysis with no reflection-based preservation rules.

## Installation

```bash
dotnet add package GlobalKeyboardCapture.Maui
```

## Configure MAUI

Both calls are required: one wires native lifecycle events and the other registers services.

```csharp
using GlobalKeyboardCapture.Maui.Configuration;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();

    builder
        .UseMauiApp<App>()
        .UseKeyboardHandling();

    builder.Services.AddKeyboardHandling(options =>
    {
        options.StopOnHandled = true;
        options.CaptureKeyUp = false;
        options.AllowKeyRepeat = false;
        options.EnableDiagnostics = false;

        options.BarcodeProfiles.Add(new BarcodeScannerProfile("Retail")
        {
            InterCharacterTimeout = TimeSpan.FromMilliseconds(120),
            MaxAverageInterCharacterDelay = TimeSpan.FromMilliseconds(60),
            MinLength = 5,
            MaxLength = 128
        });
    });

    return builder.Build();
}
```

`StopOnHandled` defaults to `false` for 1.x compatibility. When enabled, dispatch stops after the first handler that sets `KeyEventArgs.Handled = true`.

## Register handlers safely

Use a scope or retain the returned registration token. Higher priorities run first; equal priorities preserve registration order.

```csharp
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Handlers;

public partial class CheckoutPage : ContentPage
{
    private readonly IKeyHandlerService _keyboard;
    private readonly BarcodeHandler _barcode;
    private readonly HotkeyHandler _hotkeys;
    private IKeyboardCaptureScope? _scope;

    public CheckoutPage(
        IKeyHandlerService keyboard,
        BarcodeHandler barcode,
        HotkeyHandler hotkeys)
    {
        InitializeComponent();
        _keyboard = keyboard;
        _barcode = barcode;
        _hotkeys = hotkeys;

        _barcode.ScanCompleted += (_, scan) =>
            MainThread.BeginInvokeOnMainThread(() => ProcessBarcode(scan.Value));

        _hotkeys.RegisterHotkey("F2", EnableEditMode);
        _hotkeys.RegisterHotkey("Ctrl+S", Save);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _scope = _keyboard.CreateScope("Checkout");
        _scope.RegisterHandler(_barcode, priority: 100);
        _scope.RegisterHandler(_hotkeys, priority: 50);
    }

    protected override void OnDisappearing()
    {
        _scope?.Dispose();
        _scope = null;
        base.OnDisappearing();
    }
}
```

Calling `RegisterHandler` more than once for the same handler and scope is idempotent. Disposing an old token never removes a newer replacement registration.

## Typed gestures and app-wide hotkeys

Typed gestures avoid spelling errors and are the preferred API for reusable code.

```csharp
using GlobalKeyboardCapture.Maui.Core.Models;

IDisposable saveRegistration = hotkeys.RegisterHotkey(
    new KeyGesture('S', KeyModifiers.Control),
    Save);

IDisposable escapeRegistration = hotkeys.RegisterHotkey(
    new KeyGesture(KeyboardKey.Escape),
    Cancel);

saveRegistration.Dispose();
```

The string API remains available and is case/order insensitive. Aliases include `Control` → `Ctrl`, `Command`/`Windows` → `Win`, `Escape` → `Esc`, `Return` → `Enter`, `PgUp`, `PgDn`, `PrtSc`, and `Pause`.

`KeyGesture.ToString()` and `KeyEventArgs.ToString()` use the canonical modifier order `Ctrl+Alt+Shift+Win`. That representation is a compatibility contract.

## Windows OS-global hotkeys

Resolve `IGlobalHotkeyService` and check capability before registration. Other platforms provide a discoverable unsupported implementation instead of failing dependency injection.

```csharp
private IDisposable? _globalRegistration;

void RegisterGlobalShortcut(IGlobalHotkeyService globalHotkeys)
{
    if (!globalHotkeys.IsSupported || !globalHotkeys.IsAttached)
        return;

    _globalRegistration = globalHotkeys.RegisterHotkey(
        "Ctrl+Alt+G",
        BringApplicationToFront,
        suppressRepeat: true);
}
```

Windows can reject a combination already owned by another process. Registration then throws `InvalidOperationException` with the native error as its inner exception. Dispose the token or call `ClearHotkeys()` during teardown.

## Scanner profiles

Profiles are snapshotted when `BarcodeHandler` is created. Configure them during service registration.

```csharp
var profile = new BarcodeScannerProfile("Warehouse")
{
    Prefix = "STX",
    Suffix = "ETX",
    RequirePrefix = true,
    RequireSuffix = true,
    StripPrefix = true,
    StripSuffix = true,
    InterCharacterTimeout = TimeSpan.FromMilliseconds(150),
    MaxAverageInterCharacterDelay = TimeSpan.FromMilliseconds(70),
    MinLength = 8,
    MaxLength = 512,
    DeviceId = 12
};

profile.TerminatorKeys.Clear();
profile.TerminatorKeys.Add(KeyboardKey.Tab);
profile.TerminatorCharacters.Add('\r');
options.BarcodeProfiles.Add(profile);
```

`ScanCompleted` exposes decoded and raw values, profile, duration, character count, terminator, and device metadata. `BarcodeScanned` remains as a value-only compatibility event. An unterminated stream is reset at `MaxLength`, preventing unbounded memory growth.

## Suspend capture around text input

Suspension tokens are nest-safe: capture resumes only after every active token is disposed.

```csharp
private IDisposable? _textInputSuspension;

void OnEntryFocused(object? sender, FocusEventArgs args) =>
    _textInputSuspension ??= keyboard.SuspendCapture();

void OnEntryUnfocused(object? sender, FocusEventArgs args)
{
    _textInputSuspension?.Dispose();
    _textInputSuspension = null;
}
```

`ResumeCapture()` is an escape hatch that clears every active suspension. Prefer token disposal for normal ownership.

## Diagnostics and device metadata

Enable diagnostics only while investigating input because every native event then formats additional platform data.

```csharp
options.EnableDiagnostics = true;

keyboard.DiagnosticEvent += (_, diagnostic) =>
{
    logger.LogInformation(
        "{Stage} {Platform} {Key} native={NativeKey}/{ScanCode} flags={Flags} device={Device} reason={Reason}",
        diagnostic.Stage,
        diagnostic.Platform,
        diagnostic.NormalizedKey,
        diagnostic.NativeKeyCode,
        diagnostic.NativeScanCode,
        diagnostic.NativeFlags,
        diagnostic.Device?.Id,
        diagnostic.Reason);
};
```

Normalized events expose platform, event type, physical location, native key/scan codes, repeat count, and `KeyboardDeviceInfo`. Native device metadata is best effort; Apple exposes the coalesced physical keyboard rather than a per-device identifier.

## Key-up and repeat events

```csharp
options.CaptureKeyUp = true;
options.AllowKeyRepeat = true;
```

Built-in hotkey, scanner, and sequence handlers react only to key-down. Custom handlers can inspect `KeyEventArgs.EventType` and `IsRepeat`. Defaults remain key-down only with native auto-repeat suppressed.

## Key sequences

```csharp
IDisposable sequenceRegistration = sequenceHandler.RegisterSequence(
    ["Ctrl+K", "Ctrl+C"],
    CommentSelection,
    timeout: TimeSpan.FromSeconds(1.5));
```

Sequences require at least two gestures, reject repeat events, reset after the timeout, and execute actions on the MAUI main thread.

## Asynchronous handlers

Use `IAsyncKeyHandler` for I/O that must not block the native input callback.

```csharp
public sealed class ProductLookupHandler : IAsyncKeyHandler
{
    public bool ShouldHandle(KeyEventArgs key) =>
        key.EventType == KeyboardEventType.KeyDown && key.Key == KeyboardKey.F4;

    public async ValueTask HandleKeyAsync(
        KeyEventArgs key,
        CancellationToken cancellationToken)
    {
        await LookupProductAsync(cancellationToken);
    }
}
```

Async handlers receive a detached snapshot and run as fire-and-observe work. They cannot set `Handled` in time to affect native propagation, and their cancellation token is cancelled when the service is disposed.

## Custom synchronous handlers

```csharp
public sealed class CustomKeyHandler : IKeyHandler
{
    public bool ShouldHandle(KeyEventArgs key) =>
        key.EventType == KeyboardEventType.KeyDown;

    public void HandleKey(KeyEventArgs key)
    {
        Console.WriteLine($"{key} ({key.Platform}, native {key.NativeKeyCode})");
        key.Handled = true;
    }
}
```

Synchronous handlers run on the native input thread. Keep them short and marshal UI work with `MainThread.BeginInvokeOnMainThread`.

## Key support matrix

| Key group | Windows | Android | iOS / Mac Catalyst |
|---|---|---|---|
| Letters, digits, common punctuation | ✅ | ✅ | ✅ USB-HID mapping |
| Modifiers | Ctrl, Alt, Shift, Win | Ctrl, Alt, Shift, Meta | Ctrl, Option, Shift, Command |
| Function keys | F1–F24 | F1–F12 | F1–F20 |
| Navigation/editing | ✅ | ✅ | ✅ |
| Numpad Enter location | Platform may report standard Enter | ✅ `Numpad` | ✅ `Numpad` |
| Lock / Print Screen / Pause | ✅ | Device-dependent | HID keys when delivered |
| Volume, media, Back, Forward, Search | OS-global registration; app capture varies | ✅ | Not currently normalized |
| Key-up | Opt-in | Opt-in | Opt-in |
| Multiple windows | ✅ | Active Activity rebind | App-level coalesced keyboard |
| Prevent native propagation when handled | ✅ | ✅ | No; Apple GameController keyboard events are observational |
| True OS-global activation | ✅ | — | — |

Keyboard layouts can change printable symbols. Windows and Android use platform labels where available; the Apple implementation currently maps USB HID usages with a US-style symbol table. Use `NativeKeyCode` and diagnostics when a layout-specific character matters.

## Lifecycle, trimming, and AOT

`UseKeyboardHandling()` attaches native sources when platform views are created and releases them when views are destroyed. Windows supports concurrent windows. Android preserves and forwards the original `Window.Callback` and safely rebinds after Activity recreation.

The package is marked `IsTrimmable` and `IsAotCompatible`, does not root its entire assembly, and contains no reflection-based key mapping. CI builds every platform, runs the platform-neutral suite on Windows and Linux, packages symbols, and injects F1, F12, and numpad Enter in an Android emulator.

## Migrating from 1.x to 2.x

- Move the application to .NET MAUI 10 and install the corresponding workloads.
- Keep both `.UseKeyboardHandling()` and `.AddKeyboardHandling(...)` in shared startup code.
- Prefer `KeyGesture` and registration tokens; legacy string APIs remain compatible.
- Platform handler implementations are internal in 2.x. Implement or inject `IPlatformKeyHandler` for custom adapters.
- `IKeyHandlerService` now includes attachment leases, priorities, scopes, suspension, diagnostics, and state introspection.
- iOS and Mac Catalyst are first-class targets; unsupported OS-global hotkeys are discoverable through `IsSupported`.

See [CHANGELOG.md](CHANGELOG.md) for the full release history and [ROADMAP.md](ROADMAP.md) for validation status and future work.

## Build and test

```bash
dotnet restore GlobalKeyboardCapture.Maui.sln
dotnet test --project GlobalKeyboardCapture.Maui.Tests/GlobalKeyboardCapture.Maui.Tests.csproj -c Release
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-android
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-windows10.0.19041.0
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-ios
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-maccatalyst
dotnet pack GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release
```

## Contributing

Issues and pull requests are welcome. Include the platform, native diagnostic fields, keyboard/scanner model, layout, and a minimal reproduction when reporting a missing key.

## License

Licensed under the [MIT License](LICENSE). Copyright Anderson Fernandes do Nascimento.
