# Parcours photo et intentions pédagogiques

Mis à jour le 5 octobre 2026. Référence de conception de T04, issue des échanges utilisateur. Chambre/envoi et première arrivée dans DataCenter sont désormais prototypés et testés dans l'exe PC ; les autres séquences restent à implémenter. Les descriptions pédagogiques et actions proposées ne sont pas toutes finalisées. Le sujet original reste `../Le cloud.docx`.

## Choix validés

Option 3 retenue : tous les visiteurs rencontrent un message essentiel par étape ; les mécanismes détaillés, chiffres et textes juridiques sont facultatifs. Les enjeux techniques, économiques, environnementaux et stratégiques font partie de la visite principale. Ils ne sont pas relégués intégralement aux bonus.

Le parcours commence dans une chambre chaleureuse, devant un PC proposant une application photo et une application IA. Le parcours photo sert de premier fil conducteur ; l’IA viendra ensuite. Le décor exact et la manière de s’installer restent à concevoir. La borne isolée est remplacée par ce principe de PC.

Progression guidée avec exploration locale, indices naturels et aide discrète en cas d’hésitation. Plusieurs étapes peuvent partager une pièce. Pas de quiz ou défi final obligatoire ; les actions doivent aider à comprendre. Rechercher une expérience légère, visuelle et rigoureuse. La supervision est conservée et doit montrer le rôle des personnes et des outils dans le fonctionnement du service.

## Structure retenue et actions à préciser

### Installation et acteurs — proposition P06 à discuter

Proposition du 28/09, partiellement validée : acteurs acceptés, avec préférence utilisateur pour des entreprises connues. Le modèle proposé reste un data center pédagogique de colocation situé en France, avec plusieurs salles et plusieurs organisations clientes. La visite ne parcourt qu'une partie représentative du site. Le service photo exploiterait ses propres équipements dans un espace loué ; cette architecture reste à confronter aux entreprises choisies. Ne pas inventer de partenariat ou de lieu d'hébergement. Cette architecture est un cas pédagogique proposé, pas une description universelle du cloud.

Ce choix couvre les zones demandées dans le sujet original : contrôle d'accès, salles serveurs, alimentation, refroidissement et supervision. Il permet de relier coûts partagés, ressources physiques et responsabilités aux quatre familles d'enjeux. Le lieu français donne un contexte aux discussions énergétiques et stratégiques ; il ne suffit pas à conclure à la souveraineté du service. Aucun chiffre de taille, niveau de certification ou engagement de disponibilité n'est proposé à ce stade.

| Acteur proposé | Responsabilité dans ce scénario | Présence pédagogique envisagée, à valider |
|---|---|---|
| Utilisateur du service photo | Envoie et retrouve sa photo | Visiteur devant le PC ; aucune donnée personnelle nécessaire |
| Fournisseur d'accès et opérateurs réseau | Assurent la connectivité entre domicile et service | Repères dans le trajet schématique, sans personnage imposé |
| Entreprise du service photo, cliente du site dans le modèle proposé | Exploite l'application, ses serveurs et le stockage | Entreprise connue à sélectionner ; rôle de son équipe informatique à distinguer de l'équipe bâtiment |
| Exploitant du data center | Fournit l'espace, l'alimentation, le refroidissement et la sécurité physique | Accompagnement de la visite ; métiers de sécurité, exploitation et maintenance visibles selon l'étape |
| Fournisseurs d'énergie et d'équipements | Participent aux ressources et aux coûts du service | Évoqués lors de l'électricité et de la vue d'ensemble ; pas de personnages supplémentaires nécessaires |

Simplification proposée : l'entreprise photo assure ici également l'exploitation informatique, sans ajouter un fournisseur cloud intermédiaire. En approfondissement, montrer que ces rôles peuvent être répartis entre davantage d'entreprises. La supervision du bâtiment et celle du service restent distinctes, même si leur coopération est montrée dans une seule séquence. Le guide ne doit pas être présenté comme capable de tout administrer.

Inconnues restantes : confirmation de la colocation et du cadre français, entreprises connues à sélectionner et relations d'hébergement à sourcer, architecture technique exacte, technologies de refroidissement et de secours. L'accompagnement est déjà cadré : progression guidée, exploration locale, indices naturels et aide discrète ; voix finale avec sous-titres, texte possible au prototype. Ne pas rouvrir un choix générique voix/personnage ; P04 précise encore à titre de proposition le ton et la réécoute. Actions, transitions et critères PC/Quest restent à concevoir étape par étape ; aucun texte de narration définitif.

