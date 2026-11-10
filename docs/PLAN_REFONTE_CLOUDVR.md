# Plan de refonte CloudVR

Deux parcours se succèdent. **Découvrir** suit une photo à hauteur des équipements. **Exploiter** commence au balcon et suit l’électricité, la chaleur et l’eau au-delà du bâtiment. La progression se juge à la compréhension, à la qualité des gestes et au rendu.

## 0. Protéger la version livrée

Avant la première modification Unity, copier l’intégralité de `ConnectionTest/Builds/Windows` dans un dossier d’archive distinct. Comparer les empreintes et lancer l’exécutable depuis l’archive. Conserver le dossier livré à son emplacement. Les builds locaux étant ignorés par Git, un repère Git seul protège les sources et la documentation. Créer aussi un repère sur le commit de livraison T25 et travailler dans une copie Git séparée, sur `codex/cloudvr-v2`. Faire pointer la livraison V2 vers un dossier distinct, par exemple `Builds/Windows-V2`, et vérifier la destination avant chaque copie. Le présent jalon fournit le plan. La sauvegarde, le worktree et le build V2 sont des actions de démarrage.

## 1. Découvrir le data center

| Séquence | Geste et résultat | Notion à retenir |
|---|---|---|
| 1. Chambre | Cliquer sur Envoyer. La miniature entre dans l’écran et un son bref confirme le départ. | Le PC demande un service distant. |
| 2. Trajet | Suivre la miniature dans une cinématique courte et retrouver la façade à taille réelle. Le contrôle revient face à l’entrée. | Le réseau achemine la demande. |
| 3. Façade et sas | Présenter le badge de visiteur. Le sas valide l’accès physique et ouvre la seconde porte après fermeture de la première. | Le badge autorise une personne à entrer. La photo poursuit son trajet par le réseau. |
| 4. Arrivée réseau | Brancher un cordon lumineux sur un panneau école. Le port s’allume et un signal mène vers le serveur de l’application. | Le réseau oriente la demande vers le bon service. |
| 5. Allée et baie école | Ouvrir, extraire, puis lancer l’aperçu. Trois consignes successives accompagnent ces gestes. Une coupe animée montre CPU, RAM et NVMe. | Le serveur traite la donnée et produit un aperçu. |
| 6. Stockage | Prédire où la photo sera conservée en visant un des deux équipements. Le signal révèle le stockage prévu dans ce scénario. Ouvrir la photo sur un écran client. | Réception, enregistrement et relecture sont distincts. |
| 7. Balcon | Avancer vers la vitre. Retrouver la baie école grâce à son repère, puis découvrir les rangées. | Passer d’un serveur à une salle entière. |

Le trajet physique avance dans le même ordre que le trajet raconté. Un trait cyan indique seulement la prochaine cible. La miniature et son statut (« en attente », « reçue », « enregistrée », « lisible ») restent visibles avec une présentation adaptée au PC et au casque. Une ligne de consigne et un verbe à la fois. Les transitions répondent aux gestes, avec des animations courtes. La prédiction au stockage donne un retour immédiat quel que soit le choix. Les modules A/B et restauration restent facultatifs. Les durées proposées servent de repères de test. Aucun objectif global de durée. Une attente passive de plus de 20 à 25 secondes déclenche une revue de mise en scène.

## 2. Montrer les trois enjeux

1. **Électricité.** Depuis le balcon, pousser la manette de demande de calcul. Les secteurs en veille s’activent par vagues et le pupitre indique la marge de raccordement. Un parcours extérieur suit les lignes vers une centrale qui représente une des sources du réseau. Le panache blanc de sa tour montre le rejet de chaleur et les lignes mises en évidence représentent l’alimentation.
2. **Refroidissement.** Ouvrir la vanne du secteur. Suivre un aller froid et un retour chaud jusqu’à l’échangeur, puis voir la chaleur rejetée dehors et le liquide refroidi repartir. Le même secteur est identifiable du balcon à l’équipement extérieur. L’eau du circuit interne recircule.
3. **Eau.** Rejoindre la prise d’eau du site fictif. Comparer le même niveau de calcul en débit normal et en été sec. Distinguer l’appoint prélevé, le retour éventuel et la part évaporée par la tour. En été sec, la scène montre une zone peu profonde qui se rétrécit et la faune qui se replie. Réduire la charge fait diminuer la demande d’appoint. Les paramètres hydrologiques et thermiques sont illustratifs.

