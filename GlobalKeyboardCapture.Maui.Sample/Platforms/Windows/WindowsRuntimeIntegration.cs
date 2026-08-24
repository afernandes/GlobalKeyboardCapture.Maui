#if WINDOWS
using System.Runtime.InteropServices;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;
using Microsoft.Maui.Platform;
using WinUIButton = Microsoft.UI.Xaml.Controls.Button;
using WinUIControl = Microsoft.UI.Xaml.Controls.Control;
using WinUIFocusState = Microsoft.UI.Xaml.FocusState;

namespace GlobalKeyboardCapture.Maui.Sample;

internal static class WindowsRuntimeIntegration
{
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const int SW_RESTORE = 9;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_MENU = 0x12;
    private const byte VK_F7 = 0x76;
    private const byte VK_F8 = 0x77;
    private const byte VK_F9 = 0x78;
    private const byte VK_F10 = 0x79;
    private const byte VK_F11 = 0x7A;
    private const byte VK_F12 = 0x7B;

    private static int _started;
    private static int _nativePreviewKeyDownCount;
    private static int _nativePreviewKeyUpCount;
    private static nint _focusedWindowHandle;

    public static void TryStart(
        Page page,
        IKeyHandlerService service,
        IGlobalHotkeyService globalHotkeys)
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("GKC_WINDOWS_INTEGRATION"),
                "1",
                StringComparison.Ordinal)
            || Interlocked.Exchange(ref _started, 1) != 0)
        {
            return;
        }

        var outputPath = Environment.GetEnvironmentVariable("GKC_WINDOWS_INTEGRATION_OUTPUT")
            ?? Path.Combine(Path.GetTempPath(), "gkc-windows-integration.log");
        File.WriteAllText(outputPath, $"INFO HarnessScheduled{Environment.NewLine}");
        _ = RunAsync(page, service, globalHotkeys, outputPath);
    }

    private static async Task RunAsync(
        Page page,
        IKeyHandlerService service,
        IGlobalHotkeyService globalHotkeys,
        string outputPath)
    {
        var evidence = new List<string> { "INFO HarnessScheduled" };
        var exitCode = 0;
        RuntimeRecordingHandler? recorder = null;

        try
        {
            Microsoft.Maui.Controls.Window? firstWindow = null;
            await WaitUntilAsync(
                () => (firstWindow = page.Window) is not null,
                "MAUI window assignment");

            recorder = new RuntimeRecordingHandler();
            using var recorderRegistration = service.RegisterHandler(recorder, priority: int.MaxValue);
            await WaitUntilAsync(() => service.PlatformViewCount == 1, "initial window attachment");

            var firstNativeWindow = await WaitForNativeWindowAsync(firstWindow!);
            await ReplaceContentAndFocusAsync(firstNativeWindow, "First integration window");
            await WaitForCaptureReadyAsync(recorder);
            await SendAndExpectAsync(recorder, KeyboardKey.F8, VK_F8);
            evidence.Add("PASS KeyDownKeyUp");

            var application = Application.Current
                ?? throw new InvalidOperationException("MAUI application is unavailable.");
            var secondWindow = new Microsoft.Maui.Controls.Window(new ContentPage
            {
                Content = new Label { Text = "Second integration window" }
            });
            application.OpenWindow(secondWindow);
            await WaitUntilAsync(() => service.PlatformViewCount == 2, "second window attachment");

            var secondNativeWindow = await WaitForNativeWindowAsync(secondWindow);
            await ReplaceContentAndFocusAsync(secondNativeWindow, "Second integration window");
            await WaitForCaptureReadyAsync(recorder);
            await SendAndExpectAsync(recorder, KeyboardKey.F9, VK_F9);
            evidence.Add("PASS TwoWindows");

            var replacementButton = await ReplaceContentAndFocusAsync(
                firstNativeWindow,
                "Replacement content");
            await WaitForCaptureReadyAsync(recorder);
            await SendAndExpectAsync(recorder, KeyboardKey.F10, VK_F10);
            evidence.Add("PASS ContentReplacement");

            application.CloseWindow(secondWindow);
            await WaitUntilAsync(() => service.PlatformViewCount == 1, "second window detach");
            await FocusAsync(firstNativeWindow, replacementButton);
            await SendAndExpectAsync(recorder, KeyboardKey.F11, VK_F11);
            evidence.Add("PASS AttachDetach");

            var globalInvocationCount = 0;
            var globalRegistration = globalHotkeys.RegisterHotkey(
                "Ctrl+Alt+F12",
                () => Interlocked.Increment(ref globalInvocationCount));
            await FocusAsync(firstNativeWindow, replacementButton);
            await SendGlobalKeyAsync(VK_F12, VK_CONTROL, VK_MENU);
            await WaitUntilAsync(
                () => Volatile.Read(ref globalInvocationCount) == 1,
                "WM_HOTKEY dispatch");

            globalRegistration.Dispose();
            await SendGlobalKeyAsync(VK_F12, VK_CONTROL, VK_MENU);
            await Task.Delay(300);
            if (Volatile.Read(ref globalInvocationCount) != 1)
                throw new InvalidOperationException("Global hotkey fired after unregistration.");
            evidence.Add("PASS RegisterHotKeyUnregisterHotKeyWmHotkey");

            evidence.Add($"PASS PlatformViewCount={service.PlatformViewCount}");
            evidence.Add("INFO WindowInjection=SendInput");
            evidence.Add("RESULT PASS");
        }
        catch (Exception exception)
        {
            exitCode = 1;
            evidence.Add($"INFO NativePreviewKeyDown={Volatile.Read(ref _nativePreviewKeyDownCount)}");
            evidence.Add($"INFO NativePreviewKeyUp={Volatile.Read(ref _nativePreviewKeyUpCount)}");
            evidence.Add($"INFO ForegroundMatches={GetForegroundWindow() == _focusedWindowHandle}");
            evidence.Add($"INFO RecordedEvents={recorder?.Describe() ?? "none"}");
            evidence.Add($"RESULT FAIL {exception}");
        }

        await File.WriteAllLinesAsync(outputPath, evidence);
        Environment.Exit(exitCode);
    }

    private static async Task<WinUIButton> ReplaceContentAndFocusAsync(
        MauiWinUIWindow window,
        string content)
    {
        var button = new WinUIButton { Content = content };
        button.PreviewKeyDown += (_, _) =>
            Interlocked.Increment(ref _nativePreviewKeyDownCount);
        button.PreviewKeyUp += (_, _) =>
            Interlocked.Increment(ref _nativePreviewKeyUpCount);
        window.Content = button;
        await Task.Yield();
        await FocusAsync(window, button);
        await Task.Delay(100);
        return button;
    }

    private static async Task FocusAsync(MauiWinUIWindow window, WinUIControl control)
    {
        window.Activate();
        await WaitUntilAsync(() => control.IsLoaded, "WinUI control load");

        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        if (handle == 0)
            throw new InvalidOperationException("Unable to activate the WinUI integration window.");

        if (!TryActivateWindow(handle))
            throw new InvalidOperationException("Unable to move the WinUI window to the foreground.");
        _focusedWindowHandle = handle;
        await WaitUntilAsync(
            () => control.Focus(WinUIFocusState.Programmatic),
            "WinUI control focus");
    }

    private static async Task<MauiWinUIWindow> WaitForNativeWindowAsync(
        Microsoft.Maui.Controls.Window window)
    {
        MauiWinUIWindow? nativeWindow = null;
        await WaitUntilAsync(() =>
        {
            nativeWindow = window.Handler?.PlatformView as MauiWinUIWindow;
            return nativeWindow is not null;
        }, "native WinUI window");
        return nativeWindow!;
    }

    private static async Task SendAndExpectAsync(
        RuntimeRecordingHandler recorder,
        KeyboardKey key,
        byte virtualKey)
    {
        var downBefore = recorder.Count(key, KeyboardEventType.KeyDown);
        var upBefore = recorder.Count(key, KeyboardEventType.KeyUp);
        await SendWindowKeyAsync(_focusedWindowHandle, virtualKey);
        await WaitUntilAsync(
            () => recorder.Count(key, KeyboardEventType.KeyDown) == downBefore + 1
                && recorder.Count(key, KeyboardEventType.KeyUp) == upBefore + 1,
            $"{key} key-down/key-up");
        await Task.Delay(100);
        if (recorder.Count(key, KeyboardEventType.KeyDown) != downBefore + 1
            || recorder.Count(key, KeyboardEventType.KeyUp) != upBefore + 1)
        {
            throw new InvalidOperationException($"{key} was delivered more than once.");
        }
    }

    private static async Task WaitForCaptureReadyAsync(RuntimeRecordingHandler recorder)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var downBefore = recorder.Count(KeyboardKey.F7, KeyboardEventType.KeyDown);
            var upBefore = recorder.Count(KeyboardKey.F7, KeyboardEventType.KeyUp);
            await SendWindowKeyAsync(_focusedWindowHandle, VK_F7);
            await Task.Delay(100);
            if (recorder.Count(KeyboardKey.F7, KeyboardEventType.KeyDown) == downBefore + 1
                && recorder.Count(KeyboardKey.F7, KeyboardEventType.KeyUp) == upBefore + 1)
            {
                return;
            }
        }

        throw new TimeoutException("Timed out waiting for the Windows key adapter to bind the active content.");
    }

    private static async Task SendWindowKeyAsync(nint windowHandle, byte virtualKey)
    {
        if (!TryActivateWindow(windowHandle))
            throw new InvalidOperationException("Unable to activate the WinUI window before SendInput.");

        SendKeyboardInput(virtualKey, keyUp: false);
        await Task.Delay(40);
        SendKeyboardInput(virtualKey, keyUp: true);
    }

    private static async Task SendGlobalKeyAsync(byte virtualKey, params byte[] modifiers)
    {
        foreach (var modifier in modifiers)
            SendKeyboardInput(modifier, keyUp: false);
        SendKeyboardInput(virtualKey, keyUp: false);
        await Task.Delay(40);
        SendKeyboardInput(virtualKey, keyUp: true);
        for (var index = modifiers.Length - 1; index >= 0; index--)
            SendKeyboardInput(modifiers[index], keyUp: true);
    }

    private static void SendKeyboardInput(byte virtualKey, bool keyUp)
    {
        var input = new NativeInput
        {
            Type = INPUT_KEYBOARD,
            Data = new NativeInputUnion
            {
                Keyboard = new NativeKeyboardInput
                {
                    VirtualKey = virtualKey,
                    Flags = keyUp ? KEYEVENTF_KEYUP : 0
                }
            }
        };

        if (SendInput(1, [input], Marshal.SizeOf<NativeInput>()) != 1)
        {
            throw new InvalidOperationException(
                $"SendInput failed for virtual key 0x{virtualKey:X2} with error {Marshal.GetLastWin32Error()}.");
        }
    }

    private static bool TryActivateWindow(nint windowHandle)
    {
        ShowWindow(windowHandle, SW_RESTORE);

        var currentThread = GetCurrentThreadId();
        var foregroundWindow = GetForegroundWindow();
        var foregroundThread = foregroundWindow == 0
            ? 0
            : GetWindowThreadProcessId(foregroundWindow, out _);
        var attached = foregroundThread != 0
            && foregroundThread != currentThread
            && AttachThreadInput(currentThread, foregroundThread, attach: true);

        try
        {
            // A synthetic Alt transition releases the foreground lock in desktop
            // automation sessions without changing the application shortcut state.
            SendKeyboardInput(VK_MENU, keyUp: false);
            SendKeyboardInput(VK_MENU, keyUp: true);
            BringWindowToTop(windowHandle);
            SetForegroundWindow(windowHandle);
            SetActiveWindow(windowHandle);
            SetFocus(windowHandle);
            return GetForegroundWindow() == windowHandle;
        }
        finally
        {
            if (attached)
                AttachThreadInput(currentThread, foregroundThread, attach: false);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, string operation)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (condition())
                return;
            await Task.Delay(50);
        }
        throw new TimeoutException($"Timed out waiting for {operation}.");
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint inputCount,
        [In] NativeInput[] inputs,
        int inputSize);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(nint windowHandle, int command);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool BringWindowToTop(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern nint SetActiveWindow(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern nint SetFocus(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(
        nint windowHandle,
        out uint processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachThreadInput(
        uint sourceThreadId,
        uint targetThreadId,
        [MarshalAs(UnmanagedType.Bool)] bool attach);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeInput
    {
        public uint Type;
        public NativeInputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct NativeInputUnion
    {
        [FieldOffset(0)]
        public NativeKeyboardInput Keyboard;

        [FieldOffset(0)]
        public NativeMouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeKeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    private sealed class RuntimeRecordingHandler : IKeyHandler
    {
        private readonly object _lockObject = new();
        private readonly List<(KeyboardKey Key, KeyboardEventType EventType)> _events = [];

        public bool ShouldHandle(KeyEventArgs key) => key.Platform == KeyboardPlatform.Windows;

        public void HandleKey(KeyEventArgs key)
        {
            lock (_lockObject)
                _events.Add((key.Key, key.EventType));
        }

        public int Count(KeyboardKey key, KeyboardEventType eventType)
        {
            lock (_lockObject)
            {
                var count = 0;
                foreach (var recorded in _events)
                {
                    if (recorded.Key == key && recorded.EventType == eventType)
                        count++;
                }
                return count;
            }
        }

        public string Describe()
        {
            lock (_lockObject)
                return string.Join(',', _events.Select(item => $"{item.Key}:{item.EventType}"));
        }
    }
}
#endif
