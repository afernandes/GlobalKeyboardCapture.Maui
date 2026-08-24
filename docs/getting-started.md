# Primeiros passos

Instale o pacote e configure as duas partes obrigatórias no `MauiProgram.cs`: lifecycle nativo e serviços.

```xml
<PackageReference Include="GlobalKeyboardCapture.Maui" Version="2.0.0" />
```

```csharp
builder
    .UseMauiApp<App>()
    .UseKeyboardHandling();

builder.Services.AddKeyboardHandling(options =>
{
    options.CaptureKeyUp = false;
    options.AllowKeyRepeat = false;
    options.EnableDiagnostics = false;
    options.EnableMetrics = false;
});
```

Injete `IKeyHandlerService`, crie um scope no ciclo de vida da página e registre os handlers necessários. Descarte o scope em `OnDisappearing`; todos os tokens pertencentes a ele serão removidos de forma idempotente.

```csharp
_scope = keyHandlerService.CreateScope("Checkout");
_scope.RegisterHandler(barcodeHandler, priority: 100);
_scope.RegisterHandler(hotkeyHandler, priority: 50);
```

Para campos de texto, suspenda a captura enquanto houver foco com `SuspendCapture()` e descarte o token ao perder foco. Consulte o [sample diagnóstico](https://github.com/afernandes/GlobalKeyboardCapture.Maui/tree/main/GlobalKeyboardCapture.Maui.Sample) para um fluxo completo.
