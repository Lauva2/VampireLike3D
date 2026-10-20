param(
    [ValidateSet('Generate','Check','Development','Release')]
    [string]$Mode = 'Development'
)
$ErrorActionPreference = 'Stop'
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$validationRoot = Join-Path $workspaceRoot '.build\Project'
$artifactsRoot = Join-Path $workspaceRoot 'Artifacts'
$unityPath = 'C:\Program Files\Unity 2022.3.62f2\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $unityPath)) { throw "Unity 2022.3.62f2 not found: $unityPath" }
New-Item -ItemType Directory -Path $validationRoot,$artifactsRoot -Force | Out-Null
foreach ($folderName in @('Assets','Packages','ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $workspaceRoot $folderName) -Destination $validationRoot -Recurse -Force
}
$method = switch ($Mode) {
    'Generate' { 'VampireLike.Editor.ProjectBuilder.Generate' }
    'Check' { 'VampireLike.Editor.ProjectChecks.Run' }
    'Development' { 'VampireLike.Editor.ProjectBuilder.VerifyAndBuild' }
    'Release' { 'VampireLike.Editor.ProjectBuilder.BuildRelease' }
}
$logPath = Join-Path $artifactsRoot ($Mode + '.log')
$arguments = @('-batchmode','-nographics','-quit','-projectPath',('"' + $validationRoot + '"'),'-executeMethod',$method,'-logFile',('"' + $logPath + '"'))
$buildProcess = Start-Process -FilePath $unityPath -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw "Unity failed with exit code $($buildProcess.ExitCode). See $logPath" }
if ($Mode -eq 'Generate') {
    Copy-Item -LiteralPath (Join-Path $validationRoot 'Assets') -Destination $workspaceRoot -Recurse -Force
    foreach ($settingsFile in @('ProjectSettings.asset','EditorBuildSettings.asset','TagManager.asset','DynamicsManager.asset')) {
        $sourcePath = Join-Path (Join-Path $validationRoot 'ProjectSettings') $settingsFile
        if (Test-Path -LiteralPath $sourcePath) { Copy-Item -LiteralPath $sourcePath -Destination (Join-Path (Join-Path $workspaceRoot 'ProjectSettings') $settingsFile) -Force }
    }
}
Copy-Item -LiteralPath (Join-Path $validationRoot 'Packages\packages-lock.json') -Destination (Join-Path $workspaceRoot 'Packages\packages-lock.json') -Force
Write-Output "$Mode completed. Log: $logPath"
