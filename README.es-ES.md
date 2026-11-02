# GlobalKeyboardCapture.Maui

[English](README.md) | Español

Captura de teclado físico en toda la aplicación para .NET MAUI, con atajos tipados, lectores de códigos de barras que emulan un teclado, secuencias de teclas, diagnósticos y atajos globales del sistema operativo en Windows.

[![NuGet](https://img.shields.io/nuget/v/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![Descargas](https://img.shields.io/nuget/dt/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![Licencia](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_MAUI-10.0-512BD4)](https://dotnet.microsoft.com/apps/maui)

> Los manejadores normales reciben teclas mientras una ventana de la aplicación está activa. `IGlobalHotkeyService` es una función independiente, exclusiva de Windows, que puede activarse cuando otra aplicación tiene el foco.

Guías y referencia de la API: [documentación del proyecto](https://afernandes.github.io/GlobalKeyboardCapture.Maui/). Este README describe la línea 2.x. La [versión en inglés](README.md) contiene ejemplos completos de las funciones avanzadas.

## Versiones y plataformas

| Línea | .NET MAUI | Android | Windows | iOS | Mac Catalyst | Política |
|---|---:|:---:|:---:|:---:|:---:|---|
| 2.x | 10 | Sí | Sí | Sí | Sí | Desarrollo activo |
| 1.x | 8 | Sí | Sí | — | — | Compatibilidad en `codex/net8-maintenance-1.x` |

Las cargas de trabajo de .NET MAUI 8 están fuera del ciclo de soporte de Microsoft. La rama 1.x se conserva por compatibilidad, pero no puede recibir correcciones de plataforma que Microsoft ya no publica. Las funciones y plataformas nuevas se desarrollan en 2.x.

Versiones mínimas de 2.x: Android 5.0 (API 21), Windows 10 compilación 17763, iOS 15 y Mac Catalyst 15.

## Características

- Manejadores ordenados por prioridad, con registros que se liberan mediante `Dispose()`.
- APIs tipadas `KeyboardKey`, `KeyModifiers` y `KeyGesture`, con alias de cadenas compatibles.
- Perfiles de lector con filtros de dispositivo, límites de tiempo, prefijos, sufijos, terminadores y búferes limitados.
- Ámbitos de captura y suspensión anidada para páginas, diálogos y controles de texto.
- Diagnósticos nativos, metadatos de dispositivos y métricas opcionales.
- Captura opcional de liberación de teclas y repetición automática.
- Manejadores asíncronos con cancelación y copias independientes de los eventos.
- Secuencias como `Ctrl+K`, seguido de `Ctrl+C`.
- Gestión del ciclo de vida de múltiples ventanas.
- Atajos globales de Windows mediante `RegisterHotKey` y `WM_HOTKEY`.
- Análisis de trimming y AOT sin conservación basada en reflexión.

## Instalación y configuración

```bash
dotnet add package GlobalKeyboardCapture.Maui
```

Las dos llamadas son necesarias: una conecta los eventos nativos del ciclo de vida y la otra registra los servicios.

```csharp
using GlobalKeyboardCapture.Maui.Configuration;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();

    builder
        .UseMauiApp<App>()
        .UseKeyboardHandling();

    builder.Services.AddKeyboardHandling(options =>
    {
        options.StopOnHandled = true;
        options.CaptureKeyUp = false;
        options.AllowKeyRepeat = false;
        options.EnableDiagnostics = false;

        options.BarcodeProfiles.Add(new BarcodeScannerProfile("Retail")
        {
            InterCharacterTimeout = TimeSpan.FromMilliseconds(120),
            MaxAverageInterCharacterDelay = TimeSpan.FromMilliseconds(60),
            MinLength = 5,
            MaxLength = 128
        });
    });

    return builder.Build();
}
```

`StopOnHandled` es `false` de forma predeterminada por compatibilidad con 1.x. Al activarlo, el procesamiento termina después del primer manejador que establece `KeyEventArgs.Handled = true`.

## Registrar manejadores de forma segura

Use un ámbito o conserve el token devuelto por el registro. Las prioridades más altas se ejecutan primero; a igual prioridad, se conserva el orden de registro.

```csharp
using GlobalKeyboardCapture.Maui.Core.Interfaces;
using GlobalKeyboardCapture.Maui.Handlers;

public partial class CheckoutPage : ContentPage
{
    private readonly IKeyHandlerService _keyboard;
    private readonly BarcodeHandler _barcode;
    private readonly HotkeyHandler _hotkeys;
    private IKeyboardCaptureScope? _scope;

    public CheckoutPage(
        IKeyHandlerService keyboard,
        BarcodeHandler barcode,
        HotkeyHandler hotkeys)
    {
        InitializeComponent();
        _keyboard = keyboard;
        _barcode = barcode;
        _hotkeys = hotkeys;

        _barcode.ScanCompleted += (_, scan) =>
            MainThread.BeginInvokeOnMainThread(() => ProcessBarcode(scan.Value));

        _hotkeys.RegisterHotkey("F2", EnableEditMode);
        _hotkeys.RegisterHotkey("Ctrl+S", Save);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _scope = _keyboard.CreateScope("Checkout");
        _scope.RegisterHandler(_barcode, priority: 100);
        _scope.RegisterHandler(_hotkeys, priority: 50);
    }

    protected override void OnDisappearing()
    {
        _scope?.Dispose();
        _scope = null;
        base.OnDisappearing();
    }
}
```

Los métodos de negocio del ejemplo (`ProcessBarcode`, `EnableEditMode` y `Save`) pertenecen a la aplicación consumidora. Registrar el mismo manejador en el mismo ámbito es idempotente. Liberar un token antiguo nunca elimina un registro posterior que lo haya sustituido.

## Atajos tipados y globales

Se recomienda `KeyGesture` para código reutilizable:

```csharp
using GlobalKeyboardCapture.Maui.Core.Models;

IDisposable saveRegistration = hotkeys.RegisterHotkey(
    new KeyGesture('S', KeyModifiers.Control),
    Save);

IDisposable escapeRegistration = hotkeys.RegisterHotkey(
    new KeyGesture(KeyboardKey.Escape),
    Cancel);

saveRegistration.Dispose();
```

La API de cadenas no distingue mayúsculas ni el orden de los modificadores. Incluye alias como `Control` → `Ctrl`, `Command`/`Windows` → `Win`, `Escape` → `Esc` y `Return` → `Enter`. `KeyGesture.ToString()` y `KeyEventArgs.ToString()` usan el orden canónico `Ctrl+Alt+Shift+Win`; esta representación es un contrato de compatibilidad.

Para atajos globales del sistema, obtenga `IGlobalHotkeyService` y compruebe `IsSupported` e `IsAttached` antes de registrar. Solo Windows ofrece esta función. Una combinación ocupada por otro proceso provoca `InvalidOperationException`, con el error nativo como excepción interna. Libere el token o llame a `ClearHotkeys()` al terminar.

## Lectores, suspensión y dispositivos

Configure `BarcodeScannerProfile` al registrar los servicios: `BarcodeHandler` copia los perfiles durante su construcción. Puede especificar `Prefix`, `Suffix`, `RequirePrefix`, `RequireSuffix`, `StripPrefix`, `StripSuffix`, `TerminatorKeys`, `TerminatorCharacters` y `DeviceId`, además de los límites de longitud y tiempo.

`ScanCompleted` entrega los valores decodificado y original, perfil, duración, cantidad de caracteres, terminador y dispositivo. `BarcodeScanned` conserva el evento compatible que entrega solo el valor. Una entrada sin terminador se reinicia al alcanzar `MaxLength`.

Use `keyboard.SuspendCapture()` mientras un control de texto necesita la entrada normal y libere el token al perder el foco. La captura se reanuda al liberar todos los tokens activos. `ResumeCapture()` elimina todas las suspensiones; reserve esa operación para recuperación explícita.

`KeyboardDeviceFilter` filtra registros y ámbitos mediante condiciones AND. Los criterios de dispositivo rechazan eventos cuando la plataforma no identifica su origen. Un filtro que solo indica la plataforma no requiere `KeyboardDeviceInfo`.

## Diagnósticos, métricas y secuencias

- Active `EnableDiagnostics` durante una investigación y suscríbase a `DiagnosticEvent`. Los metadatos dependen de la plataforma; Apple presenta el teclado físico combinado, sin identificar cada dispositivo.
- Active `EnableMetrics` cuando un `MeterListener` u OpenTelemetry consuma el medidor `GlobalKeyboardCapture.Maui`. Las métricas están desactivadas de forma predeterminada.
- `CaptureKeyUp` y `AllowKeyRepeat` habilitan liberaciones y repeticiones. Los manejadores integrados de atajos, lectores y secuencias reaccionan solo a pulsaciones.
- `KeySequenceHandler.RegisterSequence(...)` requiere al menos dos gestos. Las secuencias rechazan repeticiones, caducan al agotar el tiempo y ejecutan acciones en el hilo principal de MAUI.
- `SequenceOverlapPolicy` ofrece `ExecuteImmediately`, `PreferLongest` y `RejectAmbiguous`. `GetProgressSnapshot()` devuelve una copia independiente y `CancelPendingSequences()` conserva los registros.

## Manejadores personalizados

```csharp
public sealed class CustomKeyHandler : IKeyHandler
{
    public bool ShouldHandle(KeyEventArgs key) =>
        key.EventType == KeyboardEventType.KeyDown;

    public void HandleKey(KeyEventArgs key)
    {
        Console.WriteLine($"{key} ({key.Platform}, native {key.NativeKeyCode})");
        key.Handled = true;
    }
}
```

Los manejadores síncronos se ejecutan en el hilo de entrada nativo. Manténgalos breves y envíe las operaciones de interfaz a `MainThread.BeginInvokeOnMainThread`.

Use `IAsyncKeyHandler` para E/S que no deba bloquear la entrada. Recibe una copia independiente del evento y un token que se cancela al liberar el servicio. Sus cambios a `Handled` no pueden afectar a tiempo la propagación nativa.

## Compatibilidad de teclas

| Grupo | Windows | Android | iOS / Mac Catalyst |
|---|---|---|---|
| Letras, dígitos y puntuación común | Sí | Sí | Mapeo USB HID |
| Modificadores | Ctrl, Alt, Shift, Win | Ctrl, Alt, Shift, Meta | Ctrl, Option, Shift, Command |
| Función | F1–F24 | F1–F12 | F1–F20 |
| Navegación y edición | Sí | Sí | Sí |
| Ubicación de Enter numérico | Puede informarse como Enter estándar | `Numpad` | `Numpad` |
| Bloqueo, Imprimir pantalla y Pausa | Sí | Depende del dispositivo | Teclas HID cuando se reciben |
| Volumen, multimedia, Atrás, Adelante y Buscar | Registro global; captura en la aplicación variable | Sí | Sin normalización actualmente |
| Liberación de teclas | Opcional | Opcional | Opcional |
| Múltiples ventanas | Sí | Reconexión a la Activity activa | Teclado combinado |
| Impedir propagación nativa al manejar | Sí | Sí | No; los eventos GameController son observacionales |
| Activación global del sistema | Sí | No | No |

La distribución del teclado puede cambiar los símbolos imprimibles. Windows y Android usan etiquetas de plataforma cuando están disponibles. Apple admite `IKeyboardLayoutTranslator`; el ejemplo alimenta `KeyboardLayoutTranslationCache` desde `UIKey.Characters` y conserva el mapeo USB HID estadounidense como alternativa. Use `NativeKeyCode` y diagnósticos si importa la posición física.

## Ciclo de vida, trimming y migración

`UseKeyboardHandling()` conecta las fuentes nativas al crear las vistas y las libera al destruirlas. Windows admite ventanas simultáneas. Android conserva y reenvía el `Window.Callback` original y se reconecta al recrearse la Activity.

El paquete declara `IsTrimmable` e `IsAotCompatible`, sin conservar todo el ensamblado ni usar reflexión para mapear teclas. El CI compila las plataformas, ejecuta pruebas independientes de plataforma en Windows y Linux y pruebas de integración de Android y WinUI. Las pruebas físicas de Firebase Test Lab requieren configuración OIDC.

Para migrar de 1.x a 2.x, actualice a .NET MAUI 10 y sus cargas de trabajo, conserve las dos llamadas de configuración y prefiera gestos tipados y tokens. Las implementaciones de plataforma son internas; use `IPlatformKeyHandler` para adaptadores propios. `IKeyHandlerService` incorpora prioridades, ámbitos, suspensión, diagnósticos e introspección de estado.

Consulte [CHANGELOG.md](CHANGELOG.md) para el historial y [ROADMAP.md](ROADMAP.md) para el estado de validación.

## Compilación y pruebas

```bash
dotnet restore GlobalKeyboardCapture.Maui.sln
dotnet test --project GlobalKeyboardCapture.Maui.Tests/GlobalKeyboardCapture.Maui.Tests.csproj -c Release
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-android
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-windows10.0.19041.0
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-ios
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-maccatalyst
dotnet pack GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release
```

## Contribuciones y licencia

Las issues y contribuciones son bienvenidas. Al informar de una tecla ausente, indique plataforma, diagnósticos nativos, modelo de teclado o lector, distribución y una reproducción mínima.

Licencia [MIT](LICENSE). Copyright Anderson Fernandes do Nascimento.
