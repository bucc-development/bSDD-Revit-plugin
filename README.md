# bSDD Revit plugin

> **This is a community build tool. It is not an official buildingSMART International initiative.**
> 
<!-- TOC -->
* [Introduction](#introduction)
* [Open bSDD toolkit projects](#open-bsdd-toolkit-projects)
* [Features](#features)
* [Building](#building)
* [Installation](#installation)
* [Usage](#usage)
<!-- TOC -->

## Introduction
This project is part of the Open bSDD toolkit, an opensource development of a series of consistent bSDD plugins sharing a common bSDD web UI.

This project is initiated by Dutch contractors VolkerWessels and Heijmans. By starting this opensource development we believe we can help the industry structuring data. Proper usage of the buildingSMART Data Dictionary helps in getting consistent information in objects. Good information is the basis for further automation. 
The idea of our development is that we inspire our industry to include bSDD in their processes and softwareproducts natively.

## Open bSDD toolkit projects
-	Web UI https://github.com/buildingsmart-community/bSDD-filter-UI
-	Revit Plugin https://github.com/buildingsmart-community/bSDD-Revit-plugin
-	Sketchup bSDD plugin https://github.com/DigiBase-VolkerWessels/SketchUp-bsDD-plugin
-	Trimble Connect bSDD plugin (Not available yet)

## Features
### Validate model against bSDD
- [x] **Revit types**
- [ ] (Revit instances) **TODO**
- [ ] (Revit families) **TODO**
- [x] **validate against multiple dictionaries**
- [ ] (validate against class restrictions) **TODO**
- [ ] (validate against nested class restrictions) **TODO**
- [ ] (validate against property restrictions) **TODO**
- [ ] (apply IDS filter before linking to the model) **TODO**
### Apply bSDD classes on Revit elements
- [x] **search in main dictionary**
- [x] **select possible related classifications**
- [x] **automatic generation of persistent shared parameter GUIDs**
- [ ] (select unrelated classes from filter dictionaries)
- [ ] (add bSDD materials) **TODO**
### Export IFC, according to [bSDD documentation](https://github.com/buildingSMART/bSDD/blob/master/Documentation/bSDD-IFC%20documentation.md)
- [x] **consistent mapping of bSDD properties**
- [x] **consistent application of an unlimited number of IfcClassifications**
- [x] **leverages the built-in Revit IFC exporter, with a postprocessing step for improving Ifcclassificationreference location URL**
- [ ] (add property URL) **TODO**

## Building

### Quick Start

The easiest way to build the entire project including the installer is to use the provided build scripts:

**PowerShell (recommended):**
```powershell
.\build.ps1
```

**Batch file:**
```cmd
build.bat
```

**Build in Debug mode:**
```powershell
.\build.ps1 -Configuration Debug
```

**Build installer only (if projects are already built):**
```powershell
.\build.ps1 -BuildInstallerOnly
```

### Supported Revit versions

| Revit | Runtime | Embedded browser | Plugin project |
|-------|---------|------------------|----------------|
| 2024  | .NET Framework 4.8 | CefSharp 105 | `BsddRevitPlugin.2024` (net48) |
| 2025  | .NET 8 | CefSharp 119 | `BsddRevitPlugin.2025` (net8.0-windows) |
| 2026  | .NET 8 | WebView2 | `BsddRevitPlugin.2026` (net8.0-windows) |

The shared `BsddRevitPlugin.Logic` and `BsddRevitPlugin.Resources` libraries are **multi-targeted**
(`net48;net8.0-windows`); each version project automatically consumes the matching target.

The browser assemblies (CefSharp for 2024/2025, WebView2 for 2026) are referenced from NuGet for
**compile only** — Revit ships and loads them at runtime, so they are not deployed with the add-in.
Revit 2026 dropped CefSharp, so its version project uses a WebView2-based browser implementation.

> Revit 2023 is no longer supported (it required a separate .NET Framework lowest-common-denominator build).

### Prerequisites

- **Visual Studio 2022 (17.8+)** with the **.NET desktop** workload (needed for .NET 8 / Revit 2025+)
- **.NET 8 SDK** (for `dotnet` commands)
- All NuGet dependencies (Revit API, CefSharp/WebView2) are restored from NuGet, so **Revit does
  not need to be installed to compile**. You do of course need the matching Revit version installed
  to *run/debug* the plugin.
- **Inno Setup 6** (for building the installer)
  ```powershell
  winget install --id=JRSoftware.InnoSetup --exact --silent
  ```

### Manual Build Steps

If you prefer to build manually or need more control:

1. **Restore NuGet packages:**
   ```powershell
   dotnet restore BsddRevitPlugin.sln
   ```

2. **Build the version project(s)** (each transitively builds the shared `Logic` + `Resources` libs):
   ```powershell
   # Build only the versions whose Revit you have installed.
   dotnet build BsddRevitPlugin.2024\BsddRevitPlugin.2024.csproj -c Release -p:Platform=x64
   dotnet build BsddRevitPlugin.2025\BsddRevitPlugin.2025.csproj -c Release -p:Platform=x64
   dotnet build BsddRevitPlugin.2026\BsddRevitPlugin.2026.csproj -c Release -p:Platform=x64
   ```

3. **Build the installer:**
   ```powershell
   & "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" "BsddRevitPlugin.Installer\Installer.iss"
   ```

The installer will be created at: `BsddRevitPlugin.Installer\Output\bSDD-Revit-plugin-setup.exe`

For more details about the installer, see [BsddRevitPlugin.Installer/README.md](BsddRevitPlugin.Installer/README.md).

## Installation
1. Go to the [Releases](https://github.com/buildingsmart-community/bSDD-Revit-plugin/releases) page
2. Find the release you want to install, the latest is at the top (**Mind the "Pre-release" tags, those versions are not meant for production use!**)
3. Expand the **Assets** section, and download the installer (.exe) file.
4. When running the installer it first asks you if you want to install for:
   - just you (plugin is installed in "C:\Users\%USERNAME%\AppData\Roaming\Autodesk\Revit\Addins\")
   - all users, **needing admin privilages to install** (plugin is installed in (C:\ProgramData\Autodesk\Revit\Addins\)
6. Select installation language
7. Select Revit versions to install for (2024, 2025 and 2026 currently)
8. Accept [MIT license](https://github.com/buildingsmart-community/bSDD-Revit-plugin/blob/main/LICENSE)
9. Select start menu folder to get a shortcut to the uninstaller.

## Usage
[Go to the wiki page](https://github.com/buildingsmart-community/bSDD-Revit-plugin/wiki/)

## Development setup
### Prerequisites
- Make sure you are on MS Windows
- Github Desktop installed (or just git)
- Visual Studio installed (or VS code or another editor/IDE)

### Clone repo from Github desktop
- File → Clone repository... → URL → https://github.com/buildingsmart-community/bSDD-Revit-plugin.git
- Repository → Open in PowerShell

> **Note:** The ASRR helper code that used to come from the `lib/asrr` git submodules is now
> vendored directly into `BsddRevitPlugin.Logic` (see `Vendor/Asrr/`), so the submodules are no
> longer required to build. The `git submodule init` step is optional.

### Setup the project in Visual Studio
- File → Open → Project/Solution... → BsddRevitPlugin.sln
- Switch platform from "Any CPU" to x64 (because we use CefSharp)
- Choose a preferred Revit project as startup project (right-click BsddRevitPlugin.2024/2025/2026 in solution explorer → Set as Startup Project)
- Make Revit start on debug (right-click the version project → Properties → Debug → Start external program: ```C:\Program Files\Autodesk\Revit <year>\Revit.exe```)
- run debug...