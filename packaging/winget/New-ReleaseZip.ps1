param(
    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$Framework = "net45"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$buildOutput = Join-Path $repoRoot ("bin\" + $Configuration + "\" + $Framework)
$distDir = Join-Path $repoRoot "dist"
$assetName = "LanguageLayoutOsd-$Version-win-x64.zip"
$assetPath = Join-Path $distDir $assetName
$tempDir = Join-Path $distDir ("_tmp_" + [Guid]::NewGuid().ToString("N"))

if (-not (Test-Path $buildOutput)) {
    throw "Build output does not exist: $buildOutput. Run 'dotnet build -c $Configuration' first."
}

New-Item -ItemType Directory -Path $distDir -Force | Out-Null
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

$requiredFiles = @(
    "LanguageLayoutOsd.exe",
    "LanguageLayoutOsd.exe.config",
    "tray.ico",
    "osd-config.sample.json"
)

foreach ($file in $requiredFiles) {
    $source = Join-Path $buildOutput $file
    if (-not (Test-Path $source)) {
        throw "Missing file in build output: $source"
    }
    Copy-Item -LiteralPath $source -Destination (Join-Path $tempDir $file)
}

if (Test-Path $assetPath) {
    Remove-Item -LiteralPath $assetPath -Force
}

Compress-Archive -Path (Join-Path $tempDir "*") -DestinationPath $assetPath -CompressionLevel Optimal
Remove-Item -LiteralPath $tempDir -Recurse -Force

$hash = (Get-FileHash -LiteralPath $assetPath -Algorithm SHA256).Hash.ToUpperInvariant()

Write-Host "Release asset created:"
Write-Host "  $assetPath"
Write-Host "SHA256:"
Write-Host "  $hash"
