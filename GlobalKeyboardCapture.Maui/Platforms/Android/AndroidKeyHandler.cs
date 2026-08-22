using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Views;
using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Platforms.Android;
using Activity = Android.App.Activity;
using IWindowCallback = Android.Views.Window.ICallback;
using NativeView = Android.Views.View;

namespace GlobalKeyboardCapture.Maui;

internal sealed class AndroidKeyHandler : IPlatformKeyHandler, IDisposable
{
    private const KeyEventActions KEY_ACTION_DOWN = KeyEventActions.Down;
    private const KeyEventFlags KEY_FLAGS_FROM_SYSTEM = KeyEventFlags.FromSystem;

    private readonly object _lockObject = new();
    private readonly ConcurrentDictionary<int, KeyboardDeviceInfo> _deviceCache = new();
    private readonly KeyHandlerOptions _options;
    private Action<KeyEventArgs>? _onKeyPressed;
    private Action<KeyboardDiagnosticEventArgs>? _onDiagnostic;
    private Activity? _activity;
    private IWindowCallback? _originalDispatcher;
    private KeyEventCallback? _installedCallback;
    private object? _boundPlatformView;
    private bool _isDisposed;

    public bool SupportsMultiplePlatformViews => false;

    public AndroidKeyHandler()
        : this(new KeyHandlerOptions())
    {
    }

