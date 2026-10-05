#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>Parcours réel entre deux scènes ; entrées PC synthétiques, VR injectée sans casque.</summary>
public class JourneySmoke : MonoBehaviour
{
    [Serializable] public class Result
    {
        public bool passed, vr, captures;
        public string utc, unityVersion, platform, graphics, error;
        public Vector3 arrivalPosition, approachPosition, collisionPosition;
        public List<string> checks = new List<string>();
    }
    readonly Result _result = new Result();
    Mouse _mouse, _oldMouse;
    Keyboard _keyboard, _oldKeyboard;
    InputSettings.BackgroundBehavior _background;
    InputSettings.EditorInputBehaviorInPlayMode _editorInput;
    bool _runInBackground, _finished, _configured;
    string _folder;
    int _sceneLoads;

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Launch()
    {
        var args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "--cloudvr-journey-smoke") < 0 || FindFirstObjectByType<JourneySmoke>() != null) return;
        Run();
    }
#endif

    public static string Run(bool vr = false)
    {
        if (!Application.isPlaying || SceneManager.GetActiveScene().name != "Chambre")
            throw new InvalidOperationException("Lancer pendant Play dans Chambre.");
        if (FindFirstObjectByType<JourneySmoke>() != null) throw new InvalidOperationException("Test déjà présent.");
        var probe = new GameObject("JourneySmoke_TEMP").AddComponent<JourneySmoke>();
        DontDestroyOnLoad(probe.gameObject); // Le probe seul survit pour vérifier le remplacement de scène.
        probe._result.vr = vr;
        probe.StartCoroutine(probe.Guarded());
        return "JOURNEY_SMOKE_STARTED";
    }

    IEnumerator Guarded()
    {
        var routine = Exercise();
        while (true)
        {
            object step = null;
            bool moved;
            try { moved = routine.MoveNext(); if (moved) step = routine.Current; }
            catch (Exception e) { Finish(false, e.ToString()); yield break; }
            if (!moved) { Finish(true, null); yield break; }
            yield return step;
        }
    }

    IEnumerator Exercise()
    {
        _folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../tmp/unity"));
#if !UNITY_EDITOR
        var args = Environment.GetCommandLineArgs();
        int output = Array.IndexOf(args, "--cloudvr-smoke-output");
        if (output < 0 || output + 1 >= args.Length || !Path.IsPathRooted(args[output + 1]))
            throw new InvalidOperationException("Dossier de sortie absolu requis.");
        _folder = args[output + 1];
#endif
        Directory.CreateDirectory(_folder);
        _result.platform = Application.platform.ToString();
        _result.graphics = SystemInfo.graphicsDeviceType.ToString();
        _result.captures = !_result.vr && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
        _background = InputSystem.settings.backgroundBehavior;
        _editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
        _runInBackground = Application.runInBackground;
        _oldMouse = Mouse.current;
        _oldKeyboard = Keyboard.current;
        _configured = true;
        Application.runInBackground = true;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        _mouse = InputSystem.AddDevice<Mouse>("JourneyMouse");
        _keyboard = InputSystem.AddDevice<Keyboard>("JourneyKeyboard");
        _mouse.MakeCurrent();
        _keyboard.MakeCurrent();
        SceneManager.sceneLoaded += Loaded;
        yield return new WaitForSecondsRealtime(0.2f);
        var sender = FindFirstObjectByType<PhotoSender>();
        var transition = sender.GetComponent<StepTransition>();
        Check(transition.nextSceneName == "DataCenter" && Application.CanStreamedLevelBeLoaded("DataCenter"), "Destination DataCenter incluse au build");
        if (_result.vr)
        {
            InputModeManager.Instance.SetMode(InputModeManager.InputMode.VR);
            sender.Send(); // Valide la transition et la conservation du mode, pas la sélection manette.
        }
        else
        {
            var controller = FindFirstObjectByType<PCPlayerController>();
            controller.enabled = false;
            Camera.main.transform.LookAt(sender.transform.position);
            Button(true);
        }
        yield return new WaitForSecondsRealtime(0.2f);
        Check(sender.State == PhotoSender.SendState.Sending, "Envoi démarré" );
        Button(false);
        float deadline = Time.realtimeSinceStartup + 15;
        while (SceneManager.GetActiveScene().name != "DataCenter" && Time.realtimeSinceStartup < deadline) yield return null;
        Check(SceneManager.GetActiveScene().name == "DataCenter", "Chargement effectif de DataCenter après envoi");
        yield return new WaitForSecondsRealtime(0.3f);
        Check(_sceneLoads == 1 && SceneManager.sceneCount == 1, "Une seule transition, Chambre déchargée");
        var mode = InputModeManager.Instance;
        Check(mode != null && mode.gameObject.scene.name == "DataCenter", "Nouveau InputModeManager lié à la scène destination");
        Check(FindObjectsByType<InputModeManager>(FindObjectsSortMode.None).Length == 1 && Camera.allCamerasCount == 1, "Un gestionnaire et une caméra active");
        Check(mode.CurrentMode == (_result.vr ? InputModeManager.InputMode.VR : InputModeManager.InputMode.PC), "Mode PC/VR conservé entre scènes");
        var rig = _result.vr ? mode.vrRig : mode.pcRig;
        _result.arrivalPosition = rig.transform.position;
        Check(Vector2.Distance(new Vector2(rig.transform.position.x, rig.transform.position.z), new Vector2(0, 1.5f)) < 0.05f, "Arrivée au point de départ sans déplacement automatique");
        Check(GameObject.Find("Baie_01") != null && GameObject.Find("Repere_Baie") != null, "Première baie et repère présents");
        Capture("datacenter-arrival.png");
        if (_result.vr)
        {
            var area = FindFirstObjectByType<TeleportationArea>();
            var provider = rig.GetComponentInChildren<TeleportationProvider>();
            Check(area != null && area.teleportationProvider == provider && area.interactionManager.gameObject.scene.name == "DataCenter", "Zone et provider XR reliés à DataCenter");
            Check(provider.QueueTeleportRequest(new TeleportRequest { destinationPosition = new Vector3(0, 0, 4.5f), destinationRotation = Quaternion.identity, matchOrientation = MatchOrientation.WorldSpaceUp }), "Requête de téléportation injectée acceptée");
            yield return new WaitForSecondsRealtime(0.6f);
            Check(Mathf.Abs(rig.transform.position.z - 4.5f) < 0.1f, "Rig déplacé par la locomotion XRI, sans preuve matérielle");
        }
        else
        {
            var controller = rig.GetComponent<PCPlayerController>();
            Check(controller.enabled, "Contrôleur PC actif dans la nouvelle scène");
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.W));
            // Le contrôleur intègre Time.deltaTime : attendre le même temps de simulation.
            yield return new WaitForSeconds(1);
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return new WaitForSecondsRealtime(0.1f);
            _result.approachPosition = rig.transform.position;
            Check(rig.transform.position.z > 3 && rig.transform.position.y > -0.1f, "Avancée par clavier synthétique et maintien sur le sol");
            Capture("datacenter-approach.png");
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.W));
            yield return new WaitForSeconds(2);
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return new WaitForSecondsRealtime(0.1f);
            _result.collisionPosition = rig.transform.position;
            Check(rig.transform.position.z > 6 && rig.transform.position.z < 6.85f,
                "Collider de la baie bloque le passage du joueur PC : " + rig.transform.position);
        }
        yield return new WaitForSecondsRealtime(0.4f);
        Check(_sceneLoads == 1, "Aucun second chargement différé");
    }

    void Loaded(Scene scene, LoadSceneMode mode) { if (scene.name == "DataCenter") _sceneLoads++; }
    void Button(bool down)
    {
        _mouse.MakeCurrent();
        InputSystem.QueueStateEvent(_mouse, new MouseState { position = new Vector2(Screen.width / 2f, Screen.height / 2f), buttons = down ? (ushort)1 : (ushort)0 });
    }
    void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        _result.checks.Add(description);
    }
    void Capture(string name)
    {
        if (!_result.captures) return;
        Canvas.ForceUpdateCanvases();
        var camera = Camera.main;
        var target = RenderTexture.GetTemporary(1280, 720, 24);
        var previous = camera.targetTexture;
        var active = RenderTexture.active;
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(_folder, name), image.EncodeToPNG());
        }
        finally { camera.targetTexture = previous; RenderTexture.active = active; RenderTexture.ReleaseTemporary(target); Destroy(image); }
    }
    void Finish(bool passed, string error)
    {
        _finished = true;
        _result.passed = passed;
        _result.error = error;
        _result.utc = DateTime.UtcNow.ToString("o");
        _result.unityVersion = Application.unityVersion;
        Restore();
        if (_folder != null) File.WriteAllText(Path.Combine(_folder, _result.vr ? "journey-vr-result.json" : "journey-smoke-result.json"), JsonUtility.ToJson(_result, true));
        if (passed) Debug.Log("JOURNEY_SMOKE_PASS : " + _result.checks.Count);
        else Debug.LogError("JOURNEY_SMOKE_FAIL : " + error);
#if !UNITY_EDITOR
        Application.Quit(passed ? 0 : 1);
#endif
    }
    void Restore()
    {
        SceneManager.sceneLoaded -= Loaded;
        if (!_configured) return;
        if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
        if (_keyboard != null && _keyboard.added) InputSystem.RemoveDevice(_keyboard);
        if (_oldMouse != null && _oldMouse.added) _oldMouse.MakeCurrent();
        if (_oldKeyboard != null && _oldKeyboard.added) _oldKeyboard.MakeCurrent();
        InputSystem.settings.backgroundBehavior = _background;
        InputSystem.settings.editorInputBehaviorInPlayMode = _editorInput;
        Application.runInBackground = _runInBackground;
        _configured = false;
    }
    void OnDestroy() { if (!_finished && _configured) Finish(false, "Test interrompu"); else Restore(); }
}
#endif
