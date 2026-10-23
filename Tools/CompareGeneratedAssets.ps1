param([Parameter(Mandatory=$true)][string]$Output,[string]$Baseline='')
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
$cloneRoot=Join-Path $workspaceRoot '.build\Project'
$original=@{}
if($Baseline){foreach($entry in (Get-Content -LiteralPath (Join-Path $workspaceRoot $Baseline) -Raw | ConvertFrom-Json)){$original[$entry.path]=$entry.hash}}
$changedFiles=@(foreach($entry in Get-ChildItem -LiteralPath (Join-Path $cloneRoot 'Assets') -Recurse -File){
    if($entry.Extension -eq '.tmp'){continue}
    $relative=$entry.FullName.Substring($cloneRoot.Length+1)
    if($relative.StartsWith('Assets\AddressableAssetsData\Windows\') -or $relative.StartsWith('Assets\AddressableAssetsData\LinkerBuildProcessor\')){continue}
    $mainPath=Join-Path $workspaceRoot $relative
    $cloneHash=(Get-FileHash -LiteralPath $entry.FullName).Hash
    if(-not (Test-Path -LiteralPath $mainPath)){
        [pscustomobject]@{path=$relative;status='new';mainHash=$null;cloneHash=$cloneHash}
    }else{
        $mainHash=(Get-FileHash -LiteralPath $mainPath).Hash
        if($mainHash -ne $cloneHash){
            if($Baseline -and $original[$relative] -ne $mainHash){throw "Main project changed during generation; preserve it for review: $relative"}
            [pscustomobject]@{path=$relative;status='changed';mainHash=$mainHash;cloneHash=$cloneHash}
        }
    }
})
ConvertTo-Json -InputObject @($changedFiles) -Depth 4 | Set-Content -LiteralPath (Join-Path $workspaceRoot $Output) -Encoding UTF8
$changedFiles | Group-Object {$_.path.Split('\')[1]} | Select-Object Name,Count
Write-Output "Compared differences: $($changedFiles.Count)"
