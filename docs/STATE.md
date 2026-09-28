# État de reprise

Mis à jour le 28 septembre 2026.

## Situation actuelle
Projet dans ConnectionTest/, Unity 6000.0.58f2, URP 17.0.4, Input System 1.14.2, XRI 3.0.11, OpenXR 1.14.0. Scène active : `Assets/Scenes/Chambre.unity`. Serveur MCP for Unity 10.0.0 actif en session HTTP locale.

## Vérifications réussies
- uv et uvx 0.12.19 installés et vérifiés.
- Blender 4.5.2 LTS : lancement en arrière-plan et lecture Python réussis.
- Unity MCP HTTP actif sur port 8080, instance ConnectionTest reconnue.
- T05 terminé : dépôt Git initialisé avec LFS, remote GitHub configuré (`axjuillard-droid/projet-vr-cloud`), projet ConnectionTest nettoyé (TutorialInfo/Readme supprimés, SampleScene renommée Chambre).
- T06 (avancée du 28/09) :
  - Packages XR (XRI 3.0.11, OpenXR 1.14.0, XR Hands 1.5.0) importés et compilés sans erreur.
  - Scène Chambre construite avec des primitives URP via `ChambreSceneBuilder` (murs, sol, plafond, bureau, chaise, tour PC, écran, fenêtre, lumière).
  - Scripts `InputModeManager`, `PCPlayerController`, `PhotoSender`, `StepTransition`, `ScreenClickHandler`, `PCCrosshair` créés et fonctionnels.
  - Setup éditeur (`PlayerSetupEditor`) créé avec entrées de menu `CloudVR`.
  - Contrôle PC (WASD + souris + caméra FPS) et détection de survol de l'écran testés et confirmés fonctionnels dans l'éditeur Unity (log console : `[ScreenClickHandler] Survol détecté — clic gauche pour envoyer la photo.`).

## Prochaine action
T06 en cours :
- Tester le clic gauche pour déclencher l'envoi (`PhotoSender.Send()`) et le changement d'état (envoi / confirmation).
- Implémenter le rig VR (XR Origin avec téléportation pour Meta Quest) en complément du mode PC.
- Préparer un premier test de compilation / build StandaloneWindows64 et Android (Quest).

## Limites
Pas de build PC/Quest validé ni d'essai casque. Modèles des casques à confirmer. Aucun export Blender/import Unity testé. RAG réel et web conditionnels.

Remplacer les statuts périmés à chaque mise à jour plutôt qu'accumuler des résultats contradictoires.
