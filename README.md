# Language Layout OSD

Lightweight Windows tray utility that shows an on-screen indicator (`EN`, `RU`, etc.) when the keyboard layout changes.

## Features

- Detects layout of the active (foreground) window.
- Polling-based detection (default `150ms`).
- OSD appears in center of primary screen.
- Borderless, topmost, no taskbar, no Alt+Tab, no focus stealing.
- Single OSD instance: timer restarts on rapid changes, no stacked popups.
- Tray icon with `Start with Windows` checkbox and diagnostics/exit actions.
- Optional JSON config.
- Windows Startup integration (Runs on startup via user registry entry).
- Diagnostic logging for runtime issues.

## Tech Stack

- .NET Framework `4.5`
- WinForms
- Win32 API:
  - `GetForegroundWindow`
  - `GetWindowThreadProcessId`
  - `GetKeyboardLayout`

## Project Structure

- `Program.cs` - app startup, global exception hooks
- `TrayApplicationContext.cs` - tray lifecycle and background runtime
- `LayoutMonitor.cs` - polling loop and change detection
- `LayoutService.cs` - Win32 interop and layout mapping
- `OsdForm.cs` - overlay UI
- `AppConfig.cs` - config loading and defaults
- `DiagnosticLog.cs` - runtime diagnostics

## Build

```powershell
dotnet build LanguageLayoutOsd.csproj -c Release
```

Output executable:

- `bin\Release\net45\LanguageLayoutOsd.exe`

### Build Installer

An automated PowerShell script builds the project in Release mode and compiles a professional, non-admin-required Inno Setup installer package `LanguageLayoutOsd-Setup.exe` into the `dist\` folder:

```powershell
powershell -ExecutionPolicy Bypass -File .\packaging\installer\build-installer.ps1
```

*(Note: If Inno Setup is not installed, the script will suggest installing it via `winget install JRSoftware.InnoSetup`)*

## Configuration

Copy `osd-config.sample.json` to `osd-config.json` near the EXE and edit values as needed.

See full usage/config details in [USAGE.md](USAGE.md).

## Tray Icon

- App tries to load `tray.ico` from the EXE folder.
- If missing or invalid, it falls back to `SystemIcons.Application`.

## Diagnostics

Log file path:

- `%LocalAppData%\LanguageLayoutOsd\diagnostic.log`
- fallback: `diagnostic.log` near EXE

## License

MIT - see [LICENSE](LICENSE).

## Distribution

- GitHub Releases: publish portable zip assets for each version.
- WinGet: see [packaging/winget/README.md](packaging/winget/README.md) for manifests and submission flow.
