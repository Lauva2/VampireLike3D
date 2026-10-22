param(
    [string]$Version='1.0.0-rc',
    [string]$PlayerFolder='Builds\Windows',
    [Parameter(Mandatory=$true)][string]$VerificationFolder,
    [Parameter(Mandatory=$true)][string]$UIFolder,
    [Parameter(Mandatory=$true)][string]$TerrainFolder,
    [string]$CampaignFolder='',
    [string]$SoakFolder='',
    [string]$FormalGameplayFolder='',
    [switch]$Final,
    [string]$ArtReviewRecord='Docs\ArtReviewResult.json'
)
$ErrorActionPreference='Stop'
$workspaceRoot=Split-Path -Parent $PSScriptRoot
if($Version -notmatch '^[a-zA-Z0-9._-]+$'){throw 'Version must contain only letters, digits, dots, underscores or hyphens'}
if($Final -and (-not $CampaignFolder -or -not $SoakFolder -or -not $FormalGameplayFolder)){throw 'Final requires explicit same-candidate CampaignFolder, SoakFolder and FormalGameplayFolder'}
$packageRoot=Join-Path $workspaceRoot ('Builds\Packages\WanXiangExpedition-'+$Version)
if(Test-Path -LiteralPath $packageRoot){throw "Package already exists; use a new version: $packageRoot"}
$sourcePlayer=Join-Path $workspaceRoot $PlayerFolder
if(-not (Test-Path -LiteralPath (Join-Path $sourcePlayer 'WanXiangExpedition.exe'))){throw 'Formal configuration Player is missing'}
$reports=[ordered]@{}
foreach($entry in @(@('Full',$VerificationFolder,'Verification.json'),@('UI',$UIFolder,'Verification.json'),@('Terrain',$TerrainFolder,'Verification.json'),@('Campaign',$CampaignFolder,'Summary.json'),@('Soak',$SoakFolder,'Summary.json'),@('Formal',$FormalGameplayFolder,'Summary.json'))){
    $reportPath=Join-Path (Join-Path $workspaceRoot $entry[1]) $entry[2]
    $reports[$entry[0]]=if(Test-Path -LiteralPath $reportPath){Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json}else{$null}
}
foreach($key in @('Full','UI','Terrain')){if(-not $reports[$key].passed){throw "$key report has not passed"}}
$art=$null
if($Final){
    if(-not $reports.Campaign.passed){throw 'Final package requires a completed natural campaign'}
    if(-not $reports.Formal.passed -or $reports.Formal.development -or $reports.Formal.diagnosticMode){throw 'Final package requires completed instrumented formal gameplay and restart with diagnostics disabled'}
    $formalInstrumentation=Get-Content -LiteralPath (Join-Path (Join-Path $workspaceRoot $FormalGameplayFolder) 'Instrumentation.json') -Raw | ConvertFrom-Json
    foreach($entry in $formalInstrumentation.productionFilesUnchanged){if((Get-FileHash -LiteralPath (Join-Path $sourcePlayer $entry.path)).Hash -ne $entry.sha256){throw "Formal gameplay evidence differs from the distribution: $($entry.path)"}}
    if($reports.Soak.wallSeconds -lt 7200 -or $reports.Soak.errorCount -ne 0){throw 'Final package requires a completed 7200-second soak without runtime errors'}
    $soakRoot=Join-Path $workspaceRoot $SoakFolder
    if($reports.Soak.requestedSeconds -lt 7200 -or -not $reports.Soak.renderEveryFrame){throw 'Final soak must request continuous two hours with real rendering every frame'}
    foreach($termination in @('Interrupted.json','Cancelled.json')){if(Test-Path -LiteralPath (Join-Path $soakRoot $termination)){throw 'An interrupted or cancelled soak cannot certify final release'}}
    $soakExit=Get-Content -LiteralPath (Join-Path $soakRoot 'ProcessExit.json') -Raw | ConvertFrom-Json
    if($null -eq $soakExit.exitCode -or $soakExit.exitCode -ne 0){throw 'Final soak requires the original launched process observed exiting successfully'}
    if($reports.Soak.fixedImportedBuild){throw 'A fixed imported diagnostic preflight cannot replace the continuously growing final soak'}
    $soakAcceptance=Get-Content -LiteralPath (Join-Path (Join-Path $workspaceRoot $SoakFolder) 'Acceptance.json') -Raw | ConvertFrom-Json
    if(-not $soakAcceptance.passed -or $soakAcceptance.candidate -ne (Split-Path -Leaf $VerificationFolder)){
        throw 'Final package requires an explicit same-candidate performance, memory and resource-lifecycle verdict; duration alone is insufficient'
    }
    $soakErrors=Join-Path (Join-Path $workspaceRoot $SoakFolder) 'Errors.txt'
    if(-not (Test-Path -LiteralPath $soakErrors) -or (Get-Content -LiteralPath $soakErrors -Raw)){throw 'Missing or nonempty soak error report'}
    $art=Get-Content -LiteralPath (Join-Path $workspaceRoot $ArtReviewRecord) -Raw | ConvertFrom-Json
    if(-not $art.passed -or $art.total -lt 91 -or $art.candidate -ne (Split-Path -Leaf $VerificationFolder)){throw 'Independent art approval must pass for this exact candidate'}
    foreach($pair in @(@('A',13),@('B',13),@('C',18),@('D',18),@('E',18),@('F',8))){if($art.dimensions.($pair[0]) -lt $pair[1]){throw "Art dimension $($pair[0]) did not pass"}}
    $items=if($art.items -is [array]){@($art.items)}else{@($art.items.PSObject.Properties | ForEach-Object {[pscustomobject]@{id=$_.Name;score=$_.Value}})}
    $requiredItems=@('A1','A2','A3','B1','B2','B3','C1','C2','C3','C4','D1','D2','D3','D4','E1','E2','E3','E4','F1','F2')
    if($items.Count -ne 20 -or @($items | Where-Object {$_.score -lt 4 -or $_.score -gt 5}).Count -or (Compare-Object ($items.id | Sort-Object) ($requiredItems | Sort-Object))){throw 'Art approval must contain all twenty original passing item scores'}
    if(($items | Measure-Object -Property score -Sum).Sum -ne $art.total){throw 'Art item sum differs from the reported total'}
    foreach($dimension in @('A','B','C','D','E','F')){if(($items | Where-Object {$_.id.StartsWith($dimension)} | Measure-Object -Property score -Sum).Sum -ne $art.dimensions.$dimension){throw "Art dimension $dimension differs from its original items"}}
    $requiredArtGates=@('total','dimensions','items','frozen_content','finished_assets','chinese_layout','dense_readability','danger_and_invulnerability','continuous_feedback','same_candidate')
    if(@($art.hardGates).Count -ne $requiredArtGates.Count -or @($art.hardGates | Where-Object {-not $_.passed}).Count -or (Compare-Object ($art.hardGates.id | Sort-Object) ($requiredArtGates | Sort-Object))){throw 'All original independent art gates must be explicitly passed'}
    $formalIdentity=Get-Content -LiteralPath (Join-Path $sourcePlayer 'SourceIdentity.json') -Raw | ConvertFrom-Json
    if($formalIdentity.development){throw 'Final package must use the formal Release configuration'}
    $sourceSignature=($formalIdentity.files | ForEach-Object {$_.path+':'+$_.sha256}) -join "`n"
    foreach($folder in @($VerificationFolder,$UIFolder,$TerrainFolder,$CampaignFolder,$SoakFolder,$FormalGameplayFolder)){
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
if(Test-Path -LiteralPath (Join-Path $workspaceRoot 'Docs\ReleaseAcceptance.md')){Copy-Item -LiteralPath (Join-Path $workspaceRoot 'Docs\ReleaseAcceptance.md') -Destination (Join-Path $packageRoot 'Verification\ReleaseAcceptance.md')}
if($art){Copy-Item -LiteralPath (Join-Path $workspaceRoot $ArtReviewRecord) -Destination (Join-Path $packageRoot 'Verification\ArtReviewResult.json')}
if($SoakFolder){
    $soakReview=Join-Path $workspaceRoot ('Docs\'+(Split-Path -Leaf $SoakFolder)+'Review.md')
    if(Test-Path -LiteralPath $soakReview){Copy-Item -LiteralPath $soakReview -Destination (Join-Path $packageRoot 'Verification\SoakReview.md')}
}
if($art){
    $artReports=Join-Path $workspaceRoot ('Docs\ArtReviews\'+$art.candidate)
    $artTarget=Join-Path $packageRoot 'Verification\IndependentArt'
    New-Item -ItemType Directory -Path $artTarget | Out-Null
    foreach($name in @('IndependentReview.md','ObservedEvidence.json','SoakFinalFrameReview.md')){
        $report=Join-Path $artReports $name
        if(Test-Path -LiteralPath $report){Copy-Item -LiteralPath $report -Destination $artTarget}
    }
}
foreach($entry in @(@('Full',$VerificationFolder),@('UI',$UIFolder),@('Terrain',$TerrainFolder),@('Campaign',$CampaignFolder),@('Soak',$SoakFolder),@('Formal',$FormalGameplayFolder))){
    $target=Join-Path $packageRoot ('Verification\'+$entry[0]);New-Item -ItemType Directory -Path $target | Out-Null
    foreach($reportName in @('BuildIdentity.json','SourceIdentity.json','ProcessIdentity.json','ProcessExit.json','Verification.json','Summary.json','Analysis.json','Acceptance.json','Errors.txt','Samples.csv','ExternalMemory.csv','MemoryMonitoring.json','CurrentBuild.json','BuildSamples.jsonl','Progress.csv','Instrumentation.json','WorkloadAnnotation.json','FinalFrame.png')){
        $reportPath=Join-Path (Join-Path $workspaceRoot $entry[1]) $reportName
        if(Test-Path -LiteralPath $reportPath){Copy-Item -LiteralPath $reportPath -Destination $target}
    }
    if($entry[0] -eq 'Formal'){
        foreach($phase in @('Phase1','Phase2')){if(Test-Path -LiteralPath (Join-Path (Join-Path $workspaceRoot $entry[1]) $phase)){Copy-Item -LiteralPath (Join-Path (Join-Path $workspaceRoot $entry[1]) $phase) -Destination $target -Recurse}}
    }
}
$hashes=@(foreach($item in Get-ChildItem -LiteralPath $packageRoot -Recurse -File){[ordered]@{path=$item.FullName.Substring($packageRoot.Length+1);bytes=$item.Length;sha256=(Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash}})
$gitCommand=Get-Command git -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
$sourceCommit='unavailable';$sourceDirty=$true
if($gitCommand){
    $gitSafeDirectory=$workspaceRoot.Replace('\','/')
    $commit=& $gitCommand.Source -c "safe.directory=$gitSafeDirectory" -C $workspaceRoot rev-parse HEAD
    if($LASTEXITCODE -eq 0){
        $sourceCommit=$commit
        $changes=& $gitCommand.Source -c "safe.directory=$gitSafeDirectory" -C $workspaceRoot status --porcelain
        if($LASTEXITCODE -eq 0){$sourceDirty=[bool]$changes}
    }
}
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
