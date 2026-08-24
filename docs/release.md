# Processo de release

O workflow `release.yml` é a única rota suportada para publicar a linha 2.x. Ele é acionado por uma tag `vX.Y.Z`, aceita somente um commit contido em `main` e exige que a tag corresponda a `CurrentVersion` no projeto da biblioteca.

## Configuração única no NuGet.org

Cadastre uma policy de **Trusted Publishing** na conta `AndersonN` com estes valores:

- proprietário: `afernandes`;
- repositório: `GlobalKeyboardCapture.Maui`;
- workflow: `release.yml`;
- environment: `release`.

O GitHub environment `release` já está criado e exige aprovação do mantenedor `afernandes` antes da publicação. Não é necessário armazenar uma API key de longa duração.

## Ordem segura

1. concluir as validações físicas indicadas no `ROADMAP.md`;
2. aprovar e mergear o PR em `main`;
3. atualizar `CurrentVersion`, changelog e `PublicApiBaselineVersion` quando aplicável;
4. aguardar todos os checks do commit de `main`;
5. criar e enviar a tag correspondente;
6. aprovar o environment `release`;
7. acompanhar a criação do draft, publicação no NuGet, smoke test e publicação final do GitHub Release.

O workflow gera `.nupkg`, `.snupkg`, SBOM SPDX 2.2, `SHA256SUMS`, provenance SLSA e attestation do SBOM. O GitHub Release permanece em draft até o pacote aparecer no flat container do NuGet e ser restaurado em um app MAUI novo.

## Decisão sobre assinatura

Não será armazenado um certificado PFX no GitHub neste momento. Author signing exige certificado público de code signing, timestamp RFC 3161 e registro prévio do certificado no NuGet.org. Até que esse ativo exista e possua rotação e custódia definidas, a cadeia adotada é:

- autenticação OIDC de curta duração pelo Trusted Publishing;
- provenance e SBOM assinados pelo serviço de attestations do GitHub;
- hashes SHA-256 publicados junto do release;
- assinatura de repositório aplicada pelo feed NuGet.org após a publicação.

Se um certificado adequado for adquirido, a inclusão de `dotnet nuget sign` deve ocorrer antes da geração dos hashes e das attestations, com a chave privada em cofre/HSM e nunca em arquivo versionado.

## Verificação pelo consumidor

Baixe os artefatos do release e confira `SHA256SUMS`. Para provenance, use `gh attestation verify <pacote> --repo afernandes/GlobalKeyboardCapture.Maui`. Depois que o NuGet.org indexar o pacote, `dotnet nuget verify` permite inspecionar sua assinatura de repositório.
