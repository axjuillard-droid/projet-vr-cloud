# CloudVR — Visite immersive d’un data center

**Du clic sur « envoyer une photo » aux équipements physiques du cloud.** Projet universitaire ESILV année 5, Unity 6000.0.58f2 / URP / C# / OpenXR.

**État au 21 octobre 2026 : corrections de scénographie compilées, parcours complet en attente de revue humaine.** Le nouveau player Windows compile sans erreur ni avertissement. Deux essais de navigation ont échoué ; les anciens 26/26 contrôles ne valident pas cette version.

Les corrections disponibles :

- Chambre ambrée, directionnelles désactivées, lampe à 2700K, lueur écran, guirlande et pluie procédurale conservées.
- Envoi photo relié à la transition narrative V2 : six étapes sous-titrées avec progression pendant environ 10,8 s, puis chargement asynchrone. Ancien trajet fibre et chargement direct retirés. Trajet illustratif, aucun réseau réel mesuré, aucune photo personnelle transférée.
- Apparition PC/VR dans la cour à Z=−28 m, face à la façade 28×18×14 m ; ancienne boîte primitive 10×12 m supprimée.
- Sas cylindrique à Z=−4..0 m : portes coulissantes interverrouillées et pesée/scan illustratifs de 2 s. Aucun dispositif biométrique réel.
- Salle blanche orientée sur Z=0..40 m, sol continu, 32 baies de décor et baie interactive 01 à Z=14 m. Confinement et infrastructures représentés schématiquement ; aucune certification Tier ni performance industrielle revendiquée.
- Six bornes pédagogiques placées à Z=8/14/20/26/32/38 m, flux illustratif dirigé vers la baie déplacée. Clics et retour Chambre restent à revalider sur ce build.

| Cour extérieure du player | Transition réseau du player |
|---|---|
| ![Apparition réelle dans la cour](docs/validation/monumental-2026-10-21/attempt-2/demo-arrival.png) | ![HUD des six étapes réseau](docs/validation/monumental-2026-10-21/attempt-2/demo-network-transition.png) |

Captures Direct3D11 du 21 octobre. Elles ne prouvent pas une traversée complète ni une validation casque.

| Vérification | Résultat |
|---|---|
| Build Windows, 15 h 28 Paris | Réussi, 28,20 s, 146 785 672 octets, 0 erreur / 0 avertissement |
| Transition entière, spawn −28 m, déplacement initial, pause/reprise | Vérifiés dans les deux essais graphiques avec entrées synthétiques |
| Sas | Scan et fermeture des deux portes observés au premier essai ; second essai n’a pas atteint le sas |
| Marche continue jusqu’à +39 m | Deux échecs ; second essai à Z=−11,723 après 35 s. Cadence, timing ou collision restent à distinguer |
| Lancement sans smoke | Fenêtre du player ouverte et chambre observée |
| MCP Unity | Connecté au projet original, outils exposés, hors Play/compilation, zéro erreur retournée par la console |
| Android / Quest | APK antérieur inchangé ; aucune validation de ces corrections dans le casque |

La validation de navigation est arrêtée après deux tentatives, conformément à AGENTS.md. Une revue humaine du déplacement est requise avant reprise du diagnostic. [Rapports, limites et captures](docs/validation/monumental-2026-10-21/README.md), [état de reprise](docs/STATE.md), [backlog](docs/BACKLOG.md).

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
| Fermer le player Windows | Alt + F4 |

Pour tester le livrable construit localement, lancer **`ConnectionTest/Builds/Windows/CloudVR-PC.exe`**. Conserver tout le dossier Windows avec l’exécutable, ses DLL et `CloudVR-PC_Data`.

Le dernier exe contient les corrections T10, mais la traversée complète n’est pas validée. Revue humaine attendue : envoi, six étapes réseau, cour, sas et progression dans l’allée. Les détails des deux échecs sont conservés dans les preuves ci-dessus.

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

La commande de validation T10 est `./tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120`. Elle vérifie maintenant la transition complète et tente une marche continue à travers le sas avant les bornes. **Elle a échoué deux fois dans cette session ; attendre la revue humaine avant un nouvel essai.** Les scénarios historiques Photo/Journey ciblent l’ancien parcours et ne remplacent pas cette validation.

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

Priorité actuelle : revue humaine de la navigation T10 après les deux échecs. Ensuite, diagnostiquer la cadence du rendu, les temps de déplacement et les colliders via le MCP connecté, avant un nouvel essai explicitement repris.

1. Revalider la marche cour → sas → fond de salle, puis les six bornes et le retour Chambre.
2. Recompiler l’APK et tester tracking, téléportation, lisibilité, confort et performances sur un Quest identifié.
3. Valider les contenus pédagogiques et préparer la voix finale.
4. Développer le parcours IA scénarisé après stabilisation du parcours photo.

Les critères et statuts sont dans le [backlog](docs/BACKLOG.md). Le scénario complet, la narration finale et les entreprises à présenter ne sont pas encore figés.

## Méthode de travail et mises à jour

Tester chaque petit ajout dans **Play**, puis reconstruire l’exe lorsqu’une étape jouable cohérente est prête. Tester le player après l’export : il représente le livrable PC, tandis que Play montre les dernières sources. Garder les caches locaux pour les compilations incrémentales.

Publier des **commits courts et cohérents à chaque jalon vérifié**, avec un push GitHub après contrôle des fichiers. Actualiser ce README lorsque fonctionnalités, installation, validations ou feuille de route changent ; ajouter des captures réelles lorsqu’elles illustrent l’avancée. Builds, caches et sessions MCP restent locaux.

Pour reprendre avec Codex : lire [AGENTS.md](AGENTS.md), puis [STATE.md](docs/STATE.md). Les [capacités et limites de pilotage](docs/CAPACITES_CODEX.md) distinguent l’automatisation des essais nécessitant un utilisateur ou un casque. Chaque réponse du projet recommande un modèle et un niveau de réflexion pour la prochaine tâche.

## Sources, crédits et licences

Géométries de Chambre et DataCenter, baie et scripts propres sont créés pour ce projet. Les captures proviennent du player Windows. `Le cloud.docx`, sujet universitaire original, est conservé sans modification.

Les Starter Assets XRI conservent leur [Unity Companion License et notice](ConnectionTest/Assets/Samples/XR%20Interaction%20Toolkit/3.0.11/Starter%20Assets/LICENSE.md). Les dépendances UPM gardent leurs licences respectives. Le MCP for Unity est référencé depuis son dépôt, sans copie de son cache.

Aucune licence globale n’est encore attribuée au code propre ; ne pas en déduire une licence MIT pour tout le dépôt. Recherche, attributions et choix : [RESOURCES.md](docs/RESOURCES.md). Les ressources évaluées, dont les packs Kenney, ne sont pas toutes importées.
