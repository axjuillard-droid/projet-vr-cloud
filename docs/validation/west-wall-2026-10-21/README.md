# Mur ouest — Groupes électrogènes coplanaires, 21 octobre 2026

## Diagnostic reproduit

La [capture utilisateur](user-report.png), située dans la salle blanche près des baies, montre des panneaux sombres et des découpes triangulaires sur le mur. Le problème a été reproduit dans Unity, caméra X=−7,6 / Y=1,65 / Z=12,4, vers le mur ouest.

Les trois groupes électrogènes du campus étaient centrés à X=−12 m avec une largeur de 6 m. Leur face droite atteignait **X=−9 m**, exactement comme la face intérieure de `Paroi_Ouest_A`. [Mesures avant](geometry-before.json) : zéro à 0,954 µm de différence, sur des rectangles de 3 m de haut et 2,4 m de long. Ce conflit de profondeur explique les panneaux/triangles qui changent avec l'angle ; il est distinct du précédent correctif du sol.

## Correctif reproductible

`DatacenterMonumentalBuilder` déplace toutes les pièces `Genset_*` de 6 m vers l'ouest : conteneurs, grilles et échappements restent solidaires. Les trois conteneurs sont désormais à X=−18 m, entièrement hors de l'enveloppe du bâtiment (X=−14 m). Une dalle de soutien et un accès contigu à la cour sont ajoutés ; les conteneurs ont des colliders.

[Mesures après](geometry-after.json) : face droite à X=−15 m, **6 m de séparation du mur intérieur**. Ce déplacement corrige l'implantation ; aucun biais de profondeur ni masquage de shader appliqué. Il ne certifie pas les distances réglementaires d'une installation industrielle.

`P0SpatialValidation`, exécuté avant chaque build de démo, vérifie désormais que les trois groupes restent hors du bâtiment et à plus de 5,5 m du mur intérieur. [Audit éditeur](layout-editor.json) : **14 vérifications réussies**, dont les dix contrôles P0 précédents.

## Comparaison hors Play

Sept vues avant/après conservées : rotations Y=195/205/220/235/250/265/280°, mêmes position, hauteur et résolution 640×373. [Protocole de capture](capture-poses.json), [avant](before/), [après](after/). Le mur est continu aux sept orientations après correction ; les formes parasites reproduites ne sont plus visibles. Ce sont des rendus Unity réels, sans retouche, distincts d'un essai humain ou d'une mesure FPS.

| Avant, rotation 205° | Après, rotation 205° |
|---|---|
| ![Panneaux parasites sur la cloison](before/wall-205.png) | ![Cloison continue après déplacement](after/wall-205.png) |

## Build et player

**Build Windows livré le 21/10 à 22 h 38 min 14 s Paris** : [Succeeded](build-result.json), Unity 6000.0.58f2, 37,71 s de BuildPipeline, 146 933 028 octets pour le build, **0 erreur / 0 avertissement BuildPipeline**. `Build-DemoCopy.ps1 -Resume -Wait`, puis `-Complete` exécutés séparément. [Audit pré-build](p0-layout-audit.json) : 14 vérifications réussies.

`Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120` : **[40/40 contrôles](player/demo-smoke-result.json), [exitCode=0/passed=true](player/launch-result.json)**, fin à 22 h 40 min 28 s Paris. Cour → sas/scan → Z=39,001 m en 24,75 s de marche synthétique, accès au local électrique, six bornes/huit actions et retour/reset réussis. Les compteurs de frames logiques ne mesurent pas le FPS interactif.

Le probe produit sept captures contrôlées du même mur dans le player, après la marche et l'accès électrique vérifiés. Ces sept images ont été inspectées : les panneaux et triangles reproduits ne sont plus visibles. Ces poses de capture ne sont pas présentées comme une marche manuelle jusqu'au mur. Les dix autres captures de la régression sont conservées dans [player/](player/).

![Mur corrigé dans le player, rotation 205°](player/demo-west-wall-205.png)

[Journal anonymisé](player/Player.log) : avertissement de réduction de résolution des six shadow maps pour l'atlas 2048 conservé, distinct du rapport BuildPipeline. Les sources compilées conservent aussi des avertissements d'API obsolètes connus. Aucun benchmark ni essai humain de confort ; aucune absence universelle d'aliasing/scintillement revendiquée.

[Manifeste SHA-256 du dossier livré](delivery-manifest.json). Les scènes sont dans `CloudVR-PC_Data/data.unity3d` du 21/10 à 22 h 37 min 59 s ; la DLL de jeu est du 21/10 à 22 h 38 min 04 s. La date du lanceur `.exe` peut rester au 20/10 à 14 h 58 car son contenu est identique. Garder le dossier Windows complet.

MCP : comparaison et audit géométrique réalisés avant livraison, Chambre propre restituée. Après le réimport final demandé, deux pings sans réponse ; aucune relecture de console finale ni nouvel audit MCP après livraison revendiqués. Les validations build/player ci-dessus restent établies.

Publication Git toujours suspendue sur le verrou décrit dans [les preuves P0](../p0-2026-10-21/git-publication.json). Aucun nouvel essai Git dans ce correctif. Aucun nouvel APK ni test casque.


## Reprise ultérieure du 21/10

Le constat de publication suspendue ci-dessus est historique. Le verrou a été revu après GO puis sauvegardé sous tmp, sans arrêt de processus. La version locale a depuis reçu P1 : [64 contrôles et captures finales](../p1-2026-10-21/README.md). Consulter STATE pour le résultat de publication courant.
