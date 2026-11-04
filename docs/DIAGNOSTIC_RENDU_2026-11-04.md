# T23 — Halos et contours en salle blanche, 4 novembre 2026

Reprise du build T22 du 22 octobre et des captures utilisateur. Objectif borné : expliquer les halos blancs au sol/plafond et comparer les contours géométriques avec le profil PC existant. Pas de refonte des modèles, de photométrie industrielle ni de validation Quest.

## Comparaison contrôlée

Vingt images 1280×720, quatre poses identiques, FOV 65°, caméra hors Play. Les matériaux sont clonés temporairement ; le profil PC est cloné pour le seul essai MSAA 4x. État restauré dans `finally`, aucune sauvegarde des variantes de diagnostic. [Paramètres et scripts exécutés](validation/rendering-2026-11-04/comparison/capture.json).

| Variante | Observation |
|---|---|
| Origine : cinq points, appoints anciens, sol métallique/lisse, 1x | Halos blancs reproduits au plafond et au sol. |
| Matériaux seuls : sol metallic 0 / smoothness 0,30, plafond 0,15 | Le halo du sol disparaît ; celui du plafond reste. |
| Matériaux + cinq spots vers le bas, 110° / cœur 80° | L’essentiel des halos de plafond disparaît ; un appoint ancien reste visible. |
| Retrait des deux appoints de la pièce primitive | Le halo résiduel de plafond disparaît dans les vues comparées. |
| Même variante, MSAA 4x au lieu de 1x | Contours des rails, baies et lignes du sol plus réguliers sur les vues comparées ; pas de conclusion universelle sur le scintillement en mouvement. |

Deux lumières historiques actives ont été relues : `Eclairage_Allee` à (0 ; 2,7 ; 4,8), point 2,1/range 9, et `Eclairage_Lateral` à (−3 ; 2,7 ; 8,2), point 1,6/range 8. Les cinq points `Eclairage_WhiteSpace` sont à Y=3,8, Z=4/12/20/28/36, intensité 2,5/range 12. Leur conversion descendante conserve ces intensités, positions et portées. Éclairage d’inspection et local électrique conservés.

Le sol était réglé metallic 0,40 / smoothness 0,75, les joints 0,50 / 0,65. Leur aspect devient non métallique et satiné. Ce sont des choix artistiques, sans certification de matériau antistatique ni lux mesuré. Les propriétés du shader réel URP Lit et des API ont été vérifiées dans le projet.

Le [comptage de pixels](validation/rendering-2026-11-04/comparison/pixel-review.json) donne 1 742 pixels avec chacun des canaux RGB ≥250 dans la zone fixe de sol d’origine, zéro après correction du matériau. C’est un indicateur d’image, pas une mesure de luminance, de confort ou d’énergie. Les bandes LED blanches restent présentes au plafond ; le critère ne vise pas zéro pixel blanc dans toute l’image.

| Avant — plafond | Variante finale — plafond, 4x |
|---|---|
| ![Halos d’origine](validation/rendering-2026-11-04/comparison/before-ceiling.png) | ![Spots descendants et appoints retirés](validation/rendering-2026-11-04/comparison/final4x-ceiling.png) |

| Avant — sol | Variante finale — sol, 4x |
|---|---|
| ![Sol métallique et lisse](validation/rendering-2026-11-04/comparison/before-floor.png) | ![Sol satiné](validation/rendering-2026-11-04/comparison/final4x-floor.png) |

## Implémentation et limites

Builder reproductible corrigé ; retrait limité aux deux objets lumineux nommés dans la scène DataCenter. `PC_RPAsset` passe de 1 à 4 échantillons, profil Mobile inchangé. Le probe PC contrôle le profil/caméra, les cinq lumières descendantes sans appoint et le matériau du sol ; ses captures reprennent le niveau MSAA du profil au lieu d’un RenderTexture toujours à un échantillon. Trois vues supplémentaires de revue sont prises depuis des poses contrôlées, sans déplacement humain déduit.

Compilation éditeur sans erreur après import. Build du 04/11 à 10 h 26 min 56 s Paris réussi, 88 pré-build puis **196/196 contrôles PC**, fin 10 h 30 min 09 s, exitCode=0. Trois vues du player inspectées, avec la caméra PC du projet (FOV 75°, distincte du diagnostic éditeur FOV 65°). Premier lancement arrêté volontairement avant confirmation de la copie complète ; seul le second run final est retenu. [Preuves de livraison et limites](validation/rendering-2026-11-04/README.md).

Les captures `Camera.Render` omettent l’Overlay ; sa présence a déjà été observée sur les captures utilisateur T22. Aucun nouvel essai de capture complète n’a été lancé. Un premier appel long a renvoyé une réponse sans succès exploitable alors que ses douze PNG ont fini d’être produits ; inspection native, scène propre restituée et console sans erreur vérifiées ensuite. L’appel des huit variantes complémentaires a retourné explicitement `success=true`.

Restent à vérifier : cadence PC avec 4x sur matériel identifié, scintillement en mouvement, petits textes/panneaux distants, atlas d’ombres et rendu dans le casque. Le MSAA ne traite pas à lui seul l’aliasing des shaders ou textures. [Sources et décision de réutilisation](RESOURCES.md#t23--éclairage-et-anticrénelage-pc-4-novembre-2026).
