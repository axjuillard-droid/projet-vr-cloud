param(
    [string]$UnityExe = 'C:/Program Files/Unity/Hub/Editor/6000.0.58f2-x86_64/Editor/Unity.exe',
    [switch]$Resume
)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceProject = Join-Path $projectRoot 'ConnectionTest'
if (!(Test-Path -LiteralPath $UnityExe)) { throw "Éditeur absent : $UnityExe" }
$metadata = Join-Path $projectRoot 'tmp/unity/android-copy-last.json'
if ($Resume) {
    $previous = Get-Content -LiteralPath $metadata -Raw | ConvertFrom-Json
    if ($previous.source -ne $sourceProject) { throw 'La copie précédente appartient à un autre projet.' }
    if (Get-Process -Id $previous.pid -ErrorAction SilentlyContinue) { throw 'Le processus précédent est encore présent ; ne pas relancer.' }
    $stageProject = [IO.Path]::GetFullPath($previous.stage)
    $tempPrefix = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\CloudVR-Android-'
    if (!$stageProject.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw 'Copie hors du dossier temporaire prévu.' }
    if (!(Test-Path -LiteralPath (Join-Path $stageProject 'ProjectSettings/ProjectVersion.txt'))) { throw 'Copie Unity absente.' }
} else {
    $stageProject = Join-Path ([IO.Path]::GetTempPath()) ('CloudVR-Android-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
}
if ($stageProject -match '[^\x00-\x7F]') { throw 'Le dossier temporaire doit avoir un chemin ASCII.' }
if (!$Resume) {
    New-Item -ItemType Directory -Path $stageProject | Out-Null
    foreach ($folder in @('Assets','Packages','ProjectSettings')) {
        Copy-Item -LiteralPath (Join-Path $sourceProject $folder) -Destination $stageProject -Recurse
    }
}
# Le prototype utilise Input System ; Both n'est pas pris en charge sur Android.
# Modification confinée à cette copie : le projet de travail reste intact.
$settingsPath = Join-Path $stageProject 'ProjectSettings/ProjectSettings.asset'
$settingsText = Get-Content -LiteralPath $settingsPath -Raw
if ($settingsText -notmatch '(?m)^  activeInputHandler: [012]\s*$') { throw 'Réglage Input Handling introuvable.' }
$settingsText = [regex]::Replace($settingsText, '(?m)^  activeInputHandler: [012][\t ]*\r?$', '  activeInputHandler: 1')
[IO.File]::WriteAllText($settingsPath, $settingsText, [Text.UTF8Encoding]::new($false))
$editorFolder = Join-Path $stageProject 'Assets/CloudVR/Editor'
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'AndroidCopyBuild.cs') -Destination $editorFolder
$logPath = Join-Path $stageProject 'AndroidBuild.log'
$arguments = @('-batchmode','-nographics','-quit','-buildTarget','Android','-projectPath', ('"{0}"' -f $stageProject), '-executeMethod','AndroidCopyBuild.Run','-logFile', ('"{0}"' -f $logPath))
$buildProcess = Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
$run = [ordered]@{ utc=[DateTime]::UtcNow.ToString('o'); source=$sourceProject; stage=$stageProject; pid=$buildProcess.Id; log=$logPath; inputHandlingCopy='Input System Package (New)'; status='running' }
New-Item -ItemType Directory -Path (Split-Path $metadata) -Force | Out-Null
$run | ConvertTo-Json | Set-Content -LiteralPath $metadata -Encoding utf8
$run | ConvertTo-Json -Compress
$deadline = [DateTime]::UtcNow.AddMinutes(45)
while (!$buildProcess.WaitForExit(1000)) {
    if ([DateTime]::UtcNow -gt $deadline) { throw "Attente expirée ; le processus $($buildProcess.Id) reste actif. Lire $logPath avant toute reprise." }
}
$run.status = 'exited'
$run['exitCode'] = $buildProcess.ExitCode
$run | ConvertTo-Json | Set-Content -LiteralPath $metadata -Encoding utf8
$reportPath = Join-Path $stageProject 'Builds/Android/build-result.json'
if (!(Test-Path -LiteralPath $reportPath)) { throw "Rapport absent, exit $($buildProcess.ExitCode) ; lire $logPath" }
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
if ([DateTimeOffset]::Parse($report.utc) -lt [DateTimeOffset]::Parse($run.utc)) { throw "Rapport ancien ; lire $logPath avant toute livraison." }
if ($buildProcess.ExitCode -ne 0 -or $report.result -ne 'Succeeded') { throw "Build Android échoué ; lire $reportPath et $logPath" }
$destination = Join-Path $sourceProject 'Builds/Android'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
foreach ($file in @('CloudVR-Quest.apk','build-result.json','android-audit.json')) {
    Copy-Item -LiteralPath (Join-Path $stageProject ('Builds/Android/' + $file)) -Destination $destination
}
$report | ConvertTo-Json
# Pas de suppression automatique : conserver le journal et la copie pour diagnostic.
