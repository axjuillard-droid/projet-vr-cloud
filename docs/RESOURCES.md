# Ressources évaluées

## T04 — Référence pour l'installation et les acteurs, 28 septembre 2026

Besoin : choisir un cas de data center crédible pour le parcours photo. Référence gratuite à consulter : [Equinix, définition de la colocation](https://www.equinix.com/what-is-colocation) et [documentation des services de colocation](https://docs.equinix.com/colocation/). Ces sources d'opérateur décrivent l'accueil d'équipements clients et les infrastructures communes ; elles ne constituent pas une comparaison indépendante du marché.

Conclusion reuse-first : utiliser ces descriptions comme références factuelles pour P06, puis rédiger un scénario fictif adapté aux huit étapes. Aucun texte, photo, logo, plan ou modèle de ces pages n'est intégré au projet. Aucune licence ouverte de redistribution de leurs médias n'est établie : consultation gratuite ne vaut pas autorisation de copie dans un dépôt public. Conserver les liens comme références. Pas d'asset ni de dépendance à adapter pour PC/Quest à ce stade ; compatibilité et effort technique non évalués. La sélection T03 reste disponible pour la production ultérieure.

MCP for Unity 10.0.0 est intégré à ConnectionTest et testé en lecture et création d’objet par MCP HTTP. Aucun asset de data center sélectionné. Ce registre conserve les recherches et obligations de licence.

## Connecteurs évalués le 25 septembre 2026
- MCP for Unity : https://github.com/CoplayDev/unity-mcp ; licence MIT : https://github.com/CoplayDev/unity-mcp/blob/main/LICENSE. v10.0.0 installé et testé en lecture HTTP et création d’un objet vide le 25/09/2026 ; propriétés et console relues, sauvegarde non testée. Gratuit pour le pont local ; conserver les mentions MIT si redistribué.
- MCP for Blender : https://github.com/ahujasid/mcp-for-blender ; licence MIT : https://github.com/ahujasid/mcp-for-blender/blob/main/LICENSE. Candidat pour contrôle interactif, Blender 3.0+ annoncé. Pont local gratuit, génération premium facultative exclue du test. Conserver les mentions MIT si redistribué. Non installé ni testé.
- Choix provisoire : pilotage Blender par Python déjà fonctionnel sans connecteur ; tester MCP pour le contrôle des scènes ouvertes. Voir TOOLING.md pour les preuves et prérequis.

Pour chaque ressource étudiée, consigner :
- besoin couvert et nom ;
- lien vers la source et vers la licence ;
- date de vérification, version si pertinente ;
- gratuité réelle, contraintes d’usage et de redistribution ;
- attribution requise ;
- compatibilité Unity/Quest/PC ou Blender ;
- décision : retenue, rejetée ou à vérifier, avec une raison courte.

Séparer les contenus pédagogiques sourcés des éléments téléchargés. Avant toute publication, les éléments intégrés devront avoir une attribution conforme ; aucune licence globale du projet n’est choisie pour l’instant.

## T03 — Bases du prototype évaluées le 25 septembre 2026

Besoin : interaction simple et déplacement dans une petite salle, borne et baie de serveurs, Unity 6000.0.58f2 / URP 17.0.4, PC et Quest. Recherche documentaire uniquement : aucun téléchargement, installation ou essai de ces candidats. Le manifest local contient Input System 1.14.2, mais ni XR Interaction Toolkit ni OpenXR en dépendances directes. Les estimations d’effort ci-dessous sont des appréciations, pas des mesures.

| Ressource | Décision proposée et adaptation | Compatibilité et limites |
|---|---|---|
| XR Interaction Toolkit 3.0.11 + Starter Assets | Réutiliser pour interactions, UI et locomotion ; effort moyen de configuration. Importer seulement les samples utiles du package choisi. | La documentation Unity 6000.0 désigne 3.0.11 comme version publiée pour cet éditeur. Résolution des dépendances et compilation avec le projet actuel à vérifier. Le contrôle PC clavier/souris du livrable reste à concevoir ; une simulation XR ne valide pas à elle seule ce livrable. |
| OpenXR Plugin, documentation 1.14.0 | Retenir le principe OpenXR pour la cible Quest ; version exacte à fixer dans Package Manager avec les dépendances. Effort moyen. | Documentation : Unity 2021.3 LTS+, Meta Quest Android arm64/Vulkan. Modèle de Quest inconnu : aucun support matériel ni performance certifié. |
| Kenney Space Station Kit 1.0 | Candidat pour modules de salle ; sélectionner des éléments sobres et adapter l’apparence. Effort faible à moyen. | OBJ/FBX/glTF, Unity annoncé par l’auteur. Import FBX et matériaux URP à vérifier ; décor spatial à ne pas présenter comme architecture réaliste de data center. |
| Kenney Furniture Kit 1.0 | Candidat pour mobilier secondaire uniquement ; effort faible à moyen. | Pack 3D CC0 ; contenu précis, formats de l’archive, échelle et matériaux à inspecter avant intégration. Ne constitue pas un kit de baies. |
| « server rack », Jeremy Eyring, Poly Pizza | Candidat direct pour une baie, en attente de clarification de licence ; effort moyen : inspection Blender, échelle, matériaux, colliders et éventuelle séparation des éléments. | OBJ/glTF annoncés. Mention Creative Commons Attribution, version non exposée dans la page lue : aucune intégration/redistribution tant que la licence exacte et sa provenance ne sont pas confirmées. Géométrie et performance non mesurées. |
| Quaternius Sci-Fi Essentials Kit (novembre 2024) | Alternative non prioritaire : style futuriste et offre gratuite partielle. | La page distingue une partie gratuite et des éditions supplémentaires ; projet URP et fichiers Blend associés à l’édition Source. Ne pas supposer que tout est gratuit. |
| XR Interaction Toolkit Examples, branche main | Écarter comme base à copier pour cette version du projet ; conserver comme référence. | Le dépôt actuel annonce Unity 6000.3 et XRI Examples 3.4.0. Ne pas mettre à niveau le projet pour importer cette démonstration. |

### Licences, coût et dépôt public

- XRI : [licence du package 3.0](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/license/LICENSE.html), Unity Companion License. La [licence UCL](https://unity.com/legal/licenses/unity-companion-license) autorise sans redevance l’usage et la distribution dans son cadre Unity ; conserver licence, copyright et éventuelles notices tierces pour les portions reprises. Une publication du projet Unity est compatible avec ce cadre, pas une relicence des samples sous la licence de notre code. Préférer manifest/lock aux copies du cache des packages.
- OpenXR : [licence 1.14](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.14/license/LICENSE.html), sources sous UCL et autres éléments sous Unity Package Distribution License. Référencer le package par UPM ; ne pas redistribuer son contenu binaire comme un asset libre. Aucun service payant nécessaire pour le principe retenu ; la licence de l’éditeur reste distincte.
- Kenney : packs individuels gratuits, [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/), réutilisation/adaptation et redistribution des assets permises, y compris dans le dépôt public. Attribution non obligatoire selon la [FAQ de l’auteur](https://kenney.nl/support) ; conserver provenance et licence de l’archive par traçabilité, crédit Kenney recommandé.
- Quaternius : CC0 annoncé ; mêmes possibilités pour les fichiers effectivement fournis sous CC0, mais ne pas confondre droit de réutilisation et accès gratuit à toutes les éditions. Le périmètre exact de l’archive gratuite reste à inspecter.
- Baie Poly Pizza : gratuit annoncé ; attribution nécessaire d’après l’intitulé, obligations exactes à vérifier avec la version de licence. Conserver auteur, URL, licence et modifications si le candidat est retenu. Ce candidat n’est pas encore autorisé à entrer dans le dépôt.

### Sources fonctionnelles et assets

- [XRI disponible pour Unity 6000.0](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.xr.interaction.toolkit.html) ; [Starter Assets 3.0](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/samples-starter-assets.html).
- [OpenXR 1.14 : prérequis et plateformes](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.14/manual/index.html).
- [Space Station Kit](https://kenney.nl/assets/space-station-kit) et [formats, gratuité, licence par l’auteur](https://kenney-assets.itch.io/space-station-kit).
- [Furniture Kit](https://kenney.nl/assets/furniture-kit).
- [Baie de Jeremy Eyring](https://poly.pizza/m/6ijQclm8jxw).
- [Quaternius Sci-Fi Essentials Kit : éditions et formats](https://quaternius.com/packs/scifiessentialskit.html).
- [Exemples officiels XRI : prérequis actuels](https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples) et [licence UCL du dépôt](https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples/blob/main/LICENSE.md).

### Conclusion de recherche

Proposition : XRI + OpenXR, quelques modules Kenney CC0, puis inspection de la baie candidate. Si la licence ou la structure de cette baie ne conviennent pas, créer un prefab pédagogique simple de baie et de modules serveurs après cette recherche bornée. Aucun modèle de refroidissement suffisamment évalué ici : commencer par une représentation schématique, à sourcer lors de T04, plutôt que prétendre disposer d’une simulation physique.

Avant de retenir définitivement un asset : vérifier le fichier de licence livré, l’échelle, les pivots, les triangles, les matériaux URP, les colliders et le coût sur le casque réel. T03 termine la sélection documentaire ; les imports et validations techniques appartiennent aux tâches suivantes.
