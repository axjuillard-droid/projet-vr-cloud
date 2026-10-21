# Dossier de Passage de Relais — Projet CloudVR
**Destination : ChatGPT / Autre Agent IA**  
**Date : 21 octobre 2026**  
**Auteur : Codex / Antigravity Assistant**  
**Dépôt Git :** `https://github.com/axjuillard-droid/projet-vr-cloud.git` (Branche `main`)

---

## 1. Contexte du Projet & Objectifs Académiques

- **Projet :** Visite immersive en Réalité Virtuelle d'un Data Center : *« Du clic sur "envoyer une photo" aux équipements physiques du Cloud »*.
- **Cadre :** Projet d'ingénierie universitaire ESILV Année 5.
- **Technologies :** Unity 6 (version exacte `6000.0.58f2`), Universal Render Pipeline (URP), C#, Blender 4.5.2 LTS, OpenXR / Unity XR Interaction Toolkit (XRI 3.0.11).
- **Cibles :**
  1. **PC Standalone Windows (Direct3D11)** : Exécutable `ConnectionTest/Builds/Windows/CloudVR-PC.exe` (contrôles ZQSD + Souris).
  2. **Casque VR Meta Quest (Android ARM64 / Vulkan / OpenXR)** : Package `ConnectionTest/Builds/Android/CloudVR-Quest.apk`.
- **Règle absolue du projet :** **Zéro pack payant de l'Asset Store**. Tous les modèles 3D sont générés de manière 100% procédurale et reproductible par des scripts Python sous Blender 4.5.2 (`tools/art/`), puis importés au format FBX avec des matériaux URP Lit.

---

## 2. Diagnostic Lucide des Anomalies UX et des Bugs du Build Actuel

L'utilisateur a testé l'exécutable `CloudVR-PC.exe` généré le 21/10 et a constaté à juste titre des bugs majeurs qui dégradent l'expérience utilisateur (« c'est pas ouf, il y a beaucoup de bug »).

### Pourquoi le test automatisé (26/26 passed) n'a pas vu ces bugs ?
La suite de smoke tests automatisés (`tools/Test-PCBuild.ps1 -Demo`) s'exécute avec le drapeau `--cloudvr-demo-smoke`. Ce drapeau a été conçu pour valider les machines à états des scripts (envoi photo, chargement de scène, présence des 6 bornes, raycast). Mais il **court-circuite les cinématiques (bypass de 0.1s)** et ne teste pas la disposition spatiale ni la visibilité des objets pour un être humain.

### Les 4 Bugs Majeurs identifiés :

1. **La cinématique de transition réseau ne se joue pas au runtime :**
   - Le script `ConnectionTest/Assets/CloudVR/Scripts/NetworkTransitionV2.cs` a été créé avec les 6 étapes narratives et la barre de progression.
   - **Bug :** Dans la scène `Chambre.unity`, `PhotoSender.cs` déclenche directement `StepTransition.TriggerTransition()`. L'événement n'était pas relié à `NetworkTransitionV2.StartNetworkRide()`. Lors du clic du joueur, Unity coupe instantanément vers la scène `DataCenter.unity` sans transition narrative.

2. **Occlusion spatiale totale dans `DataCenter.unity` (Le joueur est enfermé) :**
   - Dans `DataCenter.unity`, l'ancienne boîte primitive de 10×12 m créée par `DataCenterSceneBuilder.cs` (`Mur_Entree` à $z=-0.1$m, `Mur_Fond` à $z=12.1$m, murs latéraux et plafond) est **restée dans la scène**.
   - Le modèle du campus extérieur (`DataCenter_Campus.fbx`) a été placé à $z=-22$m : il se trouve donc **derrière le mur plein de l'entrée ($z=-0.1$m)**. Le joueur ne peut absolument pas le voir !
   - La grande salle blanche de 40 mètres (`WhiteSpace_Hall.fbx`) a été placée à $z=+12$m : elle se trouve **derrière le mur plein du fond ($z=12.1$m)**. Elle est totalement occultée !
   - Le joueur apparaît à $z=1.5$m, bloqué dans l'ancien couloir étroit avec le sas mantrap posé à $z=0.5$m qui entre en collision avec lui.

