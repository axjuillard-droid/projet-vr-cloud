param(
    [string]$ApkPath,
    [Parameter(Mandatory=$true)][string]$SdkRoot,
    [Parameter(Mandatory=$true)][string]$JdkRoot,
    [string]$BuildToolsVersion = '34.0.0'
)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (!$ApkPath) { $ApkPath = Join-Path $projectRoot 'ConnectionTest/Builds/Android/CloudVR-Quest.apk' }
$ApkPath = (Resolve-Path -LiteralPath $ApkPath).Path
$buildTools = Join-Path $SdkRoot ('build-tools/' + $BuildToolsVersion)
$aapt = Join-Path $buildTools 'aapt2.exe'
$signer = Join-Path $buildTools 'lib/apksigner.jar'
$java = Join-Path $JdkRoot 'bin/java.exe'
foreach ($toolPath in @($aapt,$signer,$java)) {
    if (!(Test-Path -LiteralPath $toolPath)) { throw "Outil absent : $toolPath" }
}
$runFolder = Join-Path $projectRoot ('tmp/unity/apk-check-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,6))
New-Item -ItemType Directory -Path $runFolder -Force | Out-Null
$badging = (& $aapt dump badging $ApkPath 2>&1 | Out-String)
$badgingExit = $LASTEXITCODE
$badging | Set-Content -LiteralPath (Join-Path $runFolder 'badging.txt') -Encoding utf8
$manifest = (& $aapt dump xmltree --file AndroidManifest.xml $ApkPath 2>&1 | Out-String)
$manifestExit = $LASTEXITCODE
$manifest | Set-Content -LiteralPath (Join-Path $runFolder 'manifest.txt') -Encoding utf8
$signature = (& $java -jar $signer verify --verbose $ApkPath 2>&1 | Out-String)
$signatureExit = $LASTEXITCODE
$signature | Set-Content -LiteralPath (Join-Path $runFolder 'signature.txt') -Encoding utf8
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($ApkPath)
try { $entries = @($archive.Entries | ForEach-Object { $_.FullName }) }
finally { $archive.Dispose() }
$abis = @($entries | Where-Object { $_ -match '^lib/([^/]+)/[^/]+$' } | ForEach-Object { ($_ -split '/')[1] } | Sort-Object -Unique)
$entries | Where-Object { $_ -match '^lib/' } | Set-Content -LiteralPath (Join-Path $runFolder 'native-libraries.txt') -Encoding utf8
$checks = [ordered]@{
    badgingRead = $badgingExit -eq 0
    manifestRead = $manifestExit -eq 0
    signatureValid = $signatureExit -eq 0
    packageId = $badging -match "(?m)^package: name='com\.cloudvr\.prototype'"
    minSdk32 = $badging -match "(?m)^sdkVersion:'32'"
    targetSdk34 = $badging -match "(?m)^targetSdkVersion:'34'"
    arm64Only = $abis.Count -eq 1 -and $abis[0] -eq 'arm64-v8a'
    il2cpp = $entries -contains 'lib/arm64-v8a/libil2cpp.so'
    openxrPlugin = $entries -contains 'lib/arm64-v8a/libUnityOpenXR.so'
    openxrLoader = $entries -contains 'lib/arm64-v8a/libopenxr_loader.so'
    vrCategoryDeclared = $manifest.Contains('com.oculus.intent.category.VR')
    headtrackingDeclared = $manifest.Contains('android.hardware.vr.headtracking')
    questDevicesDeclared = $manifest.Contains('com.oculus.supportedDevices')
    noUnusedEyeTracking = !$manifest.Contains('oculus.software.eye_tracking') -and !$manifest.Contains('com.oculus.permission.EYE_TRACKING')
    unityPlayerData = ($entries -contains 'assets/bin/Data/level0') -or ($entries -contains 'assets/bin/Data/data.unity3d')
}
# Les déclarations VR sont contrôlées par présence ; lire aussi manifest.txt pour
# leurs valeurs exactes, permissions et éventuelles exigences matérielles.
# Les données peuvent être regroupées dans data.unity3d avec la compression LZ4.
# Leur présence ne décode pas les scènes ; Chambre est l'unique scène du build.
$result = [ordered]@{
    utc = [DateTime]::UtcNow.ToString('o')
    apk = $ApkPath
    bytes = (Get-Item -LiteralPath $ApkPath).Length
    sha256 = (Get-FileHash -LiteralPath $ApkPath -Algorithm SHA256).Hash
    sdk = $SdkRoot
    jdk = $JdkRoot
    buildTools = $BuildToolsVersion
    abis = $abis
    checks = $checks
    passed = @($checks.Values | Where-Object { !$_ }).Count -eq 0
    evidenceFolder = $runFolder
    limitation = 'Contrôle statique uniquement ; aucun lancement Android ou essai casque.'
}
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runFolder 'apk-check-result.json') -Encoding utf8
$result | ConvertTo-Json -Depth 5
if (!$result.passed) { throw "Contrôle APK échoué ; lire $runFolder" }
