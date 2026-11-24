# Template BepInEx Plugin

A small BepInEx plugin template for Unity modding **Casualties Unknown (full release)** with CUCoreLib

## Requirements

- Casualties Unknown (full release) with BepInEx installed
- CUCoreLib `v1.0.7.2` or newer in `BepInEx/plugins` (In the full steam release folder!)
- The Demo is long-term support only (breaking-bug and compatibility fixes), so mods built from this template may not work for a Demo install
- - If you are building for the demo of the game, clone [this version](https://github.com/jimmyking9999999/CCLScavTemplate/commit/a362ecedaae2320dfab9c116ac590d2e9a1736f3)

## Overview 
- Includes `CUCoreLib` references/usings, BepInEx(implicitly pulls HarmonyX), Newtonsoft.Json, and Unity assembly references (Unity 2022.3.62, matching the full release)
- Includes a `ContentReloadManager.EnableHotReload(...)` starter pattern
- Automatically embeds media placed under `images/` (`.png`, `.jpg`, `.jpeg`, `.ase`, `.aseprite`) and `assets/` (`.wav`, `.mp1`, `.mp2`, `.mp3`, `.ogg`, `.cue`, `.aif`, `.aiff`)
- Expands common Steam install paths on Windows and Linux, looking for `steamapps\common\Casualties Unknown`, also supports a relative path `$GAMEDIR/$DIR/$MODDIR`

## Build

1. Open the project in Visual Studio or JetBrains Rider. (Or any other IDE)
2. Build `ScavTemplate/Template.csproj` via Ctrl + Shift + B (`dotnet build`)
3. If auto-detection misses your setup, open the linked `Directory.Build.props` file from the project and override `BaseGamePath` 
4. The built `.dll` is automatically copied to `<game>\BepInEx\plugins\<ProjectName>\` for you!

## Usage

1. Update `Plugin.cs` with your plugin GUID, name, version.
2. Update the namespace in `Plugin.cs` and `Patches.cs`. The easy way is to rename it in the VS solution explorer.
3. Set `ProjectName` in `Directory.Build.props`. That's your `.dll` name!
4. Put startup-only functions before `ContentReloadManager.EnableHotReload(ModGUID)`.
5. Put reloadable item/recipe/content registration inside `RegisterReloadable()` / `DoStuff()`.
6. Place embedded assets under `images` and `assets` (or set `Build Action = Embedded Resource` manually elsewhere in the project).
7. Run the game with BepInEx and `CUCoreLib` installed in the plugins folder! ^^
