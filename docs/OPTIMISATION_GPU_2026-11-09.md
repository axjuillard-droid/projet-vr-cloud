# T25 — Audit du rendu PC, 9 novembre 2026

Le résultat à vérifier est un gain au même endroit et sur le même PC, avec un rendu proche de la référence. La comparaison garde le MSAA 4x, le FOV 75°, la résolution 1280×720 et le balayage de T24 dans le player Windows de développement D3D11 (Intel Iris Xe, i7-1165G7). Aucun résultat Quest ni Release.

## Audit à réglage constant

Deux références encadrent les interventions. Chacune dispose de 2 s d’échauffement puis de 8 s chronométrées. Les valeurs ci-dessous sont des médianes par fenêtre : une intervention modifie un ou plusieurs coûts de la frame et leurs interactions. Il ne s’agit pas de durées GPU isolées et les écarts ne s’additionnent pas. Les images sont prises après les mesures.

| Intervention | Intervalle entre images | Temps GPU |
|---|---:|---:|
| Référence initiale | 11,41 ms | 8,88 ms |
| SSAO demi-résolution | 10,37 ms | 8,56 ms |
| SSAO désactivé | 9,09 ms | 6,86 ms |
| Ombres de la caméra désactivées | 7,17 ms | 5,45 ms |
| Copies couleur opaque et profondeur désactivées sur la caméra | 9,36 ms | 7,88 ms |
| Renderers utilisant le matériau des vitres de confinement masqués | 8,35 ms | 7,57 ms |
| Plan lointain de la caméra ramené à 26 m | 8,69 ms | 7,42 ms |
| Référence finale | 8,95 ms | 7,96 ms |

La dérive entre références est de 2,46 ms par image et 0,92 ms GPU. Le retrait des ombres produit le signal le plus net, mais éclaircit fortement le sol et le fond. Le SSAO à demi-résolution, les copies, les vitres et le plan lointain ne donnent pas un signal suffisamment net pour justifier un réglage produit dans cette série. Le plan lointain réduit, en particulier, masque une partie de la salle et ne représente pas un réglage à livrer.

| Référence | Ombres coupées, diagnostic uniquement |
|---|---|
| ![Référence du hall](validation/gpu-2026-11-09/pass-audit/audit-reference.png) | ![Hall sans ombres](validation/gpu-2026-11-09/pass-audit/audit-no-shadows.png) |

Les deux références du jour sont déjà bien plus rapides que les 54,11 ms à 4x observées le 4 novembre. Les conditions système ne sont pas isolées et la cause n’a pas été établie. Ce différentiel entre dates ne mesure aucun gain T25.

## Lisibilité du tableau

[À 31,8 m](validation/gpu-2026-11-09/pass-audit/supervision-z8.png), le tableau est repérable mais ses libellés ne se lisent pas. [À 14,8 m](validation/gpu-2026-11-09/pass-audit/supervision-z25.png), les caractères du corps restent très petits. [À 3,8 m](validation/gpu-2026-11-09/pass-audit/supervision-z36.png), le titre et les commandes sont lisibles, mais la borne masque encore le début de certaines lignes depuis cet axe. C’est un problème de taille projetée et de placement ; augmenter seulement le MSAA ne le résout pas. Prochaine modification proposée : repère court et lisible à distance, texte détaillé à proximité, et décalage de la borne hors de l’axe du tableau.

## Comparaison des cascades d’ombres

Le test garde la portée à 50 m, l’atlas principal à 2048, le SSAO et le MSAA 4x. Après six secondes d’échauffement initial, quatre fenêtres alternent quatre, deux, deux puis quatre cascades.

| Cascades | Intervalle entre images | Temps GPU |
|---|---:|---:|
| Quatre | 9,93 ms | 7,92 ms |
| Deux | 8,41 ms | 7,73 ms |
| Deux | 8,72 ms | 7,92 ms |
| Quatre | 8,68 ms | 7,95 ms |

Le gain GPU sur les échantillons regroupés est de 0,14 ms (1,8 %). Une fenêtre à deux cascades est au niveau des références. Le profil à quatre cascades est conservé. [Mesures par fenêtre et captures](validation/gpu-2026-11-09/shadow-comparison/).

## Comparaison de l’occlusion ambiante

Dans un même player, quatre fenêtres alternent SSAO actif, inactif, inactif puis actif. Les quatre cascades, le MSAA 4x et les autres réglages sont identiques. Le player est resté au premier plan pendant chaque fenêtre.

| SSAO | Intervalle entre images | Temps GPU |
|---|---:|---:|
| Actif | 9,10 ms | 8,17 ms |
| Inactif | 8,69 ms | 6,71 ms |
| Inactif | 7,31 ms | 6,12 ms |
| Actif | 8,85 ms | 7,95 ms |

Sur les échantillons regroupés, le temps GPU médian baisse de **1,75 ms (21,6 %)** et l’intervalle médian entre images de **1,11 ms (12,3 %)**. Les deux fenêtres sans SSAO sont en dessous des deux références, et l’écart GPU dépasse la dérive de 0,22 ms entre les références de cette série. Le réglage retenu désactive le SSAO dans le renderer PC. Les ombres de contact autour du plafond et des baies sont plus discrètes, tandis que les ombres directionnelles et le MSAA 4x restent actifs. La mesure porte sur une seule série et une seule machine, en Development Player.

| SSAO actif | SSAO inactif |
|---|---|
| ![Salle blanche avec SSAO](validation/gpu-2026-11-09/ao-comparison/ao-on.png) | ![Salle blanche sans SSAO](validation/gpu-2026-11-09/ao-comparison/ao-off.png) |

[Mesures brutes et résumé](validation/gpu-2026-11-09/README.md).
