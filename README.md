# CloudVR — Visite immersive d’un data center

**Du clic sur « envoyer une photo » aux équipements physiques du cloud.** Projet universitaire ESILV année 5, Unity 6000.0.58f2 / URP / C# / OpenXR.

**État au 22 octobre 2026 : voyage réseau 3D livré à 11 h 55, 68/68 contrôles PC réussis.** Le clic photo lance six étapes sur 7,50 s : approche de la box, trajet dans la fibre avec signaux animés, puis campus avant l'arrivée dans la cour. Narration/progression vérifiées dans la fenêtre réelle du player. L'écran n'annonce plus de stockage avant la visite. [Preuves et capture native](docs/validation/network-3d-2026-10-22/README.md).

P1 conserve l'inspection CPU/RAM/NVMe et la remise en place du serveur, puis le diagnostic de fuite d'air et le retour thermique progressif. [Preuves P1](docs/validation/p1-2026-10-21/README.md). En VR, le film est présenté sur un écran fixe ; huit contrôles de logique par API sans casque sont réussis, **rendu et confort casque non validés**.

![Voyage réseau observé dans la fenêtre réelle du player, sous-titres et progression visibles](docs/validation/network-3d-2026-10-22/native-network-hud.jpg)

P0 conserve l'allée de 2,20 m, les baies regroupées, le sas transparent et le local électrique accessible. Les groupes électrogènes qui affleuraient le mur ouest sont déplacés hors du bâtiment : [sept comparaisons avant/après](docs/validation/west-wall-2026-10-21/README.md), [jalon P0](docs/validation/p0-2026-10-21/README.md). Jalons P0/T20/P1 publiés sur `main` : [commit cddd29a](https://github.com/axjuillard-droid/projet-vr-cloud/commit/cddd29a4dc8ec27334db5424fc0f21de027e914f), push confirmé.

**Travail restant identifié par l’audit :** Réseau, Stockage, Électricité et Supervision conservent leurs mécaniques antérieures. T18 doit approfondir le local TGBT et la supervision ; voix finale, rythme de lecture, performances et confort matériel restent à valider. [Audit critique](docs/AUDIT_GLOBAL_2026-10-21.md), [état de reprise](docs/STATE.md).

Les corrections disponibles :

- Chambre ambrée, directionnelles désactivées, lampe à 2700K, lueur écran, guirlande et pluie procédurale conservées.
- Envoi photo relié à la transition V2 devenue 3D : six étapes sous-titrées pendant 7,50 s, signaux mobiles, vue du campus puis chargement asynchrone. Mode confort sur écran fixe en VR. Trajet illustratif, aucun réseau réel mesuré, aucune photo personnelle transférée.
- Apparition PC/VR dans la cour à Z=−28 m, face à la façade 28×18×14 m ; ancienne boîte primitive 10×12 m supprimée.
- Sas et raccords d'entrée à Z=−4..0 m : chambre cylindrique nominale de 1,8 m, portes transparentes interverrouillées et progression du scan illustratif de 2 s dans le monde. Aucun dispositif biométrique réel.
- Salle blanche Z=0..40 m, sol continu et **32 baies au total**, dont Baie_01 à Z=14 m. Deux rangées denses entre Z=9,5 et 23,9 m autour d'une allée de 2,20 m, toit/impostes/niches de confinement raccordés et portes automatiques. Infrastructures schématiques ; aucune certification Tier ni performance thermique revendiquée.
- Local TGBT/UPS en aile attenante, porte franchissable à Z=30 m, vitrage d'observation et équipements tournés vers la circulation.
- Six bornes à gauche, Z=8/14/20/26/32/38 m. Les objectifs et textes apparaissent sur leurs écrans 3D ; HUD massif du data center supprimé, descentes PDU gênantes retirées. Baie interactive et diagnostic d’air P1 intégrés ; quatre autres activités encore à approfondir.

| Baie ouverte, serveur extrait — player | Fuite corrigée — player |
|---|---|
| ![Inspection CPU RAM NVMe](docs/validation/p1-2026-10-21/player/demo-server-extracted.png) | ![Température et auxiliaires fictifs après correction](docs/validation/p1-2026-10-21/player/demo-cooling-recovered.png) |

