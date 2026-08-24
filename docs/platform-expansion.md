# Decisão de arquitetura: Tizen, Linux e HID nativo

**Estado:** decisão aceita em 24/08/2026.
**Resultado:** não adicionar esses adapters ao pacote principal na linha 2.x; reabrir cada opção somente quando seu gate de entrada for cumprido.

## Matriz de decisão

| Opção | Situação atual | Decisão |
|---|---|---|
| Tizen | O ecossistema MAUI inclui Tizen, mas exige toolchain, hardware e validação mantidos pelo ecossistema Samsung | Futuro pacote separado, condicionado a consumidor, aparelho e mantenedor |
| Linux GTK4 | O backend `Microsoft.Maui.Platforms.Linux.Gtk4` está no `maui-labs` e é explicitamente experimental/comunitário | Não anunciar suporte de produção enquanto o backend for experimental |
| HID/Raw Input Windows | `WM_INPUT` distingue dispositivos, mas o Win32 permite apenas um target por classe de raw input no processo e alerta contra registro feito por bibliotecas genéricas | Somente módulo opt-in com ownership explícito do HWND; nunca ativação automática no core |
| USB Host Android | Exige `android.hardware.usb.host`, descoberta, permissão do usuário e comunicação por endpoint | Pacote opt-in próprio; não confundir com scanner keyboard-wedge |
| HID Apple | iOS/Mac Catalyst têm sandbox, entitlement e APIs diferentes; não há contrato portátil equivalente ao Raw Input | Sem API cross-platform fictícia; avaliar por plataforma e distribuição |

Fontes primárias: [backends experimentais do .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/developer-tools/platform-backends/?view=net-maui-10.0), [Raw Input do Win32](https://learn.microsoft.com/en-us/windows/win32/inputdev/about-raw-input), [restrição de `RegisterRawInputDevices`](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerrawinputdevices) e [USB Host no Android](https://developer.android.com/develop/connectivity/usb/host).

## Forma dos protótipos futuros

Cada adapter deve nascer fora de `GlobalKeyboardCapture.Maui` e depender apenas dos modelos neutros necessários. O protótipo precisa separar:

- descoberta e capability do dispositivo;
- solicitação/negação de permissão;
- conexão e desconexão;
- relatório bruto imutável, incluindo vendor/product/usage page/usage;
- framing e interpretação do scanner;
- lifecycle e ownership do recurso nativo;
- diagnostics sem payload sensível por padrão.

Nomes candidatos são `GlobalKeyboardCapture.Maui.Tizen`, `GlobalKeyboardCapture.Maui.Linux.Gtk4`, `GlobalKeyboardCapture.Maui.Hid.Windows` e `GlobalKeyboardCapture.Maui.UsbHost.Android`. Eles não estão reservados nem anunciados até existir protótipo validado.

## Gate para reabrir uma plataforma

Uma issue de expansão deve apresentar, antes de código no pacote principal:

1. pelo menos dois consumidores ou um caso de produção financiado;
2. modelos de hardware e versões de sistema disponíveis;
3. mantenedor nominal para adapter e dependências nativas;
4. runner/device farm contínuo, com teardown e reconnect exercitados;
5. contrato de permissões, privacidade e falha explícita;
6. benchmark do hot path e plano de trimming/AOT;
7. decisão de pacote separado aprovada.

Sem esses dados, o resultado desta avaliação permanece “não implementar”. Isso evita ampliar a matriz pública com suporte que só compila e não pode ser comprovado.
