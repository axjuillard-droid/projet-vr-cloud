#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Test du chemin souris synthétique → raycast → envoi → retour visible.
/// Lancer Run() par MCP pendant Play dans Chambre. Aucune méthode Send appelée directement.
/// Objet temporaire ; disponible aussi dans le player Windows de développement sur argument explicite.
/// Exclu des builds Windows finaux et Android. Sans graphics : états vérifiés, aucune capture.
/// </summary>
public class PhotoInteractionSmoke : MonoBehaviour
{
    [Serializable] public class Result
    {
        public bool passed;
        public string unityVersion;
        public string utc;
        public string error;
        public string platform;
        public string graphics;
        public bool captures;
        public List<string> checks = new List<string>();
    }

    private readonly Result _result = new Result();
    private PhotoSender _sender;
    private StepTransition _transition;
    private PCPlayerController _controller;
    private bool _controllerEnabled;
    private Camera _camera;
    private Vector3 _cameraPosition;
    private Quaternion _cameraRotation;
    private Mouse _mouse, _previousMouse;
    private InputSettings.BackgroundBehavior _background;
    private InputSettings.EditorInputBehaviorInPlayMode _editorInput;
    private bool _runInBackground;
    private CursorLockMode _cursorLock;
    private bool _cursorVisible, _finished, _configured;
    private int _started, _completed;
    private string _folder;

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void LaunchPlayerSmoke()
    {
        var args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "--cloudvr-pc-smoke") < 0) return;
        int output = Array.IndexOf(args, "--cloudvr-smoke-output");
        if (output < 0 || output + 1 >= args.Length || !Path.IsPathRooted(args[output + 1]))
        {
            Debug.LogError("Test PC : fournir --cloudvr-smoke-output avec un dossier absolu.");
            Application.Quit(2);
            return;
        }
        Run();
    }
