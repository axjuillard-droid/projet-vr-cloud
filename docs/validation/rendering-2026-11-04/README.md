# T23 — Rendu PC livré, 4 novembre 2026

Build **10 h 26 min 56 s Paris**, Succeeded en 93,78 s, 147 112 656 octets, **0 erreur / 0 avertissement BuildPipeline**. **88 pré-build et 196/196 contrôles Direct3D11**, exitCode=0, fin à 10 h 30 min 09 s. [Build](build-result.json), [rapport player](pc/demo-smoke-result.json), [lancement](pc/launch-result.json), [journal anonymisé](pc/Player.log), [manifeste SHA-256](delivery-manifest.json).

## Diagnostic et correctif

[Analyse sourcée et comparaisons](../../DIAGNOSTIC_RENDU_2026-11-04.md). Vingt captures hors Play : quatre poses 1280×720/FOV 65°, variantes matériau seul, cinq spots, retrait des deux appoints et MSAA 1x/4x. [Paramètres et limites](comparison/capture.json), [comptage de pixels blancs dans deux zones fixes](comparison/pixel-review.json). Matériaux/profil clonés temporairement et état restauré ; aucune retouche des PNG. Douze premières images achevées malgré réponse de transport sans succès, puis scène propre/console zéro erreur confirmées ; huit captures complémentaires retournent explicitement success=true. Aucun succès déduit de la réponse nulle.

Builder : sol/joints non métalliques à smoothness 0,30, plafond 0,15 ; cinq spots vers le bas (110°/80°), anciens Eclairage_Allee/Lateral retirés dans DataCenter seulement. MSAA 4x dans PC_RPAsset, profil Mobile inchangé. Aucun package ou média tiers ajouté.

## Régression et captures player

88 pré-build : [P0, 14](p0-layout-audit.json), [P1, 20](p1-learning-audit.json), [transition, 11](p2-network-audit.json), [exploitation, 19](p3-facility-audit.json), [données, 24](data-service-audit.json). Le player ajoute trois contrôles : profil/caméra MSAA, lumières descendantes sans appoints, matériau satiné ; les six activités et cas négatifs, marche cour/sas/allée, pauses, conclusion/retour/reset restent couverts. Marche mesurée 24.75 s dans le scénario, pas un benchmark graphique. Entrées synthétiques, certaines visées par poses contrôlées.

Trois vues de revue du player inspectées : [plafond](pc/demo-lighting-ceiling.png), [sol](pc/demo-lighting-floor.png), [profondeur](pc/demo-lighting-distance.png). Positions contrôlées à (0 ; 1,68 ; 8), rotations −28°/34°/0°. Caméra PC FOV 75° du projet, distincte du diagnostic FOV 65°. Captures Camera.Render utilisant le niveau MSAA du profil ; elles omettent l’Overlay. Aucun nouvel essai de capture complète Overlay, déjà observé par l’utilisateur sur T22.

| Plafond dans le build final | Sol dans le build final |
|---|---|
| ![Éclairage descendant](pc/demo-lighting-ceiling.png) | ![Revêtement satiné](pc/demo-lighting-floor.png) |

## Procédure et conservation

Resume/Wait réussi puis Complete réussi. [Premier test arrêté volontairement](cancelled-pc/launch-result.json), exitCode −1 : lancé avant confirmation de la fin de Complete, aucune validation déduite. Après confirmation de livraison puis restitution des matériaux, second run final ci-dessus. [Audit de conservation](preservation-audit.json) : vingt matériaux préexistants identiques par SHA-256, exclus du commit, deux scènes identiques à la copie compilée. Trois matériaux de sol/plafond T23 sont distincts et publiés. ProjectSettings de la copie compilée, profil Mobile, sujet universitaire, APK et dépendances conservés.

Un changement local de `ProjectSettings/QualitySettings.asset` a été constaté après le build et l’interruption : antiAliasing PC 0→4 et formatage YAML. Empreinte conservée dans l’audit ; fichier préservé localement et exclu du commit. Le build testé repose sur la copie antérieure de ce fichier et sur `PC_RPAsset` 4x, contrôlé dans le player.

[Six diagnostics de cache immuable de packages](build-bootstrap-notes.json) au démarrage de la copie, distincts du résumé BuildPipeline. Atlas d’ombres réduit et diagnostic mémoire de fermeture du player conservés, sans mesure de performance déduite. Chambre propre hors Play restituée dans l’éditeur, console retournant zéro erreur. Journaux publics anonymisés ; builds, caches et sessions MCP exclus.

## Limites et suite

Halos reproduits retirés et contours géométriques plus réguliers aux angles comparés ; aucune absence universelle de scintillement. Le MSAA ne résout pas tout aliasing de shader ou texture. Cadence/coût GPU, mouvement, petits textes distants, compréhension et confort restent à mesurer. Aucun casque testé ni nouvel APK. T23 terminé sur ce correctif PC ; T24 prêt sur mesures PC/mouvement. **Publication T23 confirmée : `ec8982c` sur origin/main**, SHA distant `ec8982cc2caec8d6e75bd396e444c59c5c26e9bb`, push ordinaire depuis `8865d3b`, 73 objets LFS / 26 Mo. Diff, quatorze JSON, 88 liens locaux et Git LFS fsck vérifiés ; huit sources/assets identiques à la copie compilée. Builds/tmp/sessions, vingt matériaux initiaux et réglage QualitySettings tardif exclus. Un commit documentaire suivant consigne cette confirmation.
