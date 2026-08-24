using System.Collections.ObjectModel;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;

namespace GlobalKeyboardCapture.Maui.Sample;

public partial class MainPage : ContentPage
{
    private const int MAX_DIAGNOSTIC_ENTRIES = 80;

    private readonly IKeyHandlerService _keyHandlerService;
    private readonly IGlobalHotkeyService _globalHotkeyService;
    private readonly KeyDisplayHandler _keyDisplayHandler = new();
    private readonly BarcodeHandler _barcodeHandler;
    private readonly HotkeyHandler _hotkeyHandler;
    private readonly KeySequenceHandler _sequenceHandler;
    private readonly ObservableCollection<string> _diagnostics = [];
    private readonly List<IDisposable> _gestureRegistrations = [];
    private IKeyboardCaptureScope? _pageScope;
    private IDisposable? _captureSuspension;
    private IDisposable? _globalHotkeyRegistration;
    private int _counter;

    public MainPage(
        IKeyHandlerService keyHandlerService,
        IGlobalHotkeyService globalHotkeyService,
        BarcodeHandler barcodeHandler,
        HotkeyHandler hotkeyHandler,
        KeySequenceHandler sequenceHandler)
    {
        InitializeComponent();

        _keyHandlerService = keyHandlerService;
        _globalHotkeyService = globalHotkeyService;
        _barcodeHandler = barcodeHandler;
        _hotkeyHandler = hotkeyHandler;
        _sequenceHandler = sequenceHandler;
        DiagnosticsList.ItemsSource = _diagnostics;

        PlatformLabel.Text = DeviceInfo.Platform.ToString();
        UpdatePipelineStatus();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_pageScope is not null)
            return;

        _keyDisplayHandler.KeyPressed += OnKeyPressed;
        _barcodeHandler.ScanCompleted += OnScanCompleted;
        _sequenceHandler.ProgressChanged += OnSequenceProgressChanged;
        _keyHandlerService.DiagnosticEvent += OnDiagnosticEvent;

        _pageScope = _keyHandlerService.CreateScope("Main diagnostics page");
        _pageScope.RegisterHandler(_keyDisplayHandler, priority: 100);
        _pageScope.RegisterHandler(_barcodeHandler, priority: 50);
        _pageScope.RegisterHandler(_sequenceHandler, priority: 25);
        _pageScope.RegisterHandler(_hotkeyHandler);

