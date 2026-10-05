#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>Setup reproductible de Chambre avec le prefab XRI déjà importé.</summary>
public static class VRSetupEditor
{
    public const int TeleportMask = unchecked((int)0x80000000); // couche XRI Teleport, index 31
    const string PrefabPath = "Assets/Samples/XR Interaction Toolkit/3.0.11/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

    [MenuItem("CloudVR/Setup/Rig VR et téléportation")]
    public static void Setup()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Arrêter Play avant le setup.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var screen = GameObject.Find("Screen_Face");
        var mode = UnityEngine.Object.FindFirstObjectByType<InputModeManager>();
        if (prefab == null || screen == null || mode == null || screen.GetComponent<PhotoSender>() == null)
            throw new InvalidOperationException("Préparer joueur PC, photo et Starter Assets XRI 3.0.11.");
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Setup Chambre VR");
        DestroyRoot("Player_VR");
        DestroyRoot("VR_Teleportation");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rig.name = "Player_VR";
        Undo.RegisterCreatedObjectUndo(rig, "Créer rig VR");
        rig.SetActive(false);
        rig.transform.SetPositionAndRotation(new Vector3(0.2f, 0f, 0.3f), Quaternion.Euler(0, 180, 0));
        var origin = rig.GetComponent<XROrigin>();
        origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        origin.CameraYOffset = 1.65f; // fallback Device si Floor indisponible
        origin.Camera.nearClipPlane = 0.05f;
        rig.layer = 2; // capsule exclue des raycasts par défaut
        foreach (var controller in rig.GetComponentsInChildren<ControllerInputActionManager>(true))
        {
            controller.smoothMotionEnabled = false;
            controller.smoothTurnEnabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        }
        foreach (var provider in rig.GetComponentsInChildren<LocomotionProvider>(true))
        {
            provider.enabled = provider is TeleportationProvider || provider is SnapTurnProvider;
            PrefabUtility.RecordPrefabInstancePropertyModifications(provider);
        }
        var teleport = rig.GetComponentInChildren<TeleportationProvider>(true);
        teleport.delayTime = 0f;
        var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
        if (manager == null)
        {
            var managerObject = new GameObject("XR Interaction Manager");
            Undo.RegisterCreatedObjectUndo(managerObject, "Créer XR Interaction Manager");
            manager = managerObject.AddComponent<XRInteractionManager>();
        }
        foreach (var interactor in rig.GetComponentsInChildren<XRBaseInteractor>(true))
        {
            interactor.interactionManager = manager;
            bool isTeleport = interactor is XRRayInteractor && interactor.name.Contains("Teleport");
            interactor.interactionLayers = isTeleport ? TeleportMask : 1;
            PrefabUtility.RecordPrefabInstancePropertyModifications(interactor);
        }
        var selectable = screen.GetComponent<XRSimpleInteractable>();
        if (selectable == null) selectable = Undo.AddComponent<XRSimpleInteractable>(screen);
        Undo.RecordObject(selectable, "Raccorder sélection écran VR");
        selectable.interactionManager = manager;
        selectable.interactionLayers = 1;
        selectable.colliders.Clear();
        selectable.colliders.Add(screen.GetComponent<Collider>());
        var sender = screen.GetComponent<PhotoSender>();
        bool connected = Enumerable.Range(0, selectable.selectEntered.GetPersistentEventCount()).Any(i =>
            selectable.selectEntered.GetPersistentTarget(i) == sender && selectable.selectEntered.GetPersistentMethodName(i) == nameof(PhotoSender.Send));
        if (!connected) UnityEventTools.AddVoidPersistentListener(selectable.selectEntered, sender.Send);

        var zoneRoot = new GameObject("VR_Teleportation");
        Undo.RegisterCreatedObjectUndo(zoneRoot, "Créer zone de téléportation");
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = "Teleport_Zone_Degagee";
        zone.transform.SetParent(zoneRoot.transform);
        // Zone à droite du mobilier : x=[0.9,1.9], z=[-0.7,1.3].
        zone.transform.position = new Vector3(1.4f, 0.003f, 0.3f);
        zone.transform.localScale = new Vector3(1f, 0.006f, 2f);
        zone.GetComponent<Renderer>().sharedMaterial = GetZoneMaterial();
        var area = zone.AddComponent<TeleportationArea>();
        area.interactionManager = manager;
        area.interactionLayers = TeleportMask;
        area.teleportationProvider = teleport;
        area.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnSelectExited;
        area.matchOrientation = MatchOrientation.WorldSpaceUp;
        area.filterSelectionByHitNormal = true;
        area.colliders.Clear();
        area.colliders.Add(zone.GetComponent<Collider>());

        Undo.RecordObject(mode, "Relier les deux rigs");
        mode.pcRig = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .First(t => t.parent == null && t.name == "Player_PC").gameObject;
        mode.pcCrosshair = UnityEngine.Object.FindObjectsByType<PCCrosshair>(FindObjectsInactive.Include, FindObjectsSortMode.None).First().gameObject;
        mode.vrRig = rig;
        mode.startupMode = InputModeManager.StartupMode.Auto;
        mode.pcRig.SetActive(true);
        mode.pcCrosshair.SetActive(true);
        PrefabUtility.RecordPrefabInstancePropertyModifications(origin);
        PrefabUtility.RecordPrefabInstancePropertyModifications(rig.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(rig);
        EditorSceneManager.MarkSceneDirty(screen.scene);
        Undo.CollapseUndoOperations(group);
        Debug.Log("[VRSetup] Rig XRI, écran sélectionnable et zone Teleport prêts. Validation casque requise.");
    }

    static void DestroyRoot(string name)
    {
        var root = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(t => t.parent == null && t.name == name);
        if (root != null) Undo.DestroyObjectImmediate(root.gameObject);
    }

    static Material GetZoneMaterial()
    {
        const string path = "Assets/CloudVR/Materials/Teleport_Zone.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        if (!AssetDatabase.IsValidFolder("Assets/CloudVR/Materials")) AssetDatabase.CreateFolder("Assets/CloudVR", "Materials");
        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.12f, 0.38f, 0.42f);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
}
#endif
