param([switch]$Visible,[ValidateRange(60,300)][int]$TimeoutSeconds=180,[switch]$PassAudit,[switch]$ShadowComparison,[switch]$AoComparison,[string]$PlayerPath)
$ErrorActionPreference='Stop'
if(@($PassAudit,$ShadowComparison,$AoComparison).Where({$_}).Count -gt 1){throw 'Choisir un seul protocole.'}
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $projectRoot 'ConnectionTest/Builds/Windows/CloudVR-PC.exe'}
if(!(Test-Path -LiteralPath $exe)){throw 'Build PC absent.'}
$folder=Join-Path $projectRoot ('tmp/unity/pc-performance-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,6))
New-Item -ItemType Directory -Path $folder | Out-Null
$arguments=@('-force-d3d11','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"{0}"' -f (Join-Path $folder 'Player.log')),'--cloudvr-performance','--cloudvr-performance-output',('"{0}"' -f $folder))
if($PassAudit){$arguments+='--cloudvr-pass-audit'}
if($ShadowComparison){$arguments+='--cloudvr-shadow-comparison'}
if($AoComparison){$arguments+='--cloudvr-ao-comparison'}
$windowStyle=if($Visible){'Normal'}else{'Hidden'}
$player=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory (Split-Path $exe) -WindowStyle $windowStyle -PassThru
$timedOut=!$player.WaitForExit($TimeoutSeconds*1000)
if($timedOut){Stop-Process -Id $player.Id -Force}
$reportPath=Join-Path $folder 'performance-result.json'
$report=if(Test-Path -LiteralPath $reportPath){Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json}else{$null}
$expectedPhases=if($PassAudit){8}else{4}
$renderingMeasured=$null -ne $report -and @($report.phases | Where-Object { $_.cpuRender.count -gt 0 }).Count -eq $expectedPhases
$result=[ordered]@{utc=[DateTime]::UtcNow.ToString('o');timedOut=$timedOut;exitCode=if($timedOut){$null}else{$player.ExitCode};passed=!$timedOut -and $player.ExitCode -eq 0 -and $null -ne $report -and $report.passed;gpuMeasured=$null -ne $report -and $report.gpuAvailable;renderingMeasured=$renderingMeasured;report=$reportPath;window=($windowStyle+', windowed requested 1280x720; background applications not controlled')}
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'launch-result.json') -Encoding utf8
$result | ConvertTo-Json -Compress
if(!$result.passed){throw 'Mesure incomplète : lire le rapport et Player.log.'}
