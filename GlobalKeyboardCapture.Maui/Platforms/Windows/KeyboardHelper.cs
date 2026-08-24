using System.Runtime.CompilerServices;
using GlobalKeyboardCapture.Maui.Core.Mapping;
using Windows.System;

namespace GlobalKeyboardCapture.Maui.Platforms.Windows;

internal static class KeyboardHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char? ToChar(VirtualKey key) =>
        PlatformKeyMapper.MapWindowsVirtualKey((int)key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ToFunction(VirtualKey key) =>
        PlatformKeyMapper.MapWindowsFunctionKey((int)key);
}
