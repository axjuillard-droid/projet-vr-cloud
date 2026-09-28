# Décisions et questions ouvertes

État initial issu de la discussion du 25 septembre 2026.

## Validé par l’utilisateur
- D01 (précisé le 28/09) : progression accompagnée avec exploration locale et approfondissements facultatifs ; pas de quiz ou défi final obligatoire. Les interactions servent la compréhension.
- D02 : cloud comme fil conducteur, avec ouverture vers l’IA.
- D03 : choix proposé au visiteur entre envoyer une photo et poser une question à une IA.
- D04 : version PC et version Meta Quest importantes ; web seulement éventuel.
- D05 : narration vocale comme objectif final ; texte acceptable pour prototyper.
- D06 : rechercher l’existant gratuit et adapté avant toute création, puis le réutiliser si pertinent.
- D07 : organiser la mémoire du projet en fichiers et préparer des consignes réutilisables.
- D08 (28/09) : option 3 validée, message essentiel intégré à chaque étape et approfondissements facultatifs. Les quatre familles d’enjeux restent dans le parcours principal.
- D09 (28/09) : structure en huit étapes validée : chambre/PC, arrivée, immensité des baies, fonctionnement du service, refroidissement, électricité, supervision, vue d’ensemble. Voir PARCOURS.md ; actions détaillées encore proposées.
- D10 (28/09) : conserver la supervision avec une situation compréhensible reliée aux équipements précédemment découverts ; plusieurs étapes peuvent partager une pièce.
- D11 (28/09) : acteurs proposés acceptés (utilisateur, réseaux, entreprise du service photo, exploitant du site et fournisseurs). Privilégier des entreprises connues pour rendre leurs rôles parlants ; noms et relations réelles restent à vérifier avant sélection. L'utilisateur rappelle que l'accompagnement est déjà décrit dans les documents.

## Propositions de travail, modifiables
- P01 : premier jalon centré sur la photo ; second parcours IA scénarisé avant un véritable RAG.
- P02 : représentation de la requête, exploration d’une baie et démonstration simplifiée du refroidissement.
- P03 : sessions IA limitées à une tâche, suivi court et revue humaine régulière.
- P04 : voix chaleureuse et précise, déclenchée par séquences, avec réécoute et sans superposition.
- P06 (28/09, T04, partiellement validée) : acteurs acceptés en D11. Site pédagogique de colocation en France et architecture du service encore proposés. La préférence pour des entreprises connues remplace la proposition d'identités fictives ; ne pas inventer de relation commerciale pour les faire entrer dans ce modèle. Détails dans PARCOURS.md.

## À préciser avant les choix techniques
- Modèles exacts des Quest, configuration des PC de développement et cible PC.
- Versions installées de Unity et Blender, modules Android et packages XR disponibles.
- Exigences ou barème enseignant, usage autorisé des ressources tierces et de l’IA.
- Répartition des responsabilités dans le binôme.
- Accès réseau durant les démonstrations ; intérêt d’un mode entièrement hors ligne.
- Date finale exacte et dates intermédiaires lorsqu’elles seront connues.
- Abonnement réellement disponible et réserve souhaitée pour les autres usages.

## À décider après un premier prototype
- RAG réel local ou distant, documents de référence et éventuels coûts d’exploitation.
- Solution vocale et droits de diffusion des enregistrements.
- Utilité de la version web par rapport à son coût d’adaptation.
- Style visuel et objectifs chiffrés de performance.

Ajouter les décisions importantes avec leur raison. Ne pas transformer une suggestion de l’IA en décision utilisateur.

## Point de reprise — 25 septembre 2026

T02 : l’utilisateur indique ne pas connaître les modèles de Quest, la date exacte de rendu et le barème. Configuration et cible précise des PC, règles enseignantes sur les ressources/IA, responsabilités du binôme et accès réseau restent également non renseignés. Ces inconnues sont consignées ; ne pas en déduire une exigence ni une validation. Les versions locales sont désormais inventoriées dans TOOLING.md.

P05 (proposition technique issue de T03) : évaluer XRI 3.0.11 + OpenXR sous Unity 6000.0.58f2 et des modules Kenney CC0. Préserver la version de l’éditeur pendant le premier prototype ; versions des dépendances et assets précis à confirmer lors de leur intégration. Voir RESOURCES.md pour les licences et les alternatives. Aucun choix de matériel ou de performance chiffrée n’est arrêté.
