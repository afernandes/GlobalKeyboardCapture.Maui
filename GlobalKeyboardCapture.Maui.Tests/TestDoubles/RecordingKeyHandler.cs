using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Core.Models;

namespace GlobalKeyboardCapture.Maui.Tests.TestDoubles;

internal sealed class RecordingKeyHandler : IKeyHandler
{
    private readonly Func<KeyEventArgs, bool> _shouldHandle;
    private readonly Action<KeyEventArgs>? _onHandle;

    public List<KeyEventArgs> HandledKeys { get; } = new();

    public RecordingKeyHandler(
        Func<KeyEventArgs, bool>? shouldHandle = null,
        Action<KeyEventArgs>? onHandle = null)
    {
        _shouldHandle = shouldHandle ?? (_ => true);
        _onHandle = onHandle;
    }

    public bool ShouldHandle(KeyEventArgs key) => _shouldHandle(key);

    public void HandleKey(KeyEventArgs key)
    {
        HandledKeys.Add(key);
        _onHandle?.Invoke(key);
    }
}
