# Roteamento por dispositivo

Use `KeyboardDeviceFilter` em um registro ou scope. Quando ambos existem, todos os critérios precisam corresponder.

[!code-csharp[Device routing](snippets/ScenarioExamples.cs?name=device-routing)]

Critérios disponíveis: `DeviceId`, `IsVirtual`, `IsExternal`, `Descriptor` e `Platform`. Comparação de descriptor ignora caixa. Um filtro que exige metadados de dispositivo rejeita eventos sem `KeyboardDeviceInfo`; um filtro somente por plataforma não exige esses metadados.

Android normalmente fornece a identificação mais rica. Windows e Apple podem não expor um identificador estável para todo teclado; trate `Device == null` como capability ausente e confirme o comportamento com diagnostics no hardware alvo.
