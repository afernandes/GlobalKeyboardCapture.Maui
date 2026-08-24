# Changelog

All notable changes to this project are documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- .NET MAUI 10 targets for Android, Windows, iOS, and Mac Catalyst.
- Typed `KeyboardKey`, `KeyModifiers`, and `KeyGesture` contracts with validated parsing.
- Disposable, priority-ordered registrations, reference-counted native attachments, capture scopes, and nest-safe capture suspension.
- Structured diagnostics with native key/scan codes, event type, repeat count, location, and input-device metadata.
- Configurable scanner profiles with device filters, custom terminators, prefix/suffix handling, timing validation, bounded buffers, and detailed results.
- Optional key-up and native auto-repeat capture plus detached, cancellable `IAsyncKeyHandler` execution.
- Reusable `KeyboardDeviceFilter` routing at registration and scope level.
- Advanced key sequences with shared-prefix support, explicit overlap policies, cancellation, detached progress snapshots, and progress notifications.
- Layout-aware Apple symbol translation through `IKeyboardLayoutTranslator` and a bounded `KeyboardLayoutTranslationCache`, retaining the USB HID US fallback.
- Multiple-window capture on Windows, lifecycle-safe Activity rebinding on Android, and physical-keyboard capture on Apple platforms.
- True Windows operating-system global hotkeys through `IGlobalHotkeyService`.
- Opt-in `System.Diagnostics.Metrics` counters and latency histogram with a disabled-by-default zero-allocation branch.
- BenchmarkDotNet coverage and a versioned performance policy for normalization, dispatch, hotkey lookup, scanner buffering, and metrics overhead.
- Android UI Automator coverage plus an optional Firebase Test Lab physical-device workflow.
- WinUI runtime integration coverage for key-down/up, two windows, content replacement, attach/detach, and `WM_HOTKEY` unregister behavior.
- Package/API compatibility gates prepared against the first published 2.x baseline.
- A Docfx documentation site with compiled examples and platform/scanner/hotkey/kiosk/sequence/troubleshooting guides.
- A guarded release workflow that produces SPDX SBOM, SHA-256 checksums, GitHub provenance/SBOM attestations, NuGet Trusted Publishing, and post-publish restore smoke tests.
- A diagnostic sample page covering scopes, suspension, scanner profiles, sequences, diagnostics, device metadata, and Windows global hotkeys.

### Changed

- 2.x requires .NET MAUI 10; .NET MAUI 8 compatibility maintenance remains isolated on `codex/net8-maintenance-1.x`.
- Microsoft.Maui.Controls is pinned to 10.0.100 and tests retain the Apache-licensed FluentAssertions 6.x line.
- Platform handler implementations are internal; custom integrations use `IPlatformKeyHandler`.
- The package no longer roots its entire assembly during trimming and now runs trim/AOT analysis without blanket warning suppression.
- Every public API member ships XML documentation, and package releases are validated structurally before publication.
- The expansion policy keeps Tizen, Linux, Windows Raw Input, Android USB Host, and Apple HID-specific work outside the core package until their hardware/maintenance gates are met.

### Fixed

- Android F1-F12 and numpad Enter no longer fail because of native flag combinations; window-scoped keyboard events no longer require `FLAG_FROM_SYSTEM`.
- Held Android keys no longer flood hotkeys or scanner buffers unless repeat capture is enabled.
- Windows capture waits for content, follows content replacement, tracks the exact subscribed element, and closes attach/dispose races symmetrically.
- Android callback chaining survives Activity recreation and wrappers installed by other libraries.
- Hotkey registration, handler dispatch, scanner buffering, sequence progress, and lifecycle teardown are thread-safe and deterministic.
- Invalid multi-character hotkeys fail explicitly instead of silently registering their first character.
- Barcode validation applies to the transformed payload and prevents unbounded input growth.

## [1.0.2] - 2024-11-23

### Changed

- Improved hotkey normalization and reduced allocations in keyboard processing.

## [1.0.1] - 2024-11-21

### Added

- Extended framework support and input handling improvements.

## [1.0.0] - 2024-11-21

### Added

- Initial Windows and Android keyboard capture, hotkey, and barcode-scanner support.

[Unreleased]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/compare/v1.0.2...HEAD
[1.0.2]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.2
[1.0.1]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.1
[1.0.0]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.0
