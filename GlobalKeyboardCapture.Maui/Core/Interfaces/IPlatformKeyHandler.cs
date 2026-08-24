namespace GlobalKeyboardCapture.Maui.Core.Interfaces;

/// <summary>Defines the native keyboard-source adapter used by the dispatch service.</summary>
public interface IPlatformKeyHandler
{
    /// <summary>Gets whether this adapter can attach to multiple native views concurrently.</summary>
    bool SupportsMultiplePlatformViews { get; }

    /// <summary>Attaches capture to a native platform view.</summary>
    /// <param name="platformView">The platform-specific view.</param>
    void Attach(object platformView);

    /// <summary>Detaches capture from a native platform view.</summary>
    /// <param name="platformView">The platform-specific view.</param>
    /// <returns><see langword="true"/> when the view was attached.</returns>
    bool Detach(object platformView);

    /// <summary>Configures the callback that receives normalized events.</summary>
    /// <param name="onKeyPressed">The dispatch callback.</param>
    void ConfigureHandler(Action<Models.KeyEventArgs> onKeyPressed);

    /// <summary>Configures the optional callback that receives native diagnostics.</summary>
    /// <param name="onDiagnostic">The diagnostic callback.</param>
    void ConfigureDiagnostics(Action<Models.KeyboardDiagnosticEventArgs> onDiagnostic)
    {
    }

    /// <summary>Detaches every native source owned by the adapter.</summary>
    void Cleanup();
}
