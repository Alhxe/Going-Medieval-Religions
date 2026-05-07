# Religions Expanded — build & deploy script
#
# What it does:
#   1. Validates every JSON file under ModBuild/ReligionsExpanded/.
#   2. Merges modular files in Data/Models/<category>/*.json into the single
#      repository file Going Medieval expects (e.g. BaseBuildingRepository.json).
#   3. Expands localization keys into the inline `locKeys` format.
#   4. Copies the result to the user's mods folder.
#
# Usage:
#   pwsh tools/build-mod.ps1
#   pwsh tools/build-mod.ps1 -SkipDeploy    # build only, don't copy to game
#   pwsh tools/build-mod.ps1 -Verbose

[CmdletBinding()]
param(
    [switch]$SkipDeploy
)

$ErrorActionPreference = 'Stop'
$RepoRoot   = Split-Path -Parent $PSScriptRoot
$ModSource  = Join-Path $RepoRoot 'ModBuild/ReligionsExpanded'
$ModName    = 'ReligionsExpanded'
$DeployRoot = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Foxy Voxel/Going Medieval/Mods'
$DeployDest = Join-Path $DeployRoot $ModName

# Maps modular subfolder → final repository filename the game loads.
$RepositoryMap = @{
    'buildings' = 'BaseBuildingRepository.json'
    'items'     = 'Resources.json'
    'rooms'     = 'RoomTypes.json'
    'research'  = 'Research.json'
    'events'    = 'GameEventSettingsRepository.json'
    'religions' = 'ReligionConfig.json'
    'stats'     = 'StatsRepository.json'
}

function Write-Step($message) {
    Write-Host "==> $message" -ForegroundColor Cyan
}

function Test-Json {
    param([string]$Path)
    try {
        $null = Get-Content -Raw $Path | ConvertFrom-Json -ErrorAction Stop
        return $true
    } catch {
        Write-Host "JSON error in $Path : $_" -ForegroundColor Red
        return $false
    }
}

# 1. Validate every JSON
Write-Step 'Validating JSON files'
$jsonFiles = Get-ChildItem -Path $ModSource -Filter *.json -Recurse
$failed = $false
foreach ($f in $jsonFiles) {
    if (-not (Test-Json $f.FullName)) { $failed = $true }
}
if ($failed) { throw 'JSON validation failed.' }
Write-Host "  $($jsonFiles.Count) files OK"

# 2. Load localization tables
Write-Step 'Loading localization'
$LocaleDir = Join-Path $ModSource 'Localization'
$Locales   = @{}
Get-ChildItem -Path $LocaleDir -Filter *.json | ForEach-Object {
    $data = Get-Content -Raw $_.FullName | ConvertFrom-Json
    $Locales[$data.language] = $data.strings
}
Write-Host "  Languages: $($Locales.Keys -join ', ')"

# 3. Build & merge modular files into final repository JSONs
Write-Step 'Merging modular definitions'
$BuildOut = Join-Path $env:TEMP "religions_expanded_build_$([guid]::NewGuid())"
New-Item -ItemType Directory -Force -Path "$BuildOut/Data/Models" | Out-Null

$ModelsDir = Join-Path $ModSource 'Data/Models'
foreach ($category in $RepositoryMap.Keys) {
    $catDir = Join-Path $ModelsDir $category
    if (-not (Test-Path $catDir)) { continue }

    $entries = @()
    Get-ChildItem -Path $catDir -Filter *.json | ForEach-Object {
        $data = Get-Content -Raw $_.FullName | ConvertFrom-Json
        if ($data.repository) { $entries += $data.repository }
        elseif ($data -is [array]) { $entries += $data }
        else { $entries += $data }
    }

    if ($entries.Count -eq 0) { continue }

    $merged = [pscustomobject]@{ repository = $entries }
    $outPath = Join-Path "$BuildOut/Data/Models" $RepositoryMap[$category]
    $merged | ConvertTo-Json -Depth 32 | Set-Content -Path $outPath -Encoding UTF8
    Write-Host "  $category → $($RepositoryMap[$category]) ($($entries.Count) entries)"
}

# TODO: localization key expansion. For now we copy locales as-is so the mod
# can still load; entries reference inline locKeys until the expander is wired up.

# 4. Copy ModInfo, Preview, AddressableAssets verbatim
Copy-Item (Join-Path $ModSource 'ModInfo.json') "$BuildOut/" -Force
if (Test-Path (Join-Path $ModSource 'Preview.png')) {
    Copy-Item (Join-Path $ModSource 'Preview.png') "$BuildOut/" -Force
}
$AddressableSrc = Join-Path $ModSource 'Data/AddressableAssets'
if (Test-Path $AddressableSrc) {
    Copy-Item $AddressableSrc "$BuildOut/Data/" -Recurse -Force
    Write-Host "  Copied AddressableAssets/"
}

# 5. Deploy
if ($SkipDeploy) {
    Write-Step 'Skipping deploy'
    Write-Host "Build artefacts at: $BuildOut"
    return
}

Write-Step "Deploying to $DeployDest"
if (Test-Path $DeployDest) { Remove-Item $DeployDest -Recurse -Force }
New-Item -ItemType Directory -Force -Path $DeployDest | Out-Null
Copy-Item "$BuildOut/*" $DeployDest -Recurse -Force
Remove-Item $BuildOut -Recurse -Force

Write-Step 'Done'
Write-Host "Mod deployed. Launch Going Medieval and enable 'Religions Expanded'."
