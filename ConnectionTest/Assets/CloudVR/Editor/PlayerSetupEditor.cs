#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

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
        // Les rayons de visée ne doivent pas toucher la capsule du joueur.
        player.layer = 2; // Ignore Raycast, couche intégrée à Unity.
        Undo.RegisterCreatedObjectUndo(player, "Setup PC Player");

        // Départ derrière la chaise, sans chevauchement avec son collider.
        player.transform.position = new Vector3(0.2f, 0f, 0.3f);
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

        var mode = Object.FindFirstObjectByType<InputModeManager>();
        Undo.RecordObject(mode, "Relier le rig PC");
        mode.pcRig = player;
        mode.pcCrosshair = crosshairGo;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(player.scene);

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
            // Retirer d'abord le composant qui exige PhotoSender et Collider.
            var oldClicker = oldParent.GetComponent<ScreenClickHandler>();
            if (oldClicker != null) Undo.DestroyObjectImmediate(oldClicker);
            var oldSender = oldParent.GetComponent<PhotoSender>();
            if (oldSender != null) Undo.DestroyObjectImmediate(oldSender);
            var oldTransition = oldParent.GetComponent<StepTransition>();
            if (oldTransition != null) Undo.DestroyObjectImmediate(oldTransition);
            foreach (var oldCollider in oldParent.GetComponents<Collider>())
                Undo.DestroyObjectImmediate(oldCollider);
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
            // Surface décorative devant Screen_Face : ne doit pas intercepter le clic.
            var decorativeCollider = display.GetComponent<Collider>();
            if (decorativeCollider != null) Undo.DestroyObjectImmediate(decorativeCollider);
            var rend = display.GetComponent<Renderer>();
            if (rend != null) clicker.hoverRenderer = rend;
        }

        SetupPhotoDisplay(screenFace.transform, sender);

        bool connected = false;
        for (int i = 0; i < sender.onSendCompleted.GetPersistentEventCount(); i++)
            connected |= sender.onSendCompleted.GetPersistentTarget(i) == transition
                && sender.onSendCompleted.GetPersistentMethodName(i) == nameof(StepTransition.TriggerTransition);
        if (!connected)
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sender.onSendCompleted, transition.TriggerTransition);
        EditorUtility.SetDirty(sender);
        EditorUtility.SetDirty(transition);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(screenFace.scene);

        Debug.Log("[PlayerSetup] OK — PhotoSender + ScreenClickHandler sur Screen_Face. Lance Play, approche-toi de l'écran, clic gauche.");
        Selection.activeGameObject = screenFace;
        EditorGUIUtility.PingObject(screenFace);
    }

    private static void SetupPhotoDisplay(Transform screen, PhotoSender sender)
    {
        var old = screen.Find("Photo_Canvas");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        var go = new GameObject("Photo_Canvas", typeof(RectTransform), typeof(Canvas));
        Undo.RegisterCreatedObjectUndo(go, "Configurer affichage photo");
        go.transform.SetParent(screen, false);
        var rect = go.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(0, 0, 1.3f);
        rect.localRotation = Quaternion.Euler(0, 180, 0);
        var scale = screen.lossyScale;
        rect.localScale = new Vector3(0.001f / scale.x, 0.001f / scale.y, 0.001f / scale.z);
        rect.sizeDelta = new Vector2(580, 340);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        sender.photoObject = CreateState(rect, "Photo_Ready");
        var ready = sender.photoObject.transform;
        AddLabel(ready, "Title", "Envoyer une photo", font, 32, Color.white, new Vector2(0, 122), new Vector2(540, 55));
        var preview = new GameObject("PhotoPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        preview.transform.SetParent(ready, false);
        var previewRect = preview.GetComponent<RectTransform>();
        previewRect.sizeDelta = new Vector2(230, 125);
        preview.GetComponent<Image>().color = new Color(0.14f, 0.45f, 0.60f);
        preview.GetComponent<Image>().raycastTarget = false;
        AddLabel(ready, "PreviewCaption", "PHOTO\nEXEMPLE", font, 26, Color.white, Vector2.zero, new Vector2(230, 125));
        AddLabel(ready, "Action", "Viser l'écran puis cliquer", font, 27, Color.white, new Vector2(0, -112), new Vector2(550, 45));

        sender.sendingIndicator = CreateState(rect, "Photo_Sending");
        AddLabel(sender.sendingIndicator.transform, "Status", "Envoi en cours...", font, 36, new Color(1f, 0.85f, 0.3f), Vector2.zero, new Vector2(550, 120));
        sender.confirmationIndicator = CreateState(rect, "Photo_Completed");
        AddLabel(sender.confirmationIndicator.transform, "Status", "Photo envoyée", font, 36, new Color(0.35f, 1f, 0.6f), Vector2.zero, new Vector2(550, 120));
        AddLabel(rect, "SimulationNote", "Envoi illustratif — aucun transfert réseau", font, 18, new Color(0.8f, 0.85f, 0.9f), new Vector2(0, -151), new Vector2(550, 30));
        sender.Reset();
    }

    private static GameObject CreateState(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return go;
    }

    private static void AddLabel(Transform parent, string name, string caption, Font font, int fontSize, Color color, Vector2 position, Vector2 size)
    {
        // Construction avec les composants requis ; aucun TextMesh utilisé.
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        text.text = caption;
    }

}
#endif
