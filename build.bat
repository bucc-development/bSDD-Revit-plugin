@echo off
REM Build script for bSDD Revit Plugin
REM Usage: build.bat [Debug|Release]
REM
REM Building a Revit version project requires that Revit version to be installed locally
REM (it references CefSharp from C:\Program Files\Autodesk\Revit <year>\CefSharp).
REM For more control use build.ps1 (supports -RevitVersions).

setlocal enabledelayedexpansion

set "CONFIG=%~1"
if "%CONFIG%"=="" set "CONFIG=Release"

echo.
echo ============================================================
echo   bSDD Revit Plugin - Build Script
echo ============================================================
echo   Configuration: %CONFIG%
echo ============================================================
echo.

cd /d "%~dp0"

echo [1/5] Restoring NuGet packages...
dotnet restore BsddRevitPlugin.sln
if errorlevel 1 goto :error

REM Each version project transitively builds the shared Logic + Resources libraries.
echo.
echo [2/5] Building BsddRevitPlugin.2024 (Revit 2024, net48)...
dotnet build BsddRevitPlugin.2024\BsddRevitPlugin.2024.csproj -c %CONFIG% -p:Platform=x64 --nologo -v minimal
if errorlevel 1 goto :error

echo.
echo [3/5] Building BsddRevitPlugin.2025 (Revit 2025, net8)...
dotnet build BsddRevitPlugin.2025\BsddRevitPlugin.2025.csproj -c %CONFIG% -p:Platform=x64 --nologo -v minimal
if errorlevel 1 goto :error

echo.
echo [4/5] Building BsddRevitPlugin.2026 (Revit 2026, net8)...
dotnet build BsddRevitPlugin.2026\BsddRevitPlugin.2026.csproj -c %CONFIG% -p:Platform=x64 --nologo -v minimal
if errorlevel 1 goto :error

echo.
echo [5/5] Building Installer...

REM Try to find Inno Setup
set "ISCC="
if exist "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" set "ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"

if "%ISCC%"=="" (
    echo ERROR: Inno Setup not found. Please install it first:
    echo   winget install --id=JRSoftware.InnoSetup --exact --silent
    goto :error
)

"%ISCC%" "BsddRevitPlugin.Installer\Installer.iss"
if errorlevel 1 goto :error

echo.
echo ============================================================
echo   BUILD COMPLETED SUCCESSFULLY
echo ============================================================
echo   Installer: BsddRevitPlugin.Installer\Output\bSDD-Revit-plugin-setup.exe
echo ============================================================
echo.
exit /b 0

:error
echo.
echo ============================================================
echo   BUILD FAILED
echo ============================================================
echo.
exit /b 1
