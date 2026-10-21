param([Parameter(Mandatory=$true)][ValidatePattern('^[a-zA-Z0-9._-]+$')][string]$Version)
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
$outputRoot=Join-Path $workspaceRoot 'Builds\SourceArchives'
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$zipPath=Join-Path $outputRoot ('WanXiangExpedition-Source-'+$Version+'.zip')
if(Test-Path -LiteralPath $zipPath){throw "Archive already exists: $zipPath"}
$files=@(foreach($name in @('Assets','Packages','ProjectSettings','Docs','Tools')){
    Get-ChildItem -LiteralPath (Join-Path $workspaceRoot $name) -Recurse -File | Where-Object {
        $_.FullName -notlike '*\Assets\AddressableAssetsData\Windows\*' -and
        $_.FullName -notlike '*\Assets\AddressableAssetsData\LinkerBuildProcessor\*' -and
        $_.FullName -notlike '*\__pycache__\*'
    }
})
foreach($name in @('AGENTS.md','README.md','.gitignore')){if(Test-Path -LiteralPath (Join-Path $workspaceRoot $name)){$files+=Get-Item -LiteralPath (Join-Path $workspaceRoot $name)}}
$hashes=@(foreach($file in $files | Sort-Object FullName){[ordered]@{path=$file.FullName.Substring($workspaceRoot.Length+1).Replace('\','/');bytes=$file.Length;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}})
$manifest=[ordered]@{version=$Version;createdUtc=[DateTime]::UtcNow.ToString('O');scope='Editable Unity project with metadata, source, configs, artwork, package lock, documentation and reproducible tools. Excludes caches and Player artifacts. This snapshot does not itself certify release readiness.';files=$hashes}
Add-Type -AssemblyName System.IO.Compression
$stream=[IO.File]::Open($zipPath,[IO.FileMode]::CreateNew)
try{
    $archive=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create,$true)
    try{
        foreach($file in $files){
            $relative=$file.FullName.Substring($workspaceRoot.Length+1).Replace('\','/')
            $entry=$archive.CreateEntry($relative,[IO.Compression.CompressionLevel]::Optimal)
            $inputStream=[IO.File]::OpenRead($file.FullName);$entryStream=$entry.Open()
            try{$inputStream.CopyTo($entryStream)}finally{$entryStream.Dispose();$inputStream.Dispose()}
        }
        $entry=$archive.CreateEntry('SourceArchiveManifest.json');$writer=[IO.StreamWriter]::new($entry.Open(),[Text.UTF8Encoding]::new($false))
        try{$writer.Write(($manifest | ConvertTo-Json -Depth 8))}finally{$writer.Dispose()}
    }finally{$archive.Dispose()}
}finally{$stream.Dispose()}
$verification=[IO.Compression.ZipFile]::OpenRead($zipPath)
try{
    if($verification.Entries.Count -ne $files.Count+1){throw 'Archive entry count mismatch'}
    foreach($required in @('Assets/Scenes/Boot.unity','Packages/manifest.json','Packages/packages-lock.json','ProjectSettings/ProjectVersion.txt','AGENTS.md')){
        if($null -eq $verification.GetEntry($required)){throw "Archive missing $required"}
    }
}finally{$verification.Dispose()}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath ($zipPath+'.manifest.json') -Encoding UTF8
Write-Output "Source snapshot: $zipPath ($($files.Count) files)"
