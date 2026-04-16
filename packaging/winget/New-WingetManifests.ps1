param(
    [Parameter(Mandatory = $true)]
    [string]$PackageIdentifier,

    [Parameter(Mandatory = $true)]
    [string]$Publisher,

    [Parameter(Mandatory = $true)]
    [string]$PackageName,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$InstallerUrl,

    [Parameter(Mandatory = $true)]
    [string]$InstallerSha256,

    [Parameter(Mandatory = $false)]
    [string]$PackageUrl = "",

    [Parameter(Mandatory = $false)]
    [string]$PublisherUrl = "",

    [Parameter(Mandatory = $false)]
    [string]$License = "MIT",

    [Parameter(Mandatory = $false)]
    [string]$LicenseUrl = "",

    [Parameter(Mandatory = $false)]
    [string]$ShortDescription = "Shows an on-screen indicator when keyboard layout changes.",

    [Parameter(Mandatory = $false)]
    [string]$ManifestVersion = "1.12.0",

    [Parameter(Mandatory = $false)]
    [string]$Locale = "en-US"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-NotEmpty([string]$value, [string]$name) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "Parameter '$name' cannot be empty."
    }
}

Assert-NotEmpty $PackageIdentifier "PackageIdentifier"
Assert-NotEmpty $Publisher "Publisher"
Assert-NotEmpty $PackageName "PackageName"
Assert-NotEmpty $Version "Version"
Assert-NotEmpty $InstallerUrl "InstallerUrl"
Assert-NotEmpty $InstallerSha256 "InstallerSha256"
Assert-NotEmpty $ShortDescription "ShortDescription"

if ($InstallerSha256 -notmatch '^[A-Fa-f0-9]{64}$') {
    throw "InstallerSha256 must be a 64-char hex string."
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$outDir = Join-Path $scriptDir ("out\" + $PackageIdentifier + "\" + $Version)
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

$versionManifestPath = Join-Path $outDir ($PackageIdentifier + ".yaml")
$installerManifestPath = Join-Path $outDir ($PackageIdentifier + ".installer.yaml")
$localeManifestPath = Join-Path $outDir ($PackageIdentifier + ".locale." + $Locale + ".yaml")

$yamlVersion = @"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.version.$ManifestVersion.schema.json
PackageIdentifier: $PackageIdentifier
PackageVersion: $Version
DefaultLocale: $Locale
ManifestType: version
ManifestVersion: $ManifestVersion
"@

$yamlLocale = @"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.defaultLocale.$ManifestVersion.schema.json
PackageIdentifier: $PackageIdentifier
PackageVersion: $Version
PackageLocale: $Locale
Publisher: $Publisher
PackageName: $PackageName
License: $License
ShortDescription: $ShortDescription
"@

if (-not [string]::IsNullOrWhiteSpace($PublisherUrl)) {
    $yamlLocale += "`r`nPublisherUrl: $PublisherUrl"
}

if (-not [string]::IsNullOrWhiteSpace($PackageUrl)) {
    $yamlLocale += "`r`nPackageUrl: $PackageUrl"
}

if (-not [string]::IsNullOrWhiteSpace($LicenseUrl)) {
    $yamlLocale += "`r`nLicenseUrl: $LicenseUrl"
}

$yamlLocale += @"

ManifestType: defaultLocale
ManifestVersion: $ManifestVersion
"@

$yamlInstaller = @"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.installer.$ManifestVersion.schema.json
PackageIdentifier: $PackageIdentifier
PackageVersion: $Version
InstallerType: zip
NestedInstallerType: portable
Installers:
- Architecture: x64
  InstallerUrl: $InstallerUrl
  InstallerSha256: $($InstallerSha256.ToUpperInvariant())
  NestedInstallerFiles:
  - RelativeFilePath: LanguageLayoutOsd.exe
    PortableCommandAlias: language-layout-osd
ManifestType: installer
ManifestVersion: $ManifestVersion
"@

Set-Content -LiteralPath $versionManifestPath -Value $yamlVersion -Encoding UTF8
Set-Content -LiteralPath $installerManifestPath -Value $yamlInstaller -Encoding UTF8
Set-Content -LiteralPath $localeManifestPath -Value $yamlLocale -Encoding UTF8

Write-Host "Generated winget manifests:"
Write-Host "  $versionManifestPath"
Write-Host "  $installerManifestPath"
Write-Host "  $localeManifestPath"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  wingetcreate validate `"$outDir`""
