namespace Microsoft.Maui.ApplicationModel;

internal static class MainThread
{
    public static void BeginInvokeOnMainThread(Action action) => action();
}
