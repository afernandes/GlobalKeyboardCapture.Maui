using System.ComponentModel;
using System.Runtime.InteropServices;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using Microsoft.Extensions.Logging;

namespace GlobalKeyboardCapture.Maui;

/// <summary>
/// Implements true operating-system global hotkeys through RegisterHotKey and WM_HOTKEY.
/// </summary>
internal sealed partial class WindowsGlobalHotkeyService :
    IGlobalHotkeyService,
    IPlatformViewLifecycleSink,
    IDisposable
{
    private const uint WM_HOTKEY = 0x0312;
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint MOD_NOREPEAT = 0x4000;

    private static long _nextSubclassId;

    private readonly object _lockObject = new();
    private readonly Dictionary<KeyGesture, Registration> _registrations = [];
    private readonly Dictionary<int, Registration> _registrationsById = [];
    private readonly List<Microsoft.UI.Xaml.Window> _windows = [];
    private readonly ILogger<WindowsGlobalHotkeyService> _logger;
    private readonly SubclassProcedure _subclassProcedure;
    private readonly nuint _subclassId;
    private Microsoft.UI.Xaml.Window? _activeWindow;
    private nint _windowHandle;
    private int _nextRegistrationId;
    private long _nextVersion;
    private bool _isDisposed;

    public WindowsGlobalHotkeyService(ILogger<WindowsGlobalHotkeyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subclassProcedure = WindowSubclassProcedure;
        _subclassId = (nuint)Interlocked.Increment(ref _nextSubclassId);
    }

    public bool IsSupported => true;

    public bool IsAttached
    {
        get
        {
            lock (_lockObject)
                return !_isDisposed && _windowHandle != 0;
        }
    }

    public int HotkeyCount
    {
        get
        {
            lock (_lockObject)
                return _registrations.Count;
        }
    }

    public IDisposable RegisterHotkey(KeyGesture gesture, Action action, bool suppressRepeat = true)
    {
        ArgumentNullException.ThrowIfNull(action);
        var (virtualKey, modifiers) = MapGesture(gesture);
        if (suppressRepeat)
            modifiers |= MOD_NOREPEAT;

        lock (_lockObject)
        {
            ThrowIfDisposed();
            if (_windowHandle == 0)
            {
                throw new InvalidOperationException(
                    "Global hotkeys can be registered only after a native Windows window has been created.");
            }

            if (_registrations.TryGetValue(gesture, out var existing))
            {
                var previousModifiers = existing.NativeModifiers;
                if (previousModifiers != modifiers)
                {
                    UnregisterHotKey(_windowHandle, existing.NativeId);
                    if (!RegisterHotKey(_windowHandle, existing.NativeId, modifiers, virtualKey))
                    {
                        var error = Marshal.GetLastWin32Error();
                        RegisterHotKey(_windowHandle, existing.NativeId, previousModifiers, existing.VirtualKey);
                        throw CreateRegistrationException(gesture, error);
                    }
                }

                existing.Action = action;
                existing.NativeModifiers = modifiers;
                existing.VirtualKey = virtualKey;
                existing.Version = ++_nextVersion;
                return new GlobalHotkeyRegistrationToken(this, gesture, existing.Version);
            }

            var nativeId = checked(++_nextRegistrationId);
            if (!RegisterHotKey(_windowHandle, nativeId, modifiers, virtualKey))
                throw CreateRegistrationException(gesture, Marshal.GetLastWin32Error());

            var registration = new Registration(
                nativeId,
                ++_nextVersion,
                gesture,
                action,
                modifiers,
                virtualKey);
            _registrations.Add(gesture, registration);
            _registrationsById.Add(nativeId, registration);
            return new GlobalHotkeyRegistrationToken(this, gesture, registration.Version);
        }
    }

    public IDisposable RegisterHotkey(string gesture, Action action, bool suppressRepeat = true)
    {
        ArgumentNullException.ThrowIfNull(action);
        return RegisterHotkey(KeyGesture.Parse(gesture), action, suppressRepeat);
    }

    public bool UnregisterHotkey(KeyGesture gesture)
    {
        lock (_lockObject)
        {
            if (_isDisposed || !_registrations.Remove(gesture, out var registration))
                return false;

            _registrationsById.Remove(registration.NativeId);
            if (_windowHandle != 0)
                UnregisterHotKey(_windowHandle, registration.NativeId);
            return true;
        }
    }

    public void ClearHotkeys()
    {
        lock (_lockObject)
        {
            if (_isDisposed)
                return;

            if (_windowHandle != 0)
            {
                foreach (var registration in _registrations.Values)
                    UnregisterHotKey(_windowHandle, registration.NativeId);
            }

            _registrations.Clear();
            _registrationsById.Clear();
        }
    }

    void IPlatformViewLifecycleSink.OnPlatformViewCreated(object platformView)
    {
        if (platformView is not Microsoft.UI.Xaml.Window window)
            return;

        lock (_lockObject)
        {
            if (_isDisposed || _windows.Any(candidate => ReferenceEquals(candidate, window)))
                return;

            _windows.Add(window);
            if (_activeWindow is null)
                BindWindow(window);
        }
    }

    void IPlatformViewLifecycleSink.OnPlatformViewDestroyed(object platformView)
    {
        if (platformView is not Microsoft.UI.Xaml.Window window)
            return;

        lock (_lockObject)
        {
            var index = _windows.FindIndex(candidate => ReferenceEquals(candidate, window));
            if (index >= 0)
                _windows.RemoveAt(index);

            if (!ReferenceEquals(_activeWindow, window))
                return;

            UnbindWindow();
            if (_windows.Count > 0)
                BindWindow(_windows[0]);
        }
    }

    private void BindWindow(Microsoft.UI.Xaml.Window window)
    {
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        if (windowHandle == 0)
        {
            _logger.LogError("Cannot attach global hotkeys because the WinUI window has no native handle");
            return;
        }

        if (!SetWindowSubclass(windowHandle, _subclassProcedure, _subclassId, 0))
        {
            _logger.LogError(
                new Win32Exception(Marshal.GetLastWin32Error()),
                "Cannot attach the global-hotkey window subclass");
            return;
        }

        _activeWindow = window;
        _windowHandle = windowHandle;

        foreach (var registration in _registrations.Values)
        {
            if (!RegisterHotKey(
                    _windowHandle,
                    registration.NativeId,
                    registration.NativeModifiers,
                    registration.VirtualKey))
            {
                _logger.LogError(
                    new Win32Exception(Marshal.GetLastWin32Error()),
                    "Failed to restore global hotkey {Gesture} after switching Windows windows",
                    registration.Gesture);
            }
        }
    }

    private void UnbindWindow()
    {
        if (_windowHandle == 0)
            return;

        foreach (var registration in _registrations.Values)
            UnregisterHotKey(_windowHandle, registration.NativeId);

        RemoveWindowSubclass(_windowHandle, _subclassProcedure, _subclassId);
        _windowHandle = 0;
        _activeWindow = null;
    }

    private nint WindowSubclassProcedure(
        nint windowHandle,
        uint message,
        nuint wordParameter,
        nint longParameter,
        nuint subclassId,
        nuint referenceData)
    {
        if (message == WM_HOTKEY)
        {
            Action? action = null;
            lock (_lockObject)
            {
                if (_registrationsById.TryGetValue((int)wordParameter, out var registration))
                    action = registration.Action;
            }

            if (action is not null)
                MainThread.BeginInvokeOnMainThread(() => InvokeSafely(action));
        }

        return DefSubclassProc(windowHandle, message, wordParameter, longParameter);
    }

    private void InvokeSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "A global-hotkey action failed");
        }
    }

    private void UnregisterHotkey(KeyGesture gesture, long version)
    {
        lock (_lockObject)
        {
            if (!_registrations.TryGetValue(gesture, out var registration)
                || registration.Version != version)
            {
                return;
            }

            _registrations.Remove(gesture);
            _registrationsById.Remove(registration.NativeId);
            if (_windowHandle != 0)
                UnregisterHotKey(_windowHandle, registration.NativeId);
        }
    }

    private static (uint VirtualKey, uint Modifiers) MapGesture(KeyGesture gesture)
    {
        if (gesture.Key == KeyboardKey.None)
            throw new ArgumentException("A global hotkey must contain a logical key.", nameof(gesture));

        var modifiers = MapModifiers(gesture.Modifiers);
        if (gesture.Key == KeyboardKey.Character)
        {
            var character = gesture.Character
                ?? throw new ArgumentException("A character gesture must contain a character.", nameof(gesture));
            if (character is >= 'A' and <= 'Z' or >= '0' and <= '9')
                return (character, modifiers);

            var scan = VkKeyScanW(character);
            if (scan == -1)
                throw new ArgumentException($"Character '{character}' cannot be registered as a Windows hotkey.", nameof(gesture));

            var value = unchecked((ushort)scan);
            var keyboardState = value >> 8;
            if ((keyboardState & 1) != 0) modifiers |= MOD_SHIFT;
            if ((keyboardState & 2) != 0) modifiers |= MOD_CONTROL;
            if ((keyboardState & 4) != 0) modifiers |= MOD_ALT;
            return ((uint)(value & 0xFF), modifiers);
        }

        if (gesture.Key is >= KeyboardKey.F1 and <= KeyboardKey.F24)
            return ((uint)(0x70 + gesture.Key - KeyboardKey.F1), modifiers);

        uint virtualKey = gesture.Key switch
        {
            KeyboardKey.Enter => 0x0D,
            KeyboardKey.Tab => 0x09,
            KeyboardKey.Backspace => 0x08,
            KeyboardKey.Delete => 0x2E,
            KeyboardKey.Escape => 0x1B,
            KeyboardKey.Space => 0x20,
            KeyboardKey.Insert => 0x2D,
            KeyboardKey.Up => 0x26,
            KeyboardKey.Down => 0x28,
            KeyboardKey.Left => 0x25,
            KeyboardKey.Right => 0x27,
            KeyboardKey.Home => 0x24,
            KeyboardKey.End => 0x23,
            KeyboardKey.PageUp => 0x21,
            KeyboardKey.PageDown => 0x22,
            KeyboardKey.CapsLock => 0x14,
            KeyboardKey.NumLock => 0x90,
            KeyboardKey.ScrollLock => 0x91,
            KeyboardKey.PrintScreen => 0x2C,
            KeyboardKey.PauseBreak => 0x13,
            KeyboardKey.Menu => 0x5D,
            KeyboardKey.VolumeUp => 0xAF,
            KeyboardKey.VolumeDown => 0xAE,
            KeyboardKey.VolumeMute => 0xAD,
            KeyboardKey.Back => 0xA6,
            KeyboardKey.Forward => 0xA7,
            KeyboardKey.Search => 0xAA,
            KeyboardKey.MediaPlayPause => 0xB3,
            KeyboardKey.MediaStop => 0xB2,
            KeyboardKey.MediaNext => 0xB0,
            KeyboardKey.MediaPrevious => 0xB1,
            _ => 0
        };

        if (virtualKey == 0)
            throw new ArgumentException($"Key '{gesture.Key}' is not supported by Windows global hotkeys.", nameof(gesture));
        return (virtualKey, modifiers);
    }

    private static uint MapModifiers(KeyModifiers modifiers)
    {
        var nativeModifiers = 0U;
        if ((modifiers & KeyModifiers.Alt) != 0) nativeModifiers |= MOD_ALT;
        if ((modifiers & KeyModifiers.Control) != 0) nativeModifiers |= MOD_CONTROL;
        if ((modifiers & KeyModifiers.Shift) != 0) nativeModifiers |= MOD_SHIFT;
        if ((modifiers & KeyModifiers.Meta) != 0) nativeModifiers |= MOD_WIN;
        return nativeModifiers;
    }

    private static Exception CreateRegistrationException(KeyGesture gesture, int error) =>
        new InvalidOperationException(
            $"Windows rejected global hotkey '{gesture}'. It may already be registered by another application.",
            new Win32Exception(error));

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

            ClearHotkeys();
            UnbindWindow();
            _windows.Clear();
            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(nint windowHandle, int id, uint modifiers, uint virtualKey);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(nint windowHandle, int id);

    [LibraryImport("user32.dll", EntryPoint = "VkKeyScanW")]
    private static partial short VkKeyScanW([MarshalAs(UnmanagedType.U2)] char character);

    [LibraryImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowSubclass(
        nint windowHandle,
        [MarshalAs(UnmanagedType.FunctionPtr)]
        SubclassProcedure subclassProcedure,
        nuint subclassId,
        nuint referenceData);

    [LibraryImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RemoveWindowSubclass(
        nint windowHandle,
        [MarshalAs(UnmanagedType.FunctionPtr)]
        SubclassProcedure subclassProcedure,
        nuint subclassId);

    [LibraryImport("comctl32.dll")]
    private static partial nint DefSubclassProc(
        nint windowHandle,
        uint message,
        nuint wordParameter,
        nint longParameter);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint SubclassProcedure(
        nint windowHandle,
        uint message,
        nuint wordParameter,
        nint longParameter,
        nuint subclassId,
        nuint referenceData);

    private sealed class Registration(
        int nativeId,
        long version,
        KeyGesture gesture,
        Action action,
        uint nativeModifiers,
        uint virtualKey)
    {
        public int NativeId { get; } = nativeId;
        public long Version { get; set; } = version;
        public KeyGesture Gesture { get; } = gesture;
        public Action Action { get; set; } = action;
        public uint NativeModifiers { get; set; } = nativeModifiers;
        public uint VirtualKey { get; set; } = virtualKey;
    }

    private sealed class GlobalHotkeyRegistrationToken : IDisposable
    {
        private WindowsGlobalHotkeyService? _owner;
        private readonly KeyGesture _gesture;
        private readonly long _version;

        public GlobalHotkeyRegistrationToken(
            WindowsGlobalHotkeyService owner,
            KeyGesture gesture,
            long version)
        {
            _owner = owner;
            _gesture = gesture;
            _version = version;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.UnregisterHotkey(_gesture, _version);
        }
    }
}
