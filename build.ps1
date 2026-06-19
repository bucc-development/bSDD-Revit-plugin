#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build script for bSDD Revit Plugin
.DESCRIPTION
    Restores, builds the plugin for each selected Revit version, and generates the installer.

    Building a Revit version project requires that Revit version to be installed locally,
    because it references the CefSharp assemblies shipped inside the Revit install folder
    (C:\Program Files\Autodesk\Revit <year>\CefSharp). Use -RevitVersions to build only the
    versions you have installed.
.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Release.
.PARAMETER RevitVersions
    Which Revit version projects to build. Default: 2024, 2025, 2026.
.PARAMETER SkipRestore
    Skip NuGet package restore step.
.PARAMETER BuildInstallerOnly
    Only build the installer (assumes projects are already built).
.EXAMPLE
    .\build.ps1
    .\build.ps1 -Configuration Debug
    .\build.ps1 -RevitVersions 2025,2026
    .\build.ps1 -BuildInstallerOnly
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string[]]$RevitVersions = @("2024", "2025", "2026"),

    [switch]$SkipRestore,

    [switch]$BuildInstallerOnly
)

$ErrorActionPreference = "Stop"
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  bSDD Revit Plugin - Build Script" -ForegroundColor Cyan
Write-Host "  Configuration : $Configuration" -ForegroundColor Cyan
Write-Host "  Revit versions: $($RevitVersions -join ', ')" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

try {
    $solutionDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $solutionFile = Join-Path $solutionDir "BsddRevitPlugin.sln"
    if (-not (Test-Path $solutionFile)) { throw "Solution file not found: $solutionFile" }

    if (-not $BuildInstallerOnly) {
        if (-not $SkipRestore) {
            Write-Host "`n===> Restoring NuGet packages..." -ForegroundColor Cyan
            dotnet restore $solutionFile
            if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed" }
            Write-Host "Done: packages restored" -ForegroundColor Green
        }

        # Each version project transitively builds the shared Logic + Resources libraries
        # (multi-targeted net48 for 2024, net8.0-windows for 2025/2026).
        foreach ($v in $RevitVersions) {
            $proj = Join-Path $solutionDir "BsddRevitPlugin.$v\BsddRevitPlugin.$v.csproj"
            if (-not (Test-Path $proj)) { throw "Project not found: $proj" }
            Write-Host "`n===> Building BsddRevitPlugin.$v ($Configuration, x64)..." -ForegroundColor Cyan
            dotnet build $proj -c $Configuration -p:Platform=x64 --nologo -v minimal
            if ($LASTEXITCODE -ne 0) { throw "BsddRevitPlugin.$v build failed (is Revit $v installed? CefSharp is required)" }
            Write-Host "Done: BsddRevitPlugin.$v built" -ForegroundColor Green
        }
    } else {
        Write-Host "Skipping project builds (BuildInstallerOnly mode)" -ForegroundColor Gray
    }

    # Build Installer
    Write-Host "`n===> Building Installer..." -ForegroundColor Cyan
    $isccPaths = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    $isccPath = $isccPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $isccPath) {
        Write-Host "ERROR: Inno Setup not found. Install it with:" -ForegroundColor Red
        Write-Host "  winget install --id=JRSoftware.InnoSetup --exact --silent" -ForegroundColor Yellow
        throw "Inno Setup not found"
    }
    Write-Host "  Using Inno Setup: $isccPath" -ForegroundColor Gray

    $installerScript = Join-Path $solutionDir "BsddRevitPlugin.Installer\Installer.iss"
    & $isccPath $installerScript
    if ($LASTEXITCODE -ne 0) { throw "Installer build failed" }

    $installerPath = Join-Path $solutionDir "BsddRevitPlugin.Installer\Output\bSDD-Revit-plugin-setup.exe"
    if (Test-Path $installerPath) {
        $installerSize = (Get-Item $installerPath).Length / 1MB
        Write-Host "Done: Installer built successfully!" -ForegroundColor Green
        Write-Host "  Location: $installerPath" -ForegroundColor Gray
        Write-Host "  Size: $([math]::Round($installerSize, 2)) MB" -ForegroundColor Gray
    }

    $stopwatch.Stop()
    $elapsed = $stopwatch.Elapsed
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  BUILD COMPLETED SUCCESSFULLY" -ForegroundColor Green
    Write-Host "  Time: $($elapsed.Minutes)m $($elapsed.Seconds)s" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Red
    Write-Host "  BUILD FAILED" -ForegroundColor Red
    Write-Host "============================================================" -ForegroundColor Red
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
