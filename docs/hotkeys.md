# Hotkeys

Prefira `KeyGesture` e `KeyboardKey`; a API textual continua disponível e normaliza aliases históricos.

[!code-csharp[Hotkeys](snippets/ScenarioExamples.cs?name=hotkeys)]

`HotkeyHandler` é app-wide, não OS-global. Para uma combinação disponível mesmo com outra aplicação em foco, use `IGlobalHotkeyService`; atualmente somente Windows informa `IsSupported == true`.

O token retornado remove exatamente o registro que o criou. Um token antigo não remove uma substituição mais recente da mesma combinação.
