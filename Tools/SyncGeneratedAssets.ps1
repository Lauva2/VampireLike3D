param([string]$Diff='Artifacts\Round28AssetDiff.json')
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
$cloneRoot=Join-Path $workspaceRoot '.build\Project'
$entries=ConvertFrom-Json -InputObject ([IO.File]::ReadAllText((Join-Path $workspaceRoot $Diff)))
$allowed=@('Assets\Art\Audio\Generated\','Assets\Art\Materials\Generated\','Assets\Art\Models\Generated\','Assets\Art\UI\Generated\','Assets\Art\Prefabs\','Assets\Scenes\','Assets\AddressableAssetsData\AssetGroups\')
$allowedFiles=@('Assets\AddressableAssetsData\AddressableAssetSettings.asset','Assets\AddressableAssetsData\Windows.meta','Assets\AddressableAssetsData\link.xml','Assets\AddressableAssetsData\link.xml.meta','Assets\Config\Catalogs\GameCatalog.asset')
$selected=@(foreach($entry in $entries){
    if($entry.path.StartsWith('Assets\AddressableAssetsData\Windows\')){continue}
    $isAllowed=$entry.path -in $allowedFiles
    foreach($prefix in $allowed){if($entry.path.StartsWith($prefix,[StringComparison]::Ordinal)){$isAllowed=$true}}
    if(-not $isAllowed){throw "Not a generated asset: $($entry.path)"}
    $destination=[IO.Path]::GetFullPath((Join-Path $workspaceRoot $entry.path))
    $source=[IO.Path]::GetFullPath((Join-Path $cloneRoot $entry.path))
    if(-not $destination.StartsWith($workspaceRoot+'\',[StringComparison]::OrdinalIgnoreCase) -or -not $source.StartsWith($cloneRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Path escaped workspace'}
    if((Get-FileHash -LiteralPath $source).Hash -ne $entry.cloneHash){throw "Clone changed after comparison: $($entry.path)"}
    if($entry.status -eq 'new'){
        if(Test-Path -LiteralPath $destination){throw "New destination already exists: $($entry.path)"}
    }elseif((Get-FileHash -LiteralPath $destination).Hash -ne $entry.mainHash){throw "Main file changed after comparison: $($entry.path)"}
    [pscustomobject]@{entry=$entry;source=$source;destination=$destination}
})
$backupRoot=Join-Path $workspaceRoot ('Artifacts\GeneratedBackup-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $backupRoot | Out-Null
$entries | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $backupRoot 'Manifest.json') -Encoding UTF8
# Preserve every existing target before any replacement. Source code is excluded.
foreach($file in $selected){if($file.entry.status -ne 'new'){
    $backup=Join-Path $backupRoot $file.entry.path
    New-Item -ItemType Directory -Path (Split-Path -Parent $backup) -Force | Out-Null
    Copy-Item -LiteralPath $file.destination -Destination $backup
}}
foreach($file in $selected){
    New-Item -ItemType Directory -Path (Split-Path -Parent $file.destination) -Force | Out-Null
    # A running Editor can map an asset read-only: truncating it fails, but atomic replacement is safe.
    $staged=$file.destination+'.generated-'+[Guid]::NewGuid().ToString('N')+'.tmp'
    Copy-Item -LiteralPath $file.source -Destination $staged
    if((Get-FileHash -LiteralPath $staged).Hash -ne $file.entry.cloneHash){throw "Staging verification failed: $($file.entry.path)"}
    if(Test-Path -LiteralPath $file.destination){[IO.File]::Replace($staged,$file.destination,(Join-Path $backupRoot ($file.entry.path+'.replaced')))}
    else{[IO.File]::Move($staged,$file.destination)}
    if((Get-FileHash -LiteralPath $file.destination).Hash -ne $file.entry.cloneHash){throw "Copy verification failed: $($file.entry.path)"}
}
Write-Output "Synced $($selected.Count) compared generated files; originals preserved in $backupRoot"
