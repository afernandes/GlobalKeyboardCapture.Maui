using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Platforms.Windows;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;
using DispatcherQueueTimer = Microsoft.UI.Dispatching.DispatcherQueueTimer;
using KeyEventArgs = GlobalKeyboardCapture.Maui.Core.Models.KeyEventArgs;

namespace GlobalKeyboardCapture.Maui;

internal sealed class WindowsKeyHandler : IPlatformKeyHandler, IDisposable
{
    private readonly object _lockObject = new();
    private readonly Dictionary<Microsoft.UI.Xaml.Window, WindowSubscription> _subscriptions =
        new(ReferenceEqualityComparer.Instance);
    private readonly KeyHandlerOptions _options;
    private Action<KeyEventArgs>? _onKeyPressed;
    private Action<Core.Models.KeyboardDiagnosticEventArgs>? _onDiagnostic;
    private bool _isDisposed;

    private readonly Func<VirtualKey, CoreVirtualKeyStates> _getKeyState;

    public bool SupportsMultiplePlatformViews => true;

    public WindowsKeyHandler()
        : this(new KeyHandlerOptions())
    {
    }

    public WindowsKeyHandler(KeyHandlerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _getKeyState = InputKeyboardSource.GetKeyStateForCurrentThread;
    }

    public void ConfigureHandler(Action<KeyEventArgs> onKeyPressed)
    {
        ArgumentNullException.ThrowIfNull(onKeyPressed);
        _onKeyPressed = onKeyPressed;
    }

    public void ConfigureDiagnostics(Action<Core.Models.KeyboardDiagnosticEventArgs> onDiagnostic)
    {
        ArgumentNullException.ThrowIfNull(onDiagnostic);
        _onDiagnostic = onDiagnostic;
    }

    public void Attach(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);
        if (platformView is not Microsoft.UI.Xaml.Window window)
            throw new ArgumentException("The Windows platform view must be a WinUI Window.", nameof(platformView));

        lock (_lockObject)
        {
            ThrowIfDisposed();
            if (_subscriptions.ContainsKey(window))
                return;

            var subscription = new WindowSubscription(window);
            _subscriptions.Add(window, subscription);
            window.Activated += OnWindowActivated;
            RefreshContentSubscription(subscription);
        }
    }

    private void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        if (sender is not Microsoft.UI.Xaml.Window window)
            return;

        lock (_lockObject)
        {
            if (_subscriptions.TryGetValue(window, out var subscription))
                RefreshContentSubscription(subscription);
        }
    }

    private void OnSubscribedContentUnloaded(object sender, RoutedEventArgs args)
    {
        if (sender is not FrameworkElement unloadedContent)
            return;

        lock (_lockObject)
        {
            foreach (var subscription in _subscriptions.Values)
            {
                if (!ReferenceEquals(subscription.SubscribedContent, sender))
                    continue;

                DetachSubscribedContent(subscription);
                SetUnloadedContent(subscription, unloadedContent);
                StartContentRetry(subscription);
                subscription.Window.DispatcherQueue.TryEnqueue(() =>
                {
                    lock (_lockObject)
                    {
                        if (!_subscriptions.TryGetValue(subscription.Window, out var current)
                            || !ReferenceEquals(current, subscription))
                        {
                            return;
                        }

                        RefreshContentSubscription(subscription);
                    }
                });
                return;
            }
        }
    }

    private void OnUnloadedContentLoaded(object sender, RoutedEventArgs args)
    {
        lock (_lockObject)
        {
            foreach (var subscription in _subscriptions.Values)
            {
                if (!ReferenceEquals(subscription.UnloadedContent, sender))
                    continue;

                ClearUnloadedContent(subscription);
                RefreshContentSubscription(subscription);
                return;
            }
        }
    }

    private void OnContentRetry(DispatcherQueueTimer sender, object args)
    {
        lock (_lockObject)
        {
            foreach (var subscription in _subscriptions.Values)
            {
                if (!ReferenceEquals(subscription.ContentRetryTimer, sender))
                    continue;

                RefreshContentSubscription(subscription);
                return;
            }
        }
    }

    private bool TrySubscribe(WindowSubscription subscription)
    {
        var content = subscription.Window.Content;
        if (content == null)
            return false;
        if (ReferenceEquals(content, subscription.UnloadedContent))
            return false;
        if (ReferenceEquals(content, subscription.SubscribedContent))
            return true;

        // Move the subscription to the exact current content element.
        DetachSubscribedContent(subscription);
        ClearUnloadedContent(subscription);
        content.PreviewKeyDown += OnKeyDown;
        if (_options.CaptureKeyUp)
            content.PreviewKeyUp += OnKeyUp;
        if (content is FrameworkElement frameworkElement)
            frameworkElement.Unloaded += OnSubscribedContentUnloaded;
        subscription.SubscribedContent = content;
        return true;
    }

    private void RefreshContentSubscription(WindowSubscription subscription)
    {
        if (TrySubscribe(subscription))
            StopContentRetry(subscription);
        else
            StartContentRetry(subscription);
    }

    private void StartContentRetry(WindowSubscription subscription)
    {
        if (subscription.ContentRetryTimer is null)
        {
            var timer = subscription.Window.DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.IsRepeating = true;
            timer.Tick += OnContentRetry;
            subscription.ContentRetryTimer = timer;
        }

        if (!subscription.ContentRetryTimer.IsRunning)
            subscription.ContentRetryTimer.Start();
    }

    private static void StopContentRetry(WindowSubscription subscription)
    {
        if (subscription.ContentRetryTimer?.IsRunning == true)
            subscription.ContentRetryTimer.Stop();
    }

    public bool Detach(object platformView)
    {
        if (platformView is not Microsoft.UI.Xaml.Window window)
            return false;

        lock (_lockObject)
        {
            if (!_subscriptions.Remove(window, out var subscription))
                return false;

            Unsubscribe(subscription);
            return true;
        }
    }

    private void Unsubscribe(WindowSubscription subscription)
    {
        subscription.Window.Activated -= OnWindowActivated;
        if (subscription.ContentRetryTimer is not null)
        {
            StopContentRetry(subscription);
            subscription.ContentRetryTimer.Tick -= OnContentRetry;
            subscription.ContentRetryTimer = null;
        }
        DetachSubscribedContent(subscription);
        ClearUnloadedContent(subscription);
    }

    private void SetUnloadedContent(
        WindowSubscription subscription,
        FrameworkElement unloadedContent)
    {
        ClearUnloadedContent(subscription);
        subscription.UnloadedContent = unloadedContent;
        unloadedContent.Loaded += OnUnloadedContentLoaded;
    }

    private void ClearUnloadedContent(WindowSubscription subscription)
    {
        if (subscription.UnloadedContent is null)
            return;

        subscription.UnloadedContent.Loaded -= OnUnloadedContentLoaded;
        subscription.UnloadedContent = null;
    }

    private void DetachSubscribedContent(WindowSubscription subscription)
    {
        if (subscription.SubscribedContent != null)
        {
            subscription.SubscribedContent.PreviewKeyDown -= OnKeyDown;
            subscription.SubscribedContent.PreviewKeyUp -= OnKeyUp;
            if (subscription.SubscribedContent is FrameworkElement frameworkElement)
                frameworkElement.Unloaded -= OnSubscribedContentUnloaded;
            subscription.SubscribedContent = null;
        }
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs args)
    {
        if (args.Handled)
            return;

        if (args.KeyStatus.WasKeyDown && !_options.AllowKeyRepeat)
            return;

        ProcessKey(args, Core.Models.KeyboardEventType.KeyDown);
    }

    private void OnKeyUp(object sender, KeyRoutedEventArgs args)
    {
        if (args.Handled || !_options.CaptureKeyUp)
            return;

        ProcessKey(args, Core.Models.KeyboardEventType.KeyUp);
    }

    private void ProcessKey(KeyRoutedEventArgs args, Core.Models.KeyboardEventType eventType)
    {
        var keyEvent = new Core.Models.KeyEventArgs
        {
            Platform = Core.Models.KeyboardPlatform.Windows,
            EventType = eventType,
            Location = GetKeyLocation(args.Key),
            NativeKeyCode = (int)args.Key,
            NativeScanCode = (int)args.KeyStatus.ScanCode,
            NativeFlags = _onDiagnostic is null
                ? null
                : $"Extended={args.KeyStatus.IsExtendedKey};Menu={args.KeyStatus.IsMenuKeyDown};WasDown={args.KeyStatus.WasKeyDown}",
            RepeatCount = eventType == Core.Models.KeyboardEventType.KeyDown && args.KeyStatus.WasKeyDown
                ? Math.Max(1, (int)args.KeyStatus.RepeatCount)
                : 0,

            // Modifiers
            ControlKey = (_getKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down,
            AltKey = (_getKeyState(VirtualKey.Menu) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down,
            ShiftKey = (_getKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down,
            WindowsKey = ((_getKeyState(VirtualKey.LeftWindows) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down) ||
                         ((_getKeyState(VirtualKey.RightWindows) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down),

            // Navigation
            UpKey = args.Key == VirtualKey.Up,
            DownKey = args.Key == VirtualKey.Down,
            LeftKey = args.Key == VirtualKey.Left,
            RightKey = args.Key == VirtualKey.Right,
            HomeKey = args.Key == VirtualKey.Home,
            EndKey = args.Key == VirtualKey.End,
            PageUpKey = args.Key == VirtualKey.PageUp,
            PageDownKey = args.Key == VirtualKey.PageDown,

            // Edition
            EnterKey = args.Key == VirtualKey.Enter,
            TabKey = args.Key == VirtualKey.Tab,
            BackspaceKey = args.Key == VirtualKey.Back,
            DeleteKey = args.Key == VirtualKey.Delete,
            EscapeKey = args.Key == VirtualKey.Escape,
            SpaceKey = args.Key == VirtualKey.Space,
            InsertKey = args.Key == VirtualKey.Insert,

            // System
            CapsLockKey = args.Key == VirtualKey.CapitalLock,
            NumLockKey = args.Key == VirtualKey.NumberKeyLock,
            ScrollLockKey = args.Key == VirtualKey.Scroll,
            PrintScreenKey = args.Key == VirtualKey.Print,
            PauseBreakKey = args.Key == VirtualKey.Pause,
            MenuKey = args.Key == VirtualKey.Application,

            PlatformEvent = args
        };

        var character = KeyboardHelper.ToChar(args.Key);
        var functionKey = character == null ? KeyboardHelper.ToFunction(args.Key) : null;

        keyEvent.Character = character;
        keyEvent.FunctionKey = functionKey;


        _onKeyPressed?.Invoke(keyEvent);

        if (keyEvent.Handled)
            args.Handled = true;
    }

    public void Cleanup()
    {
        lock (_lockObject)
        {
            if (_isDisposed)
                return;

            foreach (var subscription in _subscriptions.Values)
                Unsubscribe(subscription);
            _subscriptions.Clear();
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

            foreach (var subscription in _subscriptions.Values)
                Unsubscribe(subscription);
            _subscriptions.Clear();
            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private static Core.Models.KeyLocation GetKeyLocation(VirtualKey key)
    {
        if (key is >= VirtualKey.NumberPad0 and <= VirtualKey.Divide)
            return Core.Models.KeyLocation.Numpad;
        if (key is VirtualKey.LeftControl or VirtualKey.LeftMenu or VirtualKey.LeftShift or VirtualKey.LeftWindows)
            return Core.Models.KeyLocation.Left;
        if (key is VirtualKey.RightControl or VirtualKey.RightMenu or VirtualKey.RightShift or VirtualKey.RightWindows)
            return Core.Models.KeyLocation.Right;
        return Core.Models.KeyLocation.Standard;
    }

    private sealed class WindowSubscription(Microsoft.UI.Xaml.Window window)
    {
        public Microsoft.UI.Xaml.Window Window { get; } = window;
        public Microsoft.UI.Xaml.UIElement? SubscribedContent { get; set; }
        public FrameworkElement? UnloadedContent { get; set; }
        public DispatcherQueueTimer? ContentRetryTimer { get; set; }
    }
}
