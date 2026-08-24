# Scanner keyboard-wedge

Um scanner keyboard-wedge se apresenta como teclado. Configure framing, velocidade, tamanho e dispositivo em `BarcodeScannerProfile`.

[!code-csharp[Scanner](snippets/ScenarioExamples.cs?name=scanner)]

Use `ScanCompleted` quando precisar de profile, valor bruto, duração e `KeyboardDeviceInfo`. `BarcodeScanned` mantém o evento textual compatível com a linha 1.x.

O buffer possui limite máximo e é reiniciado em fluxo inválido. Valide em hardware real porque firmware, terminadores e `DeviceId` variam por fabricante e conexão USB/Bluetooth.
