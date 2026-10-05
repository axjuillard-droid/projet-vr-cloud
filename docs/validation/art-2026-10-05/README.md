# Premier module graphique original — 5 octobre 2026

Baie générée par script Blender 4.5.2, importée dans Unity 6000.0.58f2, matériaux URP originaux ; trois instances du prefab existant. Architecture : sol à dalles, joints mur/plafond, chemins de câbles, luminaires géométriques, deux armoires et support du panneau guide. Aucun pack tiers importé ou acheté.

- 6 228 triangles / sept meshes dans la baie, import mesuré environ 1,11 × 2,20 × 1,00 m et pivot au sol ; collider nominal de navigation conservé.
- Cinq meshes architecturaux fusionnés par matière, sans collider par détail ; colliders simples des deux armoires hors zone VR.
- Trois lumières, aucune par voyant. Pipeline PC/AA inchangé : MSAA=1 relu. Aucune mesure de FPS, mémoire GPU ou coût Quest.

## Contrôles dans Unity

- `editor-pc/` : premier essai, 13 contrôles réussis à 15 h 30 Paris ; captures intermédiaires. Plaque trop petite et sol trop contrasté constatés visuellement, ajustés ensuite.
- `editor-pc-final/` : 13 contrôles réussis à 15 h 34, transition/caméra/mode/spawn uniques, clavier synthétique et collision de baie conservés. Captures finales arrivée/approche inspectées, titre BAIE 01 lisible. `datacenter-detail.png` : caméra repositionnée uniquement en Play après le scénario terminé pour montrer le côté de la baie ; aucune pose de caméra sauvegardée.
- `editor-vr/` : 13 contrôles réussis à 15 h 37, mode forcé et téléportation injectée. Aucun tracking, geste de manette/rayon ou casque réel testé.

Après ces essais : Chambre propre, Windows64/Mono, hors Play/compilation et Console sans erreur ; deux scènes enabled. Player précédent conservé dans Builds/Windows-Journey-20261005/.

## Player Windows

Premier build deux scènes réussi à 15 h 40, job `build-9449de5de8` : 42,9 s, 0 erreur/1 avertissement (code changes uncompiled dans BuildReport). Première exécution graphique échouée après 10 contrôles à l'avance : z=2,846 après l'attente fixe, au lieu du seuil z>3 ; preuves player-first-failed/. Pas de collision prématurée ou panne de déplacement affirmée.

Probe adapté : attente d'une position cible avec timeout, puis au moins 5 m d'intention de déplacement avant de vérifier la collision ; PCPlayerController inchangé. Le compteur collisionCommandedDistance rapporte l'intention accumulée, pas une distance effectivement parcourue.

Second build `build-c01bad23d9` réussi à 15 h 48 : 61,6 s, 138,73 Mo rapportés, 0 erreur/0 avertissement. Nouveau champ du probe présent après recompilation ; avertissement initial absent. `windows-build-final-result.json` et `player-sha256.json` identifient ce livrable.

Seconde exécution (`player-second-failed/`) : 11 contrôles réussis, arrivée et avancée à z=3,300 confirmées ; position z=5,256 après une intention accumulée de 5,000 m, avant la baie, exitCode=1/passed=false. Cause non établie : ne pas affirmer une collision réelle, une régression du contrôleur ou un simple problème du test sans diagnostic. Après deux tentatives infructueuses, validation du player arrêtée selon AGENTS.md pour revue humaine. Aucun troisième essai.

Les captures du player montrent le nouveau décor ; elles ne valident pas la navigation complète. Un essai manuel doit vérifier déplacement depuis l'arrivée jusqu'à la baie, arrêt contre elle et contournement. La version précédente validée est conservée dans Builds/Windows-Journey-20261005/ ; ne pas la confondre avec le nouveau build.

## Reproduction et limites

Les chemins du projet et du compte Windows, le nom de machine et les IP privées sont anonymisés dans les journaux publics ; résultats, positions, compteurs et horodatages sont conservés. Les originaux restent localement dans `tmp/unity/publication-originals/art-2026-10-05/`, exclu du dépôt. `editor-final.json` consigne la relecture finale : Chambre propre, hors Play/compilation/build, une caméra.

Voir [art/README.md](../../../art/README.md) pour la génération et le builder. Le scénario PC existant contrôle la navigation fonctionnelle, pas une note de qualité visuelle. Les pièces sont génériques, pas une reproduction normalisée d'une baie industrielle. L'APK existant reste l'ancien export Chambre seule ; aucun nouveau build Android ni essai Quest pendant ce module.
