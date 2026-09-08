# WeiDoctor Workshop Port

This repository keeps development data separate from the runtime package.

Use `tools/Build-WorkshopPackage.ps1` to create a clean Workshop payload:

```powershell
cd "D:/大黄算法？？？/WeiDoctor_v1.0_beta"
./tools/Build-WorkshopPackage.ps1 -Configuration Release
```

The script outputs `dist/WeiDoctor_Workshop`. That folder is the Workshop-facing port and intentionally excludes `source`, `data`, `design`, `docs`, and `config`.

Current beta notes:

- Runtime asset paths are resolved from the mod assembly directory, so the same asset loader works in local testing and Workshop packaging.
- Keep development json files out of the installed mod folder. In the current RitsuLib beta, loose json files can be scanned as possible mod manifests, so the package script removes asset `manifest.json` files and excludes localization templates by default.
- `localization_override_template` can be included as a migration aid with `-IncludeLocalizationTemplate`, but the default Workshop payload leaves it out until localization is registered through a runtime API.
- The mod manifest remains `WeiDoctor.json`; keep its `id` stable for save compatibility.
