# État de reprise

Mis à jour le 28 septembre 2026.

## Situation actuelle
Projet de test dans ConnectionTest/, Unity 6000.0.58f2, URP 17.0.4 et Input System 1.14.2. MCP for Unity 10.0.0 installé et configuré pour Codex. Aucun prototype pédagogique développé. Aucun dépôt Git initialisé à la racine.

## Vérifications réussies
- uv et uvx 0.12.19 installés via Astral dans C:\Users\axjui\.local\bin ; exécutables et ajout au PATH utilisateur vérifiés. Python 3.11.5 détecté par MCP Setup, sans réinstallation.
- Blender 4.5.2 LTS : lancement en arrière-plan et lecture Python des objets réussis, sans sauvegarde. Pas de MCP Blender installé.
- Unity : initialisation MCP HTTP et lecture des instances, du projet et de l’état de l’éditeur réussies sur http://127.0.0.1:8080/mcp. Instance ConnectionTest@07be33646bc3e5b2, scène Assets/Scenes/SampleScene.unity, cible StandaloneWindows64. Éditeur disponible sans importation ni compilation en cours au moment du test.
- Test d’écriture MCP réussi le 25 septembre 2026 via les outils Unity désormais exposés nativement dans Codex. Disponibilité du serveur et de l’instance vérifiée avant création, éditeur prêt hors Play, sans compilation/importation. Objet vide CodexConnectionTest créé dans SampleScene (instanceID de session -2504), actif, sans parent ni enfant, Transform seul, position/rotation (0,0,0), échelle (1,1,1), tag Untagged, layer Default, non statique ; propriétés relues par une ressource indépendante.
- Hiérarchie passée de 3 à 4 racines ; propriétés exposées de Main Camera, Directional Light et Global Volume identiques avant/après. Console inchangée : 4 logs MCP, aucune erreur ni avertissement. Seule mutation de scène : création de CodexConnectionTest. Objet conservé dans l’éditeur, scène non sauvegardée (isDirty=true) ; persistance sur disque non testée.

## Prochaine action
T02 clos au titre des inconnues explicites : modèles Quest, date exacte et barème inconnus de l’utilisateur. T03 terminé : sélection sourcée dans RESOURCES.md, proposition XRI/OpenXR et modules Kenney CC0 ; baie candidate avec licence exacte à clarifier. Aucun package ni asset installé, aucune scène modifiée pendant cette recherche.

T04 en cours : structure en huit étapes et option 3 validées par l’utilisateur, consignées dans PARCOURS.md. Message essentiel intégré à chaque étape, détails facultatifs ; chambre/PC remplace la borne, pas de défi final obligatoire. Une étape explique le fonctionnement réseau/traitement/stockage ; supervision conservée avec rôle humain. Intentions économiques, environnementales et stratégiques et précautions factuelles enregistrées. Actions détaillées encore proposées, aucune scène pédagogique implémentée.

Reprise T04 du 28/09 : sujet original relu en lecture seule. Acteurs de P06 acceptés en D11 ; préférence pour des entreprises connues, à sélectionner sans inventer leurs relations d'hébergement. Modèle de colocation en France encore proposé. Accompagnement déjà documenté, à ne pas rouvrir comme question générique. Recherche documentaire bornée consignée dans RESOURCES.md. Aucun changement Unity ni installation.

Prochaine action : concevoir l'étape chambre/PC avec l'utilisateur : actions, résultats visibles, transition, simplifications et critères PC/Quest. Choisir et sourcer les entreprises avant de figer le service et le site représentés. Continuer ensuite étape par étape, puis délimiter le premier morceau jouable ; terminer T04 avant T05. Ne pas recréer CodexConnectionTest sans vérifier sa présence ; sa sauvegarde reste à tester.

Documentation synchronisée le 28/09 : README, VISION, DECISIONS et BACKLOG ; CONTEXT.md décrit les responsabilités des fichiers, l’ordre de lecture et les limites des skills. Vérification documentaire locale, pas de test de reprise par une nouvelle session. Aucun changement Unity ni modification du sujet original pendant cette mise à jour.

## Limites
Pas de build PC/Quest validé ni d’essai casque. Modèles des casques à confirmer. Aucun export Blender/import Unity testé. RAG réel et web conditionnels. Aucune automatisation périodique. Le skill reuse-first est référencé par AGENTS.md ; découverte automatique non configurée.

Remplacer les statuts périmés à chaque mise à jour plutôt qu’accumuler des résultats contradictoires.
