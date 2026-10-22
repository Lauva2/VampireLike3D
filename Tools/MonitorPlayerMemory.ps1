param(
    [Parameter(Mandatory=$true)][string]$Evidence,
    [ValidateRange(1,60)][int]$SampleSeconds=45
)
$ErrorActionPreference='Stop'
$evidenceRoot=(Resolve-Path -LiteralPath $Evidence).Path
$identity=Get-Content -LiteralPath (Join-Path $evidenceRoot 'ProcessIdentity.json') -Raw | ConvertFrom-Json
$monitorStarted=[datetime]::UtcNow
while($true){
    $observedProcess=Get-Process -Id $identity.pid -ErrorAction SilentlyContinue
    if($null -eq $observedProcess){$reason='Original Player no longer exists';break}
    # The sampler checks both executable path and process start time before every append.
    & (Join-Path $PSScriptRoot 'SamplePlayerMemory.ps1') -Evidence $evidenceRoot
    if(Test-Path -LiteralPath (Join-Path $evidenceRoot 'Summary.json')){$reason='Player wrote a completion summary; original launcher separately observes its exit';break}
    Start-Sleep -Seconds $SampleSeconds
}
$rows=@(Import-Csv -LiteralPath (Join-Path $evidenceRoot 'ExternalMemory.csv'))
[ordered]@{
    startedUtc=$monitorStarted.ToString('O');finishedUtc=[datetime]::UtcNow.ToString('O')
    pid=$identity.pid;sampleIntervalSeconds=$SampleSeconds;samples=$rows.Count
    firstProcessElapsedSeconds=$rows[0].processElapsedSeconds;lastProcessElapsedSeconds=$rows[-1].processElapsedSeconds
    stoppedBecause=$reason;scope='Read-only Windows process memory observer; not a Player exit code, test pass or performance verdict'
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'MemoryMonitoring.json') -Encoding UTF8
Write-Output "Memory observation finished: $evidenceRoot ($($rows.Count) samples). No pass inferred."
