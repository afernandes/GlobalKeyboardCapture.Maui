# GlobalKeyboardCapture.Maui

Captura de teclado em toda a aplicação para .NET MAUI 10, com handlers tipados, hotkeys, scanner keyboard-wedge, scopes, sequências, diagnostics e metadados do dispositivo de entrada.

O pipeline normal é **app-wide**: recebe teclas enquanto uma janela do aplicativo está ativa. Hotkey realmente global ao sistema operacional é um recurso separado, disponível apenas no Windows por `IGlobalHotkeyService`.

## Plataformas

| Plataforma | Captura app-wide | Teclas de função | Propagação nativa |
|---|---:|---:|---|
| Windows | Sim | F1–F24 | Pode ser interrompida quando `Handled` é respeitado |
| Android | Sim | F1–F12 | O callback original sempre permanece encadeado |
| iOS | Sim, teclado externo | F1–F20 | Observacional; não impede a propagação Apple |
| Mac Catalyst | Sim | F1–F20 | Observacional; não impede a propagação Apple |

Comece em [Primeiros passos](getting-started.md), consulte os guias por cenário e use a <xref:GlobalKeyboardCapture.Maui> para os contratos completos.
