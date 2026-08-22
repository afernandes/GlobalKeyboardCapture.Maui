# Changelog

All notable changes to this project are documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

No changes yet.

## [2.0.0] - 2026-08-22

### Added

- .NET MAUI 10 targets for Android, Windows, iOS, and Mac Catalyst.
- Microsoft.Maui.Controls 10.0.100 and the latest Apache-licensed FluentAssertions 6.x patch for tests.
- Typed `KeyboardKey`, `KeyModifiers`, and `KeyGesture` contracts with validated parsing.
- Disposable, priority-ordered handler and hotkey registrations.
- Reference-counted native platform-view attachments and capture scopes.
- Nest-safe capture suspension for focused text input and modal workflows.
- Structured diagnostics, native key/scan codes, event type, repeat count, key location, and input-device metadata.
- Configurable scanner profiles with device filters, custom terminators, prefix/suffix handling, timing validation, and detailed scan results.
- Optional key-up and native auto-repeat capture.
- Non-blocking `IAsyncKeyHandler` execution with detached snapshots and disposal cancellation.
- Ordered `KeySequenceHandler` shortcuts with per-sequence timeout.
- Multiple-window capture on Windows and lifecycle-safe Activity rebinding on Android.
- True Windows operating-system global hotkeys through `IGlobalHotkeyService`.
- Physical-keyboard capture for iOS and Mac Catalyst through `GCKeyboard`.
- Android emulator integration coverage for F1, F12, and numpad Enter.
- Four-platform library/sample builds and cross-platform tests in CI.
- A diagnostic sample page covering scopes, suspension, scanner profiles, sequences, diagnostics, and Windows global hotkeys.

### Changed

- 2.x now requires .NET MAUI 10; .NET MAUI 8 remains on the compatibility-maintenance branch `codex/net8-maintenance-1.x`.
- Platform handler implementations are internal; custom integrations use `IPlatformKeyHandler`.
- The package no longer roots its entire assembly during trimming.
- Public API members now ship complete XML documentation.

### Fixed

- Android function keys and numpad Enter no longer fail because of native flag combinations; window-scoped keyboard events no longer require `FLAG_FROM_SYSTEM`.
- Held Android keys no longer flood hotkeys or scanner buffers unless repeat capture is enabled.
- Windows capture waits for window content, tracks the exact subscribed element, and cleans up symmetrically.
- Android callback chaining survives Activity recreation and wrappers installed by other libraries.
- Hotkey registration, handler dispatch, scanner buffering, and lifecycle teardown are thread-safe and deterministic.
- Invalid multi-character hotkeys fail explicitly instead of silently registering their first character.
- Barcode validation applies to the transformed payload and prevents unbounded input growth.

## [1.1.0] - 2026-08-21

### Added

- Hotkey removal APIs and deterministic `StopOnHandled` dispatch.
- Configurable maximum barcode length.
- Cross-platform unit test suite.

### Fixed

- Android F1–F12, Meta, PrintScreen, PauseBreak, Space, numpad Enter, flag handling, auto-repeat, activity recreation, and callback lifetime.
- Windows content-ready key subscription and cleanup.
- Thread-safe hotkey registration and alias normalization.
- Barcode buffer cleanup, trimmed-length validation, and overflow rejection.
- Compatibility constructor for manual `KeyHandlerService` consumers.
- Android 21–22 fallbacks for callback APIs introduced in Android 23.

### Changed

- The documentation now describes the current capture mode accurately as application-wide.
- .NET MAUI 8 is now the frozen 1.x maintenance line. New feature development moves to MAUI 10 in 2.x.

## [1.0.2] - 2024-11-23

### Changed

- Improved hotkey normalization and reduced allocations in keyboard processing.

## [1.0.1] - 2024-11-21

### Added

- Extended framework support and input handling improvements.

## [1.0.0] - 2024-11-21

### Added

- Initial Windows and Android keyboard capture, hotkey, and barcode-scanner support.

[Unreleased]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/compare/v1.1.0...v2.0.0
[1.1.0]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/compare/v1.0.2...v1.1.0
[1.0.2]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.2
[1.0.1]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.1
[1.0.0]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.0
