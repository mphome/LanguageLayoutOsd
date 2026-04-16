# WinGet Publishing Guide

This folder contains everything needed to publish `LanguageLayoutOsd` to the public `winget` repository.

## Prerequisites

- Public GitHub repository with Releases enabled
- A release asset (recommended: `.zip` with app files)
- `wingetcreate` installed:

```powershell
winget install wingetcreate
```

## Recommended Package Setup

For this app, use a portable package shipped as a zip archive:

- `InstallerType: zip`
- `NestedInstallerType: portable`
- `NestedInstallerFiles` points to `LanguageLayoutOsd.exe` inside zip

Why: the app is a standalone EXE with no installer and no admin requirement.

## Create Release Asset

Include at least:

- `LanguageLayoutOsd.exe`
- `LanguageLayoutOsd.exe.config`
- `tray.ico`
- `osd-config.sample.json`

Example asset name:

- `LanguageLayoutOsd-1.0.0-win-x64.zip`

You can generate the zip and SHA256 automatically:

```powershell
dotnet build .\LanguageLayoutOsd.csproj -c Release
.\packaging\winget\New-ReleaseZip.ps1 -Version "1.0.0"
```

## Generate Winget Manifests

Run:

```powershell
.\packaging\winget\New-WingetManifests.ps1 `
  -PackageIdentifier "mp.LanguageLayoutOsd" `
  -Publisher "mp" `
  -PackageName "Language Layout OSD" `
  -Version "1.0.0" `
  -InstallerUrl "https://github.com/<owner>/<repo>/releases/download/v1.0.0/LanguageLayoutOsd-1.0.0-win-x64.zip" `
  -InstallerSha256 "<SHA256_HEX>" `
  -PackageUrl "https://github.com/<owner>/<repo>" `
  -PublisherUrl "https://github.com/<owner>" `
  -License "MIT" `
  -LicenseUrl "https://github.com/<owner>/<repo>/blob/main/LICENSE" `
  -ShortDescription "Shows an on-screen indicator when keyboard layout changes."
```

Output folder:

- `packaging\winget\out\<PackageIdentifier>\<Version>\`

## Validate Locally

```powershell
winget validate --manifest .\packaging\winget\out\mp.LanguageLayoutOsd\1.0.0
```

Or with wingetcreate:

```powershell
wingetcreate validate .\packaging\winget\out\mp.LanguageLayoutOsd\1.0.0
```

## Submit to winget-pkgs

1. Fork `https://github.com/microsoft/winget-pkgs`.
2. Copy generated files to:
   - `manifests\m\mp\LanguageLayoutOsd\1.0.0\`
3. Commit and push in your fork.
4. Open PR to `microsoft/winget-pkgs`.

## Notes

- `PackageIdentifier` should be stable forever.
- For updates, only create a new version folder (e.g. `1.0.1`).
- Keep release assets immutable (do not replace files under same URL after publishing).
