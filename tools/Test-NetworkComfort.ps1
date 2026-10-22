param([ValidateRange(20,120)][int]$TimeoutSeconds=45)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $projectRoot 'ConnectionTest/Builds/Windows/CloudVR-PC.exe'
if (!(Test-Path -LiteralPath $exe)) { throw 'Build Windows absent.' }
$runFolder=Join-Path $projectRoot ('tmp/unity/network-comfort-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $runFolder -Force | Out-Null
$log=Join-Path $runFolder 'Player.log'
$arguments=@('-batchmode','-logFile',('"{0}"' -f $log),'--cloudvr-network-comfort-smoke','--cloudvr-smoke-output',('"{0}"' -f $runFolder))
$player=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru
$timedOut=!$player.WaitForExit($TimeoutSeconds*1000)
if ($timedOut) { Stop-Process -Id $player.Id -Force }
$reportPath=Join-Path $runFolder 'network-comfort-result.json'
$report=if (Test-Path -LiteralPath $reportPath) { Get-Content -Raw -LiteralPath $reportPath | ConvertFrom-Json } else { $null }
$result=[ordered]@{utc=[DateTime]::UtcNow.ToString('o');scenario='network-comfort-api-no-headset';timedOut=$timedOut;exitCode=if($timedOut){$null}else{$player.ExitCode};passed=!$timedOut -and $player.ExitCode -eq 0 -and $null -ne $report -and $report.passed;report=$reportPath;log=$log}
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $runFolder 'launch-result.json') -Encoding utf8
$result | ConvertTo-Json -Compress
if (!$result.passed) { throw ('Contrôle confort par API échoué ; lire '+$log) }
