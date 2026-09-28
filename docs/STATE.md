# État de reprise

Mis à jour le 28 septembre 2026.

## Situation actuelle
Projet de test dans ConnectionTest/, Unity 6000.0.58f2, URP 17.0.4 et Input System 1.14.2. MCP for Unity 10.0.0 installé et configuré pour Codex.

## Vérifications réussies
- uv et uvx 0.12.19 installés via Astral dans C:\Users\axjui\.local\bin ; exécutables et ajout au PATH utilisateur vérifiés. Python 3.11.5 détecté par MCP Setup, sans réinstallation.
- Blender 4.5.2 LTS : lancement en arrière-plan et lecture Python des objets réussis, sans sauvegarde. Pas de MCP Blender installé.
- Unity : initialisation MCP HTTP et lecture des instances, du projet et de l'état de l'éditeur réussies sur http://127.0.0.1:8080/mcp. Instance ConnectionTest@07be33646bc3e5b2, scène Assets/Scenes/SampleScene.unity, cible StandaloneWindows64.
- Test d'écriture MCP réussi le 25 septembre 2026 : objet vide CodexConnectionTest créé dans SampleScene, actif, sans parent ni enfant, Transform seul, position/rotation (0,0,0), échelle (1,1,1). Scène non sauvegardée (isDirty=true) ; persistance sur disque non testée.

## Prochaine action
T02 clos. T03 terminé. T04 en cours (structure, acteurs et niveaux validés ; actions/transitions et entreprises à détailler).

T05 en cours (28/09) : Git initialisé à la racine de projet_vr_cloud, .gitignore Unity 6 et .gitattributes Git LFS (3.7.1) créés et activés. Remote GitHub public : https://github.com/axjuillard-droid/projet-vr-cloud. Premier commit poussé (74 fichiers, branche main). Invitation collaborateur tjoliot-30 à faire manuellement via Settings → Collaborators. Décision ouverte : conserver ConnectionTest/ ou créer un nouveau projet Unity propre.

Prochaine action : décider du sort de ConnectionTest/ pour clore T05, puis démarrer T06 (première interaction PC/Quest).

Documentation synchronisée le 28/09 : README, VISION, DECISIONS et BACKLOG ; CONTEXT.md décrit les responsabilités des fichiers, l'ordre de lecture et les limites des skills.

## Limites
Pas de build PC/Quest validé ni d'essai casque. Modèles des casques à confirmer. Aucun export Blender/import Unity testé. RAG réel et web conditionnels. Aucune automatisation périodique. Le skill reuse-first est référencé par AGENTS.md ; découverte automatique non configurée.

Remplacer les statuts périmés à chaque mise à jour plutôt qu'accumuler des résultats contradictoires.