#endif

    public static string Run()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Lancer pendant Play.");
        if (FindFirstObjectByType<PhotoInteractionSmoke>() != null)
            throw new InvalidOperationException("Un test est déjà présent ; arrêter Play avant de relancer.");
        var probe = new GameObject("PhotoInteractionSmoke_TEMP").AddComponent<PhotoInteractionSmoke>();
        probe.StartCoroutine(probe.Guarded());
        return "PHOTO_SMOKE_STARTED";
    }

    private IEnumerator Guarded()
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

    private IEnumerator Exercise()
    {
        _folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../tmp/unity"));
#if !UNITY_EDITOR
        var args = Environment.GetCommandLineArgs();
        _folder = Path.GetFullPath(args[Array.IndexOf(args, "--cloudvr-smoke-output") + 1]);
#endif
        Directory.CreateDirectory(_folder);
        _result.platform = Application.platform.ToString();
        _result.graphics = SystemInfo.graphicsDeviceType.ToString();
        _result.captures = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
        _sender = GameObject.Find("Screen_Face").GetComponent<PhotoSender>();
        _transition = _sender.GetComponent<StepTransition>();
        _camera = Camera.main;
        _controller = FindFirstObjectByType<PCPlayerController>();
        _cameraPosition = _camera.transform.position;
        _cameraRotation = _camera.transform.rotation;
        _controllerEnabled = _controller.enabled;
        _controller.enabled = false;
        _previousMouse = Mouse.current;
        _background = InputSystem.settings.backgroundBehavior;
        _editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
        _runInBackground = Application.runInBackground;
        _cursorLock = Cursor.lockState;
        _cursorVisible = Cursor.visible;
        _configured = true;
        Application.runInBackground = true;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        _mouse = InputSystem.AddDevice<Mouse>("CloudVR_TestMouse");
        _mouse.MakeCurrent();
        _sender.onSendStarted.AddListener(OnStarted);
        _sender.onSendCompleted.AddListener(OnCompleted);
        yield return new WaitForSeconds(0.2f);
        _sender.Reset();
        Check(_sender.State == PhotoSender.SendState.Ready && _sender.photoObject.activeInHierarchy
            && !_sender.sendingIndicator.activeInHierarchy && !_sender.confirmationIndicator.activeInHierarchy,
            "État initial Ready : photo visible, indicateurs masqués");

        _camera.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        QueueButton(true);
        yield return new WaitForSeconds(0.15f);
        Check(_sender.State == PhotoSender.SendState.Ready && _started == 0, "Clic hors écran ignoré");
        QueueButton(false);
        yield return new WaitForSeconds(0.15f);
        _camera.transform.LookAt(_sender.transform.position);
        Capture("photo-ready.png");
        float began = Time.realtimeSinceStartup;
        QueueButton(true);
        yield return new WaitForSeconds(0.15f);
        Check(_sender.State == PhotoSender.SendState.Sending && _started == 1
            && !_sender.photoObject.activeInHierarchy && _sender.sendingIndicator.activeInHierarchy
            && !_sender.confirmationIndicator.activeInHierarchy && !_transition.IsTriggered,
            "Clic via Input System : Sending, une entrée, indicateur d'envoi visible");
        Capture("photo-sending.png");
        QueueButton(false);
        yield return new WaitForSeconds(0.15f);
        QueueButton(true);
        yield return new WaitForSeconds(0.15f);
        Check(_started == 1 && _completed == 0, "Second clic pendant l'envoi ignoré");
        QueueButton(false);
        yield return new WaitForSeconds(_sender.sendDuration);
        Check(_sender.State == PhotoSender.SendState.Completed && _completed == 1
            && !_sender.photoObject.activeInHierarchy && !_sender.sendingIndicator.activeInHierarchy
            && _sender.confirmationIndicator.activeInHierarchy && _transition.IsTriggered,
            "Completed : confirmation visible, fin unique et transition déclenchée");
        Check(Time.realtimeSinceStartup - began >= _sender.sendDuration, "Confirmation après le délai illustratif configuré");
        Capture("photo-completed.png");

        _sender.Reset();
        Check(_sender.State == PhotoSender.SendState.Ready && _sender.photoObject.activeInHierarchy
            && !_transition.IsTriggered, "Reset rétablit Ready et réarme la transition");
        QueueButton(true);
        yield return new WaitForSeconds(0.15f);
        Check(_sender.State == PhotoSender.SendState.Sending && _started == 2, "Nouvel envoi après reset");
        QueueButton(false);
        _sender.Reset();
        yield return new WaitForSeconds(_sender.sendDuration + 0.2f);
        Check(_sender.State == PhotoSender.SendState.Ready && _completed == 1
            && !_sender.confirmationIndicator.activeInHierarchy && !_transition.IsTriggered,
            "Reset pendant l'envoi annule la confirmation différée");
    }

    private void QueueButton(bool down)
    {
        _mouse.MakeCurrent();
        Cursor.lockState = CursorLockMode.Locked;
        InputSystem.QueueStateEvent(_mouse, new MouseState
        {
            position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),
            buttons = (ushort)(down ? 1 : 0)
        });
    }

    private void OnStarted() { _started++; }
    private void OnCompleted() { _completed++; }
    private void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        _result.checks.Add(description);
    }

    private void Capture(string filename)
    {
        if (!_result.captures) return;
        Canvas.ForceUpdateCanvases();
        var target = RenderTexture.GetTemporary(960, 540, 24);
        var previousTarget = _camera.targetTexture;
        var previousActive = RenderTexture.active;
        var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
        try
        {
            _camera.targetTexture = target;
            _camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(_folder, filename), image.EncodeToPNG());
        }
        finally
        {
            _camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);
            Destroy(image);
        }
    }

    private void Finish(bool passed, string error)
    {
        _finished = true;
        _result.passed = passed;
        _result.error = error;
        _result.utc = DateTime.UtcNow.ToString("o");
        _result.unityVersion = Application.unityVersion;
        Restore();
        if (_folder != null) File.WriteAllText(Path.Combine(_folder, "photo-smoke-result.json"), JsonUtility.ToJson(_result, true));
        if (passed) Debug.Log("PHOTO_SMOKE_PASS : " + _result.checks.Count + " vérifications");
        else Debug.LogError("PHOTO_SMOKE_FAIL : " + error);
#if !UNITY_EDITOR
        Application.Quit(passed ? 0 : 1);
#endif
    }

    private void Restore()
    {
        if (!_configured) return;
        if (_sender != null)
        {
            _sender.onSendStarted.RemoveListener(OnStarted);
            _sender.onSendCompleted.RemoveListener(OnCompleted);
            _sender.Reset();
        }
        if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
        if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent();
        InputSystem.settings.backgroundBehavior = _background;
        InputSystem.settings.editorInputBehaviorInPlayMode = _editorInput;
        Application.runInBackground = _runInBackground;
        Cursor.lockState = _cursorLock;
        Cursor.visible = _cursorVisible;
        if (_camera != null) _camera.transform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        if (_controller != null) _controller.enabled = _controllerEnabled;
        _configured = false;
    }

    private void OnDestroy()
    {
        if (!_finished && _configured) Finish(false, "Test interrompu avant la fin.");
        else Restore();
    }
}
#endif
