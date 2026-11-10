# Relier les deux parcours de CloudVR V2

## Parcours Photo

1. **Fibre et brassage.** Une conduite cyan arrive de la cour, passe par le boîtier de façade et rejoint le tiroir optique. Le joueur prend le cordon et branche le port A. L’impulsion traverse le cordon puis suit le tracé vers la baie école. La cinématique représente la fibre du site ; un raccord de cadrage à son entrée reste à faire pour rendre ce lien évident.
2. **Calcul dans l’allée.** Installer la façade de la baie école et son tiroir face au chemin. Une fois le tiroir tiré, éclairer les composants dans l’ordre où le joueur en a besoin : mémoire de travail, traitement par le CPU, stockage local de travail sur le NVMe. Le joueur lance la fabrication de l’aperçu depuis le serveur. Cette séquence illustre un exemple de séparation des rôles ; la destination durable de la photo est le service de stockage du scénario.
3. **Stockage à manipuler.** Retirer la question à deux réponses. Prolonger le tracé à partir du port de sortie du serveur. Deux banques visibles indiquent leur disponibilité. Le joueur sélectionne une banque libre, déclenche l’écriture et voit la miniature suivre le câble puis entrer dans la banque. Il ouvre l’écran client pour vérifier la relecture. Une banque indisponible explique le refus sur place et laisse réessayer. Utiliser le trajet et la miniature pour distinguer réception, traitement, écriture et relecture.

## Parcours Exploitation

| Étape | Action | Effet observable et condition de réussite |
| --- | --- | --- |
| Demande | Augmenter le calcul demandé au balcon. | Les rangées demandées restent éteintes et leurs repères apparaissent sur le tableau. |
| Énergie | Baisser les leviers de commande dans l’armoire électrique du site. | Les chemins réseau électrique → tableau → départs de salle s’éclairent. Chaque départ autorise une rangée identifiée, dans la limite de la puissance disponible. |
| Refroidissement | Suivre la conduite orange et augmenter la circulation du liquide. | La chaleur monte après la mise en service des rangées. Le retour chaud va des baies à l’échangeur, puis le départ refroidi revient aux baies. La température redescend progressivement ; la pompe et les ventilateurs consomment de l’électricité. |
| Eau | Au bord de la rivière, amorcer la pompe puis régler le prélèvement. | La conduite de la pompe rejoint le circuit d’appoint du refroidissement évaporatif du data center. Sur le scénario d’été sec, un prélèvement maintenu trop fort abaisse le niveau, puis le stress des poissons apparaît. Une hypoxie prolongée peut provoquer des mortalités dans cette simulation. Réduire le débit ou la charge évite d’atteindre ce seuil. |

Le circuit de liquide interne est fermé. L’eau de rivière sert ici d’appoint à un équipement évaporatif distinct qui rejette de la chaleur. La centrale vue à l’extérieur appartient à l’illustration du réseau électrique. Le rejet d’un échangeur sec est de l’air réchauffé ; les enjeux de qualité de l’air s’illustrent séparément, par exemple avec les groupes diesel de secours. Les seuils, débits et temps sont des paramètres de jeu.

- [DOE — Gestion de l’eau des tours de refroidissement](https://www.energy.gov/cmei/femp/best-management-practice-10-cooling-tower-management)
- [EPA — Oxygène dissous et stress des poissons](https://www.epa.gov/caddis/dissolved-oxygen)
- [EPA — Qualité de l’air et data centers](https://www.epa.gov/stationary-sources-air-pollution/clean-air-act-resources-data-centers)
