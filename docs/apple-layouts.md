# Layouts de teclado no Apple

`GCKeyboard` informa a posição física USB HID e é a fonte da captura. Para letras e teclas nomeadas isso é estável; símbolos de pontuação dependem do layout ativo. A biblioteca mantém o mapa HID US como fallback e aceita um `IKeyboardLayoutTranslator` opcional antes desse fallback.

O sample registra um `KeyboardLayoutTranslationCache` em `KeyHandlerOptions.KeyboardLayoutTranslator`. Os `AppDelegate` de iOS e Mac Catalyst observam `UIPress.Key`: `UIKey.KeyCode` identifica a posição física e `UIKey.Characters` fornece o texto traduzido pelo sistema. O collector apenas alimenta o cache; ele não dispara uma segunda tecla. O evento físico continua vindo de `GCKeyboard`.

Esse desenho preserva três propriedades:

- nenhuma reflexão ou consulta bloqueante no callback de input;
- `NativeKeyCode` continua disponível para consumidores que querem posição física;
- se o responder UIKit não observar a tecla, o mapa HID US continua funcionando.

## Configuração

Registre uma instância compartilhada:

```csharp
var translations = new KeyboardLayoutTranslationCache();
builder.Services.AddSingleton(translations);

builder.Services.AddKeyboardHandling(options =>
{
    options.KeyboardLayoutTranslator = translations;
});
```

Em um `UIResponder` adequado do aplicativo, converta `UIKey.ModifierFlags` para `KeyModifiers`, crie `KeyboardLayoutTranslationContext` com `UIKey.KeyCode` e chame `SetTranslation(context, key.Characters)`. O sample contém a implementação compilável completa em `AppleKeyboardLayoutCollector.cs`.

Chame `Clear()` quando o aplicativo detectar mudança de layout. Entradas com controle, surrogate ou mais de um caractere são rejeitadas; nesses casos o fallback HID é usado.

## Validação

Os testes neutros cobrem precedência, fallback, caracteres inválidos e exemplos de pontuação US/ABNT2/ISO. A aceitação final exige iPad e Mac físicos: altere o layout do sistema, pressione as mesmas posições físicas e confirme o caractere normalizado e o `NativeKeyCode` no stream diagnóstico.
