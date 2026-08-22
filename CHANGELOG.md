# Changelog

All notable changes to this project are documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- Project roadmap for the supported MAUI 10 / 2.x line.

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

[Unreleased]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/compare/v1.0.2...v1.1.0
[1.0.2]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.2
[1.0.1]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.1
[1.0.0]: https://github.com/afernandes/GlobalKeyboardCapture.Maui/releases/tag/v1.0.0
