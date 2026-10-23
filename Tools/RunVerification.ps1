param(
    [ValidateSet('Full','UI','UIFramework','Boss','Weapons','Weapons2','Weapons3','Terrain','Campaign','Soak')][string]$Stage='Full',
    [string]$Player='Builds\Development\WanXiangExpedition.exe',
    [string]$Evidence='',
    [ValidateRange(30,43200)][int]$SoakSeconds=7200,
    [string]$SoakUpgrades='',
    [ValidateSet(-1,60)][int]$SoakFrameRateLimit=60,
    [switch]$MultithreadedGraphics,
    [switch]$Background
)
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
if(-not [IO.Path]::IsPathRooted($Player)){$Player=Join-Path $workspaceRoot $Player}
$Player=(Resolve-Path -LiteralPath $Player).Path
$evidenceRoot=if($Evidence){Join-Path $workspaceRoot $Evidence}else{Join-Path $workspaceRoot ('Artifacts\'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+$Stage)}
New-Item -ItemType Directory -Path $evidenceRoot | Out-Null
$playerRoot=Split-Path -Parent $Player
if(Test-Path -LiteralPath (Join-Path $playerRoot 'SourceIdentity.json')){Copy-Item -LiteralPath (Join-Path $playerRoot 'SourceIdentity.json') -Destination (Join-Path $evidenceRoot 'SourceIdentity.json')}
$dataRoot=Join-Path $playerRoot (([IO.Path]::GetFileNameWithoutExtension($Player))+'_Data')
$identityFiles=@($Player,(Join-Path $dataRoot 'Managed\Assembly-CSharp.dll'))
$identityFiles+=@(Get-ChildItem -LiteralPath (Join-Path $dataRoot 'StreamingAssets') -File -Recurse | Select-Object -ExpandProperty FullName)
$fileHashes=@(foreach($identityPath in $identityFiles){
    [ordered]@{file=$identityPath.Substring($playerRoot.Length+1);sha256=(Get-FileHash -LiteralPath $identityPath -Algorithm SHA256).Hash}
})
[ordered]@{startedUtc=[DateTime]::UtcNow.ToString('O');stage=$Stage;player=$Player;desktopControl=$false;multithreadedGraphicsOverride=[bool]$MultithreadedGraphics;files=$fileHashes} |
    ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'BuildIdentity.json') -Encoding UTF8
$playerArguments=@('-batchmode','-screen-width','1600','-screen-height','900','--verify','--evidence',('"'+$evidenceRoot+'"'),'-logFile',('"'+(Join-Path $evidenceRoot 'Player.log')+'"'))
if($MultithreadedGraphics){$playerArguments+='-force-gfx-mt'}
if($SoakUpgrades){
    if($Stage -ne 'Soak'){throw 'SoakUpgrades is only for fixed-build diagnostic stress preflights'}
    $upgradePath=(Resolve-Path -LiteralPath $SoakUpgrades).Path
    $importedPath=Join-Path $evidenceRoot 'ImportedUpgradeBuild.json'
    Copy-Item -LiteralPath $upgradePath -Destination $importedPath
    [ordered]@{source=$upgradePath;sha256=(Get-FileHash -LiteralPath $importedPath).Hash;scope='Fixed diagnostic ability/upgrade/level/difficulty/quality input only; world keeps its new seed. Not a final continuously growing soak.'} |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'ImportedUpgradeSource.json') -Encoding UTF8
    $playerArguments+=@('--soak-upgrades',('"'+$importedPath+'"'))
}
switch($Stage){
    'Weapons3' {$playerArguments+='--weapon-batch-three-probe'}
    'Weapons2' {$playerArguments+='--weapon-batch-two-probe'}
    'Weapons' {$playerArguments+='--weapon-identity-probe'}
    'Boss' {$playerArguments+='--boss-entry-probe'}
    'UIFramework' {$playerArguments+='--ui-framework-probe'}
    'UI' {$playerArguments+='--ui-probe'}
    'Terrain' {$playerArguments+='--terrain-probe'}
    'Campaign' {$playerArguments+='--campaign'}
    'Soak' {$playerArguments+=@('--soak','--soak-seconds',$SoakSeconds.ToString(),'--soak-frame-rate',$SoakFrameRateLimit.ToString())}
}
$playerProcess=Start-Process -FilePath $Player -ArgumentList $playerArguments -WindowStyle Hidden -PassThru
[ordered]@{pid=$playerProcess.Id;startedUtc=$playerProcess.StartTime.ToUniversalTime().ToString('O');player=$Player;stage=$Stage;evidence=$evidenceRoot;desktopControl=$false;arguments=$playerArguments} |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'ProcessIdentity.json') -Encoding UTF8
Write-Output "Background Player PID: $($playerProcess.Id); evidence: $evidenceRoot"
if($Stage -eq 'Soak'){& (Join-Path $PSScriptRoot 'SamplePlayerMemory.ps1') -Evidence $evidenceRoot}
if($Background){return}
$playerProcess.WaitForExit()
    [ordered]@{pid=$playerProcess.Id;startedUtc=$playerProcess.StartTime.ToUniversalTime().ToString('O');player=$Player;observedExitUtc=[DateTime]::UtcNow.ToString('O');exitCode=$playerProcess.ExitCode;scope='Original launched process observed through WaitForExit; no desktop control'} |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'ProcessExit.json') -Encoding UTF8
if($playerProcess.ExitCode -ne 0){throw "Verification exited $($playerProcess.ExitCode). See $evidenceRoot"}
$resultPath=Join-Path $evidenceRoot $(if($Stage -in @('Campaign','Soak')){'Summary.json'}else{'Verification.json'})
$result=Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
if($Stage -eq 'Soak'){
    if($result.errorCount -ne 0 -or $result.wallSeconds -lt $SoakSeconds){throw "Soak did not finish successfully. See $resultPath"}
}elseif(-not $result.passed){throw "Verification did not pass. See $resultPath"}
Write-Output "Verified: $resultPath"
