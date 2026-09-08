# Beta Compatibility Notes

Target verified on 2026-09-07:

- Slay the Spire 2 install: `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/`
- Game version: `v0.111.0`
- Game commit: `41cef1ea`
- Game build date: `2026-08-13T17:39:18-07:00`
- Main assembly hash from `release_info.json`: `222455745`
- Runtime target: `.NET 9.0`, bundled `Microsoft.NETCore.App 9.0.7`
- RitsuLib workshop id: `3747602295`
- RitsuLib version: `0.5.19`
- RitsuLib selected compatibility target: `0.111.0`

Manifest alignment:

- `WeiDoctor.json` uses `min_game_version: 0.111.0`.
- `WeiDoctor.json` depends on `STS2-RitsuLib >= 0.5.19`.
- The package is intended to load beside RitsuLib's `lib/0.111.0/STS2-RitsuLib.dll`.
- `has_pck` is currently `false` because `WeiDoctor.pck` has not been generated yet. Turn it back on after Godot assets are packed.

Current build status:

- .NET 9 SDK `9.0.317` is installed.
- The placeholder source compiles successfully to `source/bin/Debug/net9.0/WeiDoctor.dll`.
- This DLL is not gameplay-complete yet because `source/WeiDoctor.BetaAdapter.cs` still uses an abstract adapter instead of direct Slay the Spire 2/RitsuLib registration.

Recommended local test layout after compiling:

- Copy the final mod folder to `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/mods_pending/WeiDoctor/`.
- Include `WeiDoctor.json`, `WeiDoctor.dll`, `data/`, `assets/`, `config/`, and optionally `design/`.
- Include `WeiDoctor.pck` only after Godot resource packing is implemented and `has_pck` is restored to `true`.
- Keep RitsuLib enabled through Steam Workshop or as a local mod so dependency resolution can find `STS2-RitsuLib`.
