using GlobalKeyboardCapture.Maui.Core.Interfaces;

namespace GlobalKeyboardCapture.Maui.Handlers;

/// <summary>
/// Owns one platform-view attachment lease for each native window or activity.
/// </summary>
public sealed class KeyHandlerLifecycleHandler : ILifecycleHandler
{
    private readonly object _lockObject = new();
    private readonly IKeyHandlerService _keyHandlerService;
    private readonly IPlatformViewLifecycleSink[] _lifecycleSinks;
    private readonly Dictionary<object, IDisposable> _attachments =
        new(ReferenceEqualityComparer.Instance);
    private bool _isDisposed;

    public KeyHandlerLifecycleHandler(IKeyHandlerService keyHandlerService)
        : this(keyHandlerService, [])
    {
    }

    internal KeyHandlerLifecycleHandler(
        IKeyHandlerService keyHandlerService,
        IEnumerable<IPlatformViewLifecycleSink> lifecycleSinks)
    {
        _keyHandlerService = keyHandlerService ?? throw new ArgumentNullException(nameof(keyHandlerService));
        ArgumentNullException.ThrowIfNull(lifecycleSinks);
        _lifecycleSinks = lifecycleSinks.ToArray();
    }

    public void OnPlatformViewCreated(object platformView)
    {
        ArgumentNullException.ThrowIfNull(platformView);

        lock (_lockObject)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            if (_attachments.ContainsKey(platformView))
                return;

            var attachment = _keyHandlerService.AttachPlatformView(platformView);
            NotifyCreated(platformView);
            _attachments.Add(platformView, attachment);
        }
    }

    public void OnPlatformViewDestroyed(object platformView)
    {
        if (platformView is null)
            return;

        IDisposable? attachment;
        lock (_lockObject)
        {
            if (_isDisposed || !_attachments.Remove(platformView, out attachment))
                return;
        }

        NotifyDestroyed(platformView);
        attachment.Dispose();
    }

    public void Dispose()
    {
        KeyValuePair<object, IDisposable>[] attachments;
        lock (_lockObject)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            attachments = _attachments.ToArray();
            _attachments.Clear();
        }

        foreach (var attachment in attachments)
        {
            NotifyDestroyed(attachment.Key);
            attachment.Value.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private void NotifyCreated(object platformView)
    {
        foreach (var lifecycleSink in _lifecycleSinks)
        {
            try
            {
                lifecycleSink.OnPlatformViewCreated(platformView);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[GlobalKeyboardCapture] Platform-view creation observer failed: {exception}");
            }
        }
    }

    private void NotifyDestroyed(object platformView)
    {
        foreach (var lifecycleSink in _lifecycleSinks)
        {
            try
            {
                lifecycleSink.OnPlatformViewDestroyed(platformView);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[GlobalKeyboardCapture] Platform-view destruction observer failed: {exception}");
            }
        }
    }
}
