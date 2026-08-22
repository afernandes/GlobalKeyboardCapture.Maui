# Roadmap — GlobalKeyboardCapture.Maui

Este documento consolida o fechamento da linha 2.0 e o backlog recomendado para as próximas versões. A prioridade considera, nesta ordem: correção de captura, segurança e ciclo de vida, compatibilidade pública, capacidade de diagnóstico, cobertura real de plataforma e novas funcionalidades.

## Legenda

- ✅ concluído e validado localmente;
- 🟡 implementado, aguardando validação externa ou CI do PR;
- ⬜ backlog futuro;
- **P0** bloqueia a versão; **P1** deve entrar nas próximas versões menores; **P2** agrega robustez/DX; **P3** depende de demanda.

## Lista priorizada

| # | Prioridade | Estado | Item | Entrega sugerida |
|---:|:---:|:---:|---|---|
| 1 | P0 | ✅ | Matriz local completa de testes, builds, trimming e pacote 2.0 | 2.0.0 |
| 2 | P0 | 🟡 | Executar e estabilizar todos os checks no PR | 2.0.0 |
| 3 | P0 | 🟡 | Validar Android com teclado físico/scanner real | 2.0.0 |
| 4 | P0 | 🟡 | Validar iPad e Mac físicos com teclado externo | 2.0.0 |
| 5 | P0 | ⬜ | Publicar a versão 2.0.0 após merge e checks verdes | 2.0.0 |
| 6 | P1 | ⬜ | Tornar o mapeamento Apple sensível ao layout do teclado | 2.1.0 |
| 7 | P1 | ⬜ | Adicionar teste de integração runtime para Windows | 2.1.0 |
| 8 | P1 | ⬜ | Criar matriz recorrente de dispositivos Android reais | 2.1.0 |
| 9 | P1 | ⬜ | Congelar baseline de compatibilidade da API pública | 2.1.0 |
| 10 | P1 | ⬜ | Assinar/atestar artefatos e gerar SBOM do release | 2.1.0 |
| 11 | P2 | ⬜ | Adicionar políticas de roteamento por dispositivo | 2.2.0 |
| 12 | P2 | ⬜ | Evoluir sequências para múltiplos prefixos e estado observável | 2.2.0 |
| 13 | P2 | ⬜ | Publicar métricas leves e benchmarks do hot path | 2.2.0 |
| 14 | P2 | ⬜ | Criar documentação navegável e exemplos por cenário | 2.2.0 |
| 15 | P3 | ⬜ | Avaliar Tizen, Linux e captura HID nativa separada | Futuro |

## Fechamento das recomendações originais

A auditoria anterior levantou 19 itens. O estado consolidado é:

| Item original | Estado | Resultado na linha 2.0 |
|---|:---:|---|
| Android F1–F12 e Enter do numpad | 🟡 | Correções implementadas, testes de mapeamento e injeção em emulador no CI; falta confirmação com hardware físico. |
| Versionar trabalho e `global.json` | ✅ | SDK 10.0.400 fixado, commits atômicos e branch 1.x preservada para MAUI 8. |
| Validar builds MAUI | ✅ | Biblioteca e sample compilam para Android, Windows, iOS e Mac Catalyst. |
| DI em plataformas não suportadas | ✅ | iOS/Mac Catalyst têm implementação real; demais plataformas recebem implementação sem suporte explícito para hotkeys globais. |
| Corrigir trimming | ✅ | Removido o root integral do assembly; analisadores ativos e publish trimado incluído no gate final. |
| Corrigir teardown/lifecycle | ✅ | Attach/detach por view, leases idempotentes, Activity recreation, callback chaining e disposal simétrico. |
| Suspender/retomar captura | ✅ | Tokens aninháveis, `IsCapturing`, `ResumeCapture()` e demonstração com `Entry`. |
| Scanner configurável | ✅ | Perfis, terminadores, prefixo/sufixo, velocidade, limite, dispositivo e resultado detalhado. |
| Testar `KeyboardHelper` | ✅ | Mapeamento puro extraído e coberto para Android, Windows e Apple. |
| Diagnóstico estruturado | ✅ | Eventos nativos/normalizados, motivo de descarte, flags, scan code, repeat e dispositivo. |
| Multi-janela | ✅ | Windows suporta múltiplas janelas; Android faz rebind seguro da Activity; Apple usa teclado coalescido no app. |
| iOS/Mac Catalyst | 🟡 | Implementado e compilado; falta validação em hardware Apple real. |
| Eventos de `KeyUp` | ✅ | Opt-in em todas as plataformas, sem alterar o default histórico. |
| Integração de plataforma no CI | 🟡 | Builds das quatro plataformas e injeção Android implementados; runtime Windows permanece como melhoria P1. |
| Introspecção do serviço | ✅ | Estado, views, handlers, captura, diagnostics e scopes expostos. |
| Handlers assíncronos | ✅ | Snapshot sem objeto nativo, cancelamento no disposal e isolamento de exceções. |
| Changelog/versionamento | ✅ | `CHANGELOG.md`, política 1.x/2.x, notas de pacote e breaking changes documentados. |
| Matriz de teclas | ✅ | Matriz por plataforma, limitações de layout e distinção app-wide/OS-global no README. |
| Página de diagnóstico | ✅ | Sample operacional com stream, metadados, scanner, scope, suspension, sequences e hotkey global. |