        RegisterGestures();
        TryRegisterGlobalHotkey();
        CaptureScopeSwitch.IsToggled = true;
        UpdatePipelineStatus();
#if WINDOWS
        if (Window is not null)
            WindowsRuntimeIntegration.TryStart(Window, _keyHandlerService, _globalHotkeyService);
#endif
#if ANDROID
        Android.Util.Log.Info(
            "GKC.Integration",
            $"Ready;Views={_keyHandlerService.PlatformViewCount}");
#endif
    }

    protected override void OnDisappearing()
    {
        ReleaseCaptureSuspension();
        _globalHotkeyRegistration?.Dispose();
        _globalHotkeyRegistration = null;

        foreach (var registration in _gestureRegistrations)
            registration.Dispose();
        _gestureRegistrations.Clear();

        _pageScope?.Dispose();
        _pageScope = null;
        _keyDisplayHandler.KeyPressed -= OnKeyPressed;
        _barcodeHandler.ScanCompleted -= OnScanCompleted;
        _sequenceHandler.ProgressChanged -= OnSequenceProgressChanged;
        _sequenceHandler.CancelPendingSequences();
        _keyHandlerService.DiagnosticEvent -= OnDiagnosticEvent;
        UpdatePipelineStatus();
        base.OnDisappearing();
    }

    private void RegisterGestures()
    {
        _gestureRegistrations.Add(_hotkeyHandler.RegisterHotkey(
            new KeyGesture(KeyboardKey.F2),
            ToggleTestButton));
        _gestureRegistrations.Add(_hotkeyHandler.RegisterHotkey("Ctrl+S", () =>
            ShowMessage($"Changes saved at {TimeProvider.System.GetLocalNow():HH:mm:ss}.")));
        _gestureRegistrations.Add(_hotkeyHandler.RegisterHotkey("Escape", () =>
            ShowMessage("Operation cancelled.")));
        _gestureRegistrations.Add(_sequenceHandler.RegisterSequence(
            ["Ctrl+K", "Ctrl+C"],
            () => ShowMessage("Short Ctrl+K, Ctrl+C sequence completed."),
            TimeSpan.FromSeconds(1.5)));
        _gestureRegistrations.Add(_sequenceHandler.RegisterSequence(
            ["Ctrl+K", "Ctrl+C", "Ctrl+D"],
            () => ShowMessage("Longest Ctrl+K, Ctrl+C, Ctrl+D sequence completed."),
            TimeSpan.FromSeconds(1.5)));
    }

    private void TryRegisterGlobalHotkey()
    {
        if (!_globalHotkeyService.IsSupported)
        {
            GlobalHotkeyStatusLabel.Text = "Not supported on this platform";
            return;
        }

        if (!_globalHotkeyService.IsAttached)
        {
            GlobalHotkeyStatusLabel.Text = "Supported; waiting for a native window";
            return;
        }

        try
        {
            _globalHotkeyRegistration = _globalHotkeyService.RegisterHotkey(
                "Ctrl+Alt+G",
                () => ShowMessage("Windows OS-global Ctrl+Alt+G activated."));
            GlobalHotkeyStatusLabel.Text = "Ctrl+Alt+G registered (Windows)";
        }
        catch (InvalidOperationException exception)
        {
            GlobalHotkeyStatusLabel.Text = $"Registration failed: {exception.Message}";
        }
        catch (System.ComponentModel.Win32Exception exception)
        {
            GlobalHotkeyStatusLabel.Text = $"Registration failed: {exception.Message}";
        }
    }

    private void OnKeyPressed(object? sender, KeyEventArgs key)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            KeyPressedLabel.Text = string.IsNullOrEmpty(key.ToString()) ? "(modifier only)" : key.ToString();
            var device = key.Device is null
                ? "unknown device"
                : $"device {key.Device.Id} ({key.Device.Name ?? "unnamed"})";
            KeyMetadataLabel.Text =
                $"{key.Platform} · {key.EventType} · native={key.NativeKeyCode} · scan={key.NativeScanCode} · " +
                $"location={key.Location} · repeat={key.RepeatCount} · {device}";
        });
    }

    private void OnScanCompleted(object? sender, BarcodeScanResult result)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ScannedCodeLabel.Text = result.Value;
            ScanMetadataLabel.Text =
                $"Profile={result.ProfileName} · raw={result.RawValue} · chars={result.CharacterCount} · " +
                $"duration={result.Duration.TotalMilliseconds:F0} ms · device={result.Device?.Id.ToString() ?? "unknown"}";
        });
    }

    private void OnDiagnosticEvent(object? sender, KeyboardDiagnosticEventArgs diagnostic)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var message =
                $"{diagnostic.Timestamp:HH:mm:ss.fff} {diagnostic.Stage,-10} " +
                $"{diagnostic.Platform,-11} key={diagnostic.NormalizedKey ?? "-"} " +
                $"native={diagnostic.NativeKeyCode}/{diagnostic.NativeScanCode} " +
                $"action={diagnostic.NativeAction ?? "-"} repeat={diagnostic.RepeatCount}";
            if (!string.IsNullOrWhiteSpace(diagnostic.Reason))
                message += $" reason={diagnostic.Reason}";

            _diagnostics.Insert(0, message);
            while (_diagnostics.Count > MAX_DIAGNOSTIC_ENTRIES)
                _diagnostics.RemoveAt(_diagnostics.Count - 1);
        });
    }

    private void OnSequenceProgressChanged(object? sender, EventArgs e)
    {
        var progress = _sequenceHandler.GetProgressSnapshot();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SequenceStatusLabel.Text = progress.Count == 0
                ? "No pending sequence."
                : string.Join(
                    " · ",
                    progress.Select(item =>
                        $"{item.Sequence}: {item.MatchedGestureCount}/{item.GestureCount}"));
        });
    }

    private void OnCancelSequenceClicked(object? sender, EventArgs e)
    {
        var cancelled = _sequenceHandler.CancelPendingSequences();
        ShowMessage(cancelled
            ? "Pending sequence cancelled."
            : "No sequence was pending.");
    }

    private void OnCaptureScopeToggled(object? sender, ToggledEventArgs e)
    {
        if (_pageScope is not null)
            _pageScope.IsEnabled = e.Value;
        UpdatePipelineStatus();
    }

    private void OnCaptureEntryFocused(object? sender, FocusEventArgs e)
    {
        _captureSuspension ??= _keyHandlerService.SuspendCapture();
        UpdatePipelineStatus();
    }

    private void OnCaptureEntryUnfocused(object? sender, FocusEventArgs e)
    {
        ReleaseCaptureSuspension();
        UpdatePipelineStatus();
    }

    private void ReleaseCaptureSuspension()
    {
        _captureSuspension?.Dispose();
        _captureSuspension = null;
    }

    private void ToggleTestButton()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CounterButton.IsEnabled = !CounterButton.IsEnabled;
            ShowMessage($"Test button {(CounterButton.IsEnabled ? "enabled" : "disabled")}.");
        });
    }

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        _counter++;
        CounterButton.Text = $"Clicked {_counter} {(_counter == 1 ? "time" : "times")}";
        ShowMessage($"Button clicked at {TimeProvider.System.GetLocalNow():HH:mm:ss}.");
    }

    private void OnClearDiagnosticsClicked(object? sender, EventArgs e) => _diagnostics.Clear();

    private void ShowMessage(string message) =>
        MainThread.BeginInvokeOnMainThread(() => MessageLabel.Text = message);

    private void UpdatePipelineStatus()
    {
        CaptureStateLabel.Text = _captureSuspension is null ? "Active" : "Suspended for text input";
        PipelineStateLabel.Text =
            $"views={_keyHandlerService.PlatformViewCount}, handlers={_keyHandlerService.HandlerCount}, " +
            $"page scope={(_pageScope?.IsEnabled == true ? "enabled" : "disabled")}";
    }
}
