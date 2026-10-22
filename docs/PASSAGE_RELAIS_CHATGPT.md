# Dossier de Passage de Relais — Projet CloudVR
**Destination : ChatGPT / Autre Agent IA**  
**Date : 21 octobre 2026 (Mise à jour après Audit Global Approfondi)**  
**Auteur : Antigravity Assistant (DeepMind)**  
**Dépôt Git :** `https://github.com/axjuillard-droid/projet-vr-cloud.git` (Branche `main`)  
**Rapport d'audit de référence :** `docs/AUDIT_GLOBAL_2026-10-21.md`

---

## État courant — T17 livré le 22/10 à 11 h 55 Paris

Voyage 3D de 7,50 s réutilisant les FBX fibre/campus et le son existants, statut de stockage prématuré retiré. **68/68 contrôles PC**, 14 P0 + 20 P1 + 11 P2 pré-build, 8/8 contrôles de logique confort par API sans casque. Narration observée dans la fenêtre native. Les captures `Camera.Render` omettent les Canvas Overlay : ne pas en déduire une absence d'interface. [Preuves T17 et limites](validation/network-3d-2026-10-22/README.md).

MCP HTTP/SSE utilisé après activation utilisateur, outils natifs absents du catalogue courant. Chambre propre restituée, console sans erreur retournée. Vingt matériaux modifiés dès le départ préservés/exclus du commit T17 ; ne pas les réinitialiser. VR sur écran fixe : logique testée, rendu/tracking/Quest et confort non validés. **Prochaine tâche prête : T18**, local TGBT et supervision. **Publication T17 confirmée : `a6ce9f6` sur origin/main**, 30 objets LFS / 11 Mo ; vingt matériaux initiaux restent hors commit. STATE et les preuves T17 consignent les validations.

## Historique — P1 livré le 21/10 à 23 h 15 Paris

**T16 terminé** : Baie_01 ouvre sa porte, extrait son serveur, demande l'inspection CPU/RAM/NVMe puis la remise en place. La borne 04 demande de réparer une fuite de confinement, avec mauvaises options et retour thermique progressif ; refroidissement toujours actif. Modèle fictif, ratio de puissance sous hypothèse de régime constant. **64/64 contrôles player DX11**, 20 P1 + 14 P0 pré-build, BuildPipeline 0 erreur/avertissement. Captures et limites : [preuves P1](validation/p1-2026-10-21/README.md).

**Prochaine tâche prête : T17/P2, voyage réseau 3D** ; quatre autres activités encore au prototype, casque et performance non validés. Les demandes P0/P1 historiques plus bas ne doivent pas faire refaire les lots livrés. MCP utilisé ; Chambre restituée propre, console retournant zéro erreur. Audit composite final additionnel incomplet après deux erreurs NullReferenceException de requêtes : ne pas revendiquer de bilan complet des matériaux final. Les scènes originales sont identiques à la copie compilée par SHA-256.

Verrou Git historique revu après GO puis sauvegardé sous tmp, sans arrêter de processus. **P0/T20/P1 publiés : `cddd29a` sur origin/main**, SHA distant confirmé ; STATE et les preuves P1 consignent la validation. Les mentions de publication suspendue plus bas sont historiques.

## Historique de reprise — P0 livré le 21/10 à 22 h 05 Paris

Correctif utilisateur ultérieur **T20 livré à 22 h 38** : trois groupes électrogènes étaient coplanaires au mur ouest de salle blanche à X=−9 m. Déplacés hors du bâtiment, écart désormais 6 m ; comparaison de sept angles et sept vues player propres, 14 contrôles géométriques et 40/40 contrôles PC. [Preuves et limites](validation/west-wall-2026-10-21/README.md). Dernier build local à utiliser ; publication Git toujours suspendue. Après réimport final, le MCP ne répond plus aux pings ; revérifier sa connexion avant de piloter l'éditeur.

