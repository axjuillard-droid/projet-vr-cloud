# Proposition de refonte de la visite CloudVR

## Diagnostic du build T25

- Réseau. Le joueur coupe A, observe le basculement automatique vers B, lit la réponse de SERVICE puis rétablit A. La réponse vérifie l’accessibilité du service dans la maquette. La cinématique et cette activité ne transmettent aucun état de la photo jusqu’au stockage, qui initialise sa propre « photo v1 sur A ». Cette rupture explique le faible enjeu de l’exercice.
- Calcul. Les 31 baies de décor représentent la même famille d’équipements. La Baie_01 possède un cadre ouvert spécifique, une porte sur charnières et un tiroir serveur 2U sur rails. Le modèle fermé a été désactivé pour laisser voir l’intérieur. Conserver cette manipulation et identifier clairement la baie comme matériel de formation hors production.
- Stockage. La distinction entre disponibilité après panne et récupération après suppression est pertinente. Elle est noyée dans une suite imposée de commandes. Remplacer la préparation des copies à la main par un incident que le joueur diagnostique et résout.
- Orientation. Les textes « à droite » supposent un point de vue fixe alors que le joueur tourne pour lire les bornes. Les coordonnées Z servent au placement dans Unity. Les deux formulations sont à retirer de l’interface de visite.

## Une mission en trois actes

Objectif du visiteur : revoir sur le PC de la chambre la photo envoyée au début. La même miniature et le même identifiant fictif persistent dans le parcours. Le client distingue « en attente », « reçue », « enregistrée » et « lisible ». En fin de mission, une lecture simulée affiche la miniature à partir de l’état validé du scénario. L’équipe d’exploitation guide le visiteur dans un environnement de formation. Durée cible proposée : six à huit minutes, à mesurer.

| Acte | Décision du joueur | Résultat perceptible |
|---|---|---|
| **1. Faire arriver la photo** | Le chemin principal tombe avant confirmation. Depuis le panneau de test, sélectionner deux ports pour établir un chemin de secours préautorisé. Les voyants des ports et la carte donnent les indices. Lancer le renvoi depuis le client de test. | La demande suit le nouveau chemin, le service confirme la réception. L’écran continue d’indiquer que l’enregistrement reste à vérifier. Un mauvais choix montre où la demande s’arrête et permet un nouvel essai. |
| **2. Récupérer la photo** | Après l’enregistrement confirmé, le scénario déclenche une suppression accidentelle. Le joueur essaie d’ouvrir la photo, compare la réplique à jour et le point de restauration antérieur, choisit la source, puis vérifie la lecture sur l’espace de récupération. | Sur la réplique, la photo apparaît supprimée dans ce scénario. La restauration depuis le bon point rend la miniature visible. Un mauvais choix donne une explication concrète. Les protections sont déjà configurées et leurs horaires sont lisibles. |
| **3. Maintenir le service** | À la première tentative de lecture après récupération, diagnostiquer un point chaud à l’entrée des serveurs. Comparer ventilation et fermeture du confinement sur deux mesures, température et puissance auxiliaire. Une coupure secteur est la variante longue du scénario, avec l’UPS, la stabilisation du groupe et le transfert. | Des filets d’air rendent la recirculation visible. En fermant la porte, la température revient dans la plage de la simulation et les auxiliaires reviennent au régime nominal. Le client peut relire la photo une fois le service stabilisé. Le bilan de supervision explique les effets des actions. |

La baie de formation devient un arrêt physique facultatif pendant l’acte 1. Ouvrir, tirer, inspecter et remettre le tiroir en place. Montrer où s’exécute, par exemple, la génération de miniature. Le NVMe du tiroir illustre le stockage local, tandis que le service de stockage de l’acte 2 représente un rôle distinct. La supervision reprend les événements vécus et peut se consulter au retour, avec le résultat de lecture affiché sur le PC de la chambre.

## Guidage et lisibilité

Installer l’objectif au poste d’action. Utiliser des repères fixes comme « Portique Réseau », « Baie école 01 » ou « Local Énergie », visibles depuis l’approche et répétés sur l’équipement. Afficher un jalon ponctuel sur la prochaine cible ou au seuil à franchir. Une consigne courte donne un verbe et une cible, par exemple « Relie ENTRÉE au port de secours ». Coupler couleur, pictogramme et libellé. Déplacer le pupitre qui masque le tableau. Garder une aide à la demande après une hésitation, ainsi que la reprise après un mauvais choix. PC par visée et sélection de deux points, même logique pour XRI.

## Priorité et validation

1. Corriger le guidage et la visibilité dans le parcours actuel. Vérifier la découverte depuis l’entrée et depuis l’allée après un demi-tour.
2. Prototyper seulement l’enchaînement réseau → récupération → lecture de la même photo dans la chambre. Réutiliser les cibles PC/XRI, les maquettes et l’animation de signal existantes. Couvrir le chemin correct, une erreur récupérable, et les états « reçue » versus « lisible ».
3. Faire essayer ce prototype sans explication orale à trois personnes nouvelles au projet. Cible proposée : chacune trouve le premier poste et sait expliquer pourquoi la réplique ne suffit pas après la suppression. Noter les hésitations, les demandes d’aide et le temps. Revoir la séquence sur ces observations avant de relier la thermique et l’électricité.

Les temps, capacités et températures du futur prototype resteront des paramètres de jeu fictifs. La faisabilité PC et la validation Quest seront suivies séparément. Les inspirations de jeux et les choix de réutilisation sont dans [RESOURCES](RESOURCES.md).