    public AndroidKeyHandler(KeyHandlerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public void ConfigureHandler(Action<Core.Models.KeyEventArgs> onKeyPressed)
    {
        ArgumentNullException.ThrowIfNull(onKeyPressed);
        _onKeyPressed = onKeyPressed;
    }

    public void ConfigureDiagnostics(Action<KeyboardDiagnosticEventArgs> onDiagnostic)
    {
        ArgumentNullException.ThrowIfNull(onDiagnostic);
        _onDiagnostic = onDiagnostic;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DispatchKeyEvent(KeyEvent e)
    {
        ArgumentNullException.ThrowIfNull(e);
        ThrowIfDisposed();

        // Trusted system key-downs only. Use a BIT TEST instead of "Flags == FromSystem":
        // Flags is a bitfield and some keyboards set extra flags on function/numpad keys,
        // so exact equality silently dropped F1-F12 and the numpad Enter. Fallback events
        // are skipped to avoid double-processing (e.g. numpad keys with NumLock off, which
        // the system re-emits as DPAD/Move keys).
        if (e.Action is not (KeyEventActions.Down or KeyEventActions.Up))
        {
            ReportDiagnostic(e, null, KeyboardDiagnosticStage.Ignored, "Only key-down and key-up events are supported.");
            return false;
        }
        if (e.Action == KeyEventActions.Up && !_options.CaptureKeyUp)
        {
            ReportDiagnostic(e, null, KeyboardDiagnosticStage.Ignored, "Key-up events are disabled.");
            return false;
        }
        if ((e.Flags & KEY_FLAGS_FROM_SYSTEM) != KEY_FLAGS_FROM_SYSTEM)
        {
            ReportDiagnostic(e, null, KeyboardDiagnosticStage.Ignored, "Event is not marked as system input.");
            return false;
        }
        if ((e.Flags & KeyEventFlags.Fallback) == KeyEventFlags.Fallback)
        {
            ReportDiagnostic(e, null, KeyboardDiagnosticStage.Ignored, "Fallback event suppressed to prevent duplicate input.");
            return false;
        }
        // Ignore auto-repeat from a held key: Android streams ACTION_DOWN with an
        // incrementing RepeatCount, which would otherwise re-fire a held hotkey dozens
        // of times per second and flood the barcode buffer. Distinct keystrokes (and
        // barcode-scanner output) arrive as RepeatCount == 0, so this only drops repeats.
        if (e.RepeatCount > 0 && !_options.AllowKeyRepeat)
        {
            ReportDiagnostic(e, null, KeyboardDiagnosticStage.Ignored, "Auto-repeat is disabled.");
            return false;
        }

        var keyEvent = CreateKeyEventArgs(e);
        ProcessKeyEvent(keyEvent, e);
        _onKeyPressed?.Invoke(keyEvent);

        return keyEvent.Handled;
    }

    public void Attach(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);
        ThrowIfDisposed();

        lock (_lockObject)
        {
            var currentActivity = ResolveActivity(platformView)
                ?? throw new ArgumentException("The Android platform view must resolve to an Activity.", nameof(platformView));

            if (currentActivity.Window == null)
                throw new InvalidOperationException("Activity window is null");

            // No-op if we're already bound to this same activity.
            if (ReferenceEquals(_activity, currentActivity) && ReferenceEquals(_boundPlatformView, platformView))
                return;

            // Restore the previous dispatcher before binding to the new activity.
            // This handles Android Activity recreation (e.g. configuration changes).
            RestoreOriginalDispatcher();

            _activity = currentActivity;
            _originalDispatcher = _activity.Window.Callback;
            _installedCallback = new KeyEventCallback(this, _activity.Window.Callback!);
            _activity.Window.Callback = _installedCallback;
            _boundPlatformView = platformView;
        }
    }

    public bool Detach(object platformView)
    {
        if (platformView is null)
            return false;

        lock (_lockObject)
        {
            if (_isDisposed || !ReferenceEquals(_boundPlatformView, platformView))
                return false;

            RestoreOriginalDispatcher();
            return true;
        }
    }

    private void RestoreOriginalDispatcher()
    {
        var installedCallback = _installedCallback;
        installedCallback?.DisableCapture();

        if (_activity?.Window != null
            && _originalDispatcher != null
            && ReferenceEquals(_activity.Window.Callback, installedCallback))
        {
            _activity.Window.Callback = _originalDispatcher;
            installedCallback?.Dispose();
        }
        _originalDispatcher = null;
        _activity = null;
        _boundPlatformView = null;

        // If another library wrapped our callback after installation, do not overwrite
        // its callback chain or dispose an object it still references. Disable capture
        // instead; that proxy now only forwards to the original callback.
        _installedCallback = null;
    }

    private static Activity? ResolveActivity(object platformView)
    {
        if (platformView is Activity activity)
            return activity;

        if (platformView is not NativeView view)
            return null;

        Context? context = view.Context;
        while (context is ContextWrapper wrapper)
        {
            if (wrapper is Activity wrappedActivity)
                return wrappedActivity;
            context = wrapper.BaseContext;
        }

        return context as Activity;
    }

    public void Cleanup()
    {
        Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private KeyEventArgs CreateKeyEventArgs(KeyEvent e)
    {
        var keyEvent = new KeyEventArgs
        {
            Platform = KeyboardPlatform.Android,
            EventType = e.Action == KEY_ACTION_DOWN
                ? KeyboardEventType.KeyDown
                : KeyboardEventType.KeyUp,
            Location = IsNumpadKey(e.KeyCode) ? KeyLocation.Numpad : KeyLocation.Standard,
            NativeKeyCode = (int)e.KeyCode,
            NativeScanCode = e.ScanCode,
            NativeFlags = _onDiagnostic is null ? null : e.Flags.ToString(),
            RepeatCount = e.RepeatCount,
            Device = GetDeviceInfo(e),

            // Modifiers
            ControlKey = e.IsCtrlPressed,
            AltKey = e.IsAltPressed,
            ShiftKey = e.IsShiftPressed,
            WindowsKey = e.IsMetaPressed || e.KeyCode == Keycode.Window,

            // Navigation
            UpKey = e.KeyCode == Keycode.DpadUp,
            DownKey = e.KeyCode == Keycode.DpadDown,
            LeftKey = e.KeyCode == Keycode.DpadLeft,
            RightKey = e.KeyCode == Keycode.DpadRight,
            HomeKey = e.KeyCode == Keycode.MoveHome,
            EndKey = e.KeyCode == Keycode.MoveEnd,
            PageUpKey = e.KeyCode == Keycode.PageUp,
            PageDownKey = e.KeyCode == Keycode.PageDown,

            // Edition
            EnterKey = e.KeyCode == Keycode.Enter || e.KeyCode == Keycode.NumpadEnter,
            TabKey = e.KeyCode == Keycode.Tab,
            BackspaceKey = e.KeyCode == Keycode.Del,
            DeleteKey = e.KeyCode == Keycode.ForwardDel,
            EscapeKey = e.KeyCode == Keycode.Escape,
            SpaceKey = e.KeyCode == Keycode.Space,
            InsertKey = e.KeyCode == Keycode.Insert,

            // System
            CapsLockKey = e.KeyCode == Keycode.CapsLock,
            NumLockKey = e.KeyCode == Keycode.NumLock,
            ScrollLockKey = e.KeyCode == Keycode.ScrollLock,
            PrintScreenKey = e.KeyCode == Keycode.Sysrq,
            PauseBreakKey = e.KeyCode == Keycode.Break,
            MenuKey = e.KeyCode == Keycode.Menu,
            PlatformEvent = e
        };

        if (keyEvent.Key == KeyboardKey.None)
        {
            keyEvent.Key = e.KeyCode switch
            {
                Keycode.VolumeUp => KeyboardKey.VolumeUp,
                Keycode.VolumeDown => KeyboardKey.VolumeDown,
                Keycode.VolumeMute => KeyboardKey.VolumeMute,
                Keycode.Back => KeyboardKey.Back,
                Keycode.Forward => KeyboardKey.Forward,
                Keycode.Search => KeyboardKey.Search,
                Keycode.MediaPlayPause => KeyboardKey.MediaPlayPause,
                Keycode.MediaStop => KeyboardKey.MediaStop,
                Keycode.MediaNext => KeyboardKey.MediaNext,
                Keycode.MediaPrevious => KeyboardKey.MediaPrevious,
                _ => KeyboardKey.None
            };
        }

        return keyEvent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessKeyEvent(KeyEventArgs keyEvent, KeyEvent e)
    {
        // Resolve function keys from the KeyCode FIRST. DisplayLabel is unreliable for
        // them (usually '\0', occasionally a stray char), so testing ToChar first could
        // misclassify F1..F12 as a character. Literal mapping keeps this trim/AOT-safe and
        // produces the same "F1".."F12" strings KeyEventArgs.ToString() expects.
        keyEvent.FunctionKey = e.KeyCode switch
        {
            Keycode.F1 => "F1",
            Keycode.F2 => "F2",
            Keycode.F3 => "F3",
            Keycode.F4 => "F4",
            Keycode.F5 => "F5",
            Keycode.F6 => "F6",
            Keycode.F7 => "F7",
            Keycode.F8 => "F8",
            Keycode.F9 => "F9",
            Keycode.F10 => "F10",
            Keycode.F11 => "F11",
            Keycode.F12 => "F12",
            _ => null
        };

        if (keyEvent.FunctionKey == null)
        {
            keyEvent.Character = KeyboardHelper.ToChar(e.DisplayLabel);
        }

    }

    private KeyboardDeviceInfo? GetDeviceInfo(KeyEvent keyEvent)
    {
        if (keyEvent.DeviceId < 0)
            return null;

        return _deviceCache.GetOrAdd(keyEvent.DeviceId, static (_, nativeEvent) =>
        {
            var device = nativeEvent.Device;
            return new KeyboardDeviceInfo(
                nativeEvent.DeviceId,
                device?.Name,
                device?.IsVirtual ?? false,
                OperatingSystem.IsAndroidVersionAtLeast(29) && device?.IsExternal == true,
                device?.Descriptor);
        }, keyEvent);
    }

    private void ReportDiagnostic(
        KeyEvent nativeEvent,
        KeyEventArgs? normalizedEvent,
        KeyboardDiagnosticStage stage,
        string? reason)
    {
        var diagnostic = _onDiagnostic;
        if (diagnostic is null)
            return;

        diagnostic(new KeyboardDiagnosticEventArgs(
            stage,
            KeyboardPlatform.Android,
            normalizedEvent?.ToString(),
            (int)nativeEvent.KeyCode,
            nativeEvent.ScanCode,
            nativeEvent.Action.ToString(),
            nativeEvent.Flags.ToString(),
            nativeEvent.RepeatCount,
            GetDeviceInfo(nativeEvent),
            reason));
    }

    private static bool IsNumpadKey(Keycode keyCode) => keyCode is
        >= Keycode.Numpad0 and <= Keycode.NumpadRightParen;

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(AndroidKeyHandler));
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        lock (_lockObject)
        {
            if (_isDisposed) return;

            RestoreOriginalDispatcher();
            _isDisposed = true;
        }
    }
}
