# Métricas e performance

Métricas ficam desativadas por padrão. Habilite somente quando houver um `MeterListener` ou pipeline OpenTelemetry preparado.

[!code-csharp[Metrics](snippets/ScenarioExamples.cs?name=metrics)]

Meter: `GlobalKeyboardCapture.Maui`.

Instrumentos estáveis:

- `keyboard.events.received`;
- `keyboard.events.dispatched`;
- `keyboard.events.ignored`;
- `keyboard.events.repeated`;
- `keyboard.handlers.invocations`;
- `keyboard.handlers.duration` (nanosegundos);
- `keyboard.handlers.errors`.

Quando `EnableMetrics` é `false`, o dispatch não inicializa instrumentos nem lê o relógio. O projeto `GlobalKeyboardCapture.Maui.Benchmarks` mede normalização, lookup de hotkey, buffer do scanner e dispatch com métricas ligadas/desligadas. A execução semanal publica resultados como artifact e a política versionada define 20% como limite de investigação.
