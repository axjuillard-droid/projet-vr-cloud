# Reprise GO — correction validée sur PC, 21 octobre 2026

Le build final Windows est réussi : Unity 6000.0.58f2, 38,84 s, 146 790 730 octets, 0 erreur / 0 avertissement. Le player final passe **37 contrôles sur 37**, `passed=true`, `exitCode=0`, sous Direct3D11.

## Résultats

- Clic synthétique sur l’écran, envoi photo et transition V2 entière en six étapes (au moins 10,7 s), sans bypass smoke.
- Spawn à −28 m, maintien au sol, pause/reprise, contrôle de 100 petits déplacements de 0,5 mm donnant 5 cm.
- Marche au clavier de la cour au fond : position finale Z=39,002 m en 24,28 s ; aucun saut de position pour cette traversée. Deux portes interverrouillées et scan observés.
- Six bornes à 8/14/20/26/32/38 m, atteintes par raycast et clic avec poses de visée contrôlées ; huit actions, synthèse, retour Chambre par Entrée, photo retrouvée et reset R.
- Audit MCP des scènes originales : 0 matériau manquant, 0 shader non supporté (DataCenter : 1 466 slots ; Chambre : 99). Scènes propres, éditeur hors Play, aucune erreur retournée par la console.
- Lancement normal sans argument smoke également observé dans Chambre, puis fermé ; le parcours complet ci-dessus utilise des entrées synthétiques. Cadence visuelle en situation d’usage et confort humain restent à évaluer.

## Causes établies et corrections

1. Les scènes de build référencent des matériaux Campus/WS/Mantrap/Room créés dans la copie temporaire. Le runner ne copiait que Demo_* : 1 216 références manquaient dans DataCenter original. Tous les .mat et .mat.meta sont maintenant rapatriés ; les GUID sont conservés.
2. Des racks masquaient les bornes 01 et 05. Les 32 racks sont répartis sur x=−6/−4/+4/+6 m, Z=6/10/14/18/22/26/30/34 m. Les confinements sont latéraux, l’allée de visite reste dégagée. Le builder vérifie les six visées avant sauvegarde.
3. Le CharacterController ignorait les déplacements inférieurs à 1 mm. Expérience MCP : 100×0,5 mm donnent 0 m avec ce seuil, 0,049999 m avec zéro. Le contrôleur PC et le builder utilisent maintenant zéro. Cela supprime la perte à très haute cadence logique ; les milliers de frames du batch ne mesurent pas une cadence de rendu interactif.
4. Le probe maintient explicitement W avant Dynamic Update et restaure son réglage Input System. Temps réel, temps de jeu et positions sont enregistrés. Le maintien des entrées n’est pas présenté comme l’unique cause des anciens échecs.
5. L’éditeur original utilisait des assemblies anciennes et attendait une confirmation de rechargement. Refresh, vérification par réflexion et Reload de scène propre ont rétabli la version réelle. MCP prêt ne suffit pas à prouver que l’assembly est fraîche.

## Preuves

- [Player final et captures](player-final/demo-smoke-result.json), [lancement](player-final/launch-result.json), [build](build-final.json).
- [Expérience du seuil](micro-movement.json), [traversée éditeur](navigation-editor.txt), [audit matériaux final](material-audit-final.json).
- Rapports intermédiaires conservés : échec de raycast avant répartition des racks, puis premier parcours complet réussi avant correction du seuil. Les deux échecs de la phase précédente restent dans les dossiers parents.

Les journaux publics sont anonymisés ; originaux dans tmp ignoré. Aucun APK recompilé, aucun casque testé. Sous-titres provisoires et trajet réseau illustratif ; voix finale et validation pédagogique restent à préparer.
