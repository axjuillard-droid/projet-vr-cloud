# 03 · Des résultats scientifiques aux choix de CloudVR

## Ce que les études permettent de défendre

**Partir d’un usage familier.** Dans l’enquête de Gujral et al., peu de répondants disent avoir déjà réfléchi à l’impact de leur stockage cloud. Attari et al. montrent que les intuitions sur différents usages énergétiques peuvent mal représenter les écarts réels. Dans CloudVR, la photo fournit un point de départ concret. Le belvédère permet ensuite de passer du service individuel à la demande agrégée du site. L’animation représente un trajet possible et la mission fait varier une charge fictive. [A1 et A3](01-sensibilisation.md)

**Donner au joueur quelque chose à expliquer.** Des visites immersives ont accompagné des gains de connaissances sur l’acidification ou une meilleure rétention sur le climat dans des cadres précis. Dans une simulation de laboratoire, le casque a renforcé la présence alors que le PC a donné de meilleurs gains de connaissances. Une autre expérience trouve un bénéfice sur le transfert quand les participants rejouent la procédure après l’exposition en VR. Pour CloudVR, cela motive une vérification de la compréhension et un petit problème inédit après la visite. [B1 à B4](02-visites-vr.md)

**Distinguer compréhension, perception et comportement.** Une enquête sur la pollution numérique observe des liens différents entre connaissances et disposition à agir ou à payer. Une enquête expérimentale sur l’usage d’Internet prend également en compte le prix. Les études qui observent une action réelle suivent des indicateurs spécifiques, comme la consommation électrique de foyers ou le nombre de serviettes utilisées dans une situation de laboratoire. Dans CloudVR, la réussite de la mission renseigne sur l’action en jeu, et l’entretien sur l’explication du mécanisme. [A2, A4 et A5](01-sensibilisation.md) · [B5](02-visites-vr.md)

## Application à la V2 du 14 novembre

Les deux essais précédents ont montré que la flèche pouvait aider à trouver une commande alors que le rôle du clic restait opaque. La V2 a depuis modifié la transition, le traitement et la relecture de la photo, ainsi que le couplage entre météo, service et pompe. Les preuves disponibles pour ces changements sont fonctionnelles sur PC. Le prochain pilote doit observer la compréhension du parcours révisé. [État et preuves V2](https://github.com/axjuillard-droid/projet-vr-cloud/blob/51ee40c25d6983a64c1c0bf56196741f08d395a9/docs/STATE.md) · [Observations des deux essais](https://github.com/axjuillard-droid/projet-vr-cloud/blob/51ee40c25d6983a64c1c0bf56196741f08d395a9/docs/RETOURS_JOUEURS_V2.md)

| Moment du parcours | Résultat scientifique mobilisé | Observation ou décision proposée |
| --- | --- | --- |
| Chambre → fibre → sas | A1, B1 | Faire reconstruire le trajet de la demande. Repérer si le badge est confondu avec l’accès des données. Si oui, revoir la transition ou le signal au seuil. |
| Baie école → banque de stockage → relecture | A1, B3, B4 | Demander ce que font la RAM, le CPU et la banque. Vérifier si l’original est identifié grâce au trajet et à son usage, ou choisi au hasard parmi deux images. |
| Belvédère → rangées → échangeur | A3, B1, B3 | Faire verbaliser ce qui change quand davantage de rangées travaillent. Vérifier que le joueur suit la chaleur jusqu’au rejet et comprend le rôle du refroidissement. |
| Pompe et rivière | A2, A4, B4, B5, C1, C2 | Demander quelle conséquence du réglage est visible et quel compromis le joueur ferait sous une autre contrainte d’eau. Vérifier s’il distingue le fluide qui recircule et l’eau d’appoint de la tour. Si l’indicateur est lu sans explication du mécanisme, revoir le retour à l’écran ou le lien visuel entre les équipements. |
| Après la visite | A2, A5, B2, B3 | Relever séparément l’intérêt, la perception et la compréhension. Si le rappel différé est réalisable, chercher ce que la personne explique encore spontanément. |

## Appuis techniques sur l’eau et l’énergie

### C1 · Eau consommée sur site et eau liée à l’électricité

Siddik, M. A. B., Shehabi, A., & Marston, L. (2021). *The environmental footprint of data centers in the United States*. Environmental Research Letters, 16(6), 064017. [DOI](https://doi.org/10.1088/1748-9326/abfba1) · [Texte intégral, Université de Californie](https://escholarship.org/content/qt3xw3z2qp/qt3xw3z2qp.pdf)

La modélisation distingue l’eau consommée sur le site et celle liée à la production d’électricité. Les résultats dépendent du lieu et du stress hydrique du bassin versant. Dans CloudVR, la rivière représente l’effet local du prélèvement du site fictif. Elle illustre une composante du bilan hydrique. Les estimations américaines ne calibrent pas la rivière simulée.

### C2 · Les compromis varient avec le système de refroidissement

Karimi, L., Yacuel, L., Degraft-Johnson, J., Ashby, J., Green, M., Renner, M., Bergman, A., Norwood, R., & Hickenbottom, K. L. (2022). *Water-energy tradeoffs in data centers: A case study in hot-arid climates*. Resources, Conservation and Recycling, 181, 106194. [DOI](https://doi.org/10.1016/j.resconrec.2022.106194) · [Texte intégral, projet des auteurs](https://dcewise.weebly.com/uploads/1/3/2/1/132162760/11_water-energy_tradeoffs_in_data_centers-_a_case_study_in_hot-arid_climates_karimi__2022_.pdf)

L’étude compare deux sites de colocation proches de Phoenix. Le site refroidi par des groupes à air présente une efficacité énergétique globale plus faible selon le PUE, mais une intensité d’usage de l’eau à la source plus faible selon le WUE, que l’autre site, qui combine des groupes refroidis à l’eau et du refroidissement évaporatif. Les mesures saisonnières diffèrent aussi entre les sites. CloudVR peut faire comparer des choix dans un contexte explicite, avec une même charge pendant la comparaison. La configuration et la météo déterminent ce que représente le compromis dans le jeu.

## Priorité du prochain essai

Utiliser le [protocole pilote](04-protocole-pilote.md) sur une version identifiée et vérifier la chaîne complète avec les mots du joueur. Le signe de compréhension recherché est une explication qui relie **usage → service physique → charge → chaleur à évacuer → choix de refroidissement et effet local**. Une réponse qui répète seulement la consigne guide la révision de la séquence correspondante.
