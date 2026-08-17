# TestMod Project Design

Date: 2026-08-17

## Purpose

Add a second, independent BepInEx plugin project for testing new mod content
against the DisfigureModApi, mimicking how a community developer would consume
the API.

## Structure

```
DisfigureModApi.sln          (new — includes both projects)
DisfigureModApi.csproj       (stays at root, unchanged)
TestMod/
  TestMod.csproj             (new)
  TestPlugin.cs              (new — BepInEx BasePlugin entry point)
```

## TestMod.csproj

- `net6.0`, `ImplicitUsings` and `Nullable` enabled (matching the API project)
- Same game references as the API project (interop + core + unity-libs),
  pointing at `/mnt/seagate/SteamLibraryD/steamapps/common/Disfigure/BepInEx/`
  via the existing `..\..\..\..\mnt\seagate\...` relative path convention
- Reference to the built API DLL:
  `HintPath ..\bin\$(Configuration)\net6.0\DisfigureModApi.dll`
- Post-build `Copy` task deploys the built DLL to
  `BepInEx/plugins/TestMod/` using the cross-platform MSBuild `Copy` approach

## TestPlugin.cs

- `[BepInPlugin("com.disfigure.testmod", "TestMod", "1.0.0")]`
- Extends `BasePlugin`
- `Load()` logs a startup message to verify BepInEx picks it up
- Contains a stub method demonstrating registering test content through the
  DisfigureModApi

## Build Order

- The `.sln` lists TestMod after DisfigureModApi so the API DLL build output
  exists before TestMod compiles

## Success Criteria

- `dotnet build DisfigureModApi.sln` succeeds
- TestMod.dll appears in `BepInEx/plugins/TestMod/`
- TestMod references the API as a plain DLL dependency, not a project reference