# Usage

## Run

1. Build:

```powershell
dotnet build LanguageLayoutOsd.csproj -c Release
```

2. Start:

```powershell
.\bin\Release\net45\LanguageLayoutOsd.exe
```

After startup, the app runs in the system tray and tracks layout changes in background.

## Tray Menu

- Right-click tray icon -> `Exit` to stop the app.

## Config File

Place `osd-config.json` next to `LanguageLayoutOsd.exe`.

Example:

```json
{
  "displayDurationMs": 500,
  "fontSizePt": 42,
  "pollingIntervalMs": 150,
  "defaultBackColor": "#A0282828",
  "defaultTextColor": "#F5F5F5",
  "ruBackColor": "#B4782814",
  "ruTextColor": "#FFFFFF",
  "languageStyles": {
    "EN": { "backColor": "#A0282828", "textColor": "#F5F5F5" },
    "RU": { "backColor": "#B4782814", "textColor": "#FFFFFF" },
    "UK": { "backColor": "#B41F4E9A", "textColor": "#FFFFFF" }
  }
}
```

## Config Fields

- `displayDurationMs`: OSD visibility in milliseconds (valid range: `100..5000`)
- `fontSizePt`: label font size in points (`10..120`)
- `pollingIntervalMs`: polling interval in milliseconds (`50..1000`)
- `defaultBackColor`: `#RRGGBB` or `#AARRGGBB`
- `defaultTextColor`: `#RRGGBB` or `#AARRGGBB`
- `ruBackColor`: `#RRGGBB` or `#AARRGGBB`
- `ruTextColor`: `#RRGGBB` or `#AARRGGBB`
- `languageStyles`: optional per-layout style map (`EN`, `RU`, `UK`, `DE`, etc.)
  - `backColor`: `#RRGGBB` or `#AARRGGBB`
  - `textColor`: `#RRGGBB` or `#AARRGGBB`

If a value is invalid, default is used.

## Diagnostics

Primary log location:

- `%LocalAppData%\LanguageLayoutOsd\diagnostic.log`

Fallback:

- `diagnostic.log` near executable

Useful checks:

1. App startup sequence (`Application starting`, `Starting Application.Run`)
2. Tray startup (`Layout monitor started`)
3. Layout events (`Layout changed: EN/RU`)
4. Exceptions (`ThreadException`, `UnhandledException`, `ShowLayout failed`)

## Common Troubleshooting

1. No tray icon after launch:
   - Check diagnostics log for startup exception.
2. OSD does not show:
   - Confirm `Layout changed` appears in log.
   - Reduce `pollingIntervalMs` to `100`.
3. App exits unexpectedly:
   - Share full diagnostics log for investigation.
