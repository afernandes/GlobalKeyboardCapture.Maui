# Kiosk e checkout

Organize cada tela ou feature em um scope. A prioridade é descendente e empates preservam a ordem de registro.

[!code-csharp[Kiosk](snippets/ScenarioExamples.cs?name=kiosk)]

Com `StopOnHandled = true`, um handler prioritário pode impedir os handlers seguintes. Para permitir digitação normal em `Entry`/`Editor`, mantenha um token de `SuspendCapture()` durante o foco. Suspensões são aninháveis e cada token é idempotente.
