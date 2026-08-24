# Roadmap — GlobalKeyboardCapture.Maui

> Última atualização: **24 de agosto de 2026**. O [PR #7](https://github.com/afernandes/GlobalKeyboardCapture.Maui/pull/7) foi mergeado em `main`. O lote atual está na branch `codex/complete-roadmap` e no [PR #8](https://github.com/afernandes/GlobalKeyboardCapture.Maui/pull/8).

Este arquivo é o painel de execução da linha 2.x. Os números são estáveis e devem ser citados em issues, commits e pull requests. Um item só muda para ✅ quando o critério de aceite possui evidência; implementação pronta que depende de hardware, credenciais ou publicação permanece explícita.

## Resposta direta sobre o item 12

Sim. O item 12, **Sequências avançadas**, está implementado e coberto por testes.

Foram entregues prefixos compartilhados, `SequenceOverlapPolicy` (`ExecuteImmediately`, `PreferLongest` e `RejectAmbiguous`), cancelamento explícito, snapshot imutável, evento de progresso, timers testáveis, sample e documentação. O checklist completo está na seção 12.

## Legenda

- ✅ **Concluído** — implementação e evidência do critério de aceite concluídas;
- 🚧 **Em validação** — código pronto nesta branch, mas o gate local/PR/deploy ainda está rodando;
- 🧪 **Aguardando ambiente externo** — engenharia pronta, porém a aceitação exige hardware, device farm ou credencial que não existe neste ambiente;
- ⏳ **Sequenciado** — não há código pendente agora, mas a ativação depende de outro item;
- **P0** bloqueia o release; **P1** protege a linha 2.x; **P2** evolui produto/DX; **P3** depende de demanda comprovada.

## Resumo atual

| Estado | Quantidade | Itens |
|---|---:|---|
| ✅ Concluído | 5 | 1, 11, 12, 13, 15 |
| 🚧 Em validação | 3 | 2, 7, 14 |
| 🧪 Ambiente externo | 5 | 3, 4, 6, 8, 10 |
| ⏳ Sequenciado | 2 | 5, 9 |

## Próximas ações, na ordem

1. Concluir a matriz final local, abrir o PR e deixar verdes os checks, incluindo runtime Windows (itens 1, 2 e 7).
2. Após o merge, confirmar o primeiro deploy do Pages, que já está habilitado no repositório (item 14).
3. Configurar Workload Identity/Firebase e NuGet Trusted Publishing nas contas externas (itens 8 e 10).
4. Executar a bancada Android e os testes Apple com hardware físico, incluindo layouts não US (itens 3, 4 e 6).
5. Somente com os gates P0 aprovados, publicar 2.0.0 (item 5); a publicação passa a ser o baseline automático da compatibilidade de API (item 9).

## Painel priorizado

| # | Pri. | Estado | Item | Engenharia entregue | Pendência objetiva |
|---:|:---:|:---:|---|---|---|
| 1 | P0 | ✅ | Matriz local completa | 248 testes; quatro builds; trim/AOT; format; auditoria; pacote inspecionado | Manter como gate de release |
| 2 | P0 | 🚧 | Checks do PR | PR #8 aberto; CI ampliado para plataforma, runtime e pacote | Confirmar todos os checks e reviews |
| 3 | P0 | 🧪 | Android em hardware real | Emulador e UI Automator automatizados | Teclados USB/Bluetooth e scanner físico |
| 4 | P0 | 🧪 | Apple em hardware real | Adapter, lifecycle e builds Apple prontos | iPad e Mac com teclado externo |
| 5 | P0 | ⏳ | Publicar 2.0.0 | Workflow de release e artefatos definidos | Depende de 2, 3, 4, 8 e 10 |
| 6 | P1 | 🧪 | Layout Apple | Translator/cache, collector UIKit, fallback e testes | Confirmar layouts reais em iPad/Mac |
| 7 | P1 | 🚧 | Runtime Windows no CI | Harness WinUI e correção de troca de conteúdo | Confirmar o job no novo PR |
| 8 | P1 | 🧪 | Matriz Android recorrente | UI Automator e workflow Firebase prontos | Criar WIF/variáveis e executar a primeira matriz |
| 9 | P1 | ⏳ | Baseline de API pública | Package validation e strict API compatibility prontas | Ativação automática após existir 2.0.0 no NuGet |
| 10 | P1 | 🧪 | Supply chain | SBOM, hashes, attestations, OIDC e smoke restore | Configurar Trusted Publisher e executar release real |
| 11 | P2 | ✅ | Roteamento por dispositivo | Filtro em registro/scope e testes multi-device | Manutenção contínua |
| 12 | P2 | ✅ | Sequências avançadas | Políticas, cancelamento, progresso, testes e sample | Manutenção contínua |
| 13 | P2 | ✅ | Métricas e benchmarks | Meter opt-in, 6 benchmarks, política e workflow | Revisar baseline a cada runtime/SDK |
| 14 | P2 | 🚧 | Documentação navegável | Docfx, exemplos compiláveis, CI e Pages habilitado | Primeiro deploy após merge |
| 15 | P3 | ✅ | Tizen, Linux e HID nativo | Avaliação e decisão no-go/condicionada documentadas | Reabrir apenas quando o gate de entrada existir |

## Detalhamento

### 1. Matriz local completa — ✅ Concluído

**Objetivo.** Garantir que a biblioteca, o sample e o pacote representem os quatro alvos anunciados sem regressão de trim/AOT.

**Entregue/evidência atual.** 248 testes Release aprovados; sample compilado sem warnings para Android, Windows, iOS e Mac Catalyst; Android Release executou trimming/AOT; `dotnet format` passou; auditoria NuGet não encontrou vulnerabilidades.

**Critério de aceite.** `.nupkg`/`.snupkg` finais contêm os quatro TFMs, XML docs, símbolos e metadados corretos.

- [x] Testes Release;
- [x] Builds de biblioteca e sample nos quatro TFMs;
- [x] Android trimado/AOT;
- [x] Formatação;
- [x] Auditoria de vulnerabilidades;
- [x] Pack e inspeção final: quatro TFMs, XML docs, README, licença, ícone e símbolos.

### 2. Checks do pull request — 🚧 Em validação

**Objetivo.** Reproduzir a entrega em runners limpos e impedir merge com regressão de plataforma.

**Entregue.** Testes Windows/Linux, builds das quatro plataformas, integração Android, runtime WinUI e pack com API compatibility estão definidos no CI. O PR #7 anterior terminou com oito checks verdes e foi mergeado.

**Critério de aceite.** O [PR #8](https://github.com/afernandes/GlobalKeyboardCapture.Maui/pull/8) deve ficar integralmente verde e sem comentário de revisão pendente.

- [x] Workflow implementado e validado por `actionlint`;
- [x] Abrir o PR #8;
- [x] Diagnosticar a primeira execução remota e corrigir o comando de instrumentação Android, o TFM do Docfx e a inicialização autocontida do harness Windows;
- [ ] Confirmar todos os checks;
- [ ] Tratar comentários/reviews e registrar o link aqui.

### 3. Android em hardware real — 🧪 Aguardando ambiente externo

**Objetivo.** Validar firmware, layout, flags e timing de periféricos que um emulador não reproduz.

**Roteiro.** F1-F12; Enter principal/numpad; Tab, Space, setas e modificadores; key-up/repeat; scanner por Enter; profile customizado; reconnect e background/foreground.

**Critério de aceite.** Um teclado USB, um Bluetooth e um scanner keyboard-wedge sem perda, duplicidade ou identificação incorreta.

- [x] Caminho automatizado no emulador;
- [x] Tela e diagnostics preparados para coleta;
- [ ] Teclado USB físico;
- [ ] Teclado Bluetooth físico;
- [ ] Scanner físico;
- [ ] Anexar modelo, Android, descriptor/ID e diagnostics ao PR/issue.

### 4. Apple em hardware real — 🧪 Aguardando ambiente externo

**Objetivo.** Validar `GCKeyboard`, UIKit layout translation e lifecycle em dispositivos Apple reais.

**Roteiro.** F1-F12/F20 disponíveis, Command/Option/Control/Shift, Enter do numpad, símbolos em dois layouts, reconnect, background/foreground e fechamento de janela.

**Critério de aceite.** iPad e Mac sem duplicidade/callback residual, com `NativeKeyCode` e caractere coerentes.

- [x] Builds iOS e Mac Catalyst sem warning;
- [x] Testes neutros de HID, layout e fallback;
- [ ] iPad com teclado externo;
- [ ] Mac físico;
- [ ] Reconnect/lifecycle;
- [ ] Anexar modelo, sistema, layout e diagnostics.

### 5. Publicar 2.0.0 — ⏳ Sequenciado

**Objetivo.** Publicar a primeira versão MAUI 10 sem promover código não validado fisicamente.

**Critério de aceite.** Tag em commit de `main`, NuGet indexado, restore de consumidor novo aprovado e GitHub Release público com pacote, símbolos, SBOM, hashes e provenance.

- [x] `CurrentVersion` e release notes preparados;
- [x] Workflow fail-closed implementado;
- [ ] Concluir itens 2, 3, 4, 8 e 10;
- [ ] Atualizar changelog com a data real;
- [ ] Criar `v2.0.0` somente em commit contido em `main`;
- [ ] Confirmar indexação e smoke restore antes de publicar o release.

### 6. Layout Apple — 🧪 Aguardando ambiente externo

**Objetivo.** Produzir símbolos conforme o layout ativo sem reflexão ou chamada bloqueante no callback físico.

**Entregue.** `IKeyboardLayoutTranslator`, contexto allocation-free, cache limitado/thread-safe, collector de `UIKey.Characters`, prioridade sobre o fallback HID apenas para teclas imprimíveis e documentação de limitações.

- [x] Contrato e cache;
- [x] Integração iOS/Mac Catalyst;
- [x] Named keys protegidas contra tradução indevida;
- [x] Testes US/ABNT2/ISO, inválidos e limpeza;
- [x] Sample e guia;
- [ ] Confirmar em dois layouts físicos no item 4.

### 7. Runtime Windows no CI — 🚧 Em validação

**Objetivo.** Testar o adapter WinUI real, não apenas seus mapas puros.

**Entregue.** Harness opt-in que cria duas janelas, injeta key-down/up, substitui conteúdo, testa detach e valida `RegisterHotKey`/`WM_HOTKEY`/unregister. O handler acompanha o conteúdo exato, monitora substituição, fecha a corrida Attach/Dispose e interrompe o timer de retry enquanto o conteúdo está estável.

- [x] Harness no sample sem entrar no pacote;
- [x] Execução local interativa com todos os cenários aprovados;
- [x] Job Windows hospedado no CI;
- [ ] Confirmar o job no novo PR e anexar artifact.

### 8. Matriz Android recorrente — 🧪 Aguardando ambiente externo

**Objetivo.** Rodar o mesmo teste UI Automator em aparelhos físicos de OEM/Android diferentes duas vezes por semana.

**Entregue.** APK de instrumentação, job de emulador no CI, workflow Firebase com OIDC, duas dimensões físicas, uma repetição para flake e upload de diagnostics.

- [x] Projeto AndroidX/UI Automator compilável;
- [x] F1, F12, numpad Enter e scanner automatizados;
- [x] Workflow agendado/manual fail-closed;
- [x] Guia de configuração e de bancada;
- [ ] Criar Workload Identity e service account no Google Cloud;
- [ ] Preencher as variáveis `FIREBASE_*`/`GCP_*`;
- [ ] Executar a primeira matriz física e revisar artifacts.

### 9. Baseline de API pública — ⏳ Sequenciado

**Objetivo.** Bloquear breaks acidentais após a primeira versão estável 2.x.

**Entregue.** Package validation, strict compatible-framework/API baseline, arquivo de suppressions revisável e detecção automática no CI/release. Hoje o NuGet possui somente 1.0.0-1.0.2; portanto ainda não existe pacote 2.x válido como baseline.

- [x] Infraestrutura e política de compatibilidade;
- [x] Gate no pack/CI/release;
- [x] Sem suppressions genéricas;
- [ ] Publicar 2.0.0;
- [ ] Confirmar que o próximo pack usa 2.0.0 como baseline.

### 10. Supply chain do release — 🧪 Aguardando ambiente externo

**Objetivo.** Tornar origem e conteúdo do pacote verificáveis sem armazenar certificado/API key no repositório.

**Entregue.** Actions fixadas por SHA, permissões mínimas, ambiente GitHub `release` com aprovação do mantenedor, SBOM SPDX, SHA-256, attestations de provenance/SBOM, draft release, NuGet OIDC e restore pós-indexação. A decisão de assinatura é usar Trusted Publishing + attestations até existir certificado com ciclo de vida/HSM adequado.

- [x] Pipeline e ferramentas versionadas;
- [x] Ambiente `release` criado no GitHub com required reviewer;
- [x] SBOM, hashes e attestations;
- [x] Decisão de assinatura documentada;
- [ ] Cadastrar o Trusted Publisher na conta NuGet `AndersonN`;
- [ ] Executar e verificar o primeiro release real.

### 11. Roteamento por dispositivo — ✅ Concluído

**Objetivo.** Reservar scanners/teclados específicos sem duplicar filtro em cada handler.

- [x] `KeyboardDeviceFilter` imutável por plataforma, ID, virtual, externo e descriptor;
- [x] Filtro em `RegisterHandler`;
- [x] Filtro compartilhado em `CreateScope`;
- [x] Combinação AND entre scope e registro;
- [x] Identidade de registro considera o filtro;
- [x] Testes multi-device e guia de capabilities/limitações.

### 12. Sequências avançadas — ✅ Concluído

**Objetivo.** Resolver sequências compartilhadas/ambíguas deterministicamente e tornar o progresso observável.

- [x] APIs tipadas e textuais;
- [x] Timeout individual e clock/timer testável;
- [x] Prefixos iniciais compartilhados;
- [x] `ExecuteImmediately`, `PreferLongest` e `RejectAmbiguous`;
- [x] Divergência e timeout da sequência longa;
- [x] Timer one-shot descartado imediatamente após timeout/callback;
- [x] `CancelPendingSequences()`;
- [x] `GetProgressSnapshot()` imutável;
- [x] `ProgressChanged` fora do lock;
- [x] Tokens idempotentes e substituição segura;
- [x] Testes, sample e documentação.

### 13. Métricas e benchmarks — ✅ Concluído

**Objetivo.** Medir o hot path sem adicionar custo/alocação quando desabilitado.

- [x] `EnableMetrics` opt-in e desligado por padrão;
- [x] Meter/nomes públicos estáveis;
- [x] Eventos received/dispatched/ignored/repeated, invocações, erros e duração;
- [x] Testes com `MeterListener`;
- [x] Seis benchmarks para normalização, parse, dispatch, hotkey, scanner e métricas;
- [x] Política versionada: 20% para investigação e 16 B/op de tolerância adicional;
- [x] Evidência local de 0 B/op adicional entre métricas desligadas/ligadas sem listener;
- [x] Workflow semanal/manual e artifacts.

### 14. Documentação navegável — 🚧 Em validação

**Objetivo.** Ter referência de API e guias compiláveis, pesquisáveis e publicados.

**Entregue.** Docfx com warnings-as-errors, XML API, snippets ligados à suíte, guias de início/hotkeys/scanner/kiosk/sequências/device routing/multi-window/métricas/Apple/Android/release/troubleshooting e workflow Pages. O Pages foi habilitado em modo GitHub Actions em 24/08/2026.

- [x] Site e navegação local;
- [x] Referência XML;
- [x] Exemplos compiláveis;
- [x] Build CI e artifact;
- [x] GitHub Pages habilitado com HTTPS;
- [ ] Mergear o workflow e confirmar o primeiro deploy/link público.

### 15. Novas plataformas e HID nativo — ✅ Avaliação concluída

**Decisão.** Não ampliar o pacote principal agora. Tizen requer consumidor, hardware, mantenedor e CI; Linux GTK4 permanece backend experimental/comunitário; Raw Input Windows pode conflitar com outros registradores do processo; Android USB Host exige feature/permissão; Apple possui contrato diferente. Cada opção deve nascer em pacote opt-in.

- [x] Capacidades e riscos avaliados;
- [x] No-go atual documentado;
- [x] Pacotes/contratos candidatos definidos;
- [x] Gate de entrada definido: caso real + hardware + mantenedor + CI;
- [x] Nenhuma plataforma anunciada sem suporte verificável.

## Bloqueios externos conhecidos

| Bloqueio | Afeta | Como liberar |
|---|---|---|
| Nenhum aparelho/periférico conectado a este ambiente | 3, 4 e 6 | Executar os roteiros em teclado USB/Bluetooth, scanner, iPad e Mac |
| Variáveis/WIF do Firebase ausentes | 8 | Configurar Google Cloud e repository variables documentadas |
| Trusted Publisher NuGet ainda não cadastrado | 5 e 10 | Associar pacote/owner/repositório/workflow/ambiente `release` no NuGet.org |
| 2.0.0 ainda não publicado | 9 | Publicar após os gates P0; o CI detectará o baseline automaticamente |
| Workflow Docfx ainda não está em `main` | 14 | Mergear o PR atual e confirmar o deploy Pages |

## Regra de manutenção

Toda alteração ligada a um item deve atualizar este arquivo no mesmo PR:

1. marcar somente subtarefas comprovadas;
2. manter hardware/credencial/deploy como 🧪 ou ⏳, sem chamar build de validação física;
3. registrar branch/PR, execução CI e evidência de hardware quando existirem;
4. atualizar painel, quantidades, próximas ações e histórico;
5. para item concluído, manter também testes, sample, documentação e changelog coerentes.

## Histórico

| Data | Itens | Alteração | Evidência |
|---|---|---|---|
| 2026-08-24 | 2, 7, 12, 14 | Primeira validação remota diagnosticada; corrigidos instrumentação Android multiline, restore Docfx multi-TFM, bootstrap autocontido/associação tardia da janela WinUI e descarte/parada de timers apontados na revisão | Regressão do timer reproduzida antes da correção; 248 testes; build Windows autocontido; Docfx 0 warnings; actionlint 1.7.12 |
| 2026-08-24 | 2, 7, 14 | PR de conclusão aberto; checks de runtime/plataforma/documentação iniciados | [PR #8](https://github.com/afernandes/GlobalKeyboardCapture.Maui/pull/8) |
| 2026-08-24 | 1, 6-15 | Matriz final e implementação de layout Apple, runtime Windows, device farm, API compatibility, supply chain, routing, sequências, métricas, Docfx e decisão de expansão | 248 testes; quatro builds; pacote 2.0.0 inspecionado; Docfx/actionlint; APK UI Automator; benchmark policy |
| 2026-08-24 | 14 | GitHub Pages habilitado em modo Actions e ambiente `release` criado | API do repositório |
| 2026-08-24 | 1-15 | Painel reescrito para separar engenharia concluída de dependências externas; item 12 marcado como concluído | Auditoria desta branch |
| 2026-08-24 | 1-2 | PR #7 mergeado após oito checks verdes | [PR #7](https://github.com/afernandes/GlobalKeyboardCapture.Maui/pull/7) e [CI 32557589954](https://github.com/afernandes/GlobalKeyboardCapture.Maui/actions/runs/32557589954) |

## Princípios permanentes

- `KeyEventArgs.ToString()` e `KeyGesture.ToString()` são contratos de compatibilidade e exigem testes golden/breaking-change review.
- O callback síncrono não faz I/O, reflexão, LINQ, trabalho de UI ou formatação diagnóstica quando o recurso está desligado.
- Toda inscrição nativa possui detach/disposal simétrico e tolera eventos em voo.
- Plataforma anunciada exige build em CI e roteiro runtime; build não equivale a hardware.
- Recurso não suportado expõe capability ou falha explicitamente.
- Trimming/AOT não usa root integral nem supressão genérica de warning.
