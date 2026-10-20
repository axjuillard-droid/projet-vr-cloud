# Reprise par une autre IA — 20 octobre 2026

L'utilisateur demande une première démo nettement plus complète, puis demande une transmission car il ne lui reste que 5 % de crédit. Arrêter les ajouts ; reprendre avec les sources et les preuves existantes. Lire AGENTS.md, STATE.md, puis ce fichier. Français obligatoire. Ne pas confondre compilé, testé automatiquement sur PC, essai humain et casque.

## Résultat actuel

Six bornes originales ajoutées au data center : réseau, traitement, stockage, refroidissement en deux actions, électricité en deux actions et supervision. Objectifs/HUD, narration textuelle provisoire, flux lumineux schématique, synthèse et retour Chambre/recommencer codés. WASD/ZQSD/flèches, clic/E, Échap pour suspendre/reprendre les commandes, Entrée pour revenir à la fin, R pour recommencer.

**Démo implémentée et compilée ; parcours complet NON validé.** Dernier build Windows du 20/10 à 15 h 10 Paris : 0 erreur, 0 avertissement, 45,8067 s, 142 552 464 octets rapportés. Exe local : ConnectionTest/Builds/Windows/CloudVR-PC.exe, conserver tout le dossier. Source et scènes générées rapatriées. Ancien player conservé dans Windows-PreDemo-20261020 ; les versions T07 plus anciennes restent dans leurs dossiers.

Dernier test : tmp/unity/pc-player-20261020-151130-5eda41/, exitCode=1, pas de timeout. **17 contrôles réussis**, jusqu'à la borne refroidissement comprise. Le test s'arrête sur « Raycast atteint borne 4 » : index zéro, donc **borne 05 ÉLECTRICITÉ**. Batterie, supervision, synthèse, retour et recommencer restent à contrôler. Captures arrivée/réseau/chaleur disponibles. Les poses de caméra sont contrôlées par le test ; ce n'est pas une marche humaine complète. Aucun nouvel APK ni essai Quest.

## Prochaine tâche prête et bornée

Diagnostiquer le raycast sur la borne 05, sans reprendre tout le projet ni refaire les décors. Dans DemoSmoke, les poses sont station.position + 1,8 m vers le centre de l'allée. Pour les bornes du fond, cela peut faire passer le rayon par une baie secondaire : **hypothèse géométrique**, le collider réellement touché n'est pas encore enregistré. Relever le nom/position du collider touché ; choisir une position d'approche réellement dégagée ou déplacer les dernières bornes si nécessaire pour un humain. Ne pas contourner l'interaction par un appel direct à Activate pour faire passer le test.

Puis reconstruire sur la copie existante, tester les huit actions et le retour. Comparer les captures, puis essai utilisateur naturel. Respecter la règle AGENTS : après deux échecs sur un même blocage, consigner et arrêter pour revue humaine. Le blocage actuel de raycast a été constaté une fois. Ne pas relancer les anciens appels MCP qui ont déjà échoué deux fois sans changement de connexion.

## Fichiers à connaître

- Assets Unity sous ConnectionTest/Assets/CloudVR/ : Scripts/DemoTour.cs (états, HUD, actions), DemoStation.cs (clic/XRI), DemoRoom.cs (accueil/conclusion), PCPlayerController.cs (pause explicite et ZQSD).
- Editor/DemoTourBuilder.cs : ajoute/remplace le groupe Demo_Parcours dans DataCenter ; préserve le décor. Six positions x=±3,35 ; z=3,3 / 6,4 / 9,5. Scènes propres requises, retourne sur Chambre.
- Tests/DemoSmoke.cs : scénario réel par souris/clavier synthétiques, avec repositionnement du rig entre bornes. Les assertions restent exigeantes.
- tools/Build-DemoCopy.ps1 et tools/DemoCopyBuild.cs : build isolé reproductible ; tools/Test-PCBuild.ps1 -Demo pour le scénario.
- docs/DEMO.md : parcours, commandes, limites pédagogiques ; docs/validation/demo-2026-10-20/ : rapports et captures, y compris tentatives infructueuses.

