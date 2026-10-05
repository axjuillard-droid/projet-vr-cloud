# Ressources évaluées

## T07 — Première arrivée au data center, 5 octobre 2026

Besoin borné : petite salle et baie repérable après l'envoi, PC/Quest/Unity 6000.0.58f2. Réutiliser les rigs du projet et XRI 3.0.11 déjà importé (licence UCL consignée), les primitives et URP/uGUI présents. [Kenney Furniture Kit](https://kenney.nl/assets/furniture-kit) et [licence CC0/attribution facultative](https://kenney.nl/support) vérifiés : mobilier réutilisable dans un dépôt public, mais pas un modèle de baie évalué ni un besoin pour ce premier repère. La baie Poly Pizza reste sans licence/version suffisamment confirmée dans T03, donc pas d'import. Créer une baie pédagogique originale avec quelques cubes et matériaux partagés, enregistrée en prefab ; adaptation faible, coût réel à mesurer sur Quest. Aucun logo ni modèle tiers copié, aucune nouvelle dépendance. Source du chargement : [SceneManager.LoadScene, Unity 6000.0](https://docs.unity3d.com/ja/6000.0/ScriptReference/SceneManagement.SceneManager.LoadScene.html), qui recommande LoadSceneAsync ; signature de LoadSceneAsync vérifiée par réflexion live. Réutiliser l'API native pour une transition unique ; ce chargement ne représente pas le trajet physique réel de la photo. Textes provisoires fondés sur PARCOURS.md, narration finale non figée.

## T06a — Points blancs dans l'éditeur, 5 octobre 2026

Configuration reproduite : cible Android, Mobile_RPAsset, Intel Iris Xe, Direct3D11. [Unity UUM-121981](https://issuetracker.unity.com/issues/6091/flickering-bright-white-dots-in-the-scene-when-the-android-platform-is-selected-and-dx11-graphics-api-is-used-with-irisr-xe-graphics-gpu) décrit les mêmes points blancs et propose HDR désactivé ou ombres douces activées. [UUM-102292](https://issuetracker.unity.com/issues/1274/lit-shaders-produce-sparkling-artifacts-on-3d-objects-when-using-certain-intel-integrated-gpus-with-android-build-profile-selected-on-directx11) décrit également ce contexte dans l'éditeur. Sources officielles consultées. Comparaison locale : HDR seul insuffisant ; ombres douces activées font disparaître les points, retour au réglage précédent les reproduit. Retenir ce réglage URP déjà présent, sans nouveau package, copie de code tiers ni asset à redistribuer. Licence du package URP inchangée. Coût Quest encore à mesurer ; aucun changement de version Unity ou de pilote. Preuves render-2026-10-05/.

## T06 — Contrôle statique APK, 5 octobre 2026

Contrôle du manifeste : OpenXR 1.14 installé ajoute `oculus.software.eye_tracking required=true` et `com.oculus.permission.EYE_TRACKING`, même avec Eye Gaze désactivé ; constat dans le XML Gradle et l'APK initial. [Incident Unity correspondant](https://issuetracker.unity.com/issues/17293) (contenu détaillé non accessible, intitulé indexé seulement) ; source locale `ModifyAndroidManifestMeta.cs` inspectée. Réutiliser le callback officiel [IPostGenerateGradleAndroidProject Unity 6000.0](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Android.IPostGenerateGradleAndroidProject.html), vérifié par réflexion, après les callbacks XR (ordre 1/10 dans leurs sources). Correctif original de projet, sans upgrade ni modification du cache : retirer ces deux entrées si Meta actif et Eye Gaze inactif. Aucune copie de code tiers ; pas de nouvelle dépendance. Si le projet ajoute d'autres usages du suivi oculaire, revoir ce correctif et le runner statique. Le contrôle final de l'APK reste nécessaire avec les builds incrémentaux.

Réutiliser AAPT2 et apksigner du SDK intégré à Unity plutôt qu'ajouter un framework : [AAPT2, commande dump](https://developer.android.com/tools/aapt2), [apksigner, vérification](https://developer.android.com/tools/apksigner). Recherche documentaire officielle et aide locale consultées ; SDK build-tools 34.0.0, Java intégré 17.0.9 exécuté. Script original `tools/Test-AndroidApk.ps1` : badging/manifeste, signature, ABI/bibliothèques natives, empreinte SHA256. Aucun binaire SDK ni code tiers redistribué, aucune nouvelle installation ; les conditions du SDK Android restent applicables aux outils locaux. Effort faible, compatible avec les outils Windows présents. Contrôle statique distinct d'un test Quest.

Reprise après rejet du chemin accentué : réutiliser [Unity batchmode/-executeMethod](https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html) et BuildPipeline, déjà utilisés par le MCP installé. Copie de compilation locale ASCII avec les mêmes sources/packages, sans recopier Library ni déplacer l'original. Helper et runner originaux ; pas de nouvelle dépendance. Input Handling = New dans la copie : [Unity 6 signale que Both n'est pas pris en charge sur Android](https://unity.com/es/releases/editor/beta/6000.0.0b16). La restriction de chemin est prouvée par le dialogue et BuildReport de ce projet ; l'avertissement Both est une précaution distincte.

## T06 — Build et test Windows, 5 octobre 2026

Réutiliser `manage_build` du MCP installé (MIT) pour BuildPipeline/BuildReport et le test PhotoInteractionSmoke du projet pour la régression dans un player de développement. Références officielles : [BuildPipeline.BuildPlayer Unity 6000.0](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/buildpipeline/buildplayer), [arguments du player Unity 6000.0](https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html). Pas de nouveau framework, copie de code tiers ou dépendance. Le mode batch sans graphics permet de vérifier logique et états ; il ne valide pas rendu, ergonomie ni performance. Le test embarqué est réservé aux builds Windows de développement et lancé sur argument explicite.

Résultat : build Windows réussi et 9 vérifications dans le player en headless puis avec Direct3D11/captures ; voir validation/pc-build-2026-10-05/. Préparation Quest basée sur OpenXR 1.14 installé (licences déjà évaluées), ses API XR Management/MetaQuest et sa [documentation Meta Quest Support](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.14/manual/features/metaquest.html). Aucun SDK Meta supplémentaire ni nouveau sample ajouté. MinSdk32/targetSdk34 choisis d'après les [valeurs recommandées par Meta pour les appareils actuels](https://developers.meta.com/vr/resources/publish-mobile-manifest/) ; [Horizon OS v65 minimum avec le loader Unity OpenXR](https://developers.meta.com/vr/documentation/unity/min-os-versions/), à rapprocher du casque réel. Licence Unity conservée par référence aux packages, code de configuration original. Préparation et audit partiel réussis, aucun APK ni essai casque.

## T06 — Retour visuel photo : uGUI, 5 octobre 2026

Besoin : affichage Ready/Sending/Completed sur l'écran du PC, utilisable ensuite en VR. Candidat retenu pour le prochain essai : Canvas World Space + `UnityEngine.UI.Text`, déjà fournis par uGUI 2.0.0 dans le manifest, sans téléchargement ni dépendance supplémentaire. Documentation primaire consultée dans `Library/PackageCache/com.unity.ugui@aa507f3228f0/Documentation~/script-Text.md` ; type vérifié par réflexion dans Unity. Recherche externe ciblée effectuée ; l'URL `manual/class-Text.html` n'est pas accessible via le navigateur d'outils, donc ne pas la présenter comme documentation lue. La documentation locale est la référence effectivement inspectée.

Licence vérifiée dans `LICENSE.md` du package : Unity UI copyright Unity Technologies ApS 2015–2020, Unity Companion License. Réutiliser par référence au package ; ne pas copier le cache. Coût d'adaptation estimé faible, non mesuré. Pas de texte ou image tiers copiés ; les libellés de prototype restent provisoires. Mise à jour après reconnexion du 05/10 : Canvas World Space implémenté, police intégrée LegacyRuntime.ttf, compilation et test synthétique dans l'éditeur PC réussis, captures des trois états inspectées. Aucun build/Quest ni confort utilisateur validé. Le problème TextMesh précédent n'a pas reçu de cause confirmée. Voir docs/validation/photo-2026-10-05/.

## T06 — Starter Assets XRI, 5 octobre 2026

Configuration de téléportation fondée sur la [documentation officielle TeleportationArea 3.0.11](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/teleportation-area.html) et les sources locales du package : couche partagée rayon/zone, collider explicite, déclenchement au relâchement, provider assigné et filtre de normale. ControllerInputActionManager des Starter Assets réutilisé pour choisir téléportation/Snap Turn. Pas de nouveau package ni asset tiers ; couleur de zone originale via shader URP déjà présent.

Besoin : préparer un rig Quest avec interaction et téléportation, Unity 6000.0.58f2 / XRI 3.0.11. La [documentation officielle Starter Assets 3.0.11](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/samples-starter-assets.html) confirme la présence des prefabs XR Origin et de téléportation. Sample importé via `UnityEditor.PackageManager.UI.Sample.Import()` depuis le package déjà installé, sans mise à niveau de Unity ni des packages. Dossier : `ConnectionTest/Assets/Samples/XR Interaction Toolkit/3.0.11/Starter Assets/`.

Licence locale du package vérifiée : copyright Unity Technologies 2025, Unity Companion License, référence à la [UCL](https://unity.com/legal/licenses/unity-companion-license). Gratuit dans ce cadre Unity ; conserver licence et notices, ne pas relicencier les samples sous la licence du code propre. Copie de la notice conservée dans le dossier importé. Les dépendances nécessaires étaient déjà présentes ; console sans erreur de compilation après import. Mise à jour du 05/10 : prefab instancié dans Chambre, zone de téléportation et sélection écran raccordées ; compilation, 12 vérifications logiques XR sans casque et régression PC réussies. Voir docs/validation/xr-2026-10-05/. Aucun build ni essai Quest validé. Effort d'adaptation estimé moyen, non mesuré.

## T04 — Référence pour l'installation et les acteurs, 28 septembre 2026

Besoin : choisir un cas de data center crédible pour le parcours photo. Référence gratuite à consulter : [Equinix, définition de la colocation](https://www.equinix.com/what-is-colocation) et [documentation des services de colocation](https://docs.equinix.com/colocation/). Ces sources d'opérateur décrivent l'accueil d'équipements clients et les infrastructures communes ; elles ne constituent pas une comparaison indépendante du marché.

Conclusion reuse-first : utiliser ces descriptions comme références factuelles pour P06, puis rédiger un scénario fictif adapté aux huit étapes. Aucun texte, photo, logo, plan ou modèle de ces pages n'est intégré au projet. Aucune licence ouverte de redistribution de leurs médias n'est établie : consultation gratuite ne vaut pas autorisation de copie dans un dépôt public. Conserver les liens comme références. Pas d'asset ni de dépendance à adapter pour PC/Quest à ce stade ; compatibilité et effort technique non évalués. La sélection T03 reste disponible pour la production ultérieure.

MCP for Unity 10.0.0 est intégré à ConnectionTest et testé en lecture et création d’objet par MCP HTTP. Aucun asset de data center sélectionné. Ce registre conserve les recherches et obligations de licence.

## Connecteurs évalués le 25 septembre 2026
- MCP for Unity : https://github.com/CoplayDev/unity-mcp ; licence MIT : https://github.com/CoplayDev/unity-mcp/blob/main/LICENSE. v10.0.0 installé et testé en lecture HTTP et création d’un objet vide le 25/09/2026 ; propriétés et console relues, sauvegarde non testée. Gratuit pour le pont local ; conserver les mentions MIT si redistribué.
- MCP for Blender : https://github.com/ahujasid/mcp-for-blender ; licence MIT : https://github.com/ahujasid/mcp-for-blender/blob/main/LICENSE. Candidat pour contrôle interactif, Blender 3.0+ annoncé. Pont local gratuit, génération premium facultative exclue du test. Conserver les mentions MIT si redistribué. Non installé ni testé.
- Choix provisoire : pilotage Blender par Python déjà fonctionnel sans connecteur ; tester MCP pour le contrôle des scènes ouvertes. Voir TOOLING.md pour les preuves et prérequis.

Pour chaque ressource étudiée, consigner :
- besoin couvert et nom ;
- lien vers la source et vers la licence ;
- date de vérification, version si pertinente ;
- gratuité réelle, contraintes d’usage et de redistribution ;
- attribution requise ;
- compatibilité Unity/Quest/PC ou Blender ;
- décision : retenue, rejetée ou à vérifier, avec une raison courte.

Séparer les contenus pédagogiques sourcés des éléments téléchargés. Avant toute publication, les éléments intégrés devront avoir une attribution conforme ; aucune licence globale du projet n’est choisie pour l’instant.

## T03 — Bases du prototype évaluées le 25 septembre 2026

Besoin : interaction simple et déplacement dans une petite salle, borne et baie de serveurs, Unity 6000.0.58f2 / URP 17.0.4, PC et Quest. Recherche documentaire uniquement : aucun téléchargement, installation ou essai de ces candidats. Le manifest local contient Input System 1.14.2, mais ni XR Interaction Toolkit ni OpenXR en dépendances directes. Les estimations d’effort ci-dessous sont des appréciations, pas des mesures.

| Ressource | Décision proposée et adaptation | Compatibilité et limites |
|---|---|---|
| XR Interaction Toolkit 3.0.11 + Starter Assets | Réutiliser pour interactions, UI et locomotion ; effort moyen de configuration. Importer seulement les samples utiles du package choisi. | La documentation Unity 6000.0 désigne 3.0.11 comme version publiée pour cet éditeur. Résolution des dépendances et compilation avec le projet actuel à vérifier. Le contrôle PC clavier/souris du livrable reste à concevoir ; une simulation XR ne valide pas à elle seule ce livrable. |
| OpenXR Plugin, documentation 1.14.0 | Retenir le principe OpenXR pour la cible Quest ; version exacte à fixer dans Package Manager avec les dépendances. Effort moyen. | Documentation : Unity 2021.3 LTS+, Meta Quest Android arm64/Vulkan. Modèle de Quest inconnu : aucun support matériel ni performance certifié. |
| Kenney Space Station Kit 1.0 | Candidat pour modules de salle ; sélectionner des éléments sobres et adapter l’apparence. Effort faible à moyen. | OBJ/FBX/glTF, Unity annoncé par l’auteur. Import FBX et matériaux URP à vérifier ; décor spatial à ne pas présenter comme architecture réaliste de data center. |
| Kenney Furniture Kit 1.0 | Candidat pour mobilier secondaire uniquement ; effort faible à moyen. | Pack 3D CC0 ; contenu précis, formats de l’archive, échelle et matériaux à inspecter avant intégration. Ne constitue pas un kit de baies. |
| « server rack », Jeremy Eyring, Poly Pizza | Candidat direct pour une baie, en attente de clarification de licence ; effort moyen : inspection Blender, échelle, matériaux, colliders et éventuelle séparation des éléments. | OBJ/glTF annoncés. Mention Creative Commons Attribution, version non exposée dans la page lue : aucune intégration/redistribution tant que la licence exacte et sa provenance ne sont pas confirmées. Géométrie et performance non mesurées. |
| Quaternius Sci-Fi Essentials Kit (novembre 2024) | Alternative non prioritaire : style futuriste et offre gratuite partielle. | La page distingue une partie gratuite et des éditions supplémentaires ; projet URP et fichiers Blend associés à l’édition Source. Ne pas supposer que tout est gratuit. |
| XR Interaction Toolkit Examples, branche main | Écarter comme base à copier pour cette version du projet ; conserver comme référence. | Le dépôt actuel annonce Unity 6000.3 et XRI Examples 3.4.0. Ne pas mettre à niveau le projet pour importer cette démonstration. |

### Licences, coût et dépôt public

- XRI : [licence du package 3.0](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/license/LICENSE.html), Unity Companion License. La [licence UCL](https://unity.com/legal/licenses/unity-companion-license) autorise sans redevance l’usage et la distribution dans son cadre Unity ; conserver licence, copyright et éventuelles notices tierces pour les portions reprises. Une publication du projet Unity est compatible avec ce cadre, pas une relicence des samples sous la licence de notre code. Préférer manifest/lock aux copies du cache des packages.
- OpenXR : [licence 1.14](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.14/license/LICENSE.html), sources sous UCL et autres éléments sous Unity Package Distribution License. Référencer le package par UPM ; ne pas redistribuer son contenu binaire comme un asset libre. Aucun service payant nécessaire pour le principe retenu ; la licence de l’éditeur reste distincte.
- Kenney : packs individuels gratuits, [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/), réutilisation/adaptation et redistribution des assets permises, y compris dans le dépôt public. Attribution non obligatoire selon la [FAQ de l’auteur](https://kenney.nl/support) ; conserver provenance et licence de l’archive par traçabilité, crédit Kenney recommandé.
- Quaternius : CC0 annoncé ; mêmes possibilités pour les fichiers effectivement fournis sous CC0, mais ne pas confondre droit de réutilisation et accès gratuit à toutes les éditions. Le périmètre exact de l’archive gratuite reste à inspecter.
- Baie Poly Pizza : gratuit annoncé ; attribution nécessaire d’après l’intitulé, obligations exactes à vérifier avec la version de licence. Conserver auteur, URL, licence et modifications si le candidat est retenu. Ce candidat n’est pas encore autorisé à entrer dans le dépôt.

### Sources fonctionnelles et assets

- [XRI disponible pour Unity 6000.0](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.xr.interaction.toolkit.html) ; [Starter Assets 3.0](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/samples-starter-assets.html).
- [OpenXR 1.14 : prérequis et plateformes](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.14/manual/index.html).
- [Space Station Kit](https://kenney.nl/assets/space-station-kit) et [formats, gratuité, licence par l’auteur](https://kenney-assets.itch.io/space-station-kit).
- [Furniture Kit](https://kenney.nl/assets/furniture-kit).
- [Baie de Jeremy Eyring](https://poly.pizza/m/6ijQclm8jxw).
- [Quaternius Sci-Fi Essentials Kit : éditions et formats](https://quaternius.com/packs/scifiessentialskit.html).
- [Exemples officiels XRI : prérequis actuels](https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples) et [licence UCL du dépôt](https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples/blob/main/LICENSE.md).

### Conclusion de recherche

Proposition : XRI + OpenXR, quelques modules Kenney CC0, puis inspection de la baie candidate. Si la licence ou la structure de cette baie ne conviennent pas, créer un prefab pédagogique simple de baie et de modules serveurs après cette recherche bornée. Aucun modèle de refroidissement suffisamment évalué ici : commencer par une représentation schématique, à sourcer lors de T04, plutôt que prétendre disposer d’une simulation physique.

Avant de retenir définitivement un asset : vérifier le fichier de licence livré, l’échelle, les pivots, les triangles, les matériaux URP, les colliders et le coût sur le casque réel. T03 termine la sélection documentaire ; les imports et validations techniques appartiennent aux tâches suivantes.
