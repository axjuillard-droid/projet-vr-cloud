# T24 — Mesures PC et régression, 4 novembre 2026

Build **15 h 10 min 51 s Paris**, Succeeded en 61,96 s, **147 126 950 octets**, 0 erreur / 0 avertissement BuildPipeline. **88 pré-build et 196/196 contrôles fonctionnels Direct3D11**, exitCode=0, fin **15 h 20 min 56 s**. [Build](build-result.json), [rapport fonctionnel](pc/demo-smoke-result.json), [lancement](pc/launch-result.json), [journal anonymisé](pc/Player.log), [manifeste SHA-256](delivery-manifest.json).

## Mesure valide : player visible

Après accord explicite de l’utilisateur, même exécutable en fenêtre visible, D3D11 1280×720, Iris Xe / i7-1165G7, Windows 11. ABBA 1x/4x/4x/1x, deux secondes d’échauffement puis huit secondes par phase ; focus actif dans toutes les frames mesurées. Une série seulement, applications de fond et fréquences non contrôlées.

[Méthode et interprétation](../../MESURES_PC_2026-11-04.md), [rapport original](visible/performance-result.json), [lancement](visible/launch-result.json), [journal](visible/Player.log), [résumé recalculé](summary.json). CSV bruts : [phase 0, 1x](visible/phase-0.csv), [phase 1, 4x](visible/phase-1.csv), [phase 2, 4x](visible/phase-2.csv), [phase 3, 1x](visible/phase-3.csv).

| Mode | Intervalle médian / p95 | GPU médian / p95 |
|---|---|---|
| 1x | 46,09 / 51,91 ms | 43,93 / 50,29 ms |
| 4x | 54,11 / 59,79 ms | 51,00 / 58,33 ms |

639 frames observées, 619 timings GPU frais après exclusion des cinq premiers par phase. Timestamps dédupliqués et effectifs vérifiés depuis les CSV. Surcoût médian observé 4x : +8,02 ms/frame (+17,4 %), +7,07 ms GPU (+16,1 %). Ce ne sont pas des résultats Release ou Quest ; aucune garantie de cadence sur d’autres vues ou machines.

![Médianes et p95, pas des intervalles de confiance](timings.png)

Le résumé se recalcule sans dépendance Python supplémentaire : `python tools/Summarize-PCPerformance.py <dossier-du-run>`. Le script vérifie ordre ABBA, focus, rendu/GPU présents, effectifs, timestamps et statistiques des CSV ; il refuse la collecte masquée inutilisable. Les deux APNG sont vérifiés pixel par pixel contre leurs 50 PNG source.

## Diagnostic masqué, non utilisé comme performance

[Premier rapport](hidden-unusable/performance-result.json), [lancement](hidden-unusable/launch-result.json), [journal](hidden-unusable/Player.log) : programme achevé mais aucun temps GPU/rendu, boucle CPU autour de 0,25 ms. Ces données sont **inexploitables comme cadence graphique** et exclues du résumé. Les CSV de cette boucle sans rendu restent dans le dossier local ignoré ; aucun graphe de performance n’est construit à partir d’eux. Le runner distingue ensuite collecte terminée, disponibilité GPU et disponibilité rendu.

## Séquences hors mesures

50 PNG source conservés sous `visible/sweep-1x/` et `visible/sweep-4x/` : caméra (0 ; 1,68 ; 8), FOV 75°, pitch −8°, yaw −3° à +3° par pas 0,25°. [Paramètres](motion.json). Les deux APNG [1x](motion-1x.png) / [4x](motion-4x.png) conservent taille et pixels RGB, sans palette réduite ni redimensionnement ; [vérification des 50 frames](motion-validation.json). Lecture imposée à 10 images/s pour revue, **pas une mesure de FPS**. Captures après les quatre fenêtres chronométrées, `Camera.Render` sans Overlay.

Contours plus réguliers en 4x aux angles inspectés ; petits textes distants toujours peu lisibles. Pas de validation universelle de scintillement ou de confort humain/VR.

## Conservation et régression

Probe et runner opt-in réutilisent FrameTimingManager existant ; aucun package ajouté. Aucun réglage du parcours normal modifié, profil produit 4x conservé. Le probe ne se lance pas sans argument et est exclu d’Android/Release. Aucun nouveau test de capture complète Overlay.

[P0, 14](p0-layout-audit.json), [P1, 20](p1-learning-audit.json), [transition, 11](p2-network-audit.json), [exploitation, 19](p3-facility-audit.json), [données, 24](data-service-audit.json). La suite fonctionnelle conserve cour/sas, six activités, cas négatifs, pauses, conclusion/retour/reset. Entrées synthétiques, certaines visées contrôlées ; sa durée n’est pas un benchmark graphique.

[Audit de conservation](preservation-audit.json) : vingt matériaux et QualitySettings préexistants, 21 fichiers identiques par SHA-256 et exclus du commit ; scènes identiques à la copie compilée. Packages, autres ProjectSettings, sujet universitaire et APK conservés. Compilation éditeur terminée sans erreur ; première demande de scène vide refusée pendant compilation puis exécutée après fin confirmée. Resume/Wait, Complete puis restitution initiale confirmés avant tests. Chambre propre hors Play restituée, console retournant zéro erreur.

Atlas d’ombres réduit et diagnostic mémoire de fermeture conservés dans les journaux, sans conclusion causale de performance. Journaux publics anonymisés ; builds, caches et sessions MCP exclus. T24 terminé pour ce protocole ; T25 prêt sur les coûts GPU par passe. **Publication T24 confirmée : `496ffd2` sur origin/main**, SHA distant `496ffd2bd729c2f5df959c3aadfd571d84bd7ed8`, push ordinaire depuis `0c86e11`, 90 objets LFS / 55 Mo. Diff, dix-sept JSON, 107 liens locaux, nouveau GUID unique et Git LFS fsck vérifiés ; quatre sources/assets identiques à la copie compilée. CSV/statistiques recalculés, 50 frames APNG identiques aux PNG source. Builds/tmp/sessions et 21 fichiers initiaux exclus. Un commit documentaire suivant consigne cette confirmation.
