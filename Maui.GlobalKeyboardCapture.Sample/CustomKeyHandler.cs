using Maui.GlobalKeyboardCapture.Core.Interfaces;
using Maui.GlobalKeyboardCapture.Core.Models;

namespace Maui.GlobalKeyboardCapture.Sample;

/// <summary>
/// Handler de demonstração que exibe todas as teclas capturadas.
/// Útil para debug e visualização do funcionamento da captura de teclas.
/// </summary>
public class KeyDisplayHandler : IKeyHandler
{
    /// <summary>
    /// Evento disparado quando uma tecla é pressionada, retornando sua representação em string
    /// </summary>
    public event EventHandler<string> KeyPressed;

    public bool ShouldHandle(KeyEventArgs key) => true;

    public void HandleKey(KeyEventArgs key)
    {
        KeyPressed?.Invoke(this, key.ToString());
    }
}

