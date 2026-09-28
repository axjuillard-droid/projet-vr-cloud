#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Ajoute le rig joueur PC dans la scène Chambre :
/// - CharacterController + PCPlayerController (WASD + souris)
/// - Caméra FPS enfant du joueur
/// - InputModeManager (singleton)
/// Menu : CloudVR → Setup → Joueur PC
/// </summary>
public static class PlayerSetupEditor
{
    [MenuItem("CloudVR/Setup/Joueur PC (clavier-souris)")]
    public static void SetupPCPlayer()
    {
        // Supprimer l'ancien rig si présent
        var old = GameObject.Find("Player_PC");
        if (old != null) Undo.DestroyObjectImmediate(old);

        // ── Rig joueur ────────────────────────────────────────────────
        var player = new GameObject("Player_PC");
        Undo.RegisterCreatedObjectUndo(player, "Setup PC Player");

        // Position de départ : devant la chaise, regardant l'écran
        player.transform.position = new Vector3(0f, 0f, -0.6f);
        player.transform.rotation = Quaternion.Euler(0, 180, 0); // face au bureau

        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.25f;
        cc.center = new Vector3(0, 0.9f, 0);

        var pcCtrl = player.AddComponent<PCPlayerController>();
        pcCtrl.moveSpeed = 3f;
        pcCtrl.mouseSensitivity = 2f;

        // ── Caméra FPS ────────────────────────────────────────────────
        var camGo = new GameObject("FPS_Camera");
        camGo.transform.SetParent(player.transform, false);
        camGo.transform.localPosition = new Vector3(0, 1.65f, 0); // hauteur yeux

        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 75f;
        cam.nearClipPlane = 0.05f;
        cam.tag = "MainCamera";

        // Désactiver la caméra par défaut si elle existe
        var defaultCam = Camera.main;
        if (defaultCam != null && defaultCam.gameObject != camGo)
        {
            defaultCam.gameObject.SetActive(false);
            Debug.Log("[PlayerSetup] Main Camera désactivée au profit de FPS_Camera.");
        }

        // Lier la caméra au PCPlayerController
        pcCtrl.cameraTransform = camGo.transform;

        // ── InputModeManager (singleton) ──────────────────────────────
        if (GameObject.FindObjectOfType<InputModeManager>() == null)
        {
            var imm = new GameObject("InputModeManager");
            imm.AddComponent<InputModeManager>();
            Undo.RegisterCreatedObjectUndo(imm, "Create InputModeManager");
            Debug.Log("[PlayerSetup] InputModeManager créé.");
        }

        // ── Crosshair UI ──────────────────────────────────────────────
        var existingCrosshair = GameObject.Find("PC_Crosshair");
        if (existingCrosshair != null) Undo.DestroyObjectImmediate(existingCrosshair);
        var crosshairGo = new GameObject("PC_Crosshair");
        Undo.RegisterCreatedObjectUndo(crosshairGo, "Create Crosshair");
        crosshairGo.AddComponent<PCCrosshair>();

        Debug.Log("[PlayerSetup] Rig PC créé avec crosshair. Appuie sur Play → WASD + souris. Clic gauche sur l'écran pour envoyer la photo.");
        Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(player);
    }

    [MenuItem("CloudVR/Setup/Interaction Écran (PhotoSender)")]
    public static void SetupScreenInteraction()
    {
        // Le collider physique est sur Screen_Face (primitive Unity)
        var screenFace = GameObject.Find("Screen_Face");
        if (screenFace == null)
        {
            Debug.LogError("[PlayerSetup] Screen_Face introuvable. Lance d'abord CloudVR → Build Scene → Chambre.");
            return;
        }

        // Nettoyer l'ancien setup sur le parent s'il existe
        var oldParent = GameObject.Find("PC_Screen_Interactive");
        if (oldParent != null)
        {
            var oldSender = oldParent.GetComponent<PhotoSender>();
            if (oldSender != null) Undo.DestroyObjectImmediate(oldSender);
            var oldTransition = oldParent.GetComponent<StepTransition>();
            if (oldTransition != null) Undo.DestroyObjectImmediate(oldTransition);
            var oldCollider = oldParent.GetComponent<BoxCollider>();
            if (oldCollider != null) Undo.DestroyObjectImmediate(oldCollider);
        }

        // ── PhotoSender sur Screen_Face ───────────────────────────────
        var sender = screenFace.GetComponent<PhotoSender>();
        if (sender == null) sender = Undo.AddComponent<PhotoSender>(screenFace);
        sender.sendDuration = 2.5f;

        // ── StepTransition sur Screen_Face ────────────────────────────
        var transition = screenFace.GetComponent<StepTransition>();
        if (transition == null) transition = Undo.AddComponent<StepTransition>(screenFace);
        transition.delayBeforeTransition = 2f;

        // Le BoxCollider est déjà là (primitive Unity l'ajoute automatiquement)
        // On s'assure juste qu'il existe
        if (screenFace.GetComponent<BoxCollider>() == null)
            Undo.AddComponent<BoxCollider>(screenFace);

        // ── ScreenClickHandler sur Screen_Face ────────────────────────
        var clicker = screenFace.GetComponent<ScreenClickHandler>();
        if (clicker == null) clicker = Undo.AddComponent<ScreenClickHandler>(screenFace);
        clicker.maxRayDistance = 5f;

        // Brancher le Screen_Display comme feedback visuel au survol
        var display = GameObject.Find("Screen_Display");
        if (display != null)
        {
            var rend = display.GetComponent<Renderer>();
            if (rend != null) clicker.hoverRenderer = rend;
        }

        Debug.Log("[PlayerSetup] OK — PhotoSender + ScreenClickHandler sur Screen_Face. Lance Play, approche-toi de l'écran, clic gauche.");
        Selection.activeGameObject = screenFace;
        EditorGUIUtility.PingObject(screenFace);
    }
}
#endif
