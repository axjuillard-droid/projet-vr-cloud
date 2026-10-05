# Rig XR — validation logique du 5 octobre 2026

Unity 6000.0.58f2, scène Chambre, XRI 3.0.11, sans casque connecté. Rig, interactions et locomotion **implémentés et compilés** ; **12 vérifications XR dans l'éditeur** réussies à 11 h 07 (Paris), puis **9 vérifications de régression souris** à 11 h 08. Preuves : `vr-smoke-result.json`, `photo-smoke-result.json` et captures photo. Les timestamps UTC sont conservés dans les rapports.

## Ce qui a été ajouté

- Prefab officiel Starter Assets `XR Origin (XR Rig)` instancié en Player_VR, départ `(0.2, 0, 0.3)` face au bureau, tracking Floor demandé et hauteur Device de secours 1,65 m.
- InputModeManager contrôle les deux rigs et le viseur : Auto privilégie PC sans XR sur desktop, VR dans un player Android, et peut reconnaître XR après Awake. PC/VR explicites disponibles dans startupMode ; SetMode permet les tests. Manager propre à la scène.
- Téléportation et Snap Turn conservés ; locomotions continues, saisie du rig et escalade désactivées. Deux rayons Teleport sur la couche XRI 31 ; interactions ordinaires sur Default.
- Zone de 1 × 2 m à droite du mobilier : x de 0,9 à 1,9 m, z de −0,7 à 1,3 m. Surface à y=0,006 m, filtre de normale vers le haut. Le reste du sol et le mobilier ne sont pas téléportables. Zone limitée de prototype ; pas de frontière physique du système Quest.
- XRSimpleInteractable sélectionne Screen_Face et appelle PhotoSender.Send via un listener persistant ; même affichage Ready/Sending/Completed qu'en PC.
- Setup éditeur exécuté deux fois : 1 rig, 1 zone, 1 listener persistant XR. Il remplace ses racines Player_VR/VR_Teleportation ; examiner toute personnalisation avant de le relancer.

Les sources réutilisées, leur licence UCL conservée et la documentation sont référencées dans RESOURCES.md. Les textes de l'écran restent provisoires et leur lisibilité à distance est encore faible.

## Portée du test

VRInteractionSmoke est une coroutine réservée à l'éditeur, pas une suite Unity Test Runner. Elle teste la bascule PC → VR → PC, l'unicité de la caméra, les providers/couches/références, l'enregistrement de l'écran, son événement de sélection, les indicateurs de photo et la fin reçue par StepTransition. La sélection est injectée par XRInteractionManager avec un XRRayInteractor temporaire : aucun périphérique ni rayon de manette réel n'est simulé. La requête de téléportation est injectée dans le provider ; son traitement et le déplacement horizontal effectif de la caméra XR sont vérifiés à 3 cm près.

Le test **ne parcourt pas** rayon de manette → hit sur TeleportationArea → sélection/relâchement → création de requête. Il ne teste pas non plus les bindings physiques, le tracking, le déplacement libre dans l'espace, la rotation effective, le confort, les performances ni un build. La prochaine scène est absente : seule la réception de fin de PhotoSender est validée.

Un contrôle distinct a forcé startupMode=VR avant Play : caméra XR, rig PC/viseur inactifs, puis retour PC avec Canvas du viseur et FPS_Camera. Résultat dans `vr-startup-result.json` ; observation ponctuelle complémentaire au test reproductible. Auto a ensuite été rétabli et sauvegardé.

PhotoInteractionSmoke a été relancé après les changements : 9 vérifications par souris synthétique et raycast normal réussies, sans appel direct à Send. Les captures prêtes/envoi/confirmation ont été inspectées. Le contrôleur de déplacement est temporairement désactivé pendant ce test ; pas de nouvelle validation complète des déplacements PC.

Fin de session : hors Play, compilation inactive, Chambre sauvegardée propre, PC actif, VR inactif, 1 caméra active, 0 probe. Console sans erreur détectée ; avertissement URP connu de réduction de résolution de six shadow maps dans l'atlas 2048. Aucun build ni essai casque.

## Reproduire

1. Ouvrir Chambre et attendre imports/compilation. Si le rig est absent : menu `CloudVR > Setup > Rig VR et téléportation`, puis sauvegarder. Prérequis : joueur PC, photo et Starter Assets 3.0.11 déjà préparés.
2. Vérifier InputModeManager.startupMode=Auto et aucune session XR active pour ce scénario.
3. Entrer en Play ; par MCP execute_code : `return VRInteractionSmoke.Run();`.
4. Attendre environ 4 secondes de simulation ; vérifier timestamp récent, passed=true et 12 checks dans `tmp/unity/vr-smoke-result.json`, ainsi que `VR_SMOKE_PASS` dans la console.
5. Dans ce même Play, lancer `return PhotoInteractionSmoke.Run();`, attendre environ 8 secondes, vérifier le nouveau rapport (9 checks) et les captures.
6. Arrêter Play ; vérifier état propre et absence des probes. Aucun test ne modifie durablement la pose du rig ; l'arrêt de Play restaure aussi les réglages de scène.

Pour un essai matériel futur, les bindings importés associent Select au GripButton, Teleport Mode au Primary2DAxis et l'annulation à GripButton. Ces associations sont lues dans l'asset, **pas testées en casque** ; vérifier le geste exact et les profils OpenXR dans le build.

Suite prête : build PC et essai distinct ; ensuite configuration/build Android/OpenXR puis essais Quest, avec modèle, date et résultats conservés.
