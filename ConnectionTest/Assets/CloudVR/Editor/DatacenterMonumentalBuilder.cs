#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Décorateur monumental du Data Center Tier IV (Jalons C & D) :
/// - Campus extérieur complet (bâtiment principal, guérite vigile, barrière, groupes électrogènes, transformateurs, caméras PTZ)
/// - Sas Mantrap cylindrique biométrique avec contrôle d'accès, portes motorisées, diodes et pesée
/// - Grande Salle Blanche monumentale (40m × 18m, 32 baies 42U, confinement allée froide CAC, faux plancher soufflant, CRAC, busways)
/// - Local technique TGBT / UPS (Voies A et B 2N, barres cuivre, onduleurs, batteries)
/// </summary>
public static class DatacenterMonumentalBuilder
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

    [MenuItem("CloudVR/Art/4. Construire Data Center Monumental Tier IV")]
    public static void InstallMonumentalDataCenter()
    {
        var dcRoot = GameObject.Find("DataCenter_Root");
        if (dcRoot == null)
        {
            Debug.LogError("[DatacenterMonumentalBuilder] DataCenter_Root introuvable dans la scène active !");
            return;
        }

        DecorateDataCenter(dcRoot.transform);
        EditorUtility.SetDirty(dcRoot);
        EditorSceneManager.MarkSceneDirty(dcRoot.scene);
    }

    public static void DecorateDataCenter(Transform dcRoot)
    {
        // Retirer aussi le décor fusionné et les équipements de l'ancienne salle 10 x 12 m.
        foreach (var name in new[] { "Mur_Entree", "Mur_Fond", "Mur_Gauche", "Mur_Droit", "Sol", "Plafond",
            "Architecture_Originale", "Chemin_Visiteur", "Sas_Securite_TierIV", "Baie_Secondaire_-1", "Baie_Secondaire_1",
            "Accueil", "Accueil_Support", "Repere_Baie", "Repere_Baie_Support", "Guide_Visite", "Guide_Visite_Support", "Zone_Teleportation_Cour", "Zone_Teleportation_Sas" })
        {
            var legacy = dcRoot.Find(name);
            if (legacy != null) UnityEngine.Object.DestroyImmediate(legacy.gameObject);
        }
        var mode = UnityEngine.Object.FindFirstObjectByType<InputModeManager>();
        foreach (var rig in new[] { mode.pcRig, mode.vrRig })
            rig.transform.SetPositionAndRotation(new Vector3(0, .08f, -28f), Quaternion.identity);
        var rack = dcRoot.Find("Baie_01");
        if (rack != null) rack.SetPositionAndRotation(new Vector3(2.8f, 0, 14f), Quaternion.Euler(0, 90, 0));
        // ── 0. Nettoyage idempotent ──────────────────────────────────
        var oldCampus = dcRoot.Find("Monumental_Architecture");
        if (oldCampus != null) UnityEngine.Object.DestroyImmediate(oldCampus.gameObject);

        var archRoot = new GameObject("Monumental_Architecture").transform;
        archRoot.SetParent(dcRoot, false);

        // ── 1. Matériaux URP ──────────────────────────────────────────
        var matConcrete   = GetOrCreateMaterial("Campus_Concrete",   new Color(0.45f, 0.44f, 0.42f), 0f, 0.2f);
        var matFacade     = GetOrCreateMaterial("Campus_Facade",     new Color(0.18f, 0.20f, 0.22f), 0.2f, 0.45f);
        var matGlass      = GetOrCreateMaterial("Campus_Glass",      new Color(0.55f, 0.75f, 0.80f), 0.1f, 0.95f);
        var matYellow     = GetOrCreateMaterial("Campus_Yellow",     new Color(0.98f, 0.82f, 0.05f), 0f, 0.5f);
        var matBlack      = GetOrCreateMaterial("Campus_Black",      new Color(0.06f, 0.06f, 0.06f), 0.1f, 0.5f);
        var matMetal      = GetOrCreateMaterial("Campus_Metal",      new Color(0.35f, 0.38f, 0.42f), 0.8f, 0.7f);
        var matLogo       = GetOrCreateMaterial("Campus_Logo",       new Color(0.90f, 0.92f, 0.94f), 0f, 0.8f, 2.5f, new Color(0.90f, 0.92f, 0.94f));
        var matFence      = GetOrCreateMaterial("Campus_Fence",      new Color(0.22f, 0.24f, 0.26f), 0.6f, 0.6f);
        var matAsphalt    = GetOrCreateMaterial("Campus_Asphalt",    new Color(0.12f, 0.12f, 0.13f), 0f, 0.1f);
        var matGenset     = GetOrCreateMaterial("Campus_Generator",  new Color(0.20f, 0.22f, 0.24f), 0.2f, 0.5f);
        var matTrafo      = GetOrCreateMaterial("Campus_Trafo",      new Color(0.25f, 0.30f, 0.35f), 0.2f, 0.45f);
        var matLedG       = GetOrCreateMaterial("Campus_LED_Green",  new Color(0.15f, 0.95f, 0.35f), 0f, 0.8f, 3.5f, new Color(0.15f, 0.95f, 0.35f));
        var matLedR       = GetOrCreateMaterial("Campus_LED_Red",    new Color(0.95f, 0.15f, 0.10f), 0f, 0.8f, 3.5f, new Color(0.95f, 0.15f, 0.10f));
        var matPipe       = GetOrCreateMaterial("Campus_Pipe",       new Color(0.60f, 0.62f, 0.65f), 0.75f, 0.75f);

        // Matériaux Salle Blanche
        var matFloorTile  = GetOrCreateMaterial("WS_FloorTile",      new Color(0.60f, 0.62f, 0.64f), 0.4f, 0.75f);
        var matFloorPerf  = GetOrCreateMaterial("WS_FloorPerf",      new Color(0.25f, 0.28f, 0.32f), 0.5f, 0.65f);
        var matCeiling    = GetOrCreateMaterial("WS_Ceiling",        new Color(0.85f, 0.86f, 0.88f), 0f, 0.3f);
        var matRackBody   = GetOrCreateMaterial("WS_RackBody",       new Color(0.10f, 0.11f, 0.13f), 0.6f, 0.7f);
        var matRackDoor   = GetOrCreateMaterial("WS_RackDoor",       new Color(0.08f, 0.09f, 0.11f), 0.7f, 0.85f);
        var matRackLedG   = GetOrCreateMaterial("WS_RackLED_G",      new Color(0.12f, 0.95f, 0.35f), 0f, 0.8f, 3.0f, new Color(0.12f, 0.95f, 0.35f));
        var matRackLedB   = GetOrCreateMaterial("WS_RackLED_B",      new Color(0.05f, 0.55f, 1.00f), 0f, 0.8f, 2.5f, new Color(0.05f, 0.55f, 1.00f));
        var matCacGlass   = GetOrCreateMaterial("WS_CAC_Glass",      new Color(0.55f, 0.80f, 0.85f), 0f, 0.95f);
        var matCacFrame   = GetOrCreateMaterial("WS_CAC_Frame",      new Color(0.20f, 0.22f, 0.25f), 0.6f, 0.7f);
        var matBusway     = GetOrCreateMaterial("WS_Busway",         new Color(0.68f, 0.50f, 0.12f), 0.3f, 0.5f);
        var matCrac       = GetOrCreateMaterial("WS_CRAC",           new Color(0.20f, 0.22f, 0.26f), 0.1f, 0.6f);
        var matColdAir    = GetOrCreateMaterial("WS_ColdAirGlow",    new Color(0.55f, 0.85f, 1.0f),  0f, 0.8f, 1.8f, new Color(0.55f, 0.85f, 1.0f));

        // Matériaux Sas Mantrap
        var matSteel      = GetOrCreateMaterial("Mantrap_Steel",     new Color(0.30f, 0.33f, 0.36f), 0.85f, 0.75f);
        var matMantrapG   = GetOrCreateMaterial("Mantrap_Glass",     new Color(0.45f, 0.65f, 0.75f), 0.05f, 0.95f);
        var matScanner    = GetOrCreateMaterial("Mantrap_Scanner",   new Color(0.05f, 0.55f, 0.90f), 0f, 0.8f, 2.2f, new Color(0.05f, 0.55f, 0.90f));

        // ── 2. Campus extérieur (placé à l'extérieur / entrée) ──────
        var campusPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/DataCenter_Campus.fbx");
        if (campusPrefab != null)
        {
            var campusGo = (GameObject)PrefabUtility.InstantiatePrefab(campusPrefab, archRoot);
            campusGo.name = "Campus_Exterieur_TierIV";
            // Placé devant l'entrée à Z = -22m
            campusGo.transform.localPosition = new Vector3(0, 0, .1f);
            campusGo.transform.localRotation = Quaternion.Euler(0, 180, 0);
            PrefabUtility.UnpackPrefabInstance(campusGo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            AssignMaterial(campusGo, "Campus_Concrete", matConcrete);
            AssignMaterial(campusGo, "Campus_Facade", matFacade);
            AssignMaterial(campusGo, "Campus_Glass", matGlass);
            AssignMaterial(campusGo, "Campus_Yellow", matYellow);
            AssignMaterial(campusGo, "Campus_Black", matBlack);
            AssignMaterial(campusGo, "Campus_Metal", matMetal);
            AssignMaterial(campusGo, "Campus_Logo", matLogo);
            AssignMaterial(campusGo, "Campus_Fence", matFence);
            AssignMaterial(campusGo, "Campus_Asphalt", matAsphalt);
            AssignMaterial(campusGo, "Campus_Generator", matGenset);
            AssignMaterial(campusGo, "Campus_Trafo", matTrafo);
            AssignMaterial(campusGo, "Campus_LED_Green", matLedG);
            AssignMaterial(campusGo, "Campus_LED_Red", matLedR);
            AssignMaterial(campusGo, "Campus_Pipe", matPipe);

            // Colliders pour le sol parking et le bâtiment
            // Le modèle de référence était un bloc plein : conserver une véritable façade ajourée.
            RemovePart(campusGo, "DC_Building_Core");
            RemovePart(campusGo, "DC_Facade_Front");
            RemovePart(campusGo, "Fence_0_Base");
            RemovePart(campusGo, "Fence_0_Barbelé");
            foreach (var r in campusGo.GetComponentsInChildren<Renderer>())
                if (r.name.StartsWith("Fence_Poteau_") && Mathf.Abs(r.bounds.center.x) < 3f)
                    r.gameObject.SetActive(false);
            Box(archRoot, "Facade_Gauche", new Vector3(-8.1f, 7, -4), new Vector3(11.8f, 14, .4f), matFacade);
            Box(archRoot, "Facade_Droite", new Vector3(8.1f, 7, -4), new Vector3(11.8f, 14, .4f), matFacade);
            Box(archRoot, "Facade_Linteau", new Vector3(0, 8.6f, -4), new Vector3(4.4f, 10.8f, .4f), matFacade);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(archRoot, "Campus_Paroi", new Vector3(side * 14, 7, 5), new Vector3(.2f, 14, 18), matConcrete);
                Box(archRoot, "Cloture_Ouverte", new Vector3(side * 8.2f, .75f, -12.9f), new Vector3(11.6f, 1.5f, .15f), matFence);
            }
            Box(archRoot, "Campus_Toiture", new Vector3(0, 14, 5), new Vector3(28, .2f, 18), matConcrete);
        }
        Box(archRoot, "Cour_Continue", new Vector3(0, -.10f, -18), new Vector3(40, .20f, 28), matAsphalt);
        Box(archRoot, "Seuil_Continu", new Vector3(0, -.10f, -2), new Vector3(4.4f, .20f, 4), matFloorTile);

        // ── 3. Sas Mantrap Biométrique Unitaire ────────────────────────
        var mantrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Mantrap_Airlock.fbx");
        if (mantrapPrefab != null)
        {
            var mantrapGo = (GameObject)PrefabUtility.InstantiatePrefab(mantrapPrefab, archRoot);
            mantrapGo.name = "Sas_Mantrap_Biometrique";
            // Placé au seuil entre la zone extérieure et la salle blanche (Z = 0.5m)
            mantrapGo.transform.localPosition = new Vector3(0, 0, -2f);
            mantrapGo.transform.localRotation = Quaternion.Euler(0, 180, 0);
            mantrapGo.transform.localScale = new Vector3(2, 1, 2);
            PrefabUtility.UnpackPrefabInstance(mantrapGo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            // Les deux cylindres et les anciens cadres étaient des volumes fermés, sans ouverture.
            foreach (var name in new[] { "Mantrap_Wall_Outer", "Mantrap_Wall_Inner", "Door_Entry_Frame", "Door_Exit_Frame", "Door_Entry_Warn", "Door_Exit_Warn" })
                RemovePart(mantrapGo, name);
            for (int i = 0; i < 24; i++)
            {
                float angle = i * 15f * Mathf.Deg2Rad;
                if (Mathf.Abs(Mathf.Sin(angle)) < .64f) continue; // ouvertures nord / sud de 2,3 m
                var panel = Box(archRoot, "Mantrap_Paroi", new Vector3(Mathf.Sin(angle) * 1.84f, 1.3f, -2 + Mathf.Cos(angle) * 1.84f), new Vector3(.49f, 2.6f, .08f), matSteel);
                panel.transform.rotation = Quaternion.Euler(0, i * 15f, 0);
            }

            AssignMaterial(mantrapGo, "Mantrap_Steel", matSteel);
            AssignMaterial(mantrapGo, "Mantrap_Glass", matMantrapG);
            AssignMaterial(mantrapGo, "Mantrap_Scanner", matScanner);
            AssignMaterial(mantrapGo, "Mantrap_LED_G", matLedG);
            AssignMaterial(mantrapGo, "Mantrap_LED_R", matLedR);

            // Attacher et configurer MantrapController
            var ctrl = mantrapGo.AddComponent<MantrapController>();
            ctrl.doorEntry = mantrapGo.transform.Find("Door_Entry_Panel");
            ctrl.doorExit = mantrapGo.transform.Find("Door_Exit_Panel");
            ctrl.matLedGreen = matLedG;
            ctrl.matLedRed = matLedR;
            ctrl.matLedScanning = matScanner;
            ctrl.scanDuration = 2f;
            foreach (var door in new[] { ctrl.doorEntry, ctrl.doorExit })
                if (door != null) door.gameObject.AddComponent<BoxCollider>();

            var rEntry = mantrapGo.transform.Find("Door_Entry_LED");
            if (rEntry != null) ctrl.ledEntry = rEntry.GetComponent<Renderer>();

            var rExit = mantrapGo.transform.Find("Door_Exit_LED");
            if (rExit != null) ctrl.ledExit = rExit.GetComponent<Renderer>();

            var rScan = mantrapGo.transform.Find("BioTerm_Screen");
            if (rScan != null) ctrl.ledScanner = rScan.GetComponent<Renderer>();

            // Trigger d'entrée dans le sas
            // Détection par distance au rig actif, également utilisable avec XR Origin sans Rigidbody.
        }

        // ── 4. Grande Salle Blanche Monumentale ────────────────────────
        var whiteSpacePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/WhiteSpace_Hall.fbx");
        if (whiteSpacePrefab != null)
        {
            var wsGo = (GameObject)PrefabUtility.InstantiatePrefab(whiteSpacePrefab, archRoot);
            wsGo.name = "Grande_Salle_Blanche_Monumentale";
            // Démarre après le sas (Z = 12m)
            wsGo.transform.localPosition = new Vector3(0, -.8f, 20f);
            wsGo.transform.localRotation = Quaternion.Euler(0, 90, 0);
            PrefabUtility.UnpackPrefabInstance(wsGo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            AssignMaterial(wsGo, "WS_FloorTile", matFloorTile);
            AssignMaterial(wsGo, "WS_FloorPerf", matFloorPerf);
            AssignMaterial(wsGo, "WS_Ceiling", matCeiling);
            AssignMaterial(wsGo, "WS_RackBody", matRackBody);
            AssignMaterial(wsGo, "WS_RackDoor", matRackDoor);
            AssignMaterial(wsGo, "WS_RackLED_G", matRackLedG);
            AssignMaterial(wsGo, "WS_RackLED_B", matRackLedB);
            AssignMaterial(wsGo, "WS_CAC_Glass", matCacGlass);
            AssignMaterial(wsGo, "WS_CAC_Frame", matCacFrame);
            AssignMaterial(wsGo, "WS_Busway", matBusway);
            AssignMaterial(wsGo, "WS_CRAC", matCrac);
            AssignMaterial(wsGo, "WS_ColdAirGlow", matColdAir);
            // Les deux CRAC centraux des petits côtés coupaient l'allée après rotation du FBX.
            foreach (var r in wsGo.GetComponentsInChildren<MeshRenderer>())
                if (r.name.StartsWith("CRAC_19_0_") || r.name.StartsWith("CRAC_-19_0_"))
                    r.transform.position += Vector3.right * (r.bounds.center.z > 20 ? -8f : 8f);
            foreach (var r in wsGo.GetComponentsInChildren<MeshRenderer>())
            {
                var b = r.bounds;
                if (r.name.StartsWith("WS_Wall_") && b.center.z < 1f)
                {
                    r.gameObject.SetActive(false);
                    continue;
                }
                if (r.name.StartsWith("WS_Wall_") || r.name.EndsWith("_Body"))
                    r.gameObject.AddComponent<BoxCollider>();
            }
            Box(archRoot, "WhiteSpace_Sol_Continu", new Vector3(0, -.12f, 20), new Vector3(18, .24f, 40), matFloorTile);
            Box(archRoot, "WhiteSpace_Entree_G", new Vector3(-5.6f, 2.1f, 0), new Vector3(6.8f, 4.2f, .2f), matCeiling);
            Box(archRoot, "WhiteSpace_Entree_D", new Vector3(5.6f, 2.1f, 0), new Vector3(6.8f, 4.2f, .2f), matCeiling);
            Box(archRoot, "WhiteSpace_Entree_Haut", new Vector3(0, 3.7f, 0), new Vector3(4.4f, 1, .2f), matCeiling);
            for (int z = 4; z < 40; z += 8)
            {
                var light = new GameObject("Eclairage_WhiteSpace").AddComponent<Light>();
                light.transform.SetParent(archRoot, false);
                light.transform.localPosition = new Vector3(0, 3.8f, z);
                light.type = LightType.Point; light.range = 12; light.intensity = 2.5f;
                light.color = new Color(.80f, .90f, 1f); light.shadows = LightShadows.None;
            }
        }

        // ── 5. Local Technique Électrique TGBT / UPS ──────────────────
        var elecPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Electrical_Room.fbx");
        if (elecPrefab != null)
        {
            var elecGo = (GameObject)PrefabUtility.InstantiatePrefab(elecPrefab, archRoot);
            elecGo.name = "Local_Technique_TGBT_UPS";
            // Placé en aile latérale (X = -26m, Z = 12m)
            elecGo.transform.localPosition = new Vector3(-26f, 0, 12f);
            elecGo.transform.localRotation = Quaternion.identity;

            AssignMaterial(elecGo, "Campus_Concrete", matConcrete);
            AssignMaterial(elecGo, "WS_Busway", matBusway);
            AssignMaterial(elecGo, "Campus_LED_Green", matLedG);
            AssignMaterial(elecGo, "Campus_LED_Red", matLedR);
        }

        Debug.Log("[DatacenterMonumentalBuilder] Data Center Monumental Tier IV complet installé avec succès !");
        // Étendre les surfaces XR sans ajouter une dalle opaque au-dessus du sol.
        var zone = dcRoot.Find("Zone_Teleportation");
        if (zone != null)
        {
            zone.localPosition = new Vector3(0, .012f, 20);
            zone.localScale = new Vector3(3.6f, .018f, 39.8f);
            zone.GetComponent<Renderer>().enabled = false;
            var outside = UnityEngine.Object.Instantiate(zone.gameObject, dcRoot);
            outside.name = "Zone_Teleportation_Cour";
            outside.transform.localPosition = new Vector3(0, .012f, -18);
            outside.transform.localScale = new Vector3(3.6f, .018f, 27.8f);
            var sasZone = UnityEngine.Object.Instantiate(zone.gameObject, dcRoot);
            sasZone.name = "Zone_Teleportation_Sas";
            sasZone.transform.localPosition = new Vector3(0, .05f, -2);
            sasZone.transform.localScale = new Vector3(1.3f, .018f, 1.2f);
        }
    }

    private static void RemovePart(GameObject root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) { t.gameObject.SetActive(false); return; }
    }

    private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent, false);
        go.transform.localPosition = position; go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private static void AssignMaterial(GameObject root, string matNamePart, Material mat)
    {
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            if (r.gameObject.name.Contains(matNamePart) || (r.sharedMaterial != null && r.sharedMaterial.name.Contains(matNamePart)))
            {
                r.sharedMaterial = mat;
            }
        }
    }
}
#endif
