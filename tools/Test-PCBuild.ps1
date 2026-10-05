param([switch]$Graphics, [switch]$Journey, [ValidateRange(15,180)][int]$TimeoutSeconds = 45)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exePath = Join-Path $projectRoot 'ConnectionTest/Builds/Windows/CloudVR-PC.exe'
if (!(Test-Path -LiteralPath $exePath)) { throw "Build PC absent : $exePath" }
$runFolder = Join-Path $projectRoot ('tmp/unity/pc-player-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 6))
New-Item -ItemType Directory -Path $runFolder -Force | Out-Null
$logPath = Join-Path $runFolder 'Player.log'
$scenario = if ($Journey) { '--cloudvr-journey-smoke' } else { '--cloudvr-pc-smoke' }
$arguments = @('-batchmode', '-logFile', ('"{0}"' -f $logPath), $scenario, '--cloudvr-smoke-output', ('"{0}"' -f $runFolder))
if (!$Graphics) { $arguments += '-nographics' }
$playerProcess = Start-Process -FilePath $exePath -ArgumentList $arguments -WorkingDirectory (Split-Path $exePath) -WindowStyle Hidden -PassThru
$timedOut = !$playerProcess.WaitForExit($TimeoutSeconds * 1000)
if ($timedOut) { Stop-Process -Id $playerProcess.Id -Force }
$reportName = if ($Journey) { 'journey-smoke-result.json' } else { 'photo-smoke-result.json' }
$reportPath = Join-Path $runFolder $reportName
$report = if (Test-Path -LiteralPath $reportPath) { Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json } else { $null }
$result = [ordered]@{
    utc = [DateTime]::UtcNow.ToString('o')
    exe = $exePath
    headless = !$Graphics
    scenario = $scenario
    timedOut = $timedOut
    timeoutSeconds = $TimeoutSeconds
    exitCode = if ($timedOut) { $null } else { $playerProcess.ExitCode }
    passed = !$timedOut -and $playerProcess.ExitCode -eq 0 -and $null -ne $report -and $report.passed
    report = $reportPath
    log = $logPath
}
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $runFolder 'launch-result.json') -Encoding utf8
$result | ConvertTo-Json -Compress
if (!$result.passed) { throw "Test PC échoué ou incomplet ; lire $logPath" }