Captures réelles du player Direct3D11, poses de visée contrôlées par le probe. Sélections PC/XRI déclenchant des animations contraintes ; aucune validation casque ou manipulation libre.

| Vérification | Résultat |
|---|---|
| Build Windows, 22/10 à 11 h 55 Paris | Réussi, 28,43 s, 147 022 098 octets pour le build, 0 erreur / 0 avertissement BuildPipeline |
| Démo complète dans le player | **68/68 contrôles réussis**, Direct3D11, entrées synthétiques |
| Transition et navigation | Six étapes 3D en 7,50 s, caméra et signaux déplacés ; apparition −28 m, sas/scan, arrivée Z=39,0008 m en 24,75 s |
| Bifurcation électrique | Retour au clavier à Z≈30 puis entrée par la porte à X<−12 ; sans téléportation sur ce trajet |
| Six bornes et conclusion | Sept activations de console, inspection et remise en place obligatoires, mauvais choix thermiques et stabilisation ; synthèse, retour Chambre et reset R vérifiés |
| Petits déplacements | 100 commandes de 0,5 mm produisent 5 cm ; seuil CharacterController corrigé |
| Audit des scènes P0 à 22 h 05 | DataCenter : 577 MeshRenderers actifs, 0 matériau manquant / 0 shader non supporté ; Chambre : 0 matériau manquant. Compteur antérieur aux deux nouvelles dalles T20 |
| MCP Unity | HTTP/SSE connecté et utilisé, Chambre restituée propre, décor de film au repos inactif et narration raccordée ; console sans erreur retournée |
| Confort VR par API sans casque | 8/8 contrôles de logique ; aucune validation du rendu optique, des manettes ou du confort matériel |
| Conditions de l'essai T17 | Probe automatisé dans le vrai player ; pas d'essai humain de confort ou de benchmark |
| Android / Quest | APK antérieur inchangé ; ces corrections restent à compiler et tester dans le casque |

Le premier échec de visée P0 et sa correction sont conservés dans [les résultats P0](docs/validation/p0-2026-10-21/README.md), avec captures et journaux anonymisés. [État de reprise](docs/STATE.md), [backlog](docs/BACKLOG.md). Réduction de résolution des ombres signalée dans le journal ; aucun benchmark PC/Quest ni confort humain validé. Les cinq meshes de rangées ne représentent pas le nombre total de draw calls.

L’objectif reste un parcours photo guidé, puis un parcours IA scénarisé. Sous-titres provisoires, voix finale et validation pédagogique à préparer. Les extensions web et RAG réel ne sont pas implémentées. [Vision](docs/VISION.md), [parcours](docs/PARCOURS.md), [décisions](docs/DECISIONS.md).

## Démarrer le projet

### Prérequis

- **Unity 6000.0.58f2**, installé avec Unity Hub ; développement et tests réalisés sous Windows.
- Git et **Git LFS** : scènes, prefabs, matériaux et images sont suivis par LFS.
- Pour Android : Android Build Support, SDK/NDK et OpenJDK de cette version Unity.
- Pour tester la VR : un Meta Quest identifié, avec les outils d’installation et le mode développeur appropriés.

```powershell
git lfs install
git clone https://github.com/axjuillard-droid/projet-vr-cloud.git
cd projet-vr-cloud
git lfs pull
```

Dans Unity Hub, ajouter **`ConnectionTest`**, puis l’ouvrir avec la version indiquée. Laisser Unity importer les assets et résoudre les packages. Ouvrir `Assets/Scenes/Chambre.unity`.

Pour travailler dans Play sur PC, sélectionner Windows et la qualité PC. Attendre la fin de la compilation et vérifier la Console avant de lancer Play. Le MCP sert au pilotage automatisé de l’éditeur ; il n’est pas nécessaire pour jouer. Voir sa configuration dans [TOOLING.md](docs/TOOLING.md).

### Commandes PC

| Action | Commande |
|---|---|
| Se déplacer | ZQSD / WASD ou flèches directionnelles |
| Regarder / viser | Souris |
| Envoyer la photo | Clic gauche sur l’écran, à portée de 5 m |
| Sélectionner une borne ou une commande de baie | Viser, puis clic gauche ou E |
| Mettre en pause / reprendre | Échap |
| Fermer le player Windows | Alt + F4 |

