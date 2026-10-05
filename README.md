# CloudVR — Visite immersive d’un data center

**Du clic sur « envoyer une photo » aux équipements physiques du cloud.**

Projet universitaire réalisé en binôme : une expérience pédagogique Unity sur **PC et Meta Quest**, pour comprendre les infrastructures derrière les services en ligne. Le prototype démarre dans une chambre, puis emmène le visiteur dans une première salle de data center.

**État au 5 octobre 2026 : premier parcours jouable et testé dans le player Windows.** Un premier APK Quest est compilé et contrôlé statiquement ; les essais dans un casque restent à faire.

![Première baie du prototype Windows : trois baies et le panneau « Le cloud a une adresse physique »](docs/validation/journey-2026-10-05/player-journey/datacenter-approach.png)

*Capture réelle du player Windows, 5 octobre 2026. Salle et baies originales simplifiées ; textes et rendu encore provisoires.*

## Le parcours disponible

1. Explorer la **Chambre** avec le clavier et la souris.
2. Viser l’écran du PC et cliquer pour envoyer une **photo d’exemple**.
3. Observer les états « prête », « envoi » et « terminé ».
4. Après la confirmation, arriver automatiquement dans **DataCenter**.
5. S’approcher de la **Baie 01**, parmi trois baies, avec déplacements et collisions.

L’envoi dure 2,5 secondes, puis la transition attend 2 secondes. Ces délais illustrent le scénario : **aucune photo personnelle n’est transférée et aucun trajet réseau réel n’est mesuré**. Le chargement asynchrone conserve le mode d’entrée PC/VR.

| Départ dans la chambre | Arrivée dans le data center |
|---|---|
| ![Chambre : bureau et écran d’envoi](docs/validation/journey-2026-10-05/player-photo/photo-ready.png) | ![Data center : accueil, première baie et repère de déplacement](docs/validation/journey-2026-10-05/player-journey/datacenter-arrival.png) |

*Captures du même build Windows. Les captures automatiques de caméra ne comprennent pas le viseur superposé de l’interface.*

Le parcours s’arrête à cette première exploration. L’interaction explicative avec la baie, les étapes réseau/traitement/stockage, le refroidissement et l’alimentation sont les prochains ajouts.

## Objectif pédagogique

Relier un usage quotidien aux serveurs, au réseau, au stockage et aux infrastructures communes d’un data center. La visite doit ensuite aborder les dimensions techniques, économiques, environnementales et stratégiques du cloud.

La cible interne est une démonstration d’environ **10 à 15 minutes**. Le parcours photo sera suivi d’un parcours IA scénarisé. La voix finale, les sous-titres et l’évaluation de la compréhension restent à préparer. La salle actuelle ne reproduit aucun site industriel ni aucune entreprise partenaire.

Voir la [vision](docs/VISION.md), le [parcours pédagogique](docs/PARCOURS.md) et les [décisions validées et propositions](docs/DECISIONS.md). Les extensions web et RAG réel ne sont pas implémentées.

## Ce qui est validé

| Élément | Niveau atteint au 05/10/2026 | À vérifier ensuite |
|---|---|---|
| Photo et transition Chambre → DataCenter | Implémentées, compilées et testées automatiquement sur PC | Essai utilisateur du nouveau parcours, lisibilité et confort |
| Player Windows, deux scènes | Build réussi, 0 erreur / 0 avertissement ; tests avec GPU Direct3D11 | Performance mesurée et compréhension du scénario |
| Parcours dans le player Windows | **13 contrôles réussis** : clic, chargement unique, mode, caméra, spawn, déplacement, collision | Entrées naturelles et ergonomie |
| Régression photo dans le même player | **9 contrôles réussis**, sortie normale du processus | Retour utilisateur sur les états affichés |
| Rigs VR entre scènes | **13 contrôles logiques réussis dans Unity**, téléportation injectée | Tracking, manettes, rayon, rotation et confort dans le casque |
| APK Quest initial | Compilation réussie ; **15 contrôles statiques réussis** | Nouveau build à deux scènes, installation et essai sur un Quest identifié |

**L’APK existant contient seulement Chambre.** Il ne comprend pas encore DataCenter ni le dernier réglage de rendu Mobile. Aucune validation matérielle Quest n’est revendiquée.

Le build Windows final du 5 octobre a pris **34,9 secondes avec les caches locaux** ; cette durée ne prédit pas celle d’un premier build. Les tests graphiques ont utilisé un Intel Iris Xe / Direct3D11 et ne constituent pas une mesure de FPS.

Rapports, empreintes SHA256, captures et tentatives échouées : [preuves du parcours](docs/validation/journey-2026-10-05/README.md). Le [contrôle APK](docs/validation/android-build-2026-10-05/README.md) est documenté séparément.

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
| Se déplacer | WASD ou flèches directionnelles |
| Regarder / viser | Souris |
| Envoyer la photo | Clic gauche sur l’écran, à portée de 5 m |
| Fermer le player Windows | Alt + F4 |

Pour tester le livrable construit localement, lancer **`ConnectionTest/Builds/Windows/CloudVR-PC.exe`**. Conserver tout le dossier Windows avec l’exécutable, ses DLL et `CloudVR-PC_Data`.

Les exécutables et APK sont des sorties locales ignorées par Git : **ce dépôt fournit les sources et les procédures de compilation**, pas un téléchargement de ces builds.

