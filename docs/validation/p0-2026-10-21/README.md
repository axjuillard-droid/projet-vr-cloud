# P0 — Implantation et interface dans le décor, 21 octobre 2026

Build Windows livré le **21/10/2026 à 22 h 05 min 23 s, heure de Paris**, Unity 6000.0.58f2. [Rapport](build-result.json) : Succeeded, 14,75 s de BuildPipeline, 146 931 182 octets pour le build, **0 erreur / 0 avertissement**. Ce total n'est pas la taille du seul `.exe`.

## Changements constatables

- **32 baies au total** : 31 baies fixes regroupées en cinq meshes par matériau et la Baie_01 interactive existante. Deux rangées denses entre Z=9,5 et 23,9 m, niches pour les bornes, façades séparées de **2,200 m mesurés**. La salle conserve ses 40 m ; toutes ses zones ne sont pas remplies de racks.
- Confinement central : toit à 2,2 m, impostes au-dessus des baies de décor de 2 m, retours de niches, raccords de Baie_01 et portes coulissantes automatiques d'extrémité. Représentation géométrique d'une allée froide, aucune étanchéité industrielle ou performance thermique mesurée.
- Local TGBT/UPS déplacé en aile attenante à X=−19/Z=30, accès de 2 m, porte automatique, vitrage d'observation et équipements orientés vers la circulation.
- Sas ramené à un diamètre nominal de 1,8 m, toiture légèrement débordante. Portes transparentes, progression du scan dans le monde ; raccords des anciennes grandes ouvertures pour empêcher le contournement immédiat par les côtés. Contrôle d'accès simulé, aucune biométrie réelle.
- Six consoles à gauche, toujours Z=8/14/20/26/32/38 m. Le texte apparaît sur leur écran 3D, avancé devant le pupitre pour éviter son occultation. HUD massif du data center supprimé, descentes PDU gênantes désactivées. L'accueil de la chambre et sa transition conservent leurs interfaces actuelles.
- Éclairages écran/lampe/guirlande de la chambre atténués. Zones de téléportation resserrées autour de la circulation et accès électrique préparé ; **aucun essai de téléportation en casque**.

## Vérification

Commandes exécutées séparément, avec contrôle du succès avant la suivante :

```powershell
powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait
powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Complete
powershell -ExecutionPolicy Bypass -File tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120
```

- [Audit géométrique pré-build](p0-layout-audit.json) : dix vérifications réussies, mesure de l'allée, 31 colliders fixes, cinq renderers de rangées, capsule centrale Z=3..39 et bifurcation électrique libres d'obstacles fixes. Les colliders des portes de proximité sont temporairement exclus de cette inspection statique, puis restaurés ; leur franchissement est vérifié dans le player.
- [Player Direct3D11](player/demo-smoke-result.json) : **40/40 contrôles**, [exitCode=0, passed=true](player/launch-result.json), fin à 22 h 06 min 57 s. Entrées clavier/souris synthétiques dans le vrai contrôleur ; cour → sas/scan → Z=39,000 m en 24,74 s de marche réelle, puis retour à Z≈30 et bifurcation vers X<−12 dans le local électrique. Six bornes/huit actions, synthèse et retour/reset Chambre réussis. Les visées aux bornes utilisent des poses contrôlées. Le nombre de frames logiques du probe n'est pas un FPS de rendu interactif.
- [Audit MCP de la scène livrée](scene-audit.json) : 577 MeshRenderers actifs, zéro emplacement de matériau manquant, zéro shader non supporté, trois portes de proximité et six bornes. Ce compteur n'est pas une mesure de draw calls ni de performance Quest. Chambre également relue : zéro matériau manquant ; éditeur laissé dans Chambre propre, hors Play, aucune erreur console retournée.
- Captures ci-dessous inspectées. [Journal intégral anonymisé](player/Player.log) : avertissement de réduction des six shadow maps pour l'atlas 2048 conservé, distinct du bilan BuildPipeline. Pas de benchmark ni d'essai humain de confort.

Le [premier essai P0 échoué](first-failure/demo-smoke-result.json) est conservé avec son [journal](first-failure/Player.log) : arrêt sur la visée de Borne_02. Le probe utilisait encore une pose alternée gauche/droite alors que les consoles avaient toutes été déplacées à gauche. Pose corrigée selon la position réelle. Un essai suivant a réussi ; le présent essai final revalide en plus les dernières positions d'écran et zones XR.

Une copie de livraison avait également rencontré un matériau déjà mappé sous Windows. Le runner ignore désormais les fichiers identiques et permet la reprise d'une copie partielle uniquement lorsque les scènes sont exactement identiques aux scènes générées. Les protections contre les modifications concurrentes et l'échec de build restent actives. Reprise Complete effectivement réussie.

## Captures réelles

| Sas pendant le scan — player | Console réseau — player |
|---|---|
| ![Visibilité conservée dans le sas](player/demo-mantrap-scan.png) | ![Texte lisible devant le pupitre](player/demo-network.png) |

| Local électrique — player | Confinement — caméra éditeur hors Play |
|---|---|
| ![Entrée au clavier dans le local électrique](player/demo-electrical.png) | ![Rangées et confinement vus à hauteur de visite](hall-editor.png) |

Le panneau du sas affiche ici le début du scan à 0 %, capturé à son déclenchement. Son pourcentage évolue ensuite. La vue éditeur de l'allée est un rendu positionné, distinct d'une capture du player en marche. Les dix captures du player sont conservées dans [player/](player/).

## Date du lanceur et limites

`CloudVR-PC.exe` est le lanceur Unity de 672 256 octets ; sa date reste le **20/10 à 14 h 58** parce que son contenu est identique. Les scènes reconstruites se trouvent dans `CloudVR-PC_Data/data.unity3d`, daté du **21/10 à 22 h 05 min 21 s**. Les changements de scripts résident dans les DLL. Le [manifeste de livraison](delivery-manifest.json) conserve les tailles, dates UTC et empreintes SHA-256 du dossier effectivement testé. Toujours conserver et lancer le dossier Windows complet.

P0 ne remplace pas les six activités répétitives ni la transition V2 de 10,8 s : **T16/P1** doit apporter la manipulation de Baie_01 et une anomalie thermique à résoudre ; **T17/P2** doit créer le voyage réseau 3D. Le texte historique « activer le refroidissement » reste à corriger avec cette mécanique. Horizon du campus, matériaux PBR, voix finale, validation pédagogique et mesures PC/Quest restent à traiter. Aucun APK recompilé ni casque testé ; aucune absence universelle de scintillement revendiquée.

## Publication suspendue

Le build est livré localement ; **aucun commit ni push P0** n'a été effectué. Deux tentatives de `git add` ont échoué sur un verrou `index.lock` vide mais un processus Git LFS est encore présent. Le contrôle préalable a refusé son déplacement ; propriétaire non établi, aucun verrou supprimé ni processus arrêté. La règle des deux tentatives d'AGENTS.md impose l'arrêt de cette tâche pour revue humaine. [Constat anonymisé](git-publication.json). Revoir le verrou, puis reprendre la vérification des fichiers préparés et Git LFS avant publication ordinaire.


## Reprise ultérieure du 21/10

Le constat de publication suspendue ci-dessus est historique. Le verrou a été revu après GO puis sauvegardé sous tmp, sans arrêt de processus. La version locale a depuis reçu P1 : [64 contrôles et captures finales](../p1-2026-10-21/README.md). Consulter STATE pour le résultat de publication courant.