## Reprendre les builds sans reperdre les caches

Lire tmp/unity/demo-copy-last.json. Copie existante locale : <TEMP>/CloudVR-Demo-749cb06f. L'éditeur est Unity 6000.0.58f2-x86_64. Le premier import/build a été long ; les builds suivants ont pris 36,5 / 27,6 / 45,8 s pour le BuildReport, hors démarrage. Ne pas promettre une durée fixe.

```powershell
./tools/Build-DemoCopy.ps1 -Resume
# Consulter log/PID dans tmp/unity/demo-copy-last.json ; attendre l'arrêt normal.
./tools/Build-DemoCopy.ps1 -Complete
./tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120
```

Resume synchronise Assets/Packages/ProjectSettings et sauvegarde le journal précédent. Complete exige rapport frais, succès, arrêt normal et empreintes originales inchangées avant recopie. Les scènes identiques ne sont pas réécrites : Unity avait verrouillé Chambre alors qu'elle était identique. Si l'éditeur original reste ouvert, rouvrir Chambre depuis le disque avant de sauvegarder une ancienne scène en mémoire.

Dans la copie seulement, MCP éditeur est omis pour éviter son clone Git bloqué ; sa dépendance Newtonsoft inutilisée est éliminée par UPM. Pas de mise à niveau des packages du jeu. Manifest/lock originaux préservés. Ne pas copier les réglages de la copie sur l'original. Sans accès local à cette copie, lancer le script sans Resume pour en créer une nouvelle (coût d'import à prévoir).

## Historique de test utile : éviter de rediagnostiquer

1. player-first-failed : clic initial non reconnu. Corrigé dans le probe en réutilisant PhotoInteractionSmoke : MakeCurrent, centre de l'écran et verrouillage avant le bouton. Clic ensuite réussi.
2. player-walk-failed : avancée bloquée. Nouveau garde Cursor.lockState inadapté au player masqué ; remplacé par InputPaused explicite via Échap. Dernier test : avancée z=1,5 → 2,5, pause/reprise réussies et curseur OS None. Cela ne prouve PAS la cause de l'ancien échec T07a du 05/10.
3. player-capture-failed : ScreenCapture.CaptureScreenshotAsTexture retournait null dans le player masqué. Remplacé par Camera.Render vers RenderTexture, méthode déjà éprouvée dans le projet ; captures désormais produites.
4. player-final-incomplete : 17 contrôles réussis, raycast borne 05 à diagnostiquer. Pas de nouvelle tentative après la demande de transmission.

## Préserver et publier

ProjectSettings/ProjectSettings.asset était déjà modifié avant cette tâche ; conservé, **à exclure des commits**. SHA256 initial : A58229EB41CD8117914A2B512CEDEB32870E5C2C020C16792D452D16155F30EB. Le PDF universitaire CDC_INSIDE_THE_DATA_CENTER.pdf est local non suivi : ne pas le publier ; préserver aussi Le cloud.docx. Builds, Library et tmp sont ignorés.

Utilisateur autorise des commits/pushes réguliers et le README illustré. Avant publication, anonymiser chemins utilisateur, machine et IP dans les preuves, sans changer résultats/horodatages ; garder les originaux sous tmp. Pas de push forcé. Vérifier STATE/BACKLOG pour le statut réel de publication de cette transmission. Ne pas prétendre qu'un commit est poussé sans confirmation.

Le retour humain du 20/10 confirme seulement que l'ancien exe T07a permet d'atteindre/contourner la baie. Le nouveau parcours doit encore être testé par l'utilisateur. Voix, validation pédagogique complète, performances et matériel Quest restent à faire. Pas de web ni RAG réel à ajouter spontanément.

## Prompt de reprise à transmettre

« Lis AGENTS.md, docs/STATE.md et docs/HANDOFF.md. Reprends T08 au raycast de la borne 05, relève le collider réellement touché, corrige l'approche ou la disposition puis termine la validation de la démo PC avec le build isolé déjà en cache. Préserve les modifications utilisateur et les preuves d'échecs. N'ajoute pas de nouvelles fonctionnalités avant un parcours complet réussi. »
