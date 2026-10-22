param(
    [string]$PlayerFolder='Builds\Windows',
    [string]$ColdFolder='',
    [string]$Evidence=''
)
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'
if(-not $ColdFolder){$ColdFolder='Builds\ColdSmoke-'+$stamp}
if(-not $Evidence){$Evidence='Artifacts\ReleaseSmoke-'+$stamp}
$source=Join-Path $workspaceRoot $PlayerFolder
$cold=[IO.Path]::GetFullPath((Join-Path $workspaceRoot $ColdFolder))
$output=[IO.Path]::GetFullPath((Join-Path $workspaceRoot $Evidence))
if(-not $cold.StartsWith($workspaceRoot+'\',[StringComparison]::OrdinalIgnoreCase) -or -not $output.StartsWith($workspaceRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Smoke paths must stay within workspace'}
if((Test-Path -LiteralPath $cold) -or (Test-Path -LiteralPath $output)){throw 'Use fresh cold-player and evidence folders'}
$identity=Get-Content -LiteralPath (Join-Path $source 'SourceIdentity.json') -Raw|ConvertFrom-Json
if($identity.development){throw 'Smoke requires a formal Release Player'}
Copy-Item -LiteralPath $source -Destination $cold -Recurse
New-Item -ItemType Directory -Path $output|Out-Null
Copy-Item -LiteralPath (Join-Path $source 'SourceIdentity.json') -Destination $output
$exe=Join-Path $cold 'WanXiangExpedition.exe'
$attempts=@()
foreach($attempt in 1,2){
    $log=Join-Path $output ('Startup'+$attempt+'.log')
    $launchArguments=@('-batchmode','--portable','--verify','--evidence',('"'+$output+'"'),'-logFile',('"'+$log+'"'))
    $process=Start-Process -FilePath $exe -ArgumentList $launchArguments -WindowStyle Hidden -PassThru
    $started=$process.StartTime.ToUniversalTime().ToString('O')
    try{
        $deadline=[DateTime]::UtcNow.AddSeconds(45);$ready=$false
        while([DateTime]::UtcNow -lt $deadline -and -not $process.HasExited){
            $text=if(Test-Path -LiteralPath $log){Get-Content -LiteralPath $log -Raw}else{''}
            if($text -match 'MAIN_MENU_READY.*development=False' -and (Test-Path -LiteralPath (Join-Path $cold 'Saves\profile-v1.json'))){$ready=$true;break}
            Start-Sleep -Milliseconds 250
        }
        if(-not $ready){throw "Formal startup $attempt failed; see $log"}
        if($text -match '(?m)^(NullReferenceException|InvalidOperationException|Exception:|Error:|Addressables.*Exception)'){throw "Runtime error in $log"}
        if(Test-Path -LiteralPath (Join-Path $output 'RuntimeStatus.json')){throw 'Formal Player unexpectedly enabled diagnostic mode'}
        $attempts+=@{attempt=$attempt;pid=$process.Id;startedUtc=$started;mainMenuReady=$ready;portableProfileExists=$true;diagnosticModeEnabled=$false}
    }finally{
        # This process object came directly from the isolated launch above; never target an existing Editor or Player.
        if(-not $process.HasExited){Stop-Process -Id $process.Id}
    }
}
[ordered]@{passed=$true;finishedUtc=[DateTime]::UtcNow.ToString('O');player=$exe;sha256=(Get-FileHash -LiteralPath $exe).Hash;desktopControl=$false;freshColdFolder=$true;attempts=$attempts;scope='Formal local-AA startup and portable-profile restart only; no gameplay input or full gameplay claim.'}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $output 'Summary.json') -Encoding utf8
Write-Output "Formal startup/restart smoke passed: $output"