Les instructions P0 ci-dessous sont désormais **implémentées** : allée mesurée à 2,20 m, 32 baies au total, confinement raccordé avec portes, local électrique accessible au clavier, sas compact transparent et six consoles 3D sans HUD massif. Build Windows 0 erreur/0 avertissement ; **40/40 contrôles player réussis**, captures et limites dans [les preuves P0](validation/p0-2026-10-21/README.md). MCP Unity effectivement connecté et utilisé lors de cette reprise, contrairement au constat de l'audit de 16h56. Scènes originales relues sans matériau manquant.

Lire d'abord `STATE.md` et `BACKLOG.md` ; le prompt P0 historique plus bas ne doit pas faire refaire ce travail. **Prochain morceau prêt : T16/P1**, manipulation de Baie_01 et résolution d'une anomalie thermique, puis T17/P2 pour le voyage réseau 3D. Les six activités et la transition V2 restent celles du prototype ; le texte « activer le refroidissement » est encore à reprendre. Aucun test Quest ni nouvel APK.

Publication locale en attente de revue : deux préparations de commit refusées par `index.lock`, Git LFS filter-process encore actif. Aucun commit/push P0 ; sources, scènes et preuves conservées non commitées. Voir T19 et `validation/p0-2026-10-21/git-publication.json` avant tout nouveau travail ou publication.

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

## 2. Bilan de l'Audit Approfondi du Build 16h56

L'exécutable `ConnectionTest/Builds/Windows/CloudVR-PC.exe` (build du 21/10 à 16h56) a été exécuté sur GPU Direct3D11 et audité visuellement à partir de 9 captures d'écran réelles (`tmp/unity/pc-player-20261021-180632-b5ab9e/`).

### Le paradoxe des tests : 37/37 au vert, mais expérience humaine défectueuse
La suite de smoke tests automatisés (`tools/Test-PCBuild.ps1 -Demo`) valide la logique d'état (marche à Z=39m, raycasts sur les bornes, progression des étapes). Cependant, ces assertions n'évaluent ni le rendu subjectif, ni la pertinence pédagogique, ni l'ergonomie VR.

### Tableau synthétique des 12 défauts identifiés (cf. `docs/AUDIT_GLOBAL_2026-10-21.md`) :
1. **DEF-01 (Bloquant)** : **Allée centrale de 8 m de large** avec baies clairsemées ($X=\pm 4$m et $\pm 6$m). Inconcevable dans un vrai data center où le m² de salle blanche est optimisé.
2. **DEF-02 (Majeur)** : **Confinement thermique CAC flottant** à 2,6 m de haut sans reposer sur les baies ni fermer les rangées.
3. **DEF-03 (Bloquant)** : **Local TGBT/UPS (`Electrical_Room.fbx`) totalement inaccessible**, placé à $X=-26$m derrière un mur plein étanche de la salle blanche.
4. **DEF-04 (Majeur)** : **Sas Mantrap géant ($\times 2$, 4m de large)** perdant son rôle anti-tailgating, et écran 100% gris aveugle pendant les 2s de scan (risque de nausée VR).
5. **DEF-05 (Majeur)** : **HUD 2D massif** (3 bandeaux noirs fixes) couvrant 45% du champ de vision avec du texte brut, rompant l'immersion.
6. **DEF-06 (Majeur)** : **Poteau électrique PDU traversant l'écran de la Borne 01** et coupant son texte ("01 R|SEAU").
7. **DEF-07 (Majeur)** : **Transition réseau statique** : le joueur reste immobile dans la chambre pendant 10,8s devant du texte, avec un statut "Fichier persisté" prématuré.
8. **DEF-08 (Bloquant)** : **Boucle d'interaction répétitive** : les 6 bornes appellent la même sphère glissant sur une ligne au plafond pendant 2,8s vers la Baie 01.
9. **DEF-09 (Majeur)** : **Contresens conceptuel sur le refroidissement** : l'interaction demande d'"activer le refroidissement" pour une photo au lieu de maintenir une régulation ou corriger une perturbation thermique.
10. **DEF-10 (Moyen)** : **Campus extérieur flottant dans un néant sombre** sans skybox atmosphérique fermée ni décor d'horizon.
11. **DEF-11 (Moyen)** : **Éclairage saturé de blanc** dans la chambre et matériaux plastiques unis sans PBR.
12. **DEF-12 (Moyen)** : **1 490 MeshRenderers non batchés** risquant de pénaliser le framerate sur Meta Quest.

