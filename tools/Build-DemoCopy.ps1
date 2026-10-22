param([switch]$Resume,[switch]$Complete,[switch]$Wait)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceProject=Join-Path $projectRoot 'ConnectionTest'
$metadata=Join-Path $projectRoot 'tmp/unity/demo-copy-last.json'
$unityExe='C:/Program Files/Unity/Hub/Editor/6000.0.58f2-x86_64/Editor/Unity.exe'
function Copy-ChangedFile([string]$Source,[string]$Destination) {
    if ((Test-Path -LiteralPath $Destination) -and (Get-FileHash -LiteralPath $Source).Hash -eq (Get-FileHash -LiteralPath $Destination).Hash) { return }
    Copy-Item -LiteralPath $Source -Destination $Destination -Force
}
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
    foreach ($guard in $run.guards) {
        $currentHash=(Get-FileHash -LiteralPath (Join-Path $projectRoot $guard.file)).Hash
        if ($currentHash -eq $guard.hash) { continue }
        # Reprise d'une livraison interrompue : accepter seulement la scène générée identique.
        $generated=Join-Path $stage $guard.file.Substring('ConnectionTest/'.Length)
        if ($guard.file -like 'ConnectionTest/Assets/Scenes/*.unity' -and
            $currentHash -eq (Get-FileHash -LiteralPath $generated).Hash) { continue }
        throw ('Fichier original modifié pendant le build, revue requise : '+$guard.file)
    }
    foreach ($name in @('WhiteSpace_FloorTiles.asset','WhiteSpace_FloorTiles.asset.meta')) {
        if (!(Test-Path -LiteralPath (Join-Path $stage ('Assets/CloudVR/Models/'+$name)))) { throw ('Ressource de sol générée absente : '+$name) }
    }
    $p0Meshes = @(Get-ChildItem -LiteralPath (Join-Path $stage 'Assets/CloudVR/Models') -Filter 'P0_Racks_*.asset' -File)
    if ($p0Meshes.Count -eq 0) { throw 'Meshes des rangées P0 absents.' }
    foreach ($mesh in $p0Meshes) { if (!(Test-Path -LiteralPath ($mesh.FullName+'.meta'))) { throw 'GUID de mesh P0 absent.' } }
    $destination=Join-Path $sourceProject 'Builds/Windows'
    $backup=Join-Path $sourceProject 'Builds/Windows-PreDemo-20261020'
    if ((Test-Path -LiteralPath $destination) -and !(Test-Path -LiteralPath $backup)) { Copy-Item -LiteralPath $destination -Destination $backup -Recurse }
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    $generatedBuild=Join-Path $stage 'Builds/Demo'
    # Un diagnostic d'un essai échoué peut rester dans la copie après une reprise réussie.
    Get-ChildItem -LiteralPath $generatedBuild -File -Recurse |
        Where-Object { $_.FullName -ne (Join-Path $generatedBuild 'build-error.txt') } | ForEach-Object {
        $target=Join-Path $destination $_.FullName.Substring($generatedBuild.Length).TrimStart('\','/')
        New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
        Copy-ChangedFile $_.FullName $target
    }
    foreach ($name in @('Chambre.unity','DataCenter.unity')) {
        $generatedScene=Join-Path $stage ('Assets/Scenes/'+$name)
        $originalScene=Join-Path $sourceProject ('Assets/Scenes/'+$name)
        if ((Get-FileHash -LiteralPath $generatedScene).Hash -ne (Get-FileHash -LiteralPath $originalScene).Hash) {
            Copy-Item -LiteralPath $generatedScene -Destination $originalScene
        }
    }
    # Les scènes sérialisent les GUID des matériaux créés par tous les builders.
    # Copier aussi leurs .meta : sans eux les scènes originales deviennent roses après import.
    Get-ChildItem -LiteralPath (Join-Path $stage 'Assets/CloudVR/Materials') -File |
        Where-Object { $_.Name -like '*.mat' -or $_.Name -like '*.mat.meta' } |
        ForEach-Object { Copy-ChangedFile $_.FullName (Join-Path $sourceProject ('Assets/CloudVR/Materials/'+$_.Name)) }
    # Le dallage groupé est un mesh généré par le builder, référencé par GUID dans DataCenter.
    foreach ($name in @('WhiteSpace_FloorTiles.asset','WhiteSpace_FloorTiles.asset.meta')) {
        Copy-ChangedFile (Join-Path $stage ('Assets/CloudVR/Models/'+$name)) (Join-Path $sourceProject ('Assets/CloudVR/Models/'+$name))
    }
    foreach ($mesh in $p0Meshes) {
        Copy-ChangedFile $mesh.FullName (Join-Path $sourceProject ('Assets/CloudVR/Models/'+$mesh.Name))
        Copy-ChangedFile ($mesh.FullName+'.meta') (Join-Path $sourceProject ('Assets/CloudVR/Models/'+$mesh.Name+'.meta'))
    }
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
    $str = $json | ConvertTo-Json -Depth 30
    [IO.File]::WriteAllText($path, $str, [Text.UTF8Encoding]::new($false))
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DemoCopyBuild.cs') -Destination (Join-Path $stage 'Assets/CloudVR/Editor/DemoCopyBuild.cs') -Force
$arguments=@('-batchmode','-nographics','-quit','-buildTarget','Win64','-projectPath',$stage,'-executeMethod','DemoCopyBuild.Run','-logFile',$run.log)
$process=Start-Process -FilePath $unityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
$run.pid=$process.Id; $run.utc=[DateTime]::UtcNow.ToString('o')
New-Item -ItemType Directory -Force -Path (Split-Path $metadata) | Out-Null
$run | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $metadata -Encoding utf8
$run | ConvertTo-Json -Depth 8 -Compress
if ($Wait) {
    Write-Host "Attente du processus $($process.Id)..."
    $deadline = [DateTime]::UtcNow.AddMinutes(15)
    while (!$process.WaitForExit(1000)) {
        if ([DateTime]::UtcNow -gt $deadline) { throw "Attente expirée pour le build PC." }
    }
    Write-Host "Processus terminé (Code $($process.ExitCode))."
}