Além da auditoria original, a linha 2.0 entrega modelo tipado de teclas, prioridades determinísticas, tokens de registro, metadados de dispositivo, controle de repeat, sequências e hotkeys realmente globais no Windows.

## Detalhamento dos próximos itens

### 1. Matriz local completa da versão 2.0

**Objetivo.** Impedir que uma correção em um TFM quebre outro e garantir que o pacote publicado represente exatamente os quatro alvos anunciados.

**Escopo.** Executar testes Release, build de biblioteca e sample nos quatro TFMs, `dotnet format`, auditoria de dependências, `dotnet pack`, inspeção do `.nupkg`/`.snupkg` e publish Android trimado/AOT.

**Critério de aceite.** Zero erro e zero warning próprio; pacote contém os quatro grupos compatíveis (`net10.0-android`, `net10.0-windows`, `net10.0-ios` e `net10.0-maccatalyst`, com as versões mínimas de plataforma resolvidas pelo SDK), XML docs e símbolos.

### 2. Checks do pull request

**Objetivo.** Validar o que não é reproduzível de forma idêntica em um único host Windows.

**Escopo.** Testes em Windows/Linux, builds em runners Windows/Linux/macOS, Android emulator integration e geração do pacote.

**Critério de aceite.** Todos os checks obrigatórios verdes, inclusive os `grep` de `F1`, `F12` e `KEYCODE_NUMPAD_ENTER` no log do Android.

### 3. Validação Android em hardware real

**Objetivo.** Confirmar o comportamento com firmware, layout e flags emitidos por teclados/scanners reais.

**Roteiro mínimo.** Testar F1–F12, Enter principal/numpad, Tab, Space, setas, modificadores, repeat, key-up, scanner finalizando por Enter e scanner com profile customizado. Registrar o stream diagnóstico e o modelo do dispositivo.

**Critério de aceite.** Eventos normalizados e hotkeys corretos em pelo menos um teclado USB e um Bluetooth; scanner não perde nem duplica caracteres.

### 4. Validação Apple em hardware real

**Objetivo.** Confirmar que `GCKeyboard.CoalescedKeyboard` entrega os mesmos usages no iPad e no Mac Catalyst.

**Roteiro mínimo.** Teclado Bluetooth/USB, F1–F12, Command/Option/Control/Shift, Enter do numpad, reconnect do teclado, background/foreground e fechamento de janela.

**Critério de aceite.** Sem duplicidade após reconnect, teardown sem callback residual e diagnóstico coerente. A limitação de `Handled` observacional deve permanecer documentada.

### 5. Publicação 2.0.0

**Objetivo.** Publicar somente o commit aprovado e reproduzir o pacote validado pelo CI.

**Escopo.** Merge do PR, tag `v2.0.0`, GitHub Release, push da NuGet e verificação no flat container após o período normal de indexação.

**Critério de aceite.** Tag aponta para `main`, hashes dos artefatos conferem e o pacote pode ser restaurado em um app consumidor novo.

### 6. Mapeamento Apple sensível ao layout

**Problema.** O `GCKeyCode` representa posição física USB HID; o mapa atual de símbolos usa layout US. Letras e teclas nomeadas são estáveis, mas pontuação pode divergir em ABNT2, ISO e outros layouts.

**Proposta.** Obter texto traduzido pelo sistema quando possível e manter HID como fallback. A API deve continuar expondo `NativeKeyCode` para consumidores que dependem da posição física.

**Critério de aceite.** Testes/roteiro para US, ABNT2 e pelo menos um layout ISO sem alterar o contrato canônico das teclas nomeadas.

### 7. Integração runtime no Windows