---

## 3. Plan d'Action Priorisé de Réhabilitation

### Phase P0 : Fondations Spatiales, Confinement & Rendu (Priorité Immédiate)
1. **Resserrer l'allée centrale de la salle blanche** : passer l'allée centrale de circulation de 8,0 m à 2,2 m de large dans `DatacenterMonumentalBuilder.cs`. Rapprocher les rangées de baies 42U pour former deux allées froides confinées denses.
2. **Sceller le confinement CAC** : ajuster les toitures en polycarbonate pour qu'elles reposent directement sur le dessus des baies, avec des portes coulissantes vitrées aux extrémités.
3. **Ouvrir l'accès au local TGBT/UPS** : créer une ouverture ou une baie vitrée à $X=-3$m permettant au visiteur d'entrer et de voir les armoires d'onduleurs et les batteries.
4. **Supprimer le HUD 2D massif** : basculer l'affichage des consignes sur les pupitres et écrans 3D du monde (interface diégétique).
5. **Dégager la visée de la Borne 01** : repositionner les descentes de busway pour ne pas chevaucher la borne réseau.

### Phase P1 : Module de Référence Interactif "Traitement & Refroidissement"
1. **Interaction Baie 01 & Serveur 2U** : ouverture de la baie 01 par poignée quart-de-tour, extraction télescopique du serveur 2U modélisé sous Blender (`Server_Blade_Detailed.fbx`), révélation des bi-CPU, RAM et SSD NVMe avec étiquettes holographiques.
2. **Gestion thermique dynamique** : vue thermique diégétique (22°C allée froide, 38°C rejet arrière), simulation d'une fuite d'air froid (porte de confinement entrouverte à refermer pour rétablir le régime nominal).

### Phase P2 : Transition Réseau 3D
- Remplacer le temps d'attente statique dans la chambre par un véritable trajet 3D dynamique de 6 à 8 secondes (entrée dans le coeur de la fibre monomode, survol réseau et adduction dans le data center).

### Phase P3 : Supervision Globale et Métriques Réelles
- Clôture de la visite devant un mur de supervision DCIM avec affichage du PUE (1,22 décomposé), de l'empreinte carbone et des enjeux de souveraineté.

---

## 4. Boîte à Outils et Commandes Reproductibles

| Action | Commande PowerShell |
|---|---|
| **Vérifier l'état Git** | `git status` |
| **Régénérer les modèles Blender** | `python tools/art/generate_datacenter_hall.py`<br>`python tools/art/generate_server_blade.py` |
| **Compiler le build Windows Standalone** | `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait`<br>puis `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Complete` |
| **Exécuter la suite de tests PC GPU (avec captures)** | `powershell -ExecutionPolicy Bypass -File tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120` |
| **Compiler l'APK Android Meta Quest** | `powershell -ExecutionPolicy Bypass -File tools/Build-AndroidCopy.ps1 -Resume` |
| **Vérifier l'APK Meta Quest (15 contrôles statiques)** | `powershell -ExecutionPolicy Bypass -File tools/Test-AndroidApk.ps1` |

---

## 5. PROMPT COMPLET À COPIER-COLLER DANS CHATGPT

Copiez l'intégralité du bloc ci-dessous dans votre invite ChatGPT pour lancer la reprise :

