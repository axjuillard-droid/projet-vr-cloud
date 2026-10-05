# Build Windows et préparation Android — 5 octobre 2026

## Résultat livré

Build Windows64 Mono **de développement**, scène Chambre : `ConnectionTest/Builds/Windows/CloudVR-PC.exe`, avec son dossier de données et ses DLL. Build réussi à 11 h 57 (Paris), 138,45 Mo, durée rapportée 449,8 s, 0 erreur et 0 avertissement de build. Rapport `windows-build-result.json`, empreinte de l'exe `exe-sha256.json`. Les binaires sont ignorés par Git et ne sont pas inclus dans ce dossier de preuves.

Le premier essai a été annulé (`BuildReport: Cancelled`, « Building Player was cancelled ») pendant le packaging des assets, sans erreur C# signalée. Origine inconnue ; ne pas l'attribuer au pont ou à l'utilisateur sans preuve. La seconde tentative utilise un chemin absolu. La compilation longue des shaders URP a généré des délais MCP ; aucun deuxième build lancé pendant le premier.

## Test dans le player

`tools/Test-PCBuild.ps1` a lancé l'exécutable masqué en batch et sans graphics : **9 vérifications réussies**, WindowsPlayer, Null graphics, exitCode=0 à 11 h 58. Dossier `headless/` : rapport daté, résultat du lanceur et Player.log. Ce mode émet des erreurs de shaders faute de périphérique graphique ; il ne valide aucun rendu.

Un second contrôle `tools/Test-PCBuild.ps1 -Graphics` a réussi les **mêmes 9 vérifications** avec Direct3D11, Intel Iris Xe Graphics et exitCode=0 à 11 h 59. Dossier `graphics/` : rapports, Player.log et trois captures inspectées. Les états prêt/envoi/confirmation sont visibles et différents. Pas d'exception ni d'erreur de shader détectée dans ce journal graphique ; avertissement URP connu sur la réduction de résolution de six shadow maps dans l'atlas 2048. Cela ne certifie ni performance ni absence générale de fuite mémoire.

Le scénario réutilise PhotoInteractionSmoke : souris synthétique Input System → Update/raycast ScreenClickHandler → PhotoSender. Aucun appel direct à Send. État initial, clic hors écran, clic valide, doublon pendant envoi, confirmation/transition après délai, réarmement, nouvel envoi et annulation par reset testés. Le contrôleur de déplacement est temporairement désactivé pour isoler la visée ; mouvements réels, focus utilisateur et ergonomie non validés. Le journal confirme le démarrage PC. Les tests sont embarqués seulement en Windows développement, démarrent uniquement sur argument explicite et quittent le player avec un code de résultat.

Captures du player : [prêt](graphics/photo-ready.png), [envoi](graphics/photo-sending.png), [confirmation](graphics/photo-completed.png). Lisibilité à distance encore faible ; pas de test utilisateur/casque. L'envoi dure 2,5 s à titre illustratif, sans transfert réseau. Aucune scène de data center n'est disponible.

## Préparation Android

Menu `CloudVR > Build > Préparer Android Quest`, script QuestBuildSetupEditor : OpenXRLoader Android initialisé au démarrage, Meta Quest Support et profils Touch/Touch Pro/Touch Plus activés ; IL2CPP ARM64, Vulkan, Single Pass Instanced, minSdk32/targetSdk34, identifiant `com.cloudvr.prototype`. Cibles déclarées du package : Quest 2/Pro/3/3S ; Quest 1 désactivé, sans prétendre avoir testé ces appareils.

Audit `android-preflight.json` à 12 h 02 : 0 erreur, 3 avertissements. Deux migrations Input System facultatives ; SSAO à examiner sur la configuration Quest réelle. Le premier audit affichait un avertissement de profil alors que les profils Android étaient activés : le prédicat du package lisait le groupe Standalone sélectionné. L'audit sélectionne maintenant temporairement Android puis restaure le groupe précédent. **Plateforme active toujours Windows64**, certains autres prédicats dépendent encore de cette plateforme : audit préparatoire seulement, validation à refaire après switch Android.

SDK 34/35/36 et NDK r27c constatés dans l'installation de l'éditeur actif ; chemins réellement employés par Unity/Gradle et JDK à vérifier avant le premier APK. Script éditeur compilé après ajout du namespace UnityEditor.XR.OpenXR manquant. Aucun APK, manifeste APK vérifié, installation USB, modèle/OS réel de Quest, tracking/manettes, confort ni performance validés.

## Reproduire et continuer

Voir `docs/BUILDS.md` pour les arguments de build et la procédure du test. À partir d'un build de développement : `./tools/Test-PCBuild.ps1` puis, si un contrôle graphique est nécessaire, `./tools/Test-PCBuild.ps1 -Graphics`. Chaque run écrit dans un dossier unique sous tmp/unity, vérifie exitCode/rapport récent et se termine sans objet permanent de test.

Suite prête : activer Android, relire validation complète et chemins SDK/NDK/JDK, construire l'APK local et contrôler rapport/manifeste, puis essai sur casque identifié/autorisé. Essai utilisateur PC séparé encore nécessaire. Réglage conseillé pour le prochain prompt : GPT-6.1 Sol — High.
