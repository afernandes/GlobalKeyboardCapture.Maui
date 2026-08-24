# Multi-janela e lifecycle

O lifecycle MAUI cria leases de platform view. Cada lease é idempotente e a fonte nativa é removida depois do último release.

[!code-csharp[Multi-window](snippets/ScenarioExamples.cs?name=multi-window)]

No Windows, cada janela mantém o elemento exato inscrito em `PreviewKeyDown`/`PreviewKeyUp`, inclusive após troca de `Content`. Android aceita uma Activity por vez e faz rebind após recriação sem quebrar a cadeia de `Window.Callback`. Apple observa o teclado coalescido para o aplicativo.

Use `Initialize` apenas para compatibilidade com consumidores 1.x; código novo deve preferir `AttachPlatformView` ou deixar `.UseKeyboardHandling()` gerenciar o lifecycle.
