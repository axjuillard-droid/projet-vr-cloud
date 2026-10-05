#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

/// <summary>
/// Test logique dans l'éditeur sans matériel : sélection XR injectée et requête de téléportation.
/// Ne valide ni les bindings des manettes, ni le tracking, ni le confort en casque.
/// </summary>
public class VRInteractionSmoke : MonoBehaviour
{
    [Serializable] public class Result
    {
        public bool passed;
        public string utc, unityVersion, error;
        public string scope = "Editeur sans casque, sélection et requête injectées par API XRI";
        public List<string> checks = new List<string>();
    }
    readonly Result _result = new Result();
    InputModeManager _mode;
    InputModeManager.InputMode _previousMode;
    PhotoSender _sender;
    XROrigin _origin;
    Vector3 _position;
    Quaternion _rotation;
    bool _background, _configured, _finished;
    XRRayInteractor _probeRay;
    string _folder;

    public static string Run()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Lancer pendant Play.");
        if (FindFirstObjectByType<VRInteractionSmoke>() != null) throw new InvalidOperationException("Test déjà présent.");
        var probe = new GameObject("VRInteractionSmoke_TEMP").AddComponent<VRInteractionSmoke>();
        probe.StartCoroutine(probe.Guarded());
        return "VR_SMOKE_STARTED";
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
        Directory.CreateDirectory(_folder);
        _mode = InputModeManager.Instance;
        _previousMode = _mode.CurrentMode;
        _origin = _mode.vrRig.GetComponent<XROrigin>();
        _position = _origin.transform.position;
        _rotation = _origin.transform.rotation;
        _sender = GameObject.Find("Screen_Face").GetComponent<PhotoSender>();
        _background = Application.runInBackground;
        _configured = true;
        Application.runInBackground = true;
        _sender.Reset();
        Check(_mode.IsPC && _mode.pcRig.activeInHierarchy && !_mode.vrRig.activeInHierarchy, "Auto sans casque : rig PC actif, rig VR inactif");
        _mode.SetMode(InputModeManager.InputMode.VR);
        yield return new WaitForSeconds(0.2f);
        Check(_mode.IsVR && !_mode.pcRig.activeInHierarchy && !_mode.pcCrosshair.activeInHierarchy,
            "Bascule VR : rig PC et viseur désactivés");
        Check(Camera.main == _origin.Camera && FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.enabled) == 1,
            "Une seule caméra active : caméra XR");
        var providers = _origin.GetComponentsInChildren<LocomotionProvider>(true);
        Check(providers.Where(p => p.enabled).All(p => p is TeleportationProvider || p is SnapTurnProvider)
            && providers.OfType<TeleportationProvider>().Single().enabled && providers.OfType<SnapTurnProvider>().Single().enabled,
            "Téléportation et rotation par crans seules locomotions activées");
        var area = FindFirstObjectByType<TeleportationArea>();
        var provider = providers.OfType<TeleportationProvider>().Single();
        var manager = FindFirstObjectByType<XRInteractionManager>();
        Check(area.teleportationProvider == provider && area.interactionLayers.value == unchecked((int)0x80000000)
            && area.colliders.Count == 1, "Zone de téléportation raccordée au provider et à la couche Teleport");
        var rays = _origin.GetComponentsInChildren<XRRayInteractor>(true).Where(r => r.name.Contains("Teleport")).ToArray();
        Check(rays.Length == 2 && rays.All(r => r.interactionLayers.value == area.interactionLayers.value),
            "Deux rayons de téléportation du prefab sur la couche Teleport");

        // Interactor temporaire enregistré, sans périphérique : événement XRI réel, sélection injectée.
        var rayObject = new GameObject("XR_TestRay_TEMP");
        rayObject.transform.SetParent(transform);
        _probeRay = rayObject.AddComponent<XRRayInteractor>();
        _probeRay.interactionManager = manager;
        _probeRay.interactionLayers = 1;
        yield return null;
        var screen = _sender.GetComponent<XRSimpleInteractable>();
        Check(manager.IsRegistered((IXRSelectInteractable)screen) && screen.colliders.Contains(_sender.GetComponent<Collider>()),
            "Écran enregistré dans XR Interaction Manager avec son collider");
        manager.SelectEnter((IXRSelectInteractor)_probeRay, (IXRSelectInteractable)screen);
        Check(_sender.State == PhotoSender.SendState.Sending && _sender.sendingIndicator.activeInHierarchy,
            "Sélection XR → PhotoSender → indicateur envoi");
        if (_probeRay.interactablesSelected.Contains(screen))
            manager.SelectExit((IXRSelectInteractor)_probeRay, (IXRSelectInteractable)screen);
        yield return new WaitForSeconds(_sender.sendDuration + 0.3f);
        Check(_sender.State == PhotoSender.SendState.Completed && _sender.confirmationIndicator.activeInHierarchy
            && _sender.GetComponent<StepTransition>().IsTriggered, "Envoi XR terminé : confirmation et transition reçues");

        var destination = new Vector3(1.4f, area.colliders[0].bounds.max.y, 0.3f);
        Check(provider.QueueTeleportRequest(new TeleportRequest { destinationPosition = destination,
            destinationRotation = Quaternion.identity, matchOrientation = MatchOrientation.WorldSpaceUp }), "Requête acceptée par TeleportationProvider");
        yield return new WaitForSeconds(0.3f);
        var camera = _origin.Camera.transform.position;
        Check(Vector2.Distance(new Vector2(camera.x, camera.z), new Vector2(destination.x, destination.z)) < 0.03f,
            "Téléportation appliquée par la locomotion : caméra au-dessus de la destination");
        _mode.SetMode(InputModeManager.InputMode.PC);
        yield return null;
        Check(_mode.pcRig.activeInHierarchy && _mode.pcCrosshair.activeInHierarchy && !_mode.vrRig.activeInHierarchy
            && Camera.main.name == "FPS_Camera" && FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.enabled) == 1,
            "Retour PC : caméra FPS et viseur actifs, rig VR inactif");
    }
    void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
        _result.checks.Add(label);
    }
    void Finish(bool passed, string error)
    {
        _finished = true;
        _result.passed = passed;
        _result.error = error;
        _result.utc = DateTime.UtcNow.ToString("o");
        _result.unityVersion = Application.unityVersion;
        Restore();
        if (_folder != null) File.WriteAllText(Path.Combine(_folder, "vr-smoke-result.json"), JsonUtility.ToJson(_result, true));
        if (passed) Debug.Log("VR_SMOKE_PASS : " + _result.checks.Count + " vérifications");
        else Debug.LogError("VR_SMOKE_FAIL : " + error);
    }
    void Restore()
    {
        if (!_configured) return;
        if (_probeRay != null) Destroy(_probeRay.gameObject);
        if (_sender != null) _sender.Reset();
        if (_origin != null) _origin.transform.SetPositionAndRotation(_position, _rotation);
        if (_mode != null) _mode.SetMode(_previousMode);
        Application.runInBackground = _background;
        _configured = false;
    }
    void OnDestroy()
    {
        if (!_finished && _configured) Finish(false, "Test interrompu.");
        else Restore();
    }
}
#endif