```markdown
Bonjour ChatGPT ! Tu reprends le projet universitaire d'ingénierie ESILV Année 5 "CloudVR : Visite immersive d’un Data Center" (Unity 6000.0.58f2, URP, C#, Blender 4.5.2 LTS, OpenXR pour PC Windows et Meta Quest).

Le dépôt Git distant est à jour : https://github.com/axjuillard-droid/projet-vr-cloud.git (branche main).
Dossier local : C:\Users\axjui\Dropbox\Ecole_Axel\ESILV\année 5\projet_vr_cloud

### 1. Contexte et Diagnostic Récent
Un audit critique approfondi indépendant vient d'être réalisé sur l'exécutable `ConnectionTest/Builds/Windows/CloudVR-PC.exe` (146,87 Mo, build du 21/10 à 16h56) et documenté dans `docs/AUDIT_GLOBAL_2026-10-21.md`.
Bien que les tests automatisés réussissent (37/37 assertions), l'expérience humaine réelle souffre de plusieurs défauts majeurs identifiés :
- **Aberration spatiale** : L'allée centrale de la salle blanche fait 8 mètres de large (plus large qu'une autoroute), les baies sont clairsemées, et le confinement thermique CAC en verre flotte dans le vide à 2,6m sans toucher les racks.
- **Zone technique masquée** : Le local TGBT/UPS (`Electrical_Room.fbx`) est relégué à X=-26m, enfermé derrière un mur plein étanche de la salle blanche, invisible pour le visiteur.
- **UI étouffante** : 3 grands bandeaux noirs 2D fixes couvrent plus de 45% de l'écran avec du texte brut au lieu d'une interface diégétique 3D.
- **Poteau traversant** : Un montant de busway traverse la Borne 01 et coupe son texte ("01 R|SEAU").
- **Sas démesuré** : Le sas mantrap fait 4m de large (taille d'une rame de métro) et plonge le joueur dans un écran 100% gris uni pendant les 2s de scan.
- **Activités répétitives** : Les 6 étapes réutilisent la même boule bleue glissant sur une ligne au plafond pendant 2,8s vers la Baie 01, sans manipulation physique réelle. Le modèle 3D détaillé du serveur 2U extractible (`Server_Blade_Detailed.fbx`) n'est même pas exploité.

### 2. Ta Mission Immédiate : Implémenter la Phase P0 (Fondations Spatiales et Rendu)
Consulte impérativement `AGENTS.md`, `docs/STATE.md`, `docs/BACKLOG.md` et `docs/AUDIT_GLOBAL_2026-10-21.md`.

Modifications attendues en priorité dans `ConnectionTest/Assets/CloudVR/Editor/DatacenterMonumentalBuilder.cs` :
1. **Resserrer la Salle Blanche** :
   - Réduire la largeur de l'allée centrale de 8,0 m à 2,2 m (rapprocher les colonnes de baies 42U pour former deux rangées denses et réalistes).
   - Ajuster la toiture de confinement CAC pour qu'elle repose hermétiquement sur le sommet des baies avec des cloisons/portes fermant l'allée.
2. **Ouvrir le Local TGBT/UPS** :
   - Créer une baie vitrée d'observation et une porte franchissable donnant un accès direct depuis la salle blanche vers le local électrique attenant.
3. **Nettoyer les Obstructions et l'UI** :
   - Déplacer la descente de busway qui traverse la Borne 01.
   - Désactiver les bandeaux noirs d'overlay 2D fixes du HUD au profit de l'affichage sur les consoles 3D du monde.
4. **Sas Mantrap Réaliste** :
   - Ramener le diamètre du sas à 1,8 m (taille humaine anti-tailgating) et conserver la visibilité intérieure pendant le scan (mire de scan rétinien/badgeuse au lieu d'un écran aveugle).

### 3. Commandes de Compilation et de Test
- Pour compiler le build Windows :
  `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait`
  puis
  `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Complete`
- Pour exécuter la suite de tests et vérifier les captures réelles :
  `powershell -ExecutionPolicy Bypass -File tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120`

Reste concis, rigoureux, préserve `Le cloud.docx`, et documente tes résultats dans `docs/STATE.md` et `docs/BACKLOG.md`.
```
