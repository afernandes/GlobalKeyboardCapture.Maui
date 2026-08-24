# Troubleshooting

## Nenhuma tecla é recebida

Confirme que `.UseKeyboardHandling()` e `.AddKeyboardHandling(...)` foram chamados, que existe ao menos uma platform view (`PlatformViewCount > 0`), que o handler está registrado e que `IsCapturing` está ativo. Ative `EnableDiagnostics` temporariamente.

## Android

- F1–F12 usam keycodes Android; F13+ não existem no contrato da plataforma.
- Enter do numpad deve aparecer como `Key=Enter`, `Location=Numpad` e keycode 160.
- Eventos `Fallback` e repeats são filtrados conforme a configuração.
- Em scanner físico, registre `Device.Id`, descriptor, versão Android, conexão e terminador real.

## Windows

- Captura app-wide exige janela ativa; `IGlobalHotkeyService` é o caminho OS-global.
- Se o conteúdo da janela for substituído, verifique diagnostics de attach/detach e ausência de duplicidade.
- Uma combinação global pode falhar se já estiver registrada por outro processo.

## iOS e Mac Catalyst

- É necessário teclado externo; o teclado virtual não equivale a `GCKeyboard`.
- `Handled` é observacional e não bloqueia a propagação nativa Apple.
- Pontuação usa fallback HID de layout US enquanto a tradução sensível ao layout não estiver disponível no evento observado.
- Teste reconnect, background/foreground e fechamento de janela em hardware Apple real.

## Hotkey registrada não dispara

Registre o gesto canônico retornado por `KeyEventArgs.ToString()` ou use `KeyGesture`. Confira `EventType`, repeat, modifiers e filtro de dispositivo. Para sequências sobrepostas, confirme `OverlapPolicy` e o snapshot de progresso.
