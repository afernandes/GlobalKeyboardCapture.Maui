# Sequências

`KeySequenceHandler` reconhece gestos ordenados e aplica timeout por registro. O comportamento histórico executa uma sequência curta imediatamente; escolha outra política quando uma sequência puder ser prefixo de outra.

[!code-csharp[Sequences](snippets/ScenarioExamples.cs?name=sequences)]

Políticas:

- `ExecuteImmediately`: executa a curta e ainda permite concluir a longa;
- `PreferLongest`: adia a curta; executa a longa se ela completar, ou a curta após divergência/timeout;
- `RejectAmbiguous`: rejeita o segundo registro com prefixo estrito.

`CancelPendingSequences()` limpa progresso parcial e ações adiadas. `GetProgressSnapshot()` retorna dados imutáveis; `ProgressChanged` é emitido fora do lock e somente agenda trabalho quando possui assinantes.
