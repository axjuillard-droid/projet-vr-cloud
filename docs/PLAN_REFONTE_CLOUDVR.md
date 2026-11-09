# Plan de refonte CloudVR

Deux parcours se succèdent. **Découvrir** suit une photo à hauteur des équipements. **Exploiter** commence au balcon et suit l’électricité, la chaleur et l’eau au-delà du bâtiment. La progression se juge à la compréhension, à la qualité des gestes et au rendu.

## 0. Protéger la version livrée

Avant la première modification Unity, copier l’intégralité de `ConnectionTest/Builds/Windows` dans un dossier d’archive distinct. Comparer les empreintes et lancer l’exécutable depuis l’archive. Conserver le dossier livré à son emplacement. Les builds locaux étant ignorés par Git, un repère Git seul protège les sources et la documentation. Créer aussi un repère sur le commit de livraison T25 et travailler dans une copie Git séparée, sur `codex/cloudvr-v2`. Faire pointer la livraison V2 vers un dossier distinct, par exemple `Builds/Windows-V2`, et vérifier la destination avant chaque copie. Le présent jalon fournit le plan. La sauvegarde, le worktree et le build V2 sont des actions de démarrage.

## 1. Découvrir le data center

| Séquence | Geste et résultat | Notion à retenir |
|---|---|---|
| Chambre | Cliquer sur l’écran. Une miniature identifiable reste visible et son envoi passe en cours. | Le PC est un client qui demande un service distant. |
| Trajet et arrivée | Suivre une animation courte jusqu’à l’entrée du réseau du site. Retrouver le bâtiment à taille réelle. Franchir un sas au guidage visible. | Le trafic arrive par un réseau. Le sas contrôle les accès physiques. |
| Entrée réseau | Suivre le signal du portique à la baie réseau. Activer un repère sur un port et suivre la demande jusqu’au serveur de l’application. | Les équipements réseau acheminent les données. Un serveur exécute le service. |
| Baie école | Ouvrir la porte, extraire le tiroir de formation, puis lancer une génération de miniature sur un pupitre de démonstration. Comparer la photo à son aperçu. | Une baie contient des serveurs. Le calcul transforme des données, avec des rôles distincts pour le CPU, la mémoire et le stockage local. |
| Stockage et vérification | Suivre la photo vers le stockage, voir la confirmation, puis ouvrir la même miniature sur un écran client. | Recevoir, enregistrer et relire sont des étapes distinctes. |
| Passage au balcon | Suivre une enseigne fixe vers la passerelle. Retrouver la baie école, puis découvrir les rangées et l’étendue de la salle. | La visite change d’échelle et de point de vue. |

Une photo d’exemple, un identifiant de scénario et un état partagé suffisent au prototype. Les pannes réseau et la récupération d’un fichier peuvent revenir comme démonstrations facultatives. Des enseignes fixes et des repères sur les équipements remplacent les coordonnées et les indications dépendantes de l’orientation.

## 2. Montrer les trois enjeux

1. **Électricité.** Depuis le balcon, pousser la manette de demande de calcul. Les secteurs en veille s’activent par vagues et le pupitre indique la marge de raccordement. Un parcours extérieur suit les lignes vers une centrale qui représente une des sources du réseau. Le panache blanc de sa tour montre le rejet de chaleur et les lignes mises en évidence représentent l’alimentation.
2. **Refroidissement.** Ouvrir la vanne du secteur. Suivre un aller froid et un retour chaud jusqu’à l’échangeur, puis voir la chaleur rejetée dehors et le liquide refroidi repartir. Le même secteur est identifiable du balcon à l’équipement extérieur. L’eau du circuit interne recircule.
3. **Eau.** Rejoindre la prise d’eau du site fictif. Comparer le même niveau de calcul en débit normal et en été sec. Distinguer l’appoint prélevé, le retour éventuel et la part évaporée par la tour. En été sec, la scène montre une zone peu profonde qui se rétrécit et la faune qui se replie. Réduire la charge fait diminuer la demande d’appoint. Les paramètres hydrologiques et thermiques sont illustratifs.

Les trois séquences partagent l’état du site. Une même demande alimente les représentations du calcul, de la chaleur à évacuer et de l’eau d’appoint. La découverte introduit les équipements. Chaque enjeu a ensuite un geste et une conséquence visible.

## 3. Ordre de production et validation

| Jalon | Livrable vérifiable |
|---|---|
| A. Conservation et direction artistique | Archive démarrable, espace V2 isolé, storyboard des deux parcours, référence d’éclairage, palette, signalétique et un premier plan PC de la chambre et de l’allée. |
| B. Découverte jouable | De la chambre à la relecture et à l’arrivée au balcon. Un novice retrouve l’équipement suivant sans aide orale et explique le rôle du réseau, du calcul et du stockage. |
| C. Première interaction depuis le balcon | La manette éveille plusieurs rangées, le secteur se repère à distance et le trajet mène à l’extérieur. Un novice relie l’éveil à la demande de calcul et distingue le rôle des lignes et celui de la tour. |
| D. Boucle thermique et rivière | Le joueur suit visuellement le trajet de la chaleur et distingue le liquide qui recircule de l’eau d’appoint consommée. Le changement de débit et la réduction de charge sont observables. |
| E. Finition et livraison | Revue d’images en mouvement, qualité sonore et sous-titres, échelle et circulation, lisibilité depuis plusieurs orientations, absence de blocage, tests fonctionnels et mesures sur le PC de démonstration. Build V2 dans son dossier propre. Quest validé séparément sur casque réel. |

Créer les scènes V2, leurs parcours et leur direction artistique dans l’espace isolé. Réévaluer les modèles de baie, le tiroir, les interactions PC/XRI et les outils de build existants. Reprendre les composants qui passent la revue de qualité et de compatibilité. Voir [la proposition précédente](PROPOSITION_REFONTE_VISITE.md) et [les ressources évaluées](RESOURCES.md).
