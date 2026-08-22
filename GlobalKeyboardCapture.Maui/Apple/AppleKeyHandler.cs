#if IOS || MACCATALYST
using Foundation;
using GameController;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Mapping;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui;

/// <summary>
/// Captures physical Apple keyboard state through the GameController keyboard profile.
/// </summary>
public sealed class AppleKeyHandler : IPlatformKeyHandler, IDisposable
{
    private static readonly KeyboardDeviceInfo AppleKeyboardDevice = new(
        0,
        "Apple physical keyboard",
        isVirtual: false,
        isExternal: true,
        descriptor: "GCKeyboard.CoalescedKeyboard");

    private readonly object _lockObject = new();
    private readonly HashSet<object> _platformViews = new(ReferenceEqualityComparer.Instance);
    private readonly KeyHandlerOptions _options;
    private Action<KeyEventArgs>? _onKeyPressed;
    private Action<KeyboardDiagnosticEventArgs>? _onDiagnostic;
    private NSObject? _connectObserver;
    private NSObject? _disconnectObserver;
    private GCKeyboardInput? _keyboardInput;
    private bool _capsLockEnabled;
    private bool _isDisposed;

    public AppleKeyHandler()
        : this(new KeyHandlerOptions())
    {
    }

    public AppleKeyHandler(KeyHandlerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public bool SupportsMultiplePlatformViews => true;

    public void ConfigureHandler(Action<KeyEventArgs> onKeyPressed)
    {
        ArgumentNullException.ThrowIfNull(onKeyPressed);
        _onKeyPressed = onKeyPressed;
    }

    public void ConfigureDiagnostics(Action<KeyboardDiagnosticEventArgs> onDiagnostic)
    {
        ArgumentNullException.ThrowIfNull(onDiagnostic);
        _onDiagnostic = onDiagnostic;
    }

    public void Attach(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);

        lock (_lockObject)
        {
            ThrowIfDisposed();
            if (!_platformViews.Add(platformView) || _platformViews.Count > 1)
                return;

            _connectObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                GCKeyboard.DidConnectNotification,
                _ => BindCurrentKeyboard());
            _disconnectObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                GCKeyboard.DidDisconnectNotification,
                _ => BindCurrentKeyboard());
            BindCurrentKeyboard();
        }
    }

    public bool Detach(object platformView)
    {
        if (platformView is null)
            return false;

        lock (_lockObject)
        {
            if (_isDisposed || !_platformViews.Remove(platformView))
                return false;
            if (_platformViews.Count == 0)
                StopMonitoring();
            return true;
        }
    }

    private void BindCurrentKeyboard()
    {
        lock (_lockObject)
        {
            if (_isDisposed || _platformViews.Count == 0)
                return;

            if (_keyboardInput is not null)
                _keyboardInput.KeyChangedHandler = null;

            _keyboardInput = GCKeyboard.CoalescedKeyboard?.KeyboardInput;
            if (_keyboardInput is not null)
                _keyboardInput.KeyChangedHandler = OnKeyChanged;
        }
    }

    private void OnKeyChanged(
        GCKeyboardInput keyboard,
        GCControllerButtonInput key,
        nint keyCode,
        bool pressed)
    {
        if (!pressed && !_options.CaptureKeyUp)
        {
            ReportIgnored(keyCode, pressed, "Key-up events are disabled.");
            return;
        }

        var nativeKeyCode = checked((int)keyCode);
        bool capsLock;
        lock (_lockObject)
        {
            if (_isDisposed || _platformViews.Count == 0)
                return;
            if (pressed && nativeKeyCode == 0x39)
                _capsLockEnabled = !_capsLockEnabled;
            capsLock = _capsLockEnabled;
        }

        var control = IsPressed(keyboard, GCKeyCode.LeftControl)
            || IsPressed(keyboard, GCKeyCode.RightControl);
        var alt = IsPressed(keyboard, GCKeyCode.LeftAlt)
            || IsPressed(keyboard, GCKeyCode.RightAlt);
        var shift = IsPressed(keyboard, GCKeyCode.LeftShift)
            || IsPressed(keyboard, GCKeyCode.RightShift);
        var meta = IsPressed(keyboard, GCKeyCode.LeftGui)
            || IsPressed(keyboard, GCKeyCode.RightGui);
        var mapping = AppleKeyMapper.Map(nativeKeyCode, shift, capsLock);
        var keyEvent = new KeyEventArgs
        {
#if IOS
            Platform = KeyboardPlatform.iOS,
#else
            Platform = KeyboardPlatform.MacCatalyst,
#endif
            EventType = pressed ? KeyboardEventType.KeyDown : KeyboardEventType.KeyUp,
            Location = mapping.Location,
            NativeKeyCode = nativeKeyCode,
            NativeFlags = _onDiagnostic is null
                ? null
                : $"Pressed={pressed};Control={control};Alt={alt};Shift={shift};Meta={meta};CapsLock={capsLock}",
            Device = AppleKeyboardDevice,
            ControlKey = control,
            AltKey = alt,
            ShiftKey = shift,
            WindowsKey = meta,
            PlatformEvent = key
        };

        if (mapping.Key == KeyboardKey.Character)
            keyEvent.Character = mapping.Character;
        else if (mapping.Key != KeyboardKey.None)
            keyEvent.Key = mapping.Key;

        _onKeyPressed?.Invoke(keyEvent);
    }

    private static bool IsPressed(GCKeyboardInput keyboard, nint keyCode) =>
        keyboard.GetButton(keyCode)?.IsPressed == true;

    private void ReportIgnored(nint keyCode, bool pressed, string reason)
    {
        var diagnostic = _onDiagnostic;
        if (diagnostic is null)
            return;

        diagnostic(new KeyboardDiagnosticEventArgs(
            KeyboardDiagnosticStage.Ignored,
#if IOS
            KeyboardPlatform.iOS,
#else
            KeyboardPlatform.MacCatalyst,
#endif
            normalizedKey: null,
            nativeKeyCode: checked((int)keyCode),
            nativeScanCode: 0,
            nativeAction: pressed ? "Down" : "Up",
            nativeFlags: null,
            repeatCount: 0,
            device: AppleKeyboardDevice,
            reason));
    }

    private void StopMonitoring()
    {
        if (_keyboardInput is not null)
        {
            _keyboardInput.KeyChangedHandler = null;
            _keyboardInput = null;
        }

        _connectObserver?.Dispose();
        _connectObserver = null;
        _disconnectObserver?.Dispose();
        _disconnectObserver = null;
    }

    public void Cleanup()
    {
        lock (_lockObject)
        {
            StopMonitoring();
            _platformViews.Clear();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    public void Dispose()
    {
        lock (_lockObject)
        {
            if (_isDisposed)
                return;
            StopMonitoring();
            _platformViews.Clear();
            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
#endif
