param(
    [string]$PlayerFolder='Builds\Windows',
    [string]$ColdFolder='',
    [string]$Evidence='',
    [string]$UnityPath=$env:UNITY_EDITOR_PATH,
    [switch]$PrepareOnly,
    [switch]$RunPrepared
)
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'
if(-not $ColdFolder){$ColdFolder='Builds\FormalGameplay-'+$stamp}
if(-not $Evidence){$Evidence='Artifacts\FormalGameplay-'+$stamp}
$source=Join-Path $workspaceRoot $PlayerFolder
$cold=[IO.Path]::GetFullPath((Join-Path $workspaceRoot $ColdFolder))
$output=[IO.Path]::GetFullPath((Join-Path $workspaceRoot $Evidence))
if(-not $cold.StartsWith($workspaceRoot+'\',[StringComparison]::OrdinalIgnoreCase) -or -not $output.StartsWith($workspaceRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Validation paths must stay within the workspace'}
$dataName='WanXiangExpedition_Data'
$data=Join-Path $cold $dataName
$overlayFiles=@("$dataName\ScriptingAssemblies.json","$dataName\RuntimeInitializeOnLoads.json")
if(-not $RunPrepared){
    if((Test-Path $cold) -or (Test-Path $output)){throw 'Use fresh validation and evidence directories'}
    $identity=Get-Content (Join-Path $source 'SourceIdentity.json') -Raw|ConvertFrom-Json
    if($identity.development){throw 'A formal Release Player is required'}
    Copy-Item -LiteralPath $source -Destination $cold -Recurse
    if(Test-Path (Join-Path $cold 'Saves')){throw 'Formal test must start without saves'}
    New-Item -ItemType Directory -Path $output | Out-Null
    $managed=Join-Path $data 'Managed'
    $harness=Join-Path $managed 'FormalFlowHarness.dll'
    if(-not $UnityPath){
        $UnityPath=@((Join-Path $workspaceRoot '.build\UnityEditor\Editor\Unity.exe'),'C:\Program Files\Unity 2022.3.62f2\Editor\Unity.exe','C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe') | Where-Object {Test-Path -LiteralPath $_} | Select-Object -First 1
    }
    if(-not $UnityPath -or -not(Test-Path -LiteralPath $UnityPath)){throw 'Pass -UnityPath <2022.3.62f2 Editor\Unity.exe> or set UNITY_EDITOR_PATH'}
    $UnityPath=(Resolve-Path -LiteralPath $UnityPath).Path
    if((Get-Item -LiteralPath $UnityPath).VersionInfo.ProductVersion -notmatch '^2022\.3\.62f2(?:\b|_)'){throw 'External harness compilation requires Unity 2022.3.62f2'}
    $editorRoot=Split-Path -Parent $UnityPath
    $mono=Join-Path $editorRoot 'Data\MonoBleedingEdge\bin\mono.exe'
    $compiler=Join-Path $editorRoot 'Data\MonoBleedingEdge\lib\mono\4.5\csc.exe'
    $references=@(Get-ChildItem -LiteralPath $managed -Filter '*.dll' | ForEach-Object {'/reference:'+$_.FullName})
    & $mono $compiler /noconfig /nostdlib+ /target:library /langversion:latest ('/out:'+$harness) $references (Join-Path $PSScriptRoot 'Diagnostics\FormalFlowHarness.cs') 2>&1 | Tee-Object -FilePath (Join-Path $output 'HarnessCompile.txt')
    if($LASTEXITCODE -ne 0){throw 'External harness compilation failed'}
    $assemblies=Get-Content (Join-Path $data 'ScriptingAssemblies.json') -Raw | ConvertFrom-Json
    $assemblies.names+=@('FormalFlowHarness.dll');$assemblies.types+=@(16)
    $assemblies | ConvertTo-Json -Depth 6 -Compress | Set-Content (Join-Path $data 'ScriptingAssemblies.json') -Encoding UTF8
    $initializers=Get-Content (Join-Path $data 'RuntimeInitializeOnLoads.json') -Raw | ConvertFrom-Json
    $initializers.root+=@([ordered]@{assemblyName='FormalFlowHarness';nameSpace='ExternalValidation';className='FormalFlowBootstrap';methodName='Begin';loadTypes=1;isUnityClass=$false})
    $initializers | ConvertTo-Json -Depth 6 -Compress | Set-Content (Join-Path $data 'RuntimeInitializeOnLoads.json') -Encoding UTF8
    $unchanged=@();$modified=@()
    foreach($file in Get-ChildItem -LiteralPath $source -Recurse -File){
        $relative=$file.FullName.Substring($source.Length+1)
        $before=(Get-FileHash -LiteralPath $file.FullName).Hash;$after=(Get-FileHash -LiteralPath (Join-Path $cold $relative)).Hash
        $entry=[ordered]@{path=$relative;sha256=$before;observedSha256=$after}
        if($relative -in $overlayFiles){$modified+=@($entry)}elseif($before -ne $after){throw "A production file was changed: $relative"}else{$unchanged+=@($entry)}
    }
    [ordered]@{formalSource=$source;isolatedPlayer=$cold;createdUtc=[datetime]::UtcNow.ToString('O');desktopControl=$false;scope='External input/observation harness added to an isolated formal Player copy. All original files except two bootstrap registration JSON files are byte-identical, including game assemblies, AA catalog and bundles. The distributable is not modified. This is instrumented formal gameplay, not pristine/manual gameplay.';productionFilesUnchanged=$unchanged;bootstrapRegistrationChanges=$modified;addedHarnessSha256=(Get-FileHash $harness).Hash;harnessSourceSha256=(Get-FileHash (Join-Path $PSScriptRoot 'Diagnostics\FormalFlowHarness.cs')).Hash} | ConvertTo-Json -Depth 7 | Set-Content (Join-Path $output 'Instrumentation.json') -Encoding UTF8
    Copy-Item (Join-Path $source 'SourceIdentity.json') $output
}
if($PrepareOnly){Write-Output "Prepared external formal flow validation: $cold";return}
$instrumentation=Get-Content (Join-Path $output 'Instrumentation.json') -Raw|ConvertFrom-Json
foreach($entry in $instrumentation.productionFilesUnchanged){if((Get-FileHash -LiteralPath (Join-Path $cold $entry.path)).Hash -ne $entry.sha256){throw "Production identity changed before testing: $($entry.path)"}}
$attempts=@()
foreach($phase in 1,2){
    $phaseRoot=Join-Path $output ('Phase'+$phase);New-Item -ItemType Directory -Path $phaseRoot | Out-Null
    $launchArguments=@('-batchmode','--portable','--evidence',('"'+$phaseRoot+'"'),'-logFile',('"'+(Join-Path $phaseRoot 'Player.log')+'"'))
    if($phase -eq 2){$launchArguments+='--formal-restart'}
    $process=Start-Process -FilePath (Join-Path $cold 'WanXiangExpedition.exe') -ArgumentList $launchArguments -WindowStyle Hidden -PassThru
    [ordered]@{pid=$process.Id;startedUtc=$process.StartTime.ToUniversalTime().ToString('O');arguments=$launchArguments;desktopControl=$false}|ConvertTo-Json|Set-Content (Join-Path $phaseRoot 'ProcessIdentity.json') -Encoding UTF8
    try{
        $deadline=[datetime]::UtcNow.AddSeconds($(if($phase -eq 1){1850}else{150}));$nextStatus=[datetime]::UtcNow
        while(-not $process.HasExited){
            if([datetime]::UtcNow -gt $deadline){throw "Formal phase $phase timed out"}
            if([datetime]::UtcNow -gt $nextStatus){$nextStatus=[datetime]::UtcNow.AddSeconds(45);if(Test-Path (Join-Path $phaseRoot 'Progress.json')){Write-Output (Get-Content (Join-Path $phaseRoot 'Progress.json') -Raw)}}
            Start-Sleep -Milliseconds 500
        }
        $process.WaitForExit()
        $report=Get-Content (Join-Path $phaseRoot 'Summary.json') -Raw|ConvertFrom-Json
        if($process.ExitCode -ne 0 -or -not $report.passed -or $report.development -or $report.diagnosticMode -or $report.errorCount -ne 0){throw "Formal phase $phase failed"}
        $attempts+=@($report)
    }finally{if(-not $process.HasExited){Stop-Process -Id $process.Id}}
}
foreach($imageName in @('natural-combat.png','voluntary-boss.png','boss-victory.png','settlement-1.png')){if(-not(Test-Path (Join-Path $output ('Phase1\'+$imageName)))){throw "Missing formal original render: $imageName"}}
if(-not(Test-Path (Join-Path $output 'Phase2\restored-desert-run.png'))){throw 'Missing restored next-biome render'}
[ordered]@{passed=$true;finishedUtc=[datetime]::UtcNow.ToString('O');desktopControl=$false;development=$false;diagnosticMode=$false;phases=$attempts;scope=$instrumentation.scope}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $output 'Summary.json') -Encoding UTF8
Write-Output "Instrumented formal gameplay and restart passed: $output"
