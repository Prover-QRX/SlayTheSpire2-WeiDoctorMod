param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$OutputRoot = "../dist/WeiDoctor_Workshop",
    [switch]$IncludeLocalizationTemplate,
    [switch]$IncludeSymbols
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
$projectFile = Join-Path $projectRoot "source/WeiDoctor.csproj"
$outputPath = [System.IO.Path]::GetFullPath((Join-Path $scriptDir $OutputRoot))
$distRoot = [System.IO.Path]::GetFullPath((Join-Path $projectRoot "dist"))

if (-not $outputPath.StartsWith($distRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputRoot must resolve inside '$distRoot'."
}

dotnet build $projectFile -c $Configuration

if (Test-Path -LiteralPath $outputPath) {
    Remove-Item -LiteralPath $outputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $outputPath | Out-Null
New-Item -ItemType Directory -Path (Join-Path $outputPath "lib") | Out-Null

$buildDir = Join-Path $projectRoot "source/bin/$Configuration/net9.0"
Copy-Item -LiteralPath (Join-Path $projectRoot "WeiDoctor.json") -Destination $outputPath
Copy-Item -LiteralPath (Join-Path $buildDir "WeiDoctor.dll") -Destination (Join-Path $outputPath "lib")
Copy-Item -LiteralPath (Join-Path $buildDir "WeiDoctor.deps.json") -Destination (Join-Path $outputPath "lib")

if ($IncludeSymbols -and (Test-Path -LiteralPath (Join-Path $buildDir "WeiDoctor.pdb"))) {
    Copy-Item -LiteralPath (Join-Path $buildDir "WeiDoctor.pdb") -Destination (Join-Path $outputPath "lib")
}

Copy-Item -LiteralPath (Join-Path $projectRoot "assets") -Destination (Join-Path $outputPath "assets") -Recurse
Get-ChildItem -LiteralPath (Join-Path $outputPath "assets") -Filter "manifest.json" -Recurse |
    Remove-Item -Force
if ($IncludeLocalizationTemplate) {
    Copy-Item -LiteralPath (Join-Path $projectRoot "localization_override_template") -Destination (Join-Path $outputPath "localization_override_template") -Recurse
}

$notes = @"
WeiDoctor Workshop package

Copy or upload this folder as the Workshop item payload.

Runtime files:
- WeiDoctor.json
- lib/WeiDoctor.dll
- lib/WeiDoctor.deps.json
- assets/
- localization_override_template/ (only when built with -IncludeLocalizationTemplate)

Development-only folders intentionally excluded:
- source/
- data/
- design/
- docs/
- config/

Do not copy development json files into the live Workshop mod folder. Current RitsuLib beta scans loose json files as possible mod manifests, so design data, localization templates, and asset manifests should stay outside the default runtime package.
"@

Set-Content -LiteralPath (Join-Path $outputPath "WORKSHOP_PACKAGE_NOTES.txt") -Value $notes -Encoding UTF8
Write-Host "Workshop package created at: $outputPath"