Pour tester le livrable construit localement, lancer **`ConnectionTest/Builds/Windows/CloudVR-PC.exe`**. Conserver tout le dossier Windows avec l’exécutable, ses DLL et `CloudVR-PC_Data`.

Le dossier Windows livré contient P0, le correctif du mur, P1 et T17, et passe les 68 contrôles automatisés. Le lanceur `.exe` peut garder sa date du 20/10 à 14 h 58 : le contenu reconstruit est notamment dans `CloudVR-PC_Data/data.unity3d` (22/10, voir le manifeste T17) et les DLL. [Empreintes des fichiers livrés](docs/validation/network-3d-2026-10-22/delivery-manifest.json). Un essai libre reste utile pour apprécier le confort, la lisibilité et le rendu.

Les exécutables et APK sont des sorties locales ignorées par Git : **ce dépôt fournit les sources et les procédures de compilation**, pas un téléchargement de ces builds.

## Construire et tester

### Windows

Dans les Build Profiles Unity : Windows 64 bits, backend Mono ; inclure dans cet ordre :

```text
Assets/Scenes/Chambre.unity
Assets/Scenes/DataCenter.unity
```

Construire vers `ConnectionTest/Builds/Windows/CloudVR-PC.exe`. Utiliser un **Development Build** pour les contrôles automatiques embarqués ; leurs probes sont exclus des builds finaux et d’Android.

Depuis la racine du dépôt, le runner de compilation utilise une copie temporaire ASCII :

```powershell
powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait
powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Complete
```

`-Resume` réutilise une copie déjà créée ; sur une installation neuve, omettre cette option au premier lancement. Le builder maître est exécuté avant compilation ; `-Complete` vérifie le rapport avant de copier le player et les scènes générées.

La commande de validation actuelle est `./tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120`. Elle vérifie la transition complète, une marche continue à travers le sas, l'accès électrique et les bornes. **Dernier résultat : 68/68 contrôles réussis.**

La branche confort est contrôlée séparément par `./tools/Test-NetworkComfort.ps1` : huit contrôles par API dans le player sans casque. Cela ne remplace pas un essai Quest. Les scénarios historiques Photo/Journey ciblent l’ancien parcours et ne remplacent pas cette validation.

Les rapports sont écrits sous `tmp/unity/`. Les entrées clavier/souris sont synthétiques ; un essai manuel et un test casque restent distincts.

### Android / Meta Quest

Configuration actuelle : IL2CPP, ARM64, Vulkan, OpenXR / Meta Quest Support, application `com.cloudvr.prototype`. Les modèles déclarés sont Quest 2, Pro, 3 et 3S ; leur déclaration n’est pas une certification.

Le chemin historique contient un caractère accentué refusé par les Android Tools. Le script crée une **copie temporaire dans un chemin ASCII**, compile et rapatrie les sorties réussies, sans déplacer l’original :

```powershell
./tools/Build-AndroidCopy.ps1
$androidTools = 'C:/Program Files/Unity/Hub/Editor/6000.0.58f2-x86_64/Editor/Data/PlaybackEngines/AndroidPlayer'
./tools/Test-AndroidApk.ps1 -SdkRoot "$androidTools/SDK" -JdkRoot "$androidTools/OpenJDK"
```

L'APK généré (`ConnectionTest/Builds/Android/CloudVR-Quest.apk`, 73,60 Mo) intègre les deux scènes (`Chambre.unity` et `DataCenter.unity`) ainsi que la refonte visuelle complète et les 15 contrôles statiques validés. Pour réutiliser le cache de compilation IL2CPP lors des itérations suivantes, ajouter l'option `-Resume`.

Installation directe sur le casque connecté en USB via ADB :
```powershell
adb install -r ConnectionTest/Builds/Android/CloudVR-Quest.apk
```

### Anomalie de rendu connue dans l’éditeur

