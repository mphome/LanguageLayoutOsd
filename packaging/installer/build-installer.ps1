# PowerShell build script for Language Layout OSD Installer
# Usage: .\packaging\installer\build-installer.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Resolve-Path (Join-Path $scriptDir "..\..")

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host " Building Language Layout OSD (Release) " -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan

# 1. Clean and build in Release mode
Write-Host "Running dotnet build..." -ForegroundColor Yellow
& dotnet build (Join-Path $projectDir "LanguageLayoutOsd.csproj") -c Release

# 2. Verify release outputs exist
$releaseBinDir = Join-Path $projectDir "bin\Release\net45"
$exePath = Join-Path $releaseBinDir "LanguageLayoutOsd.exe"
if (-not (Test-Path $exePath)) {
    Write-Error "Release executable not found at: $exePath"
}
Write-Host "Build succeeded! Executable located at: $exePath" -ForegroundColor Green

# 3. Locate Inno Setup Compiler (ISCC.exe)
Write-Host "`nLocating Inno Setup Compiler (ISCC.exe)..." -ForegroundColor Yellow
$isccPath = $null

# Check PATH
$whereCheck = Get-Command iscc -ErrorAction SilentlyContinue
if ($whereCheck) {
    $isccPath = $whereCheck.Source
} else {
    # Check typical program files locations
    $searchPaths = @(
        (Join-Path $env:LocalAppData "Programs\Inno Setup 6\ISCC.exe"),
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 5\ISCC.exe",
        "C:\Program Files\Inno Setup 5\ISCC.exe"
    )

    foreach ($path in $searchPaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }
}

if (-not $isccPath) {
    Write-Host "Inno Setup compiler (ISCC.exe) was not found on your system." -ForegroundColor Red
    Write-Host "To compile the installable installer, please install Inno Setup using winget:" -ForegroundColor Yellow
    Write-Host "  winget install JRSoftware.InnoSetup" -ForegroundColor White
    Write-Host "Then run this script again.`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found Inno Setup Compiler at: $isccPath" -ForegroundColor Green

# 4. Compile the installer script
$issScript = Join-Path $scriptDir "LanguageLayoutOsd.iss"
Write-Host "`nCompiling installer script: $issScript" -ForegroundColor Yellow

$distDir = Join-Path $projectDir "dist"
if (Test-Path $distDir) {
    Remove-Item -Path $distDir -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
}
New-Item -ItemType Directory -Path $distDir -Force | Out-Null

& $isccPath $issScript

$setupFile = Join-Path $distDir "LanguageLayoutOsd-Setup.exe"
if (Test-Path $setupFile) {
    $hash = (Get-FileHash -LiteralPath $setupFile -Algorithm SHA256).Hash.ToUpperInvariant()
    Write-Host "`n==============================================" -ForegroundColor Green
    Write-Host " Installer compiled successfully!" -ForegroundColor Green
    Write-Host " Installer package: $setupFile" -ForegroundColor Green
    Write-Host " SHA256 Checksum:  $hash" -ForegroundColor Green
    Write-Host "==============================================" -ForegroundColor Green
} else {
    Write-Error "Failed to generate installer executable."
}