## Construire et tester

### Windows

Dans les Build Profiles Unity : Windows 64 bits, backend Mono ; inclure dans cet ordre :

```text
Assets/Scenes/Chambre.unity
Assets/Scenes/DataCenter.unity
```

Construire vers `ConnectionTest/Builds/Windows/CloudVR-PC.exe`. Utiliser un **Development Build** pour les contrôles automatiques embarqués ; leurs probes sont exclus des builds finaux et d’Android.

Depuis la racine du dépôt, après compilation :

```powershell
# Parcours complet, avec périphérique graphique et captures
./tools/Test-PCBuild.ps1 -Graphics -Journey

# Régression photo et resets
./tools/Test-PCBuild.ps1 -Graphics -TimeoutSeconds 90
```

Sans `-Graphics`, le runner effectue le scénario photo sans rendu. Il exige un rapport récent réussi **et** un code de sortie nul. Chaque exécution écrit dans un dossier distinct sous `tmp/unity/`. Ces tests injectent des entrées ; compléter avec un essai manuel.

### Android / Meta Quest

Configuration actuelle : IL2CPP, ARM64, Vulkan, OpenXR / Meta Quest Support, application `com.cloudvr.prototype`. Les modèles déclarés sont Quest 2, Pro, 3 et 3S ; leur déclaration n’est pas une certification.

Le chemin historique contient un caractère accentué refusé par les Android Tools. Le script crée une **copie temporaire dans un chemin ASCII**, compile et rapatrie les sorties réussies, sans déplacer l’original :

```powershell
./tools/Build-AndroidCopy.ps1
$androidTools = 'C:/Program Files/Unity/Hub/Editor/6000.0.58f2-x86_64/Editor/Data/PlaybackEngines/AndroidPlayer'
./tools/Test-AndroidApk.ps1 -SdkRoot "$androidTools/SDK" -JdkRoot "$androidTools/OpenJDK"
```

Le helper a été adapté pour les deux scènes ; **cette adaptation doit encore être compilée et validée dans un nouvel APK**. De nouvelles sources nécessitent une nouvelle copie : `-Resume` reprend une compilation précédente et ne synchronise pas l’ensemble des sources.

Tracking, manettes, performances et confort se vérifient ensuite sur le casque réel. Les paramètres et procédures sont détaillés dans le [guide des builds](docs/BUILDS.md).

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

`PhotoSender` gère l’envoi ; `ScreenClickHandler` relie le clic à l’écran ; `StepTransition` charge DataCenter ; `InputModeManager` transmet le mode et choisit le rig. `DataCenterSceneBuilder` reconstruit la salle et ses raccordements depuis **CloudVR > Build Scene > Data center et transition photo**. Ce builder régénère la scène : le réserver à une reconstruction volontaire, hors Play et avec scènes propres.

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

1. **Rendre la baie interactive** : réseau, traitement et stockage avec retour visuel et explications sourcées.
2. Compléter le parcours photo : infrastructures communes, alimentation, refroidissement et supervision.
3. Reconstruire l’APK à deux scènes et mesurer lisibilité, confort et performances sur un Quest identifié.
4. Faire tester le parcours, valider les contenus pédagogiques, ajouter voix et sous-titres.
5. Développer le parcours IA scénarisé, puis stabiliser la démonstration et sa documentation.

Les critères et statuts sont dans le [backlog](docs/BACKLOG.md). Le scénario complet, la narration finale et les entreprises à présenter ne sont pas encore figés.

## Méthode de travail et mises à jour

Tester chaque petit ajout dans **Play**, puis reconstruire l’exe lorsqu’une étape jouable cohérente est prête. Tester le player après l’export : il représente le livrable PC, tandis que Play montre les dernières sources. Garder les caches locaux pour les compilations incrémentales.

Publier des **commits courts et cohérents à chaque jalon vérifié**, avec un push GitHub après contrôle des fichiers. Actualiser ce README lorsque fonctionnalités, installation, validations ou feuille de route changent ; ajouter des captures réelles lorsqu’elles illustrent l’avancée. Builds, caches et sessions MCP restent locaux.

Pour reprendre avec Codex : lire [AGENTS.md](AGENTS.md), puis [STATE.md](docs/STATE.md). Les [capacités et limites de pilotage](docs/CAPACITES_CODEX.md) distinguent l’automatisation des essais nécessitant un utilisateur ou un casque. Chaque réponse du projet recommande un modèle et un niveau de réflexion pour la prochaine tâche.

## Sources, crédits et licences

Géométries de Chambre et DataCenter, baie et scripts propres sont créés pour ce projet. Les captures proviennent du player Windows. `Le cloud.docx`, sujet universitaire original, est conservé sans modification.

Les Starter Assets XRI conservent leur [Unity Companion License et notice](ConnectionTest/Assets/Samples/XR%20Interaction%20Toolkit/3.0.11/Starter%20Assets/LICENSE.md). Les dépendances UPM gardent leurs licences respectives. Le MCP for Unity est référencé depuis son dépôt, sans copie de son cache.

Aucune licence globale n’est encore attribuée au code propre ; ne pas en déduire une licence MIT pour tout le dépôt. Recherche, attributions et choix : [RESOURCES.md](docs/RESOURCES.md). Les ressources évaluées, dont les packs Kenney, ne sont pas toutes importées.
