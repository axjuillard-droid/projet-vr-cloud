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

        Debug.Log("[PlayerSetup] Rig PC créé. Appuie sur Play pour tester le déplacement WASD + souris.");
        Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(player);
    }

    [MenuItem("CloudVR/Setup/Interaction Écran (PhotoSender)")]
    public static void SetupScreenInteraction()
    {
        // Trouver l'écran créé par ChambreSceneBuilder
        var screen = GameObject.Find("PC_Screen_Interactive");
        if (screen == null)
        {
            Debug.LogError("[PlayerSetup] PC_Screen_Interactive introuvable. Lance d'abord CloudVR → Build Scene → Chambre.");
            return;
        }

        // Ajouter PhotoSender s'il n'est pas déjà là
        var sender = screen.GetComponent<PhotoSender>();
        if (sender == null)
            sender = Undo.AddComponent<PhotoSender>(screen);

        sender.sendDuration = 2.5f;

        // Ajouter StepTransition
        var transition = screen.GetComponent<StepTransition>();
        if (transition == null)
            transition = Undo.AddComponent<StepTransition>(screen);

        transition.delayBeforeTransition = 2f;
        // nextSceneName vide pour l'instant (étape 2 pas encore créée)

        // Ajouter un BoxCollider pour le raycast PC (clic)
        var col = screen.GetComponent<BoxCollider>();
        if (col == null)
            col = Undo.AddComponent<BoxCollider>(screen);

        // Ajouter le ScreenClickHandler pour le clic PC
        var clicker = screen.GetComponent<ScreenClickHandler>();
        if (clicker == null)
            Undo.AddComponent<ScreenClickHandler>(screen);

        Debug.Log("[PlayerSetup] PhotoSender + StepTransition + ScreenClickHandler attachés à PC_Screen_Interactive.");
        EditorGUIUtility.SetIconForObject(screen, null);
        Selection.activeGameObject = screen;
        EditorGUIUtility.PingObject(screen);
    }
}
#endif