**Problema.** O CI compila WinUI, mas ainda não injeta mensagens reais em uma janela. Regressões em `Activated`, troca de `Content`, subclass de `WM_HOTKEY` e duas janelas podem escapar.

**Proposta.** App de integração empacotado ou unpackaged que abra duas janelas, injete key down/up, troque o conteúdo e valide `RegisterHotKey`/`UnregisterHotKey`.

**Critério de aceite.** Teste automatizado comprova uma única entrega por janela e ausência de callback após detach.

### 8. Matriz Android em dispositivos reais

**Problema.** Emulador comprova o caminho Android, mas não representa scanners industriais, OEMs e combinações de flags de firmware.

**Proposta.** Rodar nightly em device farm e manter uma pequena matriz física de teclado USB, Bluetooth e scanner wedge, salvando diagnostics como artifact.

**Critério de aceite.** Cenários essenciais executados em pelo menos duas versões de Android e dois tipos de dispositivo de entrada.

### 9. Baseline de compatibilidade pública

**Objetivo.** Evitar breaking changes acidentais após a consolidação da API 2.0.

**Proposta.** Habilitar package validation/API compatibility contra a última versão 2.x publicada e manter um arquivo de baseline revisável. Alterações incompatíveis exigem versão major e nota de migração.

**Critério de aceite.** O CI falha quando um membro público é removido ou muda de assinatura sem baseline explícito.

### 10. Supply chain do release

**Objetivo.** Permitir que consumidores verifiquem origem e conteúdo do pacote.

**Proposta.** As actions do CI já estão fixadas por SHA e o workflow opera com permissão mínima de leitura. Os próximos passos são gerar SBOM, publicar hashes SHA-256, usar GitHub artifact attestation e avaliar assinatura NuGet com certificado protegido.

**Critério de aceite.** Release contém pacote, símbolos, SBOM, checksums e provenance verificável.

### 11. Roteamento por dispositivo

**Problema.** Profiles de scanner já filtram `DeviceId`, mas handlers gerais ainda recebem todos os dispositivos.

**Proposta.** Adicionar filtros reutilizáveis por ID, virtual/externo, descriptor e plataforma na criação do scope ou registro do handler.

**Critério de aceite.** Um app pode reservar um scanner para barcode e deixar outro teclado apenas para navegação, sem lógica duplicada nos handlers.

### 12. Sequências avançadas

**Proposta.** Suportar prefixos compartilhados, cancelamento explícito, evento de progresso e política para sobreposição (`Ctrl+K`, `Ctrl+C` versus `Ctrl+K`, `Ctrl+U`).

**Critério de aceite.** Sem ambiguidade silenciosa; estado reinicia deterministicamente por timeout, mismatch ou cancelamento.

### 13. Métricas e benchmarks

**Proposta.** Projeto BenchmarkDotNet para normalização, dispatch, hotkey lookup e scanner buffering; contadores opcionais de eventos dispatched/ignored/repeated e latência de handler.

**Critério de aceite.** Baseline versionado, limite de regressão definido e métricas desativadas sem alocação adicional no hot path.

### 14. Documentação navegável

**Proposta.** Gerar referência a partir do XML, guias de checkout/scanner/kiosk, troubleshooting por plataforma e captura atualizada do sample.

**Critério de aceite.** Cada feature pública tem exemplo compilável e cada limitação da matriz possui orientação de diagnóstico.

### 15. Novas plataformas e HID nativo

**Escopo condicionado à demanda.** Avaliar Tizen e desktop Linux como adapters separados. Captura HID/raw scanner deve ser um módulo opcional, pois exige permissões e um contrato diferente de keyboard wedge.

**Critério de entrada.** Caso de uso confirmado, hardware disponível e mantenedor/CI capaz de validar a plataforma continuamente.

## Princípios para evolução

- `KeyEventArgs.ToString()` e `KeyGesture.ToString()` são contratos de compatibilidade; qualquer mudança exige testes golden e análise de breaking change.
- O caminho síncrono de input não deve fazer I/O, reflexão, LINQ ou trabalho de UI.
- Toda inscrição nativa precisa de detach/disposal simétrico e seguro diante de wrappers de terceiros.
- Uma plataforma anunciada precisa de build em CI e roteiro runtime; compilação isolada não equivale a validação em hardware.
- Recursos não suportados devem ser detectáveis por capability (`IsSupported`) ou falhar com mensagem explícita, nunca silenciosamente.
- Nenhuma feature deve reintroduzir root integral de trimming ou suprimir globalmente warnings de AOT/trim.
