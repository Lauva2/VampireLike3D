param(
    [string]$Version='1.0.0-rc',
    [string]$PlayerFolder='Builds\Windows',
    [string]$VerificationFolder='Artifacts\Round31',
    [string]$UIFolder='Artifacts\UI31',
    [string]$TerrainFolder='Artifacts\Probe31',
    [string]$CampaignFolder='Artifacts\Campaign31',
    [string]$SoakFolder='Artifacts\Soak07',
    [switch]$Final,
    [string]$ArtReviewRecord='Docs\ArtReviewResult.json'
)
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
if($Version -notmatch '^[a-zA-Z0-9._-]+$'){throw 'Version must contain only letters, digits, dots, underscores or hyphens'}
$packageRoot=Join-Path $workspaceRoot ('Builds\Packages\WanXiangExpedition-'+$Version)
if(Test-Path -LiteralPath $packageRoot){throw "Package already exists; use a new version: $packageRoot"}
$sourcePlayer=Join-Path $workspaceRoot $PlayerFolder
if(-not (Test-Path -LiteralPath (Join-Path $sourcePlayer 'WanXiangExpedition.exe'))){throw 'Formal configuration Player is missing'}
$reports=[ordered]@{}
foreach($entry in @(@('Full',$VerificationFolder,'Verification.json'),@('UI',$UIFolder,'Verification.json'),@('Terrain',$TerrainFolder,'Verification.json'),@('Campaign',$CampaignFolder,'Summary.json'),@('Soak',$SoakFolder,'Summary.json'))){
    $reportPath=Join-Path (Join-Path $workspaceRoot $entry[1]) $entry[2]
    $reports[$entry[0]]=if(Test-Path -LiteralPath $reportPath){Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json}else{$null}
}
foreach($key in @('Full','UI','Terrain')){if(-not $reports[$key].passed){throw "$key report has not passed"}}
$art=$null
if($Final){
    if(-not $reports.Campaign.passed){throw 'Final package requires a completed natural campaign'}
    if($reports.Soak.wallSeconds -lt 7200 -or $reports.Soak.errorCount -ne 0){throw 'Final package requires a completed 7200-second soak without runtime errors'}
    $soakErrors=Join-Path (Join-Path $workspaceRoot $SoakFolder) 'Errors.txt'
    if(-not (Test-Path -LiteralPath $soakErrors) -or (Get-Content -LiteralPath $soakErrors -Raw)){throw 'Missing or nonempty soak error report'}
    $art=Get-Content -LiteralPath (Join-Path $workspaceRoot $ArtReviewRecord) -Raw | ConvertFrom-Json
    if(-not $art.passed -or $art.total -lt 91 -or $art.candidate -ne (Split-Path -Leaf $VerificationFolder)){throw 'Independent art approval must pass for this exact candidate'}
    foreach($pair in @(@('A',13),@('B',13),@('C',18),@('D',18),@('E',18),@('F',8))){if($art.dimensions.($pair[0]) -lt $pair[1]){throw "Art dimension $($pair[0]) did not pass"}}
    $items=@($art.items.PSObject.Properties)
    if($items.Count -ne 20 -or @($items | Where-Object {$_.Value -lt 4 -or $_.Value -gt 5}).Count){throw 'Art approval must contain all twenty passing item scores'}
    if(($items | Measure-Object -Property Value -Sum).Sum -ne $art.total){throw 'Art item sum differs from the reported total'}
    $formalIdentity=Get-Content -LiteralPath (Join-Path $sourcePlayer 'SourceIdentity.json') -Raw | ConvertFrom-Json
    if($formalIdentity.development){throw 'Final package must use the formal Release configuration'}
    $sourceSignature=($formalIdentity.files | ForEach-Object {$_.path+':'+$_.sha256}) -join "`n"
    foreach($folder in @($VerificationFolder,$UIFolder,$TerrainFolder,$CampaignFolder,$SoakFolder)){
        $identity=Get-Content -LiteralPath (Join-Path (Join-Path $workspaceRoot $folder) 'SourceIdentity.json') -Raw | ConvertFrom-Json
        if((($identity.files | ForEach-Object {$_.path+':'+$_.sha256}) -join "`n") -ne $sourceSignature){throw "Evidence source differs from final build: $folder"}
    }
}
New-Item -ItemType Directory -Path $packageRoot | Out-Null
foreach($item in Get-ChildItem -LiteralPath $sourcePlayer){if($item.Name -ne 'Saves'){Copy-Item -LiteralPath $item.FullName -Destination $packageRoot -Recurse -Force}}
Copy-Item -LiteralPath (Join-Path $workspaceRoot 'Docs\PlayerGuide.md') -Destination (Join-Path $packageRoot '游玩说明.md')
New-Item -ItemType Directory -Path (Join-Path $packageRoot 'Licenses'),(Join-Path $packageRoot 'Verification') | Out-Null
Copy-Item -LiteralPath (Join-Path $workspaceRoot 'Assets\Art\UI\Fonts\OFL.txt') -Destination (Join-Path $packageRoot 'Licenses\Noto-Sans-CJK-OFL.txt')
foreach($docName in @('ImplementationStatus.md','ArtReview.md','ContentScope.md')){Copy-Item -LiteralPath (Join-Path $workspaceRoot ('Docs\'+$docName)) -Destination (Join-Path $packageRoot ('Verification\'+$docName))}
if($art){Copy-Item -LiteralPath (Join-Path $workspaceRoot $ArtReviewRecord) -Destination (Join-Path $packageRoot 'Verification\ArtReviewResult.json')}
foreach($entry in @(@('Full',$VerificationFolder),@('UI',$UIFolder),@('Terrain',$TerrainFolder),@('Campaign',$CampaignFolder),@('Soak',$SoakFolder))){
    $target=Join-Path $packageRoot ('Verification\'+$entry[0]);New-Item -ItemType Directory -Path $target | Out-Null
    foreach($reportName in @('BuildIdentity.json','SourceIdentity.json','Verification.json','Summary.json','Analysis.json','Errors.txt','Samples.csv','Progress.csv','WorkloadAnnotation.json')){
        $reportPath=Join-Path (Join-Path $workspaceRoot $entry[1]) $reportName
        if(Test-Path -LiteralPath $reportPath){Copy-Item -LiteralPath $reportPath -Destination $target}
    }
}
$hashes=@(foreach($item in Get-ChildItem -LiteralPath $packageRoot -Recurse -File){[ordered]@{path=$item.FullName.Substring($packageRoot.Length+1);bytes=$item.Length;sha256=(Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash}})
$gitPath='C:\Users\ivy\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\git\cmd\git.exe'
$sourceCommit=if(Test-Path -LiteralPath $gitPath){& $gitPath -C $workspaceRoot rev-parse HEAD}else{'unavailable'}
$sourceDirty=if(Test-Path -LiteralPath $gitPath){[bool](& $gitPath -C $workspaceRoot status --porcelain)}else{$true}
$packageStatus=if($Final){'VERIFIED_RELEASE'}else{'RELEASE_CANDIDATE_NOT_FINAL'}
$artStatus=if($Final){'Independent art '+$art.total+'/100; all original rubric gates passed'}else{'Pending independent score above 90 and all rubric gates'}
$manifest=[ordered]@{version=$Version;status=$packageStatus;createdUtc=[DateTime]::UtcNow.ToString('O');unity='2022.3.62f2';addressables='1.22.3';sourceCommit=$sourceCommit;sourceMayHaveChanges=$sourceDirty;artApproval=$artStatus;reports=$reports;files=$hashes}
$manifest | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath (Join-Path $packageRoot 'BuildManifest.json') -Encoding UTF8
if($Final){
@"
万象远征 $Version

运行 WanXiangExpedition.exe，操作与存档见 游玩说明.md。
独立美术评分 $($art.total)/100，通过原量表门槛。验证记录见 Verification。
"@ | Set-Content -LiteralPath (Join-Path $packageRoot '版本说明.txt') -Encoding UTF8
}else{
@"
万象远征 $Version · 发布候选

此包可独立运行，但尚未通过完整成品验收。
独立美术评分必须超过90且通过原量表门槛；详细状态见 Verification。
未完成的压力或评审项目不得视为通过。此标记不会因打包自动解除。

运行 WanXiangExpedition.exe，操作与存档见 游玩说明.md。
"@ | Set-Content -LiteralPath (Join-Path $packageRoot '候选版本说明.txt') -Encoding UTF8
}
$zipPath=$packageRoot+'.zip'
Compress-Archive -LiteralPath $packageRoot -DestinationPath $zipPath -CompressionLevel Optimal
Write-Output "Candidate package: $zipPath"
