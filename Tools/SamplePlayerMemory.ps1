param([Parameter(Mandatory=$true)][string]$Evidence)
$ErrorActionPreference='Stop'
$evidenceRoot=(Resolve-Path -LiteralPath $Evidence).Path
$identity=Get-Content -LiteralPath (Join-Path $evidenceRoot 'ProcessIdentity.json') -Raw | ConvertFrom-Json
$measuredProcess=Get-Process -Id $identity.pid -ErrorAction SilentlyContinue
if($null -eq $measuredProcess){return}
$startedUtc=$measuredProcess.StartTime.ToUniversalTime()
if([math]::Abs(($startedUtc-[datetime]$identity.startedUtc).TotalSeconds) -gt 1){throw 'PID was reused; refusing to sample a different process'}
if($measuredProcess.Path -ne $identity.player){throw 'Player executable differs from the recorded process identity'}
$nowUtc=[datetime]::UtcNow
$row=[ordered]@{
    observedUtc=$nowUtc.ToString('O')
    processElapsedSeconds=[math]::Round(($nowUtc-$startedUtc).TotalSeconds,1)
    pid=$measuredProcess.Id
    privateMB=[math]::Round($measuredProcess.PrivateMemorySize64/1MB,2)
    workingSetMB=[math]::Round($measuredProcess.WorkingSet64/1MB,2)
    cpuSeconds=[math]::Round($measuredProcess.CPU,2)
}
[pscustomobject]$row | Export-Csv -LiteralPath (Join-Path $evidenceRoot 'ExternalMemory.csv') -NoTypeInformation -Append -Encoding UTF8