Des points blancs scintillants ont été reproduits en Play avec la cible Android, Intel Iris Xe et DX11, en concordance avec [Unity UUM-121981](https://issuetracker.unity.com/issues/6091/flickering-bright-white-dots-in-the-scene-when-the-android-platform-is-selected-and-dx11-graphics-api-is-used-with-irisr-xe-graphics-gpu). Activer les ombres douces dans le pipeline Mobile les a supprimés dans les captures comparatives. Leur coût sur Quest reste à mesurer. Voir les [preuves avant / après](docs/validation/render-2026-10-05/README.md).

## Organisation du dépôt

```text
ConnectionTest/
  Assets/Scenes/              Chambre et DataCenter
  Assets/CloudVR/
    Scripts/                 Photo, clic, déplacement et transition PC/VR
    Editor/                  Builders de scènes et configuration Quest
    Tests/                   Contrôles photo, parcours et logique VR
    Materials/               Matériaux partagés
    Prefabs/                 Baie originale réutilisable
  Assets/Samples/             Starter Assets XRI, avec leur licence
  Packages/                  Dépendances Unity par UPM
  ProjectSettings/           Réglages du projet
docs/                        Vision, décisions, état, builds et preuves
tools/                       Runners PowerShell et helper Android
skills/reuse-first/           Recherche avant création
```

`PhotoSender` gère l’envoi ; `ScreenClickHandler` relie le clic à l’écran ; `NetworkTransitionV2` joue les six étapes puis `StepTransition` charge DataCenter. `InputModeManager` choisit le rig PC/VR. Le builder maître `GrandMasterBuilder.BuildAll()` assemble les deux scènes, le campus et les six bornes ; menu **CloudVR > Build Scene > ★ DÉCORATION MONUMENTALE COMPLÈTE ★**. `DataCenterSceneBuilder` reste le constructeur historique de base : rejouer le builder maître pour obtenir la scénographie monumentale. Exécuter hors Play et avec scènes propres.

| Dépendance principale | Version du manifest |
|---|---|
| Universal Render Pipeline | 17.0.4 |
| Input System | 1.14.2 |
| XR Interaction Toolkit | 3.0.11 |
| OpenXR | 1.14.0 |
| XR Hands | 1.5.0 |
| Unity UI / uGUI | 2.0.0 |
| MCP for Unity | v10.0.0 |

## Prochaines étapes

1. Essai libre du nouveau player PC : confort, lisibilité, perception des volumes et appréciation du rendu.
2. Recompiler l’APK et tester tracking, téléportation, lisibilité, confort et performances sur un Quest identifié.
3. Valider les contenus pédagogiques et préparer la voix finale.
4. Développer le parcours IA scénarisé après validation utilisateur du parcours photo.

Les critères et statuts sont dans le [backlog](docs/BACKLOG.md). Le scénario complet, la narration finale et les entreprises à présenter ne sont pas encore figés.

## Méthode de travail et mises à jour

Tester chaque petit ajout dans **Play**, puis reconstruire l’exe lorsqu’une étape jouable cohérente est prête. Tester le player après l’export : il représente le livrable PC, tandis que Play montre les dernières sources. Garder les caches locaux pour les compilations incrémentales.

Publier des **commits courts et cohérents à chaque jalon vérifié**, avec un push GitHub après contrôle des fichiers. Actualiser ce README lorsque fonctionnalités, installation, validations ou feuille de route changent ; ajouter des captures réelles lorsqu’elles illustrent l’avancée. Builds, caches et sessions MCP restent locaux.

Pour reprendre avec Codex : lire [AGENTS.md](AGENTS.md), puis [STATE.md](docs/STATE.md). Les [capacités et limites de pilotage](docs/CAPACITES_CODEX.md) distinguent l’automatisation des essais nécessitant un utilisateur ou un casque. Chaque réponse du projet recommande un modèle et un niveau de réflexion pour la prochaine tâche.

## Sources, crédits et licences

Géométries de Chambre et DataCenter, baie et scripts propres sont créés pour ce projet. Les captures proviennent du player Windows. `Le cloud.docx`, sujet universitaire original, est conservé sans modification.

Les Starter Assets XRI conservent leur [Unity Companion License et notice](ConnectionTest/Assets/Samples/XR%20Interaction%20Toolkit/3.0.11/Starter%20Assets/LICENSE.md). Les dépendances UPM gardent leurs licences respectives. Le MCP for Unity est référencé depuis son dépôt, sans copie de son cache.

Aucune licence globale n’est encore attribuée au code propre ; ne pas en déduire une licence MIT pour tout le dépôt. Recherche, attributions et choix : [RESOURCES.md](docs/RESOURCES.md). Les ressources évaluées, dont les packs Kenney, ne sont pas toutes importées.
