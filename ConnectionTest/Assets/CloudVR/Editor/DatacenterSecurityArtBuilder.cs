#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Décorateur artistique du Sas de Sécurité Militaire (DataCenter)
/// et du Tunnel de Données Optique (Chambre).
/// </summary>
public static class DatacenterSecurityArtBuilder
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

    [MenuItem("CloudVR/Art/1. Installer Tunnel Fibre Optique (Chambre)")]
    public static void InstallFiberTunnelInRoom()
    {
        var tunnelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Data_Fiber_Tunnel.fbx");
        if (tunnelPrefab == null)
        {
            Debug.LogError("[SecurityArt] Data_Fiber_Tunnel.fbx introuvable !");
            return;
        }

        var roomRoot = GameObject.Find("Chambre_Root");
        if (roomRoot == null)
        {
            Debug.LogError("[SecurityArt] Chambre_Root introuvable dans la scène active !");
            return;
        }

        var oldTunnel = GameObject.Find("Network_Ride_Tunnel");
        if (oldTunnel != null) UnityEngine.Object.DestroyImmediate(oldTunnel);

        var tunnelGo = (GameObject)PrefabUtility.InstantiatePrefab(tunnelPrefab, roomRoot.transform);
        tunnelGo.name = "Network_Ride_Tunnel";
        tunnelGo.transform.position = new Vector3(0, -40f, 0); // Placé 40m sous la pièce
        tunnelGo.transform.localRotation = Quaternion.Euler(90f, 0, 0); // Aligné sur l'axe Z

        // Matériaux du tunnel
        var matOuter = GetOrCreateMaterial("Tunnel_Outer", new Color(0.04f, 0.05f, 0.08f), 0.1f, 0.2f);
        var matRing = GetOrCreateMaterial("Tunnel_Ring_Metal", new Color(0.45f, 0.50f, 0.58f), 0.85f, 0.55f);
        var matCore = GetOrCreateMaterial("Tunnel_Fiber_Core", new Color(0.05f, 0.90f, 1.0f), 0f, 0.9f, 3.5f, new Color(0.05f, 0.90f, 1.0f));
        var matPulse = GetOrCreateMaterial("Tunnel_Pulse_Magenta", new Color(0.98f, 0.15f, 0.80f), 0f, 0.9f, 4.0f, new Color(0.98f, 0.15f, 0.80f));

        AssignMaterial(tunnelGo, "Outer", matOuter);
        AssignMaterial(tunnelGo, "Ring", matRing);
        AssignMaterial(tunnelGo, "Core", matCore);
        AssignMaterial(tunnelGo, "Pulse", matPulse);

        // Composant NetworkDataRide
        var ride = roomRoot.GetComponent<NetworkDataRide>();
        if (ride == null) ride = roomRoot.AddComponent<NetworkDataRide>();
        ride.tunnelInstance = tunnelGo.transform;
        ride.sender = UnityEngine.Object.FindFirstObjectByType<PhotoSender>();
        ride.transition = UnityEngine.Object.FindFirstObjectByType<StepTransition>();

        EditorUtility.SetDirty(roomRoot);
        EditorSceneManager.MarkSceneDirty(roomRoot.scene);
        Debug.Log("[SecurityArt] Tunnel Fibre Optique installé avec succès sous Chambre_Root !");
    }

    [MenuItem("CloudVR/Art/2. Installer Sas de Sécurité (DataCenter)")]
    public static void InstallSecurityAirlockInDataCenter()
    {
        var airlockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/DataCenter_Airlock.fbx");
        if (airlockPrefab == null)
        {
            Debug.LogError("[SecurityArt] DataCenter_Airlock.fbx introuvable !");
            return;
        }

        var dcRoot = GameObject.Find("DataCenter_Root");
        if (dcRoot == null)
        {
            Debug.LogError("[SecurityArt] DataCenter_Root introuvable dans la scène active !");
            return;
        }

        var oldAirlock = GameObject.Find("Sas_Securite_TierIV");
        if (oldAirlock != null) UnityEngine.Object.DestroyImmediate(oldAirlock);

        var airlockGo = (GameObject)PrefabUtility.InstantiatePrefab(airlockPrefab, dcRoot.transform);
        airlockGo.name = "Sas_Securite_TierIV";
        airlockGo.transform.position = new Vector3(0, 0, 0.45f); // Seuil d'entrée de l'allée
        airlockGo.transform.rotation = Quaternion.identity;

        // Matériaux du sas
        var matArmor = GetOrCreateMaterial("Airlock_Armor", new Color(0.07f, 0.08f, 0.10f), 0.75f, 0.35f);
        var matSteel = GetOrCreateMaterial("Airlock_Steel", new Color(0.38f, 0.42f, 0.48f), 0.85f, 0.50f);
        var matHazard = GetOrCreateMaterial("Airlock_Hazard", new Color(0.95f, 0.72f, 0.05f), 0.1f, 0.40f);
        var matGlass = GetOrCreateMaterial("Airlock_Glass", new Color(0.06f, 0.18f, 0.25f), 0.2f, 0.90f);
        var matBeacon = GetOrCreateMaterial("Airlock_Beacon_Amber", new Color(0.98f, 0.50f, 0.05f), 0f, 0.8f, 3.0f, new Color(0.98f, 0.50f, 0.05f));
        var matLedGreen = GetOrCreateMaterial("Airlock_Led_Green", new Color(0.12f, 0.95f, 0.28f), 0f, 0.8f, 2.5f, new Color(0.12f, 0.95f, 0.28f));
        var matLedRed = GetOrCreateMaterial("Airlock_Led_Red", new Color(0.95f, 0.10f, 0.10f), 0f, 0.8f, 2.5f, new Color(0.95f, 0.10f, 0.10f));

        AssignMaterial(airlockGo, "Armor", matArmor);
        AssignMaterial(airlockGo, "Steel", matSteel);
        AssignMaterial(airlockGo, "Hazard", matHazard);
        AssignMaterial(airlockGo, "Glass", matGlass);
        AssignMaterial(airlockGo, "Beacon", matBeacon);
        AssignMaterial(airlockGo, "Green", matLedGreen);
        AssignMaterial(airlockGo, "Red", matLedRed);

        // Lumière ponctuelle d'ambiance d'alerte orange au sommet du sas
        var beaconLightGo = new GameObject("Lumiere_Gyrophare_Sas");
        beaconLightGo.transform.SetParent(airlockGo.transform, false);
        beaconLightGo.transform.localPosition = new Vector3(0, 2.76f, -0.15f);
        var bLight = beaconLightGo.AddComponent<Light>();
        bLight.type = LightType.Point;
        bLight.color = new Color(0.98f, 0.55f, 0.08f);
        bLight.range = 4.5f;
        bLight.intensity = 1.8f;
        bLight.shadows = LightShadows.None;

        EditorUtility.SetDirty(dcRoot);
        EditorSceneManager.MarkSceneDirty(dcRoot.scene);
        Debug.Log("[SecurityArt] Sas de Sécurité Militaire Tier IV installé dans DataCenter_Root !");
    }

    private static void AssignMaterial(GameObject root, string matNamePart, Material mat)
    {
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            if (r.gameObject.name.IndexOf(matNamePart, StringComparison.OrdinalIgnoreCase) >= 0 ||
               (r.sharedMaterial != null && r.sharedMaterial.name.IndexOf(matNamePart, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                r.sharedMaterial = mat;
            }
        }
    }
}
#endif
