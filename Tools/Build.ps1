param(
    [ValidateSet('Generate','Check','Development','GenerateDevelopment','Release')]
    [string]$Mode = 'Development',
    [string]$UnityPath = $env:UNITY_EDITOR_PATH
)
$ErrorActionPreference = 'Stop'
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$validationRoot = Join-Path $workspaceRoot '.build\Project'
$artifactsRoot = Join-Path $workspaceRoot 'Artifacts'
if (-not $UnityPath) {
    $candidates = @(
        (Join-Path $workspaceRoot '.build\UnityEditor\Editor\Unity.exe'),
        'C:\Program Files\Unity 2022.3.62f2\Editor\Unity.exe',
        'C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe'
    )
    $UnityPath = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $UnityPath -or -not (Test-Path -LiteralPath $UnityPath)) {
    throw 'Unity 2022.3.62f2 not found. Pass -UnityPath <Editor\Unity.exe> or set UNITY_EDITOR_PATH.'
}
$UnityPath = (Resolve-Path -LiteralPath $UnityPath).Path
$editorVersion = (Get-Item -LiteralPath $UnityPath).VersionInfo.ProductVersion
if ($editorVersion -notmatch '^2022\.3\.62f2(?:\b|_)') {
    throw "Expected Unity 2022.3.62f2, found '$editorVersion': $UnityPath"
}
New-Item -ItemType Directory -Path $validationRoot,$artifactsRoot -Force | Out-Null
$generationBaseline=@{}
$baselineRelative=''
if($Mode -in @('Generate','GenerateDevelopment')){
    $baselineRelative='Artifacts\GenerationInput-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.json'
    $entries=@(foreach($item in Get-ChildItem -LiteralPath (Join-Path $workspaceRoot 'Assets') -Recurse -File){
        $relative=$item.FullName.Substring($workspaceRoot.Length+1);$hash=(Get-FileHash -LiteralPath $item.FullName).Hash
        $generationBaseline[$relative]=$hash;[ordered]@{path=$relative;hash=$hash}
    })
    $entries | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $workspaceRoot $baselineRelative) -Encoding UTF8
    foreach($name in @('ProjectSettings.asset','EditorBuildSettings.asset','TagManager.asset','DynamicsManager.asset')){
        $relative='ProjectSettings\'+$name;$path=Join-Path $workspaceRoot $relative
        if(Test-Path -LiteralPath $path){$generationBaseline[$relative]=(Get-FileHash -LiteralPath $path).Hash}
    }
}
foreach ($folderName in @('Assets','Packages','ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $workspaceRoot $folderName) -Destination $validationRoot -Recurse -Force
}
$method = switch ($Mode) {
    'Generate' { 'VampireLike.Editor.ProjectBuilder.Generate' }
    'Check' { 'VampireLike.Editor.ProjectChecks.Run' }
    'Development' { 'VampireLike.Editor.ProjectBuilder.VerifyAndBuild' }
    'GenerateDevelopment' { 'VampireLike.Editor.ProjectBuilder.GenerateVerifyAndBuild' }
    'Release' { 'VampireLike.Editor.ProjectBuilder.BuildRelease' }
}
$logPath = Join-Path $artifactsRoot ($Mode + '.log')
$arguments = @('-batchmode','-nographics','-quit','-projectPath',('"' + $validationRoot + '"'),'-executeMethod',$method,'-logFile',('"' + $logPath + '"'))
$buildProcess = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw "Unity failed with exit code $($buildProcess.ExitCode). See $logPath" }
if ($Mode -in @('Generate','GenerateDevelopment')) {
    $diffRelative='Artifacts\GeneratedSync-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.json'
    & (Join-Path $PSScriptRoot 'CompareGeneratedAssets.ps1') -Output $diffRelative -Baseline $baselineRelative
    & (Join-Path $PSScriptRoot 'SyncGeneratedAssets.ps1') -Diff $diffRelative
    foreach ($settingsFile in @('ProjectSettings.asset','EditorBuildSettings.asset','TagManager.asset','DynamicsManager.asset')) {
        $sourcePath = Join-Path (Join-Path $validationRoot 'ProjectSettings') $settingsFile
        $relative='ProjectSettings\'+$settingsFile;$mainPath=Join-Path $workspaceRoot $relative
        if(Test-Path -LiteralPath $sourcePath){
            $sourceHash=(Get-FileHash -LiteralPath $sourcePath).Hash
            $mainHash=if(Test-Path -LiteralPath $mainPath){(Get-FileHash -LiteralPath $mainPath).Hash}else{$null}
            if($sourceHash -eq $mainHash){continue}
            if($mainHash -ne $generationBaseline[$relative]){throw "Main settings changed during generation: $relative"}
            if($mainHash){
                $backup=Join-Path $artifactsRoot ('SettingsBackup-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+$settingsFile)
                Copy-Item -LiteralPath $mainPath -Destination $backup
            }
            Copy-Item -LiteralPath $sourcePath -Destination $mainPath -Force
        }
    }
}
Copy-Item -LiteralPath (Join-Path $validationRoot 'Packages\packages-lock.json') -Destination (Join-Path $workspaceRoot 'Packages\packages-lock.json') -Force
Write-Output "$Mode completed. Log: $logPath"