Base documentaire : [description de la colocation par Equinix](https://www.equinix.com/what-is-colocation) et [documentation des services](https://docs.equinix.com/colocation/), consultées le 28/09/2026. Elles étayent la distinction espace/infrastructures du site et équipements clients ; le scénario et les entreprises restent une proposition originale. Voir RESOURCES.md pour les limites de réutilisation.

Le tableau des étapes et niveaux de lecture est validé. Les actions ci-dessous sont des pistes de mise en scène, pas des spécifications finales.

| Étape | Message essentiel intégré | Action proposée | Approfondissement facultatif |
|---|---|---|---|
| 1. Chambre et envoi | Un geste familier mobilise une infrastructure distante. | Sélectionner une photo d’exemple et l’envoyer depuis le PC. | Fichier local et stockage en ligne. |
| 2. Arrivée au data center | Le lieu est exploité par une organisation et son accès physique est contrôlé. | Suivre la représentation des données, puis entrer comme visiteur accompagné. | Fournisseur d’accès, service, hébergeur et opérateur ; responsabilités pouvant se recouper. |
| 3. Immensité des baies | De nombreux équipements servent de nombreux usages ; rendre sensible l’échelle du stockage. | Révéler les rangées à partir d’une baie et identifier quelques équipements. | Capacités et ordres de grandeur sourcés. |
| 4. Fonctionnement du service | Réseau, traitement et stockage ont des fonctions complémentaires ; distinguer logiciels et machines. | Activer une vue explicative des échanges associés à l’envoi. | Réplication, virtualisation et protocoles. |
| 5. Refroidissement | Les équipements produisent de la chaleur qu’il faut évacuer. | Révéler une vue thermique et les flux de refroidissement. | Technologies et consommation d’eau selon l’installation. |
| 6. Électricité | Informatique et refroidissement nécessitent de l’énergie ; l’alimentation et sa continuité sont organisées. | Suivre les circuits représentés jusqu’aux équipements et aux secours. | Mix électrique, redondance et indicateurs d’efficacité. |
| 7. Supervision | Des personnes et des outils surveillent, entretiennent et organisent les interventions. | Relier un événement de maintenance à un équipement déjà découvert et à l’équipe concernée. | Métiers, coûts et engagements de disponibilité. |
| 8. Vue d’ensemble | Le service dépend de ressources, d’entreprises et de choix d’hébergement. | Relier les éléments sur une maquette puis retrouver la photo disponible sur le PC. | Marché, rentabilité, souveraineté et cadres juridiques. |

## Enjeux et ton

- Technique : comprendre les rôles et dépendances, pas seulement admirer des baies.
- Économie : relier matériel, énergie, locaux et travail humain au service. La forte rentabilité de certains acteurs peut être illustrée avec un exemple daté ; ne pas généraliser à tous les opérateurs ni confondre marge opérationnelle, bénéfice net et chiffre d’affaires.
- Environnement : aborder exploitation et fabrication. L’électricité bas-carbone française est un point d’étude, pas une preuve d’absence d’impact ni une opposition générale France/étranger. Eau, matériaux et implantation sont à contextualiser.
- Stratégie : distinguer emplacement des données, contrôle de l’entreprise, dépendances techniques et droit applicable. Documenter la concentration du marché et les réponses européennes sans transformer « Europe à la traîne » en conclusion universelle imposée.

L’utilisateur souhaite un propos engagé sur les impacts et la souveraineté. Les formulations factuelles devront être vérifiées et les arbitrages rendus compréhensibles. Les affirmations évoquées dans la conversation ne sont pas toutes des faits validés.

## Précautions de représentation

- La photo est un repère symbolique, pas un objet intact circulant dans un câble ; le trajet est schématique, pas mesuré.
- Après l’arrivée, on explore les dépendances du service : la photo ne traverse pas les systèmes de refroidissement ou d’alimentation.
- Le contrôle d’accès des personnes n’est pas l’authentification réseau de la requête.
- Une seule photo ne provoque pas une surchauffe générale. Révéler un phénomène et un refroidissement déjà présents ; ne pas faire croire que l’installation attend le visiteur pour fonctionner.
- L’ambiance sécurisée, immense, presque lunaire souhaitée doit rester compatible avec une installation industrielle crédible.
- Ne pas imposer une fonction par machine ou une étape par pièce. Type de data center et architecture représentée restent à choisir.
- Une maquette globale simple peut aider aux transitions ; la modélisation détaillée de tout le site n’est pas décidée. Déplacements automatiques de caméra et position assise restent à tester en VR.

## Sources déjà consultées et travail restant

Exigences : `../Le cloud.docx`, lu le 28/09 : zones du data center, fonctionnement, quatre familles d’enjeux et livrables PC/Quest. Le sujet ne fixe pas la profondeur technique et n’impose pas explicitement un quiz final.

Pistes documentaires consultées pendant la discussion, à relier aux affirmations exactes avant narration finale :
- [AIE : demande énergétique et refroidissement](https://www.iea.org/reports/energy-and-ai/energy-demand-from-ai).
- [RTE : bilan électrique français 2025](https://analysesetdonnees.rte-france.com/bilan-electrique-2025/synthese).
- [Amazon : résultats du quatrième trimestre 2025](https://ir.aboutamazon.com/news-release/news-release-details/2026/Amazon-com-Announces-Fourth-Quarter-Results/).
- [Commission européenne : Data Act](https://digital-strategy.ec.europa.eu/en/factpages/data-act-explained).
- [Department of Justice : CLOUD Act](https://www.justice.gov/criminal/cloud-act-resources). Ne pas confondre ce texte américain de 2018 avec le Data Act européen ni le présenter comme une réponse à celui-ci.

Pour terminer T04 : choisir l’installation et les acteurs représentés ; préciser pour chaque étape déclencheur, action, résultat visible, transition et aide ; sourcer les mécanismes techniques ; définir les critères de compréhension, fonctionnement PC et confort Quest ; délimiter le premier morceau jouable. Prévoir voix finale et sous-titres, sans figer les textes avant validation pédagogique. Aucun budget de performance ni résultat matériel n’est validé.
