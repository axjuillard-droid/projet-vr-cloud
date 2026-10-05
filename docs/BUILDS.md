# Builds du prototype

Les sorties locales de build dans `ConnectionTest/Builds/` sont ignorées par Git. Garder l'exécutable Windows et tout son dossier de données ensemble. Le projet comporte désormais Chambre et DataCenter, reliées après l'envoi illustratif. L'ancien APK livré contient seulement Chambre ; les nouveaux exports doivent inclure les deux scènes.

## Windows

### Rythme de développement et de validation

Méthode retenue le 05/10 : vérifier compilation/console et tester dans Play après chaque petite fonctionnalité. Construire l'exe à la fin d'une étape jouable cohérente, avant une démonstration ou pour rechercher un problème propre au player. Ne pas attendre une longue série de modifications sans test. Le build Windows est la référence du livrable PC ; il ne se met pas à jour automatiquement lorsque le projet change. Garder les caches Unity pour les builds incrémentaux, sans garantie de durée.

Premier morceau jouable livré le 05/10 : envoi photo dans Chambre → arrivée dans DataCenter avec point de départ et première baie repérable. Contrôlé en Play puis dans l'exe : transition/caméra unique, spawn, déplacement et collision. Prochaine étape fonctionnelle : interaction explicative avec la baie ; tester chaque ajout en Play puis exporter l'ensemble. Textes provisoires, aucun trajet réseau présenté comme une mesure réelle. Les contrôles VR matériels restent distincts, à effectuer sur APK récent quand le Quest est disponible. Aucun nouveau build requis pour une simple mise à jour des notes.

Préconditions : scènes sauvegardées, hors Play, compilation terminée, aucune erreur C# nouvelle. Windows64, backend Mono, `Assets/Scenes/Chambre.unity` en premier puis `Assets/Scenes/DataCenter.unity`, toutes deux enabled. Ne pas ouvrir une seconde instance Unity sur le même projet pour construire.

Par MCP `manage_build` : action `build`, target `windows64`, development `true`, scenes `["Assets/Scenes/Chambre.unity", "Assets/Scenes/DataCenter.unity"]`, options `["compress_lz4","detailed_report"]`, output_path absolu vers `ConnectionTest/Builds/Windows/CloudVR-PC.exe`. Conserver job_id ; lire status après la compilation. Une réponse `success=true` ne valide pas un build annulé/échoué : examiner `data.result`, erreurs et présence des fichiers.

Pendant une longue compilation de shaders, le pont peut expirer ; lire le journal plutôt que relancer un deuxième build. Ne pas annuler le build uniquement parce qu'une requête MCP expire. Une compilation réellement annulée doit être documentée et faire l'objet d'une reprise bornée.

Le build de développement inclut PhotoInteractionSmoke, activé uniquement avec `--cloudvr-pc-smoke --cloudvr-smoke-output <dossier absolu>`. Les builds finaux et Android excluent ce test. Pour l'exécuter :

```powershell
./tools/Test-PCBuild.ps1
./tools/Test-PCBuild.ps1 -Graphics -Journey
./tools/Test-PCBuild.ps1 -Graphics -TimeoutSeconds 90
```

Le runner lance le player masqué en batch et sans graphics, attend au plus 45 s, puis contrôle exitCode=0 et un rapport récent passed=true dans un dossier unique sous tmp/unity. En cas de timeout il termine uniquement le processus de test qu'il a créé. Lire Player.log et launch-result.json. Aucun rendu ou essai utilisateur déduit du scénario headless ; déplacer/regarder avec les vraies entrées reste à tester. `-Graphics` conserve le périphérique graphique en mode batch pour permettre les captures de la caméra, lorsque disponible.

`-Journey` choisit JourneySmoke (--cloudvr-journey-smoke, journey-smoke-result.json) : clic synthétique, arrivée dans DataCenter, unicité/spawn/mode, avance au clavier synthétique et collision avec la baie. Sans -Journey, les 9 contrôles photo restent disponibles. Les probes sont exclus du player Android et des builds finaux Windows. Dans Unity, JourneySmoke.Run(true) vérifie la conservation VR et la locomotion injectée, pas le geste rayon/manette ni le confort matériel. Les deux scénarios doivent être lancés séparément.

`-TimeoutSeconds` borne l'attente du processus (45 s par défaut, 15–180 s autorisés). Le 05/10, une première exécution photo avait un rapport réussi mais le runner a atteint son timeout pendant la fermeture ; reprise avec 90 s réussie, exitCode=0. Un rapport passed seul ne suffit pas si le processus dépasse la borne. Build final Chambre/DataCenter à 14 h 41 : 34,9 s, 0 erreur/0 avertissement ; 13 contrôles parcours et 9 contrôles photo réussis dans le player graphique. Preuves validation/journey-2026-10-05/.

Pour un essai utilisateur, lancer CloudVR-PC.exe normalement : WASD/flèches, souris et clic gauche sur l'écran. Alt+F4 ferme le prototype. Vérifier visibilité, lisibilité, déplacements/collisions, confirmation après 2,5 s et absence d'erreurs nouvelles. Le délai illustre l'envoi ; ce n'est pas une mesure réseau.

## Android / Quest

