using System.Runtime.CompilerServices;
using GlobalKeyboardCapture.Maui.Core.Mapping;

namespace GlobalKeyboardCapture.Maui.Platforms.Android;

internal static class KeyboardHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char? ToChar(string? key) => PlatformKeyMapper.MapAndroidDisplayLabel(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char? ToChar(char key) => PlatformKeyMapper.MapAndroidDisplayLabel(key);
}
