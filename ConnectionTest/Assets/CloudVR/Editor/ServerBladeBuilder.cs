#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Intègre le serveur rack 2U extractible et la porte ouvrable sur Baie_01 dans DataCenter.unity.
/// </summary>
public static class ServerBladeBuilder
{
    private const string MaterialsPath = "Assets/CloudVR/Materials/";

    private static Material GetOrCreateMaterial(string name, Color color, float metallic = 0.0f, float smoothness = 0.4f, float emission = 0f, Color? emissionColor = null)
    {
        string path = MaterialsPath + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        if (emission > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", (emissionColor ?? color) * emission);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    [MenuItem("CloudVR/Art/3. Installer Serveur Extractible et Porte (Baie 01)")]
    public static void InstallBladeInRack()
    {
        var bladePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Server_Blade_Detailed.fbx");
        var doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Server_Rack_Door.fbx");
        if (bladePrefab == null || doorPrefab == null)
        {
            Debug.LogError("[ServerBladeBuilder] FBX introuvables !");
            return;
        }

        var baie01 = GameObject.Find("Baie_01");
        if (baie01 == null)
        {
            Debug.LogError("[ServerBladeBuilder] Baie_01 introuvable dans la scène active !");
            return;
        }

        // Nettoyage ancien setup
        var oldDoor = baie01.transform.Find("Door_Interactive_Pivot");
        if (oldDoor != null) UnityEngine.Object.DestroyImmediate(oldDoor.gameObject);
        var oldBlade = baie01.transform.Find("Server_Blade_2U");
        if (oldBlade != null) UnityEngine.Object.DestroyImmediate(oldBlade.gameObject);
        for (int i = baie01.transform.childCount - 1; i >= 0; i--)
            if (baie01.transform.GetChild(i).name == "Lumiere_Inspection_Blade")
                UnityEngine.Object.DestroyImmediate(baie01.transform.GetChild(i).gameObject);

        // ── 1. Matériaux du serveur ──
        var matChassis = GetOrCreateMaterial("Blade_Chassis", new Color(0.12f, 0.14f, 0.17f), 0.7f, 0.4f);
        var matRails = GetOrCreateMaterial("Blade_Rails", new Color(0.60f, 0.65f, 0.70f), 0.85f, 0.6f);
        var matPCB = GetOrCreateMaterial("Blade_PCB", new Color(0.04f, 0.28f, 0.12f), 0.1f, 0.3f);
        var matHeatsink = GetOrCreateMaterial("Blade_Heatsink", new Color(0.80f, 0.44f, 0.18f), 0.9f, 0.35f);
        var matRAM = GetOrCreateMaterial("Blade_RAM", new Color(0.15f, 0.18f, 0.22f), 0.2f, 0.3f);
        var matFans = GetOrCreateMaterial("Blade_Fans", new Color(0.08f, 0.08f, 0.09f), 0.1f, 0.2f);
        var matLedGreen = GetOrCreateMaterial("Blade_LED_Green", new Color(0.12f, 0.95f, 0.35f), 0f, 0.8f, 2.8f, new Color(0.12f, 0.95f, 0.35f));
        var matLedCyan = GetOrCreateMaterial("Blade_LED_Cyan", new Color(0.05f, 0.85f, 0.98f), 0f, 0.8f, 2.5f, new Color(0.05f, 0.85f, 0.98f));
        var matLedAmber = GetOrCreateMaterial("Blade_LED_Amber", new Color(0.98f, 0.55f, 0.08f), 0f, 0.8f, 2.8f, new Color(0.98f, 0.55f, 0.08f));

        // ── 2. Instanciation du serveur 2U à hauteur des yeux (~1.25m) ──
        var bladeGo = (GameObject)PrefabUtility.InstantiatePrefab(bladePrefab, baie01.transform);
        bladeGo.name = "Server_Blade_2U";
        bladeGo.transform.localPosition = new Vector3(0, 1.25f, -0.44f);
        bladeGo.transform.localRotation = Quaternion.identity;

        AssignMaterial(bladeGo, "Chassis", matChassis);
        AssignMaterial(bladeGo, "Rails", matRails);
        AssignMaterial(bladeGo, "PCB", matPCB);
        AssignMaterial(bladeGo, "Heatsink", matHeatsink);
        AssignMaterial(bladeGo, "RAM", matRAM);
        AssignMaterial(bladeGo, "Fans", matFans);
        AssignMaterial(bladeGo, "Green", matLedGreen);
        AssignMaterial(bladeGo, "Cyan", matLedCyan);
        AssignMaterial(bladeGo, "Amber", matLedAmber);

        // ── 3. Porte de baie sur charnières à gauche ──
        var doorPivot = new GameObject("Door_Interactive_Pivot");
        doorPivot.transform.SetParent(baie01.transform, false);
        // Pivot placé sur la charnière gauche de la baie (x = -0.47, z = -0.50)
        doorPivot.transform.localPosition = new Vector3(-0.47f, 0f, -0.50f);
        doorPivot.transform.localRotation = Quaternion.identity;

        var doorGo = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab, doorPivot.transform);
        doorGo.name = "Door_Model";
        doorGo.transform.localPosition = new Vector3(0.47f, 0f, 0f); // Décalé pour que le pivot soit à gauche
        doorGo.transform.localRotation = Quaternion.identity;

        var matDoorFrame = GetOrCreateMaterial("Door_Frame", new Color(0.08f, 0.09f, 0.11f), 0.75f, 0.35f);
        var matDoorHandle = GetOrCreateMaterial("Door_Handle", new Color(0.60f, 0.65f, 0.70f), 0.85f, 0.50f);
        var matDoorMesh = GetOrCreateMaterial("Door_Mesh", new Color(0.18f, 0.22f, 0.28f, 0.85f), 0.3f, 0.30f);
        var matDoorBadge = GetOrCreateMaterial("Door_Badge", new Color(0.05f, 0.85f, 0.98f), 0f, 0.8f, 2.5f, new Color(0.05f, 0.85f, 0.98f));

        AssignMaterial(doorGo, "Frame", matDoorFrame);
        AssignMaterial(doorGo, "Handle", matDoorHandle);
        AssignMaterial(doorGo, "Mesh", matDoorMesh);
        AssignMaterial(doorGo, "Badge", matDoorBadge);

        // ── 4. Lumière d'inspection douce ──
        var lightGo = new GameObject("Lumiere_Inspection_Blade");
        lightGo.transform.SetParent(baie01.transform, false);
        lightGo.transform.localPosition = new Vector3(0, 1.55f, -0.75f);
        var inspLight = lightGo.AddComponent<Light>();
        inspLight.type = LightType.Point;
        inspLight.color = new Color(0.85f, 0.95f, 1.0f);
        inspLight.range = 1.6f;
        inspLight.intensity = 1.5f;
        inspLight.shadows = LightShadows.None;
        inspLight.enabled = false;

        // ── 5. Étiquettes holographiques descriptives ──
        var labelsGo = new GameObject("Hardware_Labels");
        labelsGo.transform.SetParent(bladeGo.transform, false);
        labelsGo.transform.localPosition = new Vector3(0, 0.14f, -0.35f);

        var canvas = labelsGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rt = labelsGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 120);
        labelsGo.transform.localScale = Vector3.one * 0.0018f;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGo.transform.SetParent(labelsGo.transform, false);
        var txt = textGo.GetComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 20;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.15f, 0.95f, 1.0f);
        txt.text = "SERVEUR COMPUTE RACK 2U\n• Bi-Processeur Intel Xeon Scalable (64 Cores)\n• 512 Go RAM DDR5 ECC Régistrée\n• 8x SSD NVMe PCIe 5.0 Hot-Swap";
        textGo.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 110);
        labelsGo.SetActive(false);

        // ── 6. Contrôleur d'interaction ──
        var interaction = baie01.GetComponent<ServerBladeInteraction>();
        if (interaction == null) interaction = baie01.AddComponent<ServerBladeInteraction>();
        interaction.rackDoor = doorPivot.transform;
        interaction.serverBlade = bladeGo.transform;
        interaction.inspectionLight = inspLight;
        interaction.hardwareLabels = labelsGo;

        EditorUtility.SetDirty(baie01);
        EditorSceneManager.MarkSceneDirty(baie01.scene);
        Debug.Log("[ServerBladeBuilder] Serveur extractible 2U et porte ouvrable installés sur Baie_01 !");
    }

    private static void AssignMaterial(GameObject root, string matNamePart, Material targetMat)
    {
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            var mats = r.sharedMaterials;
            bool modified = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].name.IndexOf(matNamePart, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mats[i] = targetMat;
                    modified = true;
                }
            }
            if (modified) r.sharedMaterials = mats;
            else if (r.gameObject.name.IndexOf(matNamePart, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                r.sharedMaterial = targetMat;
            }
        }
    }
}
#endif
