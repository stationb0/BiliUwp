# Deploy / run BiliBili.UWP without a signing certificate.
# It builds the solution, expands the generated .appx into a loose layout
# (a loose layout is what Visual Studio F5 uses, and needs no certificate),
# then registers it and launches the app.
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File deploy.ps1
#   powershell -ExecutionPolicy Bypass -File deploy.ps1 -NoBuild
#   powershell -ExecutionPolicy Bypass -File deploy.ps1 -NoLaunch

param(
    [switch]$NoBuild,
    [switch]$NoLaunch
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$sln = Join-Path $root 'BiliBili.sln'
$layout = Join-Path $root 'AppPackages\Layout'
$identityName = '5421.501019FA0C51B'
$aumid = '5421.501019FA0C51B_wp9dg7z2zqgtj!App'

# ---- 1. build -------------------------------------------------------------
if (-not $NoBuild) {
    $msbuildCandidates = @(
        'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'
    )
    $msbuild = $msbuildCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $msbuild) {
        $msbuild = (Get-ChildItem 'C:\Program Files*\Microsoft Visual Studio\*\*\MSBuild\Current\Bin\MSBuild.exe' -ErrorAction SilentlyContinue |
            Select-Object -First 1).FullName
    }
    if (-not $msbuild) { throw 'MSBuild.exe not found. Please build with Visual Studio manually.' }

    Write-Host "Building ..." -ForegroundColor Cyan
    & $msbuild $sln /p:Configuration=Debug /p:Platform=x86 /restore /m /v:m /nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)." }
}

# ---- 2. expand appx into a loose layout ----------------------------------
$appx = Get-ChildItem (Join-Path $root 'BiliBili.UWP\AppPackages') -Recurse -Filter 'BiliBili.UWP_3.10.2.0_x86_Debug.appx' |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $appx) { throw 'appx not found, did the build succeed?' }

Write-Host "Preparing layout ..." -ForegroundColor Cyan
if (Test-Path $layout) { Remove-Item $layout -Recurse -Force }
New-Item -ItemType Directory -Path $layout -Force | Out-Null
$zip = Join-Path $env:TEMP 'BiliBiliUwpDeploy.zip'
Copy-Item $appx.FullName $zip -Force
Expand-Archive -Path $zip -DestinationPath $layout -Force
Remove-Item $zip -Force -ErrorAction SilentlyContinue

# ---- 3. register ----------------------------------------------------------
Write-Host "Registering package ..." -ForegroundColor Cyan
$existing = Get-AppxPackage $identityName -ErrorAction SilentlyContinue
if ($existing) {
    # keep login / history / settings data
    Remove-AppxPackage -Package $existing.PackageFullName -PreserveApplicationData -ErrorAction SilentlyContinue
}
Add-AppxPackage -Register (Join-Path $layout 'AppxManifest.xml')

# ---- 4. launch ------------------------------------------------------------
if (-not $NoLaunch) {
    Write-Host "Launching ..." -ForegroundColor Green
    Start-Process ("shell:AppsFolder\$aumid")
}
Write-Host "Done." -ForegroundColor Green