Préparation appliquée le 05/10 : menu `CloudVR > Build > Préparer Android Quest`, ou `QuestBuildSetupEditor.Prepare()` par MCP. OpenXRLoader Android au démarrage, Meta Quest Support, profils Touch/Touch Pro/Touch Plus, IL2CPP ARM64, Vulkan, Single Pass Instanced, minSdk32/targetSdk34 et identifiant `com.cloudvr.prototype`. Cibles déclarées Quest 2/Pro/3/3S du package ; Quest 1 désactivé. Ces déclarations ne certifient pas les modèles.

Audit reproductible : `return QuestBuildSetupEditor.Audit();`. Audit relancé sur Android actif, original puis copie : 0 erreur, 3 avertissements. Les deux migrations Input System sont facultatives ; SSAO reste à examiner/mesurer sur Quest. Certains prédicats lisent la plateforme active : toujours contrôler la plateforme cible avant build. APK compilé et contrôlé le 05/10 ; aucun lancement Quest validé.

Outils intégrés réellement utilisés : SDK34, NDK r27c, Java17.0.9, Gradle8.11 ; build réussi. SDK platforms android-34/35/36 présents. Le modèle et l'OS du casque restent inconnus.

La [documentation OpenXR 1.14](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.14/manual/index.html) indique Android ARM64/Vulkan pour Quest ; [Meta Quest Support](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.14/manual/features/metaquest.html) décrit le loader et les profils. Pour les cibles Quest actuelles, [Meta recommande minSdk 32 et targetSdk 34](https://developers.meta.com/vr/resources/publish-mobile-manifest/). Ce choix de prototype ne constitue ni une publication Store ni une certification. La [documentation Meta sur les versions minimales](https://developers.meta.com/vr/documentation/unity/min-os-versions/) indique Horizon OS v65 minimum avec le loader Unity OpenXR ; vérifier l'OS du casque disponible avant installation.

Avant le premier APK : activer la plateforme Android, relire l'audit et la validation complète sur cette plateforme, vérifier les chemins Unity SDK/NDK/JDK, la scène de build et le manifeste résultant. Ne pas utiliser Fix All sans examiner les changements. Construire un APK local de développement, vérifier son rapport, puis installer seulement sur un casque identifié et autorisé. Conserver modèle, OS, date et résultats de tracking/manettes/téléportation/rotation/confort/performance.

### Compilation depuis le chemin actuel

Le premier build du 05/10 a échoué : Android Tools refuse le caractère `é` dans `année 5`. Ne pas relancer un build Android dans ce chemin. Le script suivant crée une copie de compilation temporaire au chemin ASCII sous le dossier Temp utilisateur, sans déplacer le projet de travail :

```powershell
./tools/Build-AndroidCopy.ps1
```

Il copie Assets/Packages/ProjectSettings, ajoute `tools/AndroidCopyBuild.cs` uniquement dans la copie puis lance le même Unity en batch masqué sur Android. La copie utilise Input System (New) ; l'original conserve son réglage Both. Library est régénéré : le premier import peut être long. L'éditeur original peut rester ouvert car il s'agit d'un autre dossier de projet. Avec un sandbox d'exécution, une permission de lancement peut être nécessaire : premier essai arrêté avant journal ici, second lancement hors sandbox effectivement démarré. Ne pas lancer deux instances sur la même copie.

Le script attend le processus et rapatrie APK/audit/rapport dans `ConnectionTest/Builds/Android/` uniquement si exitCode=0 et BuildReport=Succeeded. `tmp/unity/android-copy-last.json` indique copie, PID et journal. Après 45 min, l'attente échoue sans terminer le build ; vérifier ce processus avant toute reprise. Aucun nettoyage automatique de la copie, conservée pour diagnostic ; ce dossier temporaire ne doit pas devenir le projet de développement.

Après correction du helper uniquement, `-Resume` reprend la dernière copie si le processus précédent est terminé et le dossier temporaire vérifié. Il recopie le helper, sans synchroniser les autres sources ; après une modification du projet, recréer une copie avec le lancement normal. Conserver un extrait du journal précédent avant reprise car AndroidBuild.log est réutilisé.

Contrôle statique, avec les chemins SDK/JDK constatés dans le précontrôle Unity :

```powershell
./tools/Test-AndroidApk.ps1 -SdkRoot '<chemin SDK>' -JdkRoot '<chemin OpenJDK>'
```

Le runner vérifie badging, signature, ABI ARM64, IL2CPP et bibliothèques OpenXR puis produit les textes de manifeste et une empreinte SHA256 sous tmp/unity. Il refuse également les déclarations eye tracking inutilisées par ce prototype. Les vérifications de déclarations VR portent sur leur présence : lire les valeurs exactes, permissions et exigences matérielles dans manifest.txt. Un résultat statique valide ne prouve ni lancement Android ni fonctionnement Quest. Les runners ne font aucune installation par ADB.

Résultat du 05/10 : APK final 53,85 Mo, 0 erreur/5 avertissements de build, 15 contrôles statiques réussis. Voir `validation/android-build-2026-10-05/README.md`. `QuestManifestSanitizer` retire en fin de génération Gradle les entrées eye tracking ajoutées par OpenXR 1.14 lorsque Meta est actif et Eye Gaze désactivé. Aucun package modifié. Vérifier le manifeste après chaque build et revoir le patch si un usage oculaire est ajouté. Le libellé affiché sur le casque sera encore ConnectionTest. L'envoi est illustratif, sans data center à charger.
