# Validação Android em aparelhos reais

O projeto usa duas camadas complementares. O CI comum executa o teste UI Automator no emulador. O workflow `Android device farm` envia os mesmos APKs a uma matriz de aparelhos físicos no Firebase Test Lab duas vezes por semana. Teclados USB, Bluetooth e scanners conectados fisicamente continuam sendo validados numa bancada própria, pois o device farm não conecta periféricos externos do projeto aos aparelhos remotos.

## Configuração do Firebase Test Lab

Configure Workload Identity Federation entre o GitHub e o Google Cloud; não armazene chave JSON no repositório. O service account precisa executar testes no Firebase Test Lab e ler/gravar no bucket de resultados. Depois, crie estas repository variables:

| Variável | Conteúdo |
|---|---|
| `FIREBASE_TEST_LAB_ENABLED` | `true` somente depois de validar toda a configuração |
| `GCP_WORKLOAD_IDENTITY_PROVIDER` | resource name completo do provider OIDC |
| `GCP_SERVICE_ACCOUNT` | e-mail do service account |
| `FIREBASE_PROJECT_ID` | projeto do Firebase/Google Cloud |
| `FIREBASE_RESULTS_BUCKET` | URI `gs://...` do bucket de resultados |
| `FIREBASE_ANDROID_DEVICE_1` | dimensões do primeiro aparelho físico, por exemplo `model=...,version=...,locale=pt_BR,orientation=portrait` |
| `FIREBASE_ANDROID_DEVICE_2` | segundo modelo e outra versão do Android |

Consulte o catálogo atual com `gcloud firebase test android models list` e selecione entradas com `form=PHYSICAL`. A matriz deve manter fabricantes diferentes e pelo menos duas versões do Android.

## Cenários automatizados

O APK de teste em `eng/android-device-tests`:

- abre o sample e confirma que ele chegou ao foreground;
- injeta F1 e F12;
- injeta `KEYCODE_NUMPAD_ENTER` e confirma `native=160`/`location=Numpad`;
- injeta `12345` seguido de Enter e confirma o scanner keyboard-wedge;
- publica resultado, logcat, vídeo e demais diagnostics do Firebase como artifact por 30 dias.

Uma nova tentativa é permitida para classificar flake de infraestrutura. Falha repetida mantém o workflow vermelho e exige inspeção dos artifacts; não deve ser silenciada por baseline.

## Bancada de periféricos

Antes de um release major/minor, execute também o roteiro do item 3 do roadmap em um aparelho real com:

1. teclado USB via OTG;
2. teclado Bluetooth;
3. scanner keyboard-wedge finalizando por Enter;
4. coleta do modelo, versão Android, descriptor/ID do dispositivo e stream diagnóstico.

Device farm comprova compatibilidade de SO/OEM e o caminho de instrumentação. Somente a bancada comprova firmware, conexão e timing dos periféricos reais.
