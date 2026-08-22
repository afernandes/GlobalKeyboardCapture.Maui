namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

internal interface IPlatformViewLifecycleSink
{
    void OnPlatformViewCreated(object platformView);
    void OnPlatformViewDestroyed(object platformView);
}
