# Première compilation Android — 5 octobre 2026

Tâche : produire un APK de développement de Chambre puis contrôler rapport, manifeste, ABI et signature. Aucun essai Quest déduit du build.

## Résultat final

**Réussi le 05/10 à 13 h 42 Paris** : `ConnectionTest/Builds/Android/CloudVR-Quest.apk`, 53 852 334 octets (53,85 Mo), exitCode=0, BuildReport Succeeded, 0 erreur/5 avertissements, 59,2 s de BuildPipeline avec caches. La taille totale du rapport (835 716 674 octets) inclut d'autres sorties et ne représente pas la taille de l'APK. Premier build froid de référence : 25 min 14 s.

**15 contrôles statiques réussis à 13 h 43** : identifiant, SDK32/34, ARM64 seul, IL2CPP/OpenXR, données Unity, manifeste lisible et signature v2 valide. SHA256 : `35CAA36B6893ACBF180B1C429B3E0A1DA9FA8C1A2CA5B2688F3986D47FEEE27D`. Textes et JSON dans `final/`. Le build ne contient que Chambre ; la présence des données Unity ne décode pas indépendamment la scène.

Manifeste relu : catégorie VR, headtracking required=true/version1, supportedDevices quest2|cambria|eureka|quest3s, debuggable=true. Aucun eye tracking exigé/permission associé. INTERNET et permission interne du receiver Android présents ; aucun envoi cloud réel déduit de ces déclarations. Le libellé installé de l'application est encore ConnectionTest.

`QuestManifestSanitizer` est compilé dans la copie et appelé après XR : 2 déclarations supprimées, preuve `final/build-validation.txt`. Patch dans Assets/CloudVR/Editor du projet, aucun cache de package modifié. Il conserve les entrées lorsque Eye Gaze est activé ; revoir patch et runner si d'autres usages oculaires apparaissent. `final/source-copy-check.json` compare les scripts C#, Chambre et packages-lock par SHA256, tous identiques pour les fichiers comparés. Les réglages générés par Unity ne sont pas tous couverts ; Input Handling = New reste une différence volontaire de la copie.

Niveau final : **APK compilé et contrôlé statiquement**, sans lancement Android ni essai Quest. ADB ne détectait aucun appareil, aucune installation effectuée. Connexion MCP indisponible sur les deux derniers appels, arrêt des tentatives ; éditeur original observé Responding=true par inspection de processus, sans nouveau contrôle live de scène/console.

## Précontrôle et premier échec

`android-preflight.json` : plateforme Android active, scène propre, hors compilation/build, IL2CPP/ARM64, minSdk32/targetSdk34, Vulkan/OpenXR/Meta, outils SDK/NDK/JDK/Gradle intégrés. Audit : 0 erreur, 3 avertissements (deux migrations Input System facultatives, SSAO).

`first-build-failed.json` : job build-0905a18b12 lancé à 12 h 21 Paris, terminé à 12 h 26, Failed, 1 erreur et 6 avertissements. Android Tools refuse le chemin contenant `année 5`. Le dialogue modal identifié par la photo utilisateur empêchait le retour MCP ; l'utilisateur a cliqué OK. Aucun APK produit. Les timeouts MCP ne constituent pas des tentatives de build supplémentaires.

## Reprise sur copie ASCII

Reproduction : `tools/Build-AndroidCopy.ps1`, helper `tools/AndroidCopyBuild.cs` ajouté uniquement dans le dossier Editor de la copie. Assets/Packages/ProjectSettings copiés, Library régénéré ; Input Handling = New dans la copie, Both conservé dans l'original. Même éditeur 6000.0.58f2, fichier packages-lock identique après résolution (SHA256 7FC3B6E4F5DE4BE2AE79416DA09084616A870F9B0392EB052C53C0BE060E028B).

Premier lancement batch dans l'environnement isolé à 12 h 29 : exit -2147483645 avant journal, crash.dmp créé, cause non établie. Le dump n'est pas ajouté au dépôt. Original toujours disponible, hors Play/build/compilation. Second lancement hors sandbox autorisé à 12 h 30 : journal créé, entitlement résolu et packages résolus ; helper arrêté sur deux CS0266 (compteurs int déclarés uint), extrait `batch-helper-compile-error.log`. Champs corrigés et reprise à 12 h 37 sur la même copie importée. À 12 h 39, scripts compilés, audit 0 erreur/3 avertissements, passage du contrôle de chemin ASCII, détection SDK/JDK en cours. Résultat de build encore en attente.

`tmp/unity/android-copy-last.json` conserve PID, copie et journal pour cette machine ; dossier ignoré par Git. Les runners ne déplacent pas l'original et ne suppriment pas la copie. Ils rapatrient l'APK uniquement après exitCode=0 et BuildReport=Succeeded.

## Validation matérielle restant à faire

- Identifier un Quest et son OS, autoriser USB puis tester lancement, tracking, sélection écran, rayon → zone → téléportation, rotation par crans, lisibilité, confort et performance. `adb devices -l` a été exécuté : aucun appareil détecté ; aucune installation.
- Conserver modèle, OS, date, empreinte de l'APK, scénarios et résultats. Ce build est de développement ; ne pas présenter sa performance comme mesure du livrable final.

Le scénario représente un envoi illustratif de photo et ne charge pas encore de data center. Le test casque et les essais utilisateurs restent distincts du build et du contrôle statique.
