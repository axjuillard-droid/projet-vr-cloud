#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>Prototype original reproductible ; aucun plan de site réel représenté.</summary>
public static class DataCenterSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/DataCenter.unity";
    public const string ChambrePath = "Assets/Scenes/Chambre.unity";

    [MenuItem("CloudVR/Build Scene/Data center et transition photo")]
    public static void Build()
    {
        if (Application.isPlaying || EditorApplication.isCompiling)
            throw new InvalidOperationException("Arrêter Play et attendre la compilation.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Sauvegarder les scènes avant de régénérer.");
        var source = EditorSceneManager.OpenScene(ChambrePath, OpenSceneMode.Single);
        var sourceMode = source.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<InputModeManager>(true)).Single();
        if (sourceMode.pcRig == null || sourceMode.vrRig == null || sourceMode.pcCrosshair == null)
            throw new InvalidOperationException("Rigs PC/VR de Chambre requis.");
        var destination = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(destination);
        try
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.46f, 0.49f, 0.54f);
            RenderSettings.fog = false;
            var pc = UnityEngine.Object.Instantiate(sourceMode.pcRig);
            pc.name = "Player_PC";
            pc.transform.SetPositionAndRotation(new Vector3(0, 0, 1.5f), Quaternion.identity);
            var vr = UnityEngine.Object.Instantiate(sourceMode.vrRig);
            vr.name = "Player_VR";
            vr.transform.SetPositionAndRotation(new Vector3(0, 0, 1.5f), Quaternion.identity);
            vr.SetActive(false);
            var crosshair = UnityEngine.Object.Instantiate(sourceMode.pcCrosshair);
            crosshair.name = "PC_Crosshair";
            var mode = new GameObject("InputModeManager").AddComponent<InputModeManager>();
            mode.pcRig = pc;
            mode.vrRig = vr;
            mode.pcCrosshair = crosshair;
            mode.startupMode = InputModeManager.StartupMode.Auto;
            var xr = new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            foreach (var interactor in vr.GetComponentsInChildren<XRBaseInteractor>(true))
                interactor.interactionManager = xr;

            var root = new GameObject("DataCenter_Root").transform;
            var floor = MakeMaterial("DC_Floor", new Color(0.18f, 0.22f, 0.28f));
            var wall = MakeMaterial("DC_Wall", new Color(0.56f, 0.62f, 0.68f));
            var dark = MakeMaterial("DC_Dark", new Color(0.055f, 0.075f, 0.11f));
            var cyan = MakeMaterial("DC_Cyan", new Color(0.10f, 0.65f, 0.72f));
            Box("Sol", root, new Vector3(0, -0.12f, 6), new Vector3(10, 0.24f, 12), floor);
            Box("Mur_Gauche", root, new Vector3(-5.1f, 1.6f, 6), new Vector3(0.2f, 3.2f, 12), wall);
            Box("Mur_Droit", root, new Vector3(5.1f, 1.6f, 6), new Vector3(0.2f, 3.2f, 12), wall);
            Box("Mur_Fond", root, new Vector3(0, 1.6f, 12.1f), new Vector3(10, 3.2f, 0.2f), wall);
            Box("Mur_Entree", root, new Vector3(0, 1.6f, -0.1f), new Vector3(10, 3.2f, 0.2f), wall);
            Box("Plafond", root, new Vector3(0, 3.3f, 6), new Vector3(10, 0.2f, 12), wall);
            Box("Chemin_Visiteur", root, new Vector3(0, 0.005f, 4), new Vector3(0.08f, 0.008f, 4), cyan, false);
            var prefab = DataCenterArtBuilder.BuildRack();
            for (int i = -1; i <= 1; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, destination);
                instance.name = i == 0 ? "Baie_01" : "Baie_Secondaire_" + i;
                instance.transform.SetParent(root);
                instance.transform.position = new Vector3(i * 2.25f, 0, i == 0 ? 7.5f : 9f);
            }
            DataCenterArtBuilder.Decorate(root);
            Sign(root, "Accueil", new Vector3(-2.5f, 2.52f, 11.85f), new Vector2(3.8f, 0.65f),
                "LE CLOUD A UNE ADRESSE PHYSIQUE", "Bienvenue dans le data center", dark);
            Sign(root, "Repere_Baie", new Vector3(0, 2.50f, 7.0f), new Vector2(1.20f, 0.50f),
                "BAIE 01", "Équipements physiques", dark);
            Sign(root, "Guide_Visite", new Vector3(-2.1f, 1.65f, 4.2f), new Vector2(1.5f, 1.0f),
                "VOTRE PHOTO, UN FIL CONDUCTEUR", "Approchez-vous de la baie 01.\n\nCette visite est une représentation\npédagogique du service photo.", dark);
            var sun = new GameObject("Eclairage_Principal").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(55, -25, 0);
            RenderSettings.sun = sun;
            var fill = new GameObject("Eclairage_Allee").AddComponent<Light>();
            fill.type = LightType.Point;
            fill.transform.position = new Vector3(0, 2.7f, 4.8f);
            fill.range = 9;
            fill.intensity = 2.1f;
            fill.shadows = LightShadows.None;
            var fillSide = new GameObject("Eclairage_Lateral").AddComponent<Light>();
            fillSide.type = LightType.Point;
            fillSide.transform.position = new Vector3(-3, 2.7f, 8.2f);
            fillSide.range = 8;
            fillSide.intensity = 1.6f;
            fillSide.shadows = LightShadows.None;
            var zone = Box("Zone_Teleportation", root, new Vector3(0, 0.012f, 3.8f), new Vector3(3.2f, 0.018f, 3.5f), floor);
            var area = zone.AddComponent<TeleportationArea>();
            area.interactionManager = xr;
            area.interactionLayers = VRSetupEditor.TeleportMask;
            area.teleportationProvider = vr.GetComponentInChildren<TeleportationProvider>(true);
            area.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnSelectExited;
            area.matchOrientation = MatchOrientation.WorldSpaceUp;
            area.filterSelectionByHitNormal = true;
            area.colliders.Clear();
            area.colliders.Add(zone.GetComponent<Collider>());
            if (!EditorSceneManager.SaveScene(destination, ScenePath)) throw new InvalidOperationException("Sauvegarde DataCenter échouée.");
            EditorSceneManager.CloseScene(destination, true);
            SceneManager.SetActiveScene(source);
            var sender = source.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<PhotoSender>(true)).Single();
            var transition = sender.GetComponent<StepTransition>();
            transition.nextSceneName = "DataCenter";
            transition.delayBeforeTransition = 2;
            EditorUtility.SetDirty(transition);
            EditorSceneManager.MarkSceneDirty(source);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ChambrePath, true), new EditorBuildSettingsScene(ScenePath, true) };
            EditorSceneManager.SaveScene(source);
            AssetDatabase.SaveAssets();
            Debug.Log("[DataCenter] Deux scènes enregistrées ; transition photo configurée.");
        }
        catch
        {
            if (destination.IsValid() && destination.isLoaded) EditorSceneManager.CloseScene(destination, true);
            SceneManager.SetActiveScene(source);
            throw;
        }
    }

    static Material MakeMaterial(string name, Color color, bool emissive = false)
    {
        string path = "Assets/CloudVR/Materials/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new InvalidOperationException("Shader URP Lit absent.");
        material = new Material(shader) { name = name, color = color };
        material.SetFloat("_Smoothness", 0.12f);
        material.SetFloat("_Metallic", 0f);
        if (emissive) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * 0.4f); }
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collision = true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static void Sign(Transform parent, string name, Vector3 position, Vector2 size, string title, string body, Material material)
    {
        Box(name + "_Support", parent, position + new Vector3(0, 0, 0.035f), new Vector3(size.x + 0.08f, size.y + 0.08f, 0.05f), material, false);
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = Vector3.one * 0.002f;
        go.GetComponent<RectTransform>().sizeDelta = size / 0.002f;
        go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        Label(go.transform, "Titre", title, 58, new Color(0.35f, 0.9f, 0.95f), new Vector2(0, size.y * 0.28f / 0.002f), new Vector2(size.x, size.y * 0.32f) / 0.002f);
        Label(go.transform, "Texte", body, 38, Color.white, new Vector2(0, -size.y * 0.1f / 0.002f), new Vector2(size.x * 0.95f, size.y * 0.6f) / 0.002f);
    }

    static void Label(Transform parent, string name, string caption, int fontSize, Color color, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        text.text = caption;
    }
}
#endif
