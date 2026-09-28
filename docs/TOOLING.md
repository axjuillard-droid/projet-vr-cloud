# Outils et connexion

État au 25 septembre 2026.

## Inventaire
- Blender 4.5.2 LTS ; Unity Hub 3.15.2.
- Unity 2022.3.62f3, 6000.0.58f2, 6000.0.58f2-x86_64 et 6000.2.6f2 présents.
- L’installation 6000.0.58f2-x86_64 possède les dossiers Android et WebGL, adb.exe, NDK/source.properties et OpenJDK/bin/java.exe. Présence de fichiers vérifiée, compilation non testée.
- Git et Codex CLI disponibles. Python 3.11.5 reconnu par MCP Setup.
- Préférences Unity Hub non inspectées : accès au dossier utilisateur refusé lors de l’inventaire.

## Installations effectuées
- Projet ConnectionTest créé par l’utilisateur : Unity 6000.0.58f2, URP 17.0.4, Input System 1.14.2.
- MCP for Unity 10.0.0 installé via https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.0.0 ; version confirmée dans le manifest.
- uv et uvx 0.12.19 installés par l’installateur officiel https://docs.astral.sh/uv/getting-started/installation/ ; chemin C:\Users\axjui\.local\bin. Exécutables testés. PATH utilisateur complété en préservant les entrées existantes, puis vérifié.
- Serveur Unity démarré par l’utilisateur en HTTP Local ; Codex configuré sous unityMCP avec http://127.0.0.1:8080/mcp.

## Preuves des tests
Blender : lancement avec --background --factory-startup --python-expr ; lecture bpy de la version et des objets. Réponse CODEX_BLENDER_OK, version 4.5.2 LTS, Camera/Cube/Light, sortie 0. Aucun fichier enregistré. Ce n’est pas une connexion à une fenêtre Blender ouverte.

Unity : initialize puis resources/read pour custom-tools, instances, project/info et editor/state via MCP HTTP. Instance ConnectionTest@07be33646bc3e5b2, scène Assets/Scenes/SampleScene.unity, cible StandaloneWindows64, éditeur disponible, aucune compilation/importation en cours. Aucun objet créé. Le serveur annonce la version 3.4.7 lors du handshake, distincte du package Unity 10.0.0.

## Reprendre une session
1. Ouvrir ConnectionTest dans Unity.
2. Window > MCP for Unity > Toggle MCP Window.
3. HTTP Local, http://127.0.0.1:8080 ; Start Server si nécessaire, puis vérifier Session Active.
4. Vérifier si les outils Unity sont disponibles nativement dans Codex. Ils le sont lors du test d’écriture du 25/09/2026 ; l’accès MCP HTTP direct par PowerShell a aussi fonctionné lors du test initial de lecture.
5. Pour ce mode direct : initialiser une nouvelle session JSON-RPC, accepter application/json et text/event-stream, puis réutiliser le nouvel en-tête Mcp-Session-Id. Les réponses peuvent être en SSE. Ne pas réutiliser un ancien identifiant de session.
6. Lire les instances et l’état avant toute modification. Ne pas confondre disponibilité du serveur et succès d’une action.

## Test d’écriture réussi — 25 septembre 2026
- Précontrôle MCP : une seule instance ConnectionTest@07be33646bc3e5b2, projet local attendu, Unity 6000.0.58f2, éditeur prêt hors Play, aucune compilation ni importation. SampleScene chargée, isDirty=false, 3 objets racines.
- Unique mutation : manage_gameobject(action="create", name="CodexConnectionTest", position=[0,0,0], rotation=[0,0,0], scale=[1,1,1]). Aucun script, asset externe ou composant supplémentaire nécessaire.
- Relecture indépendante : mcpforunity://scene/gameobject/-2504. Objet actif, tag Untagged, layer 0/Default, non statique, sans parent ni enfant, Transform seul ; position/rotation nulles et échelle unitaire confirmées. Identifiant valable pour cette session.
- Hiérarchie relue : 4 racines ; noms, identifiants, états, composants et transformations exposés des trois objets préexistants identiques avant/après.
- Console lue avant/après sans effacement : mêmes 4 logs MCP, aucune erreur ni avertissement.
- Objet laissé dans la scène ouverte ; isDirty=true. Aucune sauvegarde effectuée. Test dans l’éditeur PC uniquement, aucune compilation de build ni validation casque.

## À tester
Sauvegarde contrôlée et persistance de l’objet, export Blender/import Unity, builds PC et Quest et essai casque. MCP Blender reste facultatif et non installé.

## Sources
- https://github.com/CoplayDev/unity-mcp : connecteur MIT ; conserver les mentions si redistribué.
- https://github.com/ahujasid/mcp-for-blender : candidat MIT non installé.
- https://learn.chatgpt.com/docs/extend/mcp : configuration MCP Codex.
