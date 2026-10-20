# Démo guidée — preuves du 20 octobre 2026

Parcours PC implémenté : Chambre → DataCenter, six bornes et huit activations, synthèse → Chambre, puis recommencer. Le [guide](../../DEMO.md) précise commandes et simplifications. Aucun essai Quest dans ce jalon.

## Préparation et compilation

- Retour utilisateur du 20/10 : déplacement normal et baie atteinte/contournée dans le player T07a précédent. Retour qualitatif distinct du contrôle automatique ancien qui avait échoué ; cause de cet échec non établie.
- Éditeur original ouvert ; ressources MCP lisibles mais execute_code et read_console non répondants. Appels arrêtés après deux essais. Aucun résultat Play dans cet éditeur déduit.
- Build isolé par le même Unity 6000.0.58f2, Windows64. Premier démarrage arrêté pendant le clone Git de MCP ; seule la copie est modifiée pour omettre ce connecteur d'éditeur et sa dépendance JSON inutilisée. Versions des packages du jeu comparées, sans mise à niveau.
- Scripts compilés dans le batch, builder exécuté et six bornes enregistrées. Un avertissement C# existant dans PlayerSetupEditor concerne FindObjectOfType obsolète, pas une erreur des nouveaux composants.
- Export et test du player encore en cours à la création de cette note ; aucun résultat final revendiqué dans cette section intermédiaire.

## Portée du scénario automatisé

DemoSmoke contrôle l'envoi par souris synthétique, le chargement, l'unicité de caméra, une avancée initiale au clavier, l'impossibilité de sauter une étape, les raycasts/activations des six bornes, la chaleur puis le refroidissement, le relais batterie, la supervision, la synthèse, le retour et le redémarrage.

Le rig est repositionné pour viser chaque borne : le scénario ne prouve pas une marche naturelle complète ni le confort. Captures et animation sont des représentations du player, sans mesure physique de données, température, puissance ou disponibilité. Voix et casque restent distincts.

## Résultat final : parcours complet validé (player-passed/)

- **Diagnostic et correctif** : la borne 05 était masquée par le collider de la `Baie_Secondaire_-1`. Repositionnement des bornes 05 et 06 à `z = 10,2m` (dégagées des baies à `z = 9,0m`).
- **Build Windows final** : compilé sans erreur ni avertissement.
- **Résultat player (player-passed/)** : **26 contrôles sur 26 réussis**, `exitCode = 0`, `passed = true`.
  - Envoi photo depuis la Chambre, transition asynchrone, caméra unique, déplacement ZQSD/WASD, pause/reprise Échap.
  - Bornes 01 à 06 activées successivement sans saut d'étape : Réseau, Traitement, Stockage, Refroidissement (avec révélation thermique et rétablissement), Électricité (avec illustration du relais batterie), Supervision (avec transmission de l'alerte).
  - Synthèse interactive affichée, retour Chambre par Entrée avec photo retrouvée, et réinitialisation de la démo par touche R.
  - 5 captures GPU Direct3D11 produites et archivées sous `player-passed/` (`demo-arrival.png`, `demo-heat.png`, `demo-network.png`, `demo-summary.png`, `demo-room-complete.png`).

Les journaux publics sont anonymisés sans changer les résultats, compteurs, positions ou horaires. Originaux dans tmp/unity/demo-editor ou tmp/unity/pc-player-*, exclus de Git. Les captures de caméra utilisent le HUD en ScreenSpaceCamera et apparaissent dans les captures réelles.