Les trois séquences partagent l’état du site. Une même demande alimente les représentations du calcul, de la chaleur à évacuer et de l’eau d’appoint. La découverte introduit les équipements. Chaque enjeu a ensuite un geste et une conséquence visible.

## 3. Ordre de production et validation

**Valider le rendu en mouvement avant de produire tous les décors.** Construire d’abord un parcours gris complet à l’échelle du joueur. Vérifier les circulations, la taille des cibles, les positions d’arrêt et le passage du sas. Produire ensuite deux séquences de référence finies, de la chambre au port réseau et de la baie école au balcon. Elles fixent le niveau de qualité attendu pour l’ensemble.

- **Image.** Chambre chaude, façade neutre, salle technique froide, balcon lumineux. Fixer les matériaux, contrastes et températures de lumière sur des captures prises dans le player. Contrôler notamment le crénelage, le scintillement des surfaces et la lisibilité des voyants en tournant la caméra.
- **Décor.** Préparer une gamme cohérente de baies, portes, chemins de câbles et tuyaux. Concentrer les détails sur les objets manipulés et les premiers plans. Utiliser des variantes sobres pour les rangées visibles depuis le balcon et un plan extérieur lisible pour la centrale et la rivière.
- **Son et guidage.** Ambiances différenciées, confirmations discrètes pour les gestes, sous-titres et pictogrammes. Les cibles se distinguent par leur silhouette ou leur libellé en plus de la couleur. Ajuster la signalétique à partir des hésitations observées.
- **Architecture.** Définir un état de progression Photo et un état partagé pour les enjeux. Les commandes envoient des actions communes au PC et au casque. Les animations, les sons et les changements de lumière réagissent à ces états. Vérifier la reprise après pause, le reset et l’absence de double déclenchement. Pour le casque, prévoir une transition confortable sans déplacement forcé de la caméra.

| Jalon | Livrable vérifiable |
|---|---|
| A. Conservation et direction artistique | Archive démarrable, espace V2 isolé, storyboard des deux parcours, référence d’éclairage, palette, signalétique et un premier plan PC de la chambre et de l’allée. |
| B. Découverte jouable | De la chambre à la relecture et à l’arrivée au balcon. Un novice retrouve l’équipement suivant sans aide orale et explique le rôle du réseau, du calcul et du stockage. |
| C. Première interaction depuis le balcon | La manette éveille plusieurs rangées, le secteur se repère à distance et le trajet mène à l’extérieur. Un novice relie l’éveil à la demande de calcul et distingue le rôle des lignes et celui de la tour. |
| D. Boucle thermique et rivière | Le joueur suit visuellement le trajet de la chaleur et distingue le liquide qui recircule de l’eau d’appoint consommée. Le changement de débit et la réduction de charge sont observables. |
| E. Finition et livraison | Revue d’images en mouvement, qualité sonore et sous-titres, échelle et circulation, lisibilité depuis plusieurs orientations, absence de blocage, tests fonctionnels et mesures sur le PC de démonstration. Build V2 dans son dossier propre. Quest validé séparément sur casque réel. |

Créer les scènes V2, leurs parcours et leur direction artistique dans l’espace isolé. Réévaluer les modèles de baie, le tiroir, les interactions PC/XRI et les outils de build existants. Reprendre les composants qui passent la revue de qualité et de compatibilité. Voir [la proposition précédente](PROPOSITION_REFONTE_VISITE.md) et [les ressources évaluées](RESOURCES.md).
