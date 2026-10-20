param([switch]$Resume,[switch]$Complete)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceProject=Join-Path $projectRoot 'ConnectionTest'
$metadata=Join-Path $projectRoot 'tmp/unity/demo-copy-last.json'
$unityExe='C:/Program Files/Unity/Hub/Editor/6000.0.58f2-x86_64/Editor/Unity.exe'
if ($Resume -or $Complete) {
    $run=Get-Content -LiteralPath $metadata -Raw | ConvertFrom-Json
    $stage=[IO.Path]::GetFullPath($run.stage)
    $prefix=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')+'\CloudVR-Demo-'
    if (!$stage.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)) { throw 'Copie hors du dossier temporaire prévu.' }
    if (Get-Process -Id $run.pid -ErrorAction SilentlyContinue) { throw 'Le batch est encore actif ; consulter le journal avant de reprendre.' }
} else {
    $stage=Join-Path ([IO.Path]::GetTempPath()) ('CloudVR-Demo-'+[guid]::NewGuid().ToString('N').Substring(0,8))
    New-Item -ItemType Directory -Path $stage | Out-Null
    $guards=@(@('Assets/Scenes/Chambre.unity','Assets/Scenes/DataCenter.unity','ProjectSettings/ProjectSettings.asset') | ForEach-Object {
        [pscustomobject]@{file='ConnectionTest/'+$_;hash=(Get-FileHash -LiteralPath (Join-Path $sourceProject $_)).Hash}
    })
    $run=[pscustomobject]@{utc=[DateTime]::UtcNow.ToString('o');stage=$stage;pid=0;log=(Join-Path $stage 'DemoBuild.log');guards=$guards}
}
if ($Complete) {
    $report=Get-Content -LiteralPath (Join-Path $stage 'Builds/Demo/build-result.json') -Raw | ConvertFrom-Json
    if ($report.result -ne 'Succeeded' -or $report.errors -ne 0) { throw 'Build échoué ; ne pas livrer.' }
    $reportUtc=[DateTimeOffset]$report.utc
    if ($reportUtc -lt [DateTimeOffset]$run.utc) { throw 'Rapport antérieur au lancement.' }
    if ((Get-Content -LiteralPath $run.log -Raw) -notmatch 'Exiting batchmode successfully') { throw 'Fermeture normale du batch non constatée.' }
    foreach ($guard in $run.guards) { if ((Get-FileHash -LiteralPath (Join-Path $projectRoot $guard.file)).Hash -ne $guard.hash) { throw ('Fichier original modifié pendant le build, revue requise : '+$guard.file) } }
    $destination=Join-Path $sourceProject 'Builds/Windows'
    $backup=Join-Path $sourceProject 'Builds/Windows-PreDemo-20261020'
    if ((Test-Path -LiteralPath $destination) -and !(Test-Path -LiteralPath $backup)) { Copy-Item -LiteralPath $destination -Destination $backup -Recurse }
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    Copy-Item -Path (Join-Path $stage 'Builds/Demo/*') -Destination $destination -Recurse -Force
    foreach ($name in @('Chambre.unity','DataCenter.unity')) {
        $generatedScene=Join-Path $stage ('Assets/Scenes/'+$name)
        $originalScene=Join-Path $sourceProject ('Assets/Scenes/'+$name)
        if ((Get-FileHash -LiteralPath $generatedScene).Hash -ne (Get-FileHash -LiteralPath $originalScene).Hash) {
            Copy-Item -LiteralPath $generatedScene -Destination $originalScene
        }
    }
    Get-ChildItem -LiteralPath (Join-Path $stage 'Assets/CloudVR/Materials') -Filter 'Demo_*' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $sourceProject 'Assets/CloudVR/Materials') -Force }
    $report | ConvertTo-Json -Compress
    return
}
if ($Resume) {
    $run.guards=@(@('Assets/Scenes/Chambre.unity','Assets/Scenes/DataCenter.unity','ProjectSettings/ProjectSettings.asset') | ForEach-Object {
        [pscustomobject]@{file='ConnectionTest/'+$_;hash=(Get-FileHash -LiteralPath (Join-Path $sourceProject $_)).Hash}
    })
    Copy-Item -LiteralPath $run.log -Destination (Join-Path $stage ('DemoBuild-previous-'+[DateTime]::UtcNow.ToString('HHmmss')+'.log'))
}
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    $target=Join-Path $stage $folder
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Copy-Item -Path (Join-Path $sourceProject ($folder+'/*')) -Destination $target -Recurse -Force
}
# Le connecteur sert uniquement à piloter l'éditeur. Éviter un clone Git réseau dans le batch.
# Cette omission ne concerne que la copie ; manifest et lock originaux restent inchangés.
foreach ($file in @('manifest.json','packages-lock.json')) {
    $path=Join-Path $stage ('Packages/'+$file)
    $json=Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    $json.dependencies.PSObject.Properties.Remove('com.coplaydev.unity-mcp')
    $json | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $path -Encoding utf8
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DemoCopyBuild.cs') -Destination (Join-Path $stage 'Assets/CloudVR/Editor/DemoCopyBuild.cs') -Force
$arguments=@('-batchmode','-nographics','-quit','-buildTarget','Win64','-projectPath',('"{0}"' -f $stage),'-executeMethod','DemoCopyBuild.Run','-logFile',('"{0}"' -f $run.log))
$process=Start-Process -FilePath $unityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
$run.pid=$process.Id; $run.utc=[DateTime]::UtcNow.ToString('o')
New-Item -ItemType Directory -Force -Path (Split-Path $metadata) | Out-Null
$run | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $metadata -Encoding utf8
$run | ConvertTo-Json -Depth 8 -Compress
# Non bloquant : consulter log/PID, puis appeler -Complete uniquement après arrêt normal.
