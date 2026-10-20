# Deuxième compilation Android — 20 octobre 2026

Compilation du package Android APK intégrant le parcours complet à deux scènes (`Chambre.unity` et `DataCenter.unity` avec les 6 bornes pédagogiques).

## Résultat de compilation

- **APK livré** : `ConnectionTest/Builds/Android/CloudVR-Quest.apk`
- **Taille** : 55 196 966 octets (55,20 Mo)
- **SHA256** : `258B3A9709FD77B4EA4E0621D0FB2188A29D295584913B8E2F12E566D4FE2603`
- **BuildPipeline** : `Succeeded`, 0 erreur, 5 avertissements normaux (migrations Input System facultatives, SSAO).
- **Plateforme** : Android ARM64, IL2CPP, Vulkan, OpenXR (Meta Quest Support, profils Touch Plus / Pro / Oculus Touch), Single Pass Instanced, SDK min 32 / target 34.

## 15 contrôles statiques réussis

1. `badgingRead` : valide via aapt2 (34.0.0)
2. `manifestRead` : XML AndroidManifest lisible et vérifié
3. `signatureValid` : signature APK v2 valide (apksigner)
4. `packageId` : `com.cloudvr.prototype`
5. `minSdk32` : SDK minimum 32
6. `targetSdk34` : SDK cible 34
7. `arm64Only` : architecture `arm64-v8a` uniquement
8. `il2cpp` : binaire natif `libil2cpp.so` présent
9. `openxrPlugin` : `libopenxr_loader.so` et plugin OpenXR présents
10. `openxrLoader` : OpenXR initialisé au boot
11. `vrCategoryDeclared` : `android.intent.category.INFO` / VR
12. `headtrackingDeclared` : headtracking requis
13. `questDevicesDeclared` : `quest2|cambria|eureka|quest3s`
14. `noUnusedEyeTracking` : déclarations eye tracking inutilisées nettoyées par `QuestManifestSanitizer`
15. `unityPlayerData` : données de scènes Unity présentes (`data.unity3d`)

## Limite de validation

Contrôle statique réussi. Aucun casque physique n'était connecté via ADB lors du contrôle. Le test matériel dans le Meta Quest (confort, tracking des manettes, sélection rayon/téléportation) reste à réaliser.
