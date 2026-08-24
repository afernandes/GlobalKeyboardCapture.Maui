using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Core.Services;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Docs;

internal static class ScenarioExamples
{
    #region hotkeys
    public static IDisposable RegisterSaveHotkey(HotkeyHandler hotkeys, Action save) =>
        hotkeys.RegisterHotkey(
            new KeyGesture('S', KeyModifiers.Control),
            save);
    #endregion

    #region scanner
    public static BarcodeScannerProfile CreateScannerProfile(int deviceId) =>
        new("Checkout scanner")
        {
            DeviceId = deviceId,
            MinLength = 8,
            MaxLength = 128,
            InterCharacterTimeout = TimeSpan.FromMilliseconds(80),
            MaxAverageInterCharacterDelay = TimeSpan.FromMilliseconds(35),
            Prefix = "]C1",
            RequirePrefix = true,
            StripPrefix = true
        };
    #endregion

    #region kiosk
    public static IKeyboardCaptureScope CreateCheckoutScope(
        IKeyHandlerService service,
        BarcodeHandler scanner,
        HotkeyHandler hotkeys)
    {
        var scope = service.CreateScope("Checkout");
        scope.RegisterHandler(scanner, priority: 100);
        scope.RegisterHandler(hotkeys, priority: 50);
        return scope;
    }
    #endregion

    #region sequences
    public static KeySequenceHandler CreateSequenceHandler(Action comment, Action uncomment)
    {
        var sequences = new KeySequenceHandler(SequenceOverlapPolicy.PreferLongest);
        sequences.RegisterSequence(["Ctrl+K", "Ctrl+C"], comment);
        sequences.RegisterSequence(["Ctrl+K", "Ctrl+U"], uncomment);
        sequences.ProgressChanged += (_, _) =>
        {
            IReadOnlyList<KeySequenceProgress> progress = sequences.GetProgressSnapshot();
            _ = progress;
        };
        return sequences;
    }
    #endregion

    #region device-routing
    public static IKeyboardCaptureScope ReserveExternalAndroidScanner(
        IKeyHandlerService service,
        BarcodeHandler scanner,
        int deviceId)
    {
        var scopeFilter = new KeyboardDeviceFilter(
            isExternal: true,
            platform: KeyboardPlatform.Android);
        var scope = service.CreateScope(scopeFilter, "Dedicated scanner");
        scope.RegisterHandler(scanner, new KeyboardDeviceFilter(deviceId: deviceId));
        return scope;
    }
    #endregion

    #region multi-window
    public static (IDisposable First, IDisposable Second) AttachWindows(
        IKeyHandlerService service,
        object firstNativeWindow,
        object secondNativeWindow) =>
        (service.AttachPlatformView(firstNativeWindow),
         service.AttachPlatformView(secondNativeWindow));
    #endregion

    #region metrics
    public static KeyHandlerOptions CreateObservedOptions() => new()
    {
        EnableMetrics = true,
        EnableDiagnostics = false
    };

    public static string MeterName => KeyboardMetrics.MeterName;
    #endregion
}
