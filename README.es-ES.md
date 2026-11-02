

# GlobalKeyboardCapture.Maui

Una potente biblioteca .NET MAUI para captura global de teclado con sólido soporte para lectores de códigos de barras. Proporciona interceptación de teclas a nivel de sistema y gestión de accesos directos.

[![NuGet](https://img.shields.io/nuget/v/GlobalKeyboardCapture.Maui.svg)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![NuGet](https://img.shields.io/nuget/dt/GlobalKeyboardCapture.Maui.svg?label=Nuget&maxAge=60)](https://www.nuget.org/packages/GlobalKeyboardCapture.Maui/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg?maxAge=60)](https://raw.githubusercontent.com/afernandes/GlobalKeyboardCapture.Maui/master/LICENSE)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui?ref=badge_shield)
[![.NET Support](https://img.shields.io/badge/.NET-8.0%20|%209.0-512BD4)](https://dotnet.microsoft.com/)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=afernandes_GlobalKeyboardCapture.Maui&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=afernandes_GlobalKeyboardCapture.Maui)

![Demo de GlobalKeyboardCapture.Maui](https://raw.githubusercontent.com/afernandes/Maui.GlobalKeyboardCapture/refs/heads/main/Print.png)

*Aplicación de demostración que muestra la captura de teclas, el escaneo de códigos de barras y la funcionalidad de accesos directos*

## Características

- 🔑 Captura global de teclado
- 📊 Procesamiento avanzado de entrada de teclado
- 🏷️ Soporte integrado para lectores de códigos de barras
- ⌨️ Sistema de accesos directos personalizable
- 📱 Multiplataforma (Windows & Android)
- 🎛️ Altamente configurable
- 🧩 Fácil de integrar
- 🔧 Diseñado para .NET MAUI

## Casos de uso comunes

- Integración de lectores de códigos de barras
- Accesos directos globales y atajos de teclado
- Monitoreo de teclado a nivel de sistema
- Manejo personalizado de entrada de teclado
- Automatización de entrada
- Captura de teclado multimodo

## Soporte completo de teclas:
- Teclas estándar (A-Z, 0-9)
- Teclas de función (F1-F24)
- Teclas modificadoras (Ctrl, Alt, Shift)
- Teclas OEM de Windows (;, /, [, ], etc)
- Teclas especiales de Android (Volumen, Atrás, Menú)
- Teclas de navegación
- Caracteres especiales

## Instalación

```bash
dotnet add package GlobalKeyboardCapture.Maui
```

O a través del Administrador de paquetes NuGet:

```
Install-Package GlobalKeyboardCapture.Maui
```

## Inicio rápido

1. Registre el servicio en su `MauiProgram.cs`:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .UseKeyboardHandling();

    builder.Services.AddKeyboardHandling(options =>
    {
        // Barcode specific settings
        options.BarcodeTimeout = 150;
        options.MinBarcodeLength = 8;
    });

    return builder.Build();
}
```

2. Ejemplo de uso básico:

```csharp
public partial class MainPage : ContentPage
{
    private readonly IKeyHandlerService _keyHandlerService;
    private readonly BarcodeHandler _barcodeHandler;
    private readonly HotkeyHandler _hotkeyHandler;

    public MainPage(
        IKeyHandlerService keyHandlerService, 
        BarcodeHandler barcodeHandler,
        HotkeyHandler hotkeyHandler)
    {
        InitializeComponent();
        
        _keyHandlerService = keyHandlerService;
        _barcodeHandler = barcodeHandler;
        _hotkeyHandler = hotkeyHandler;
        
        SetupHandlers();
    }

    private void SetupHandlers()
    {
        // Setup barcode handling
        _barcodeHandler.BarcodeScanned += (sender, input) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProcessInput(input);
            });
        };

        // Setup global hotkeys
        _hotkeyHandler.RegisterHotkey("F2", () =>
        {
            EnableEditMode();
        });

        // Register handlers
        _keyHandlerService.RegisterHandler(_barcodeHandler);           
        _keyHandlerService.RegisterHandler(_hotkeyHandler);
    }
}
```

## Uso avanzado

### Manejador de teclas personalizado

Cree su propio manejador de teclas para necesidades específicas:

```csharp
public class CustomKeyHandler : IKeyHandler
{
    public bool ShouldHandle(string key) => true;

    public void HandleKey(string key)
    {
        // Your custom key handling logic
    }
}
```
### Accesos directos globales

La biblioteca cuenta con un sistema inteligente de normalización de accesos directos que permite un registro flexible de atajos de teclado:

```csharp
// Single key hotkeys
_hotkeyHandler.RegisterHotkey("F2", EnableEditMode);
_hotkeyHandler.RegisterHotkey("ESC", CancelOperation);

// All these registrations trigger the same hotkey (Ctrl+Alt+Shift+P)
_hotkeyHandler.RegisterHotkey("Ctrl+Alt+Shift+P", PrintAction);
_hotkeyHandler.RegisterHotkey("Shift+Ctrl+Alt+P", PrintAction);
_hotkeyHandler.RegisterHotkey("Alt+Shift+Control+P", PrintAction);

// Supports common aliases
_hotkeyHandler.RegisterHotkey("Control+Alt+P", PrintAction);  // "Control" is normalized to "Ctrl"
_hotkeyHandler.RegisterHotkey("Windows+Shift+X", ActionX);    // "Windows" is normalized to "Win"

// Case-insensitive handling
_hotkeyHandler.RegisterHotkey("CTRL+ALT+P", PrintAction);
_hotkeyHandler.RegisterHotkey("ctrl+alt+p", PrintAction);

//Special keys
_hotkeyHandler.RegisterHotkey("VolumeUp", VolumeControlAction); //Android Volume Up
_hotkeyHandler.RegisterHotkey("OEM173", VolumeControlAction); //OEM 173
```

El sistema de accesos directos proporciona:
- Normalización automática del orden de modificadores (Ctrl → Alt → Shift → Win)
- Manejo insensible a mayúsculas y minúsculas
- Soporte para alias comunes (por ejemplo, "Control" → "Ctrl")
- Comportamiento consistente independientemente del orden de registro
- Implementación de alto rendimiento con asignaciones mínimas

### Modo de lector de códigos de barras

```csharp
_barcodeHandler.BarcodeScanned += (sender, input) =>
{    
    ProcessProduct(input); 
};
```

### Optimizaciones de rendimiento

La biblioteca está optimizada para rendimiento y eficiencia:

- **Asignaciones mínimas**: Utiliza características modernas de .NET para minimizar la presión del recolector de basura
- **Búsquedas rápidas**: Implementaciones de diccionario optimizadas para la coincidencia de accesos directos
- **Manejo eficiente de cadenas**: Comparación y normalización inteligente de cadenas
- **Inlining agresivo**: Las rutas críticas están optimizadas para velocidad
- **Eficiencia de memoria**: Gestión cuidadosa de las asignaciones de memoria en rutas críticas

## Características clave

### Captura global de teclas
- Captura la entrada del teclado independientemente del foco
- Funciona con todos los controles de interfaz de usuario
- Interceptación de teclas a nivel de sistema

### Procesamiento de entrada
- Tiempo de espera de entrada configurable
- Validación y filtrado de entrada
- Reglas de procesamiento personalizadas

### Soporte de plataformas
- Aplicaciones de escritorio para Windows
- Aplicaciones móviles para Android
- API consistente en todas las plataformas

## Referencia de la API

### Servicios principales

- `IKeyHandlerService`: Servicio principal para el manejo global del teclado
- `BarcodeHandler`: Manejador especializado para escenarios de entrada de códigos de barras
- `HotkeyHandler`: Gestión de accesos directos globales

### Interfaces

- `IKeyHandler`: Interfaz base para manejadores personalizados
- `ILifecycleHandler`: Gestión del ciclo de vida de la aplicación

## Contribuciones

¡Las contribuciones son bienvenidas! No dude en enviar un Pull Request.

## Licencia

Este proyecto está licenciado bajo la Licencia MIT - consulte el archivo [LICENSE](LICENSE) para más detalles.


[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fafernandes%2FGlobalKeyboardCapture.Maui?ref=badge_large)

## Autor

Anderson Fernandes do Nascimento

## Soporte

Si encuentra algún problema o necesita ayuda, por favor [abra un issue](https://github.com/afernandes/GlobalKeyboardCapture.Maui/issues).
