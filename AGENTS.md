# AGENTS.md

Guidance for Codex when changing this repository.

## Repository layout and support policy

The solution contains three projects:

- `GlobalKeyboardCapture.Maui/` — NuGet library targeting `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, and, on Windows hosts, `net10.0-windows10.0.19041.0`.
- `GlobalKeyboardCapture.Maui.Sample/` — diagnostic consumer app for all four targets.
- `GlobalKeyboardCapture.Maui.Tests/` — platform-neutral xUnit v3 tests using Microsoft.Testing.Platform.

The active 2.x line uses .NET MAUI 10. The .NET MAUI 8 compatibility-maintenance line is preserved on `codex/net8-maintenance-1.x`; do not add 2.x features to that branch. `global.json` pins SDK 10.0.400. The package version is `CurrentVersion` in the library project.

## Build and validation commands

```bash
dotnet restore GlobalKeyboardCapture.Maui.sln
dotnet test --project GlobalKeyboardCapture.Maui.Tests/GlobalKeyboardCapture.Maui.Tests.csproj -c Release

dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-android
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-windows10.0.19041.0
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-ios
dotnet build GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release -f net10.0-maccatalyst

dotnet pack GlobalKeyboardCapture.Maui/GlobalKeyboardCapture.Maui.csproj -c Release
dotnet format GlobalKeyboardCapture.Maui.sln --verify-no-changes
```

Build the sample for every affected platform because that compiles lifecycle wiring, DI, XAML, and native handlers. MAUI builds are more reliable when run serially with `--disable-build-servers`. Do not use `--disable-build-servers` after the `dotnet test --project` arguments; Microsoft.Testing.Platform can interpret it as a test-runner option and discover zero tests.

## Architecture

```text
Native key source
  -> IPlatformKeyHandler
  -> KeyEventArgs / KeyGesture
  -> KeyHandlerService (priority-ordered snapshot)
  -> IKeyHandler or IAsyncKeyHandler
```

Platform adapters:

- Windows subscribes to `PreviewKeyDown`/`PreviewKeyUp` on each window's exact content element and supports concurrent windows.
- Android wraps `Window.Callback`, always delegates to the original callback, suppresses fallback duplicates, and safely rebinds after Activity recreation.
- iOS/Mac Catalyst observes `GCKeyboard.CoalescedKeyboard.KeyboardInput`; its events cannot prevent Apple native propagation.

Consumers must call both `.UseKeyboardHandling()` and `.AddKeyboardHandling(...)`. Lifecycle events create reference-counted platform-view leases. `IGlobalHotkeyService` is true OS-global behavior only on Windows; the normal pipeline is application-wide.

## Public contracts

- `KeyEventArgs.ToString()` and `KeyGesture.ToString()` are lookup/serialization contracts. Modifier order is `Ctrl+Alt+Shift+Win`. Changes require golden tests and a breaking-change review.
- Prefer typed `KeyboardKey`, `KeyModifiers`, and `KeyGesture` APIs. Preserve legacy aliases and string behavior.
- Registration, scope, suspension, sequence, and global-hotkey tokens are idempotent. An old token must not remove a newer replacement.
- Handler priority is descending; equal priorities retain registration order.
- `IAsyncKeyHandler` receives a detached snapshot. Async changes to `Handled` cannot affect native propagation.
- Scanner profiles are snapshotted in `BarcodeHandler`; mutations after construction do not change an active handler.
- Keep all public members documented. Release builds generate XML docs and should have no CS1591 warnings.

## Performance, trimming, and threading

The input callback is a hot path. Avoid reflection, LINQ, blocking I/O, unnecessary formatting, and UI work there. Marshal UI actions with `MainThread.BeginInvokeOnMainThread`. The service snapshots registrations under a lock and invokes handlers after releasing it; preserve that pattern.

The library declares `IsTrimmable`, `IsAotCompatible`, and enables trim analysis. It intentionally has no whole-assembly `trim.xml`. Do not add blanket trim-warning suppression or reflection-based mapping without a targeted preservation strategy and a trimmed consumer publish test.

Every stateful native component needs symmetric attach/detach and disposal. Preserve the Android callback chain even when another library wraps this library's callback after installation.

## Tests and platform behavior

Pure key maps live under `Core/Mapping` so tests can exercise tables without MAUI workloads. Add golden tests for canonical key strings, aliases, dispatch order, disposal races, and scanner framing. Platform compilation alone is not hardware validation; call out when a keyboard, scanner, device farm, or Apple hardware is still required.

The permanent key limits are documented in README: Windows F1-F24, Android F1-F12, Apple F1-F20. Apple printable-symbol mapping currently uses a US-style USB HID table.

## Style and git workflow

Follow `.editorconfig`: four-space C# indentation, nullable enabled, system usings first, and English identifiers/comments. Match established constant casing in an edited file. Use `#if WINDOWS`, `#elif ANDROID`, and `#elif IOS || MACCATALYST` for platform branches; MAUI auto-includes platform folders.

Preserve unrelated working-tree changes. Use a `codex/` branch, never push directly to `main`, and open a pull request only when explicitly requested.