3. **Mauvaise répartition des 6 bornes pédagogiques :**
   - Les bornes de la démo (`Borne_01` à `Borne_06` générées par `DemoTourBuilder.cs`) sont restées calées sur les coordonnées de l'ancienne petite salle ($z$ de 3.3m à 10.2m).
   - Elles doivent être redistribuées le long des 40 mètres de la grande salle blanche pour accompagner la marche du visiteur.

4. **Ambiance lumineuse de la chambre :**
   - La lumière blanche clinique par défaut n'a pas été désactivée, ce qui écrase l'ambiance chaleureuse 2700K et les guirlandes LED.

---

## 3. Plan d'Action Immédiat pour ChatGPT

Pour transformer le prototype en une expérience grandiose, fluide et sans bug, voici la feuille de route exacte :

### Étape 1 : Nettoyer et réaligner spatialement `DataCenter.unity`
Dans `ConnectionTest/Assets/CloudVR/Editor/DatacenterMonumentalBuilder.cs` :
1. **Supprimer l'ancienne boîte primitive :** Détruire les objets `Mur_Entree`, `Mur_Fond`, `Mur_Gauche`, `Mur_Droit`, `Sol` et `Plafond` hérités de l'ancien builder.
2. **Organiser l'axe spatial Z en continu :**
   - **Zone 1 (Extérieur / Sécurité) :** $Z \in [-30\text{m}, -4\text{m}]$  
     Le campus extérieur, la guérite vigile, la barrière levante et les générateurs Caterpillar.
     **Point de départ du joueur dans DataCenter :** $X=0, Y=0.08, Z=-25$ (face au campus monumental).
   - **Zone 2 (Sas d'Accès Mantrap Biométrique) :** $Z \in [-4\text{m}, +4\text{m}]$  
     Sas cylindrique avec portes coulissantes animées. La porte extérieure s'ouvre, le joueur entre, la porte se referme, pesée anti-tailgating + scan rétinien (2 secondes), puis la porte intérieure s'ouvre.
   - **Zone 3 (Grande Salle Blanche 40×18m) :** $Z \in [+4\text{m}, +44\text{m}]$  
     Les 32 baies serveurs 42U, le faux plancher soufflant bleuté, le confinement d'allée chaude CAC, la baie interactive 01 (serveur 2U extractible) et le local électrique TGBT/UPS en aile latérale ($X=-22\text{m}$).
3. **Repositionner les 6 bornes pédagogiques dans l'allée centrale de la salle blanche :**
   - Borne 01 (Réseau) : $Z = 8\text{m}$
   - Borne 02 (Traitement / CPU) : $Z = 14\text{m}$ (face à la Baie 01 interactive)
   - Borne 03 (Stockage / NVMe) : $Z = 20\text{m}$
   - Borne 04 (Refroidissement / CRAC) : $Z = 26\text{m}$
   - Borne 05 (Énergie / TGBT) : $Z = 32\text{m}$ (près de la porte du local UPS)
   - Borne 06 (Supervision & Synthèse) : $Z = 38\text{m}$

### Étape 2 : Câbler proprement la Transition Réseau V2
Dans `Chambre.unity` :
1. Attacher `NetworkTransitionV2.cs` à un GameObject dédié `Network_Transition_Manager`.
2. Connecter `PhotoSender.onSendCompleted` à `NetworkTransitionV2.StartNetworkRide()`.
3. S'assurer que `NetworkTransitionV2` affiche le HUD en plein écran, défile les 6 étapes avec la barre de progression, puis appelle `SceneManager.LoadSceneAsync("DataCenter")`.
4. Éteindre la `Directional Light` blanche au profit de la lumière chaude 2700K et des guirlandes pour obtenir le rendu cocooning.

### Étape 3 : Compiler et Valider
1. Exécuter le builder maître dans Unity.
2. Compiler le player Windows : `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait` puis `tools/Build-DemoCopy.ps1 -Complete`.
3. Tester le player : `ConnectionTest/Builds/Windows/CloudVR-PC.exe`.

---

## 4. Guide des Commandes et Outils

| Action | Commande PowerShell |
|---|---|
| **Régénérer les modèles 3D (Blender)** | `python tools/art/generate_cozy_room.py` |
| **Compiler le build Windows Standalone** | `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait`<br>puis `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Complete` |
| **Exécuter la suite de tests PC (26 contrôles)** | `powershell -ExecutionPolicy Bypass -File tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120` |
| **Compiler l'APK Android Meta Quest** | `powershell -ExecutionPolicy Bypass -File tools/Build-AndroidCopy.ps1 -Resume` |
| **Vérifier l'APK Meta Quest (15 contrôles)** | `powershell -ExecutionPolicy Bypass -File tools/Test-AndroidApk.ps1` |

---

## 5. PROMPT COMPLET À COPIER-COLLER DANS CHATGPT

Copiez l'intégralité du bloc ci-dessous dans votre invite ChatGPT :

```markdown
Bonjour ChatGPT ! Tu reprends le projet universitaire d'ingénierie ESILV Année 5 "CloudVR : Visite immersive d’un Data Center" (Unity 6000.0.58f2, URP, C#, Blender 4.5.2 LTS, OpenXR pour PC Windows et Meta Quest).

Le dépôt Git local et distant est à jour : https://github.com/axjuillard-droid/projet-vr-cloud.git (branche main).

### 1. Contexte et Retours Utilisateur
La dernière version livrée dans `ConnectionTest/Builds/Windows/CloudVR-PC.exe` a été testée par l'utilisateur et souffre de bugs majeurs qui bloquent l'expérience :
- La transition réseau réaliste V2 n'est pas jouée au clic sur l'écran (coupure brutale).
- Dans la scène `DataCenter.unity`, le joueur apparaît enfermé dans une ancienne boîte de 10x12m : le campus extérieur (placé à Z=-22m) et la nouvelle salle blanche monumentale de 40m (placée à Z=+12m) sont masqués derrière des murs pleins primitifs non supprimés !
- Le sas d'accès cylindrique mantrap entre en collision directe avec le joueur au spawn.
- Les 6 bornes pédagogiques sont restées entassées dans l'ancien espace restreint au lieu de jalonner l'allée centrale de 40 mètres.

### 2. Ce que tu dois faire immédiatement
Consulte le dossier de passage de relais dans `docs/PASSAGE_RELAIS_CHATGPT.md` et `docs/STATE.md`.
Ta première tâche est de corriger la scénographie pour la rendre grandiose, fluide et parfaitement jouable :

1. Dans `ConnectionTest/Assets/CloudVR/Editor/DatacenterMonumentalBuilder.cs` :
   - Supprime définitivement l'ancienne boîte primitive (`Mur_Entree`, `Mur_Fond`, `Mur_Gauche`, `Mur_Droit`, `Sol`, `Plafond` créés par l'ancien DataCenterSceneBuilder).
   - Réorganise l'axe Z :
     * Z = -28m : Point d'apparition du joueur (Player_PC / Player_VR) dans la cour extérieure, face au bâtiment monumental 28x18x14m, avec la guérite vigile, la barrière et les générateurs Caterpillar.
     * Z = -4m à 0m : Le sas cylindrique biométrique Mantrap (portes extérieures qui s'ouvrent à l'approche, séquence de pesée/scan, puis ouverture sur la salle blanche).
     * Z = 0m à +40m : La grande salle blanche WhiteSpace de 40m avec les 32 racks 42U, le confinement d'allée chaude, et la baie interactive 01 à Z=14m.
   - Distribue les 6 bornes de visite (`DemoTourBuilder.cs`) le long de cette grande allée (Z = 8m, 14m, 20m, 26m, 32m, 38m).

2. Dans `ConnectionTest/Assets/CloudVR/Editor/ChambreArtBuilder.cs` & `PhotoSender.cs` :
   - Câble l'événement `onSendCompleted` de `PhotoSender` pour qu'il déclenche la cinématique narrative de `NetworkTransitionV2.cs` (les 6 étapes avec HUD et barre de progression) avant de charger la scène `DataCenter`.
   - Éteins la lumière directionnelle blanche d'origine pour ne laisser que l'ambiance cocooning ambrée 2700K, la lueur de l'écran, les fairy lights et la pluie contre les vitres.

3. Recompile le projet pour Windows via PowerShell :
   `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait`
   puis
   `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Complete`
   Vérifie que le build s'exécute sans erreur et teste le résultat dans `ConnectionTest/Builds/Windows/CloudVR-PC.exe`.

Reste concis, rigoureux, et documente tes modifications dans `docs/STATE.md` et `docs/BACKLOG.md`.
```
