#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Décorateur artistique complet de la Chambre étudiante Cocooning (Jalon A) :
/// - Mobilier réaliste (lit étudiant, bureau gamer, chaise ergonomique, étagère, livres, tapis)
/// - Box Internet domestique avec voyants LED (PON, Fibre, LAN, Wi-Fi)
/// - Rideaux tombants en velours encadrant la fenêtre
/// - Guirlande micro-LEDs dorée sur l'étagère
/// - Ambiance lumineuse 2700K ambre chaleureuse et contrastée
/// - Tasse de café fumante (particules douces)
/// - Audio procédural de pluie nocturne contre la vitre
/// </summary>
public static class ChambreArtBuilder
{
    private const string MaterialsPath = "Assets/CloudVR/Materials/";

    public static Material GetOrCreateMaterial(string name, Color color, float metallic = 0.0f, float smoothness = 0.4f, float emission = 0f, Color? emissionColor = null)
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

    [MenuItem("CloudVR/Art/0. Décorer Chambre Cocooning Complète")]
    public static void DecorateActiveScene()
    {
        var roomRoot = GameObject.Find("Chambre_Root");
        if (roomRoot == null)
        {
            Debug.LogError("[ChambreArtBuilder] Chambre_Root introuvable dans la scène active !");
            return;
        }
        Decorate(roomRoot.transform);
        EditorUtility.SetDirty(roomRoot);
        EditorSceneManager.MarkSceneDirty(roomRoot.scene);
    }

    public static void Decorate(Transform roomRoot)
    {
        // ── 0. Nettoyage de l'ancien décor pour idempotence ───────────
        var oldDecor = roomRoot.Find("Mobilier_Et_Ambiance");
        if (oldDecor != null) UnityEngine.Object.DestroyImmediate(oldDecor.gameObject);

        var decorRoot = new GameObject("Mobilier_Et_Ambiance").transform;
        decorRoot.SetParent(roomRoot, false);

        // ── 1. Matériaux URP ──────────────────────────────────────────
        var matWood = GetOrCreateMaterial("Room_Wood_Warm", new Color(0.42f, 0.28f, 0.17f), 0.05f, 0.45f);
        var matMetal = GetOrCreateMaterial("Room_Metal_Black", new Color(0.10f, 0.10f, 0.12f), 0.75f, 0.35f);
        var matSheet = GetOrCreateMaterial("Room_Bed_Sheet", new Color(0.88f, 0.88f, 0.87f), 0.0f, 0.15f);
        var matDuvet = GetOrCreateMaterial("Room_Bed_Duvet", new Color(0.14f, 0.28f, 0.42f), 0.0f, 0.25f);
        var matPillow = GetOrCreateMaterial("Room_Bed_Pillow", new Color(0.92f, 0.92f, 0.91f), 0.0f, 0.10f);
        var matPlastic = GetOrCreateMaterial("Room_PC_Plastic", new Color(0.12f, 0.12f, 0.14f), 0.1f, 0.5f);
        var matMousepad = GetOrCreateMaterial("Room_Mousepad", new Color(0.16f, 0.18f, 0.22f), 0.0f, 0.2f);
        var matRgbCyan = GetOrCreateMaterial("Room_RGB_Cyan", new Color(0.05f, 0.85f, 0.95f), 0.0f, 0.8f, 2.0f, new Color(0.05f, 0.85f, 0.95f));
        var matRgbRed = GetOrCreateMaterial("Room_RGB_Red", new Color(0.95f, 0.20f, 0.10f), 0.0f, 0.8f, 1.8f, new Color(0.95f, 0.20f, 0.10f));
        var matCeramic = GetOrCreateMaterial("Room_Ceramic", new Color(0.94f, 0.94f, 0.94f), 0.0f, 0.85f);
        var matCarpet = GetOrCreateMaterial("Room_Carpet", new Color(0.72f, 0.70f, 0.66f), 0.0f, 0.1f);
        var matPlant = GetOrCreateMaterial("Room_Plant", new Color(0.18f, 0.58f, 0.24f), 0.0f, 0.35f);
        var matPot = GetOrCreateMaterial("Room_Pot", new Color(0.68f, 0.38f, 0.25f), 0.0f, 0.25f);
        var matBookBlue = GetOrCreateMaterial("Room_Book_Blue", new Color(0.16f, 0.36f, 0.65f), 0.0f, 0.4f);
        var matBookRed = GetOrCreateMaterial("Room_Book_Red", new Color(0.75f, 0.22f, 0.18f), 0.0f, 0.4f);
        var matBookYellow = GetOrCreateMaterial("Room_Book_Yellow", new Color(0.85f, 0.65f, 0.15f), 0.0f, 0.4f);

        // Matériaux Box Internet & Nouveaux éléments cocooning
        var matBoxWhite = GetOrCreateMaterial("Room_Box_White", new Color(0.88f, 0.87f, 0.85f), 0.0f, 0.35f);
        var matBoxDark = GetOrCreateMaterial("Room_Box_Dark", new Color(0.08f, 0.08f, 0.10f), 0.0f, 0.3f);
        var matLedGreen = GetOrCreateMaterial("Room_LED_Green", new Color(0.15f, 0.95f, 0.35f), 0.0f, 0.8f, 3.5f, new Color(0.15f, 0.95f, 0.35f));
        var matLedBlue = GetOrCreateMaterial("Room_LED_Blue", new Color(0.20f, 0.55f, 1.00f), 0.0f, 0.8f, 2.8f, new Color(0.20f, 0.55f, 1.00f));
        var matLedOrange = GetOrCreateMaterial("Room_LED_Orange", new Color(1.00f, 0.50f, 0.05f), 0.0f, 0.8f, 2.5f, new Color(1.00f, 0.50f, 0.05f));
        var matCurtainFabric = GetOrCreateMaterial("Room_Curtain_Fabric", new Color(0.22f, 0.16f, 0.32f), 0.0f, 0.85f);
        var matCurtainRod = GetOrCreateMaterial("Room_Curtain_Rod", new Color(0.55f, 0.48f, 0.38f), 0.7f, 0.3f);
        var matFairyBulb = GetOrCreateMaterial("Room_Fairy_Bulb", new Color(1.0f, 0.95f, 0.60f), 0.0f, 0.9f, 3.0f, new Color(1.0f, 0.80f, 0.40f));

        // ── 2. Lit étudiant ──────────────────────────────────────────
        var bedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Bed_Student.fbx");
        if (bedPrefab != null)
        {
            var bed = (GameObject)PrefabUtility.InstantiatePrefab(bedPrefab, decorRoot);
            bed.name = "Lit_Etudiant";
            bed.transform.localPosition = new Vector3(-1.95f, 0f, 0.50f);
            bed.transform.localRotation = Quaternion.Euler(0, 0, 0);

            AssignMaterial(bed, "Bed_Wood", matWood);
            AssignMaterial(bed, "Bed_Metal", matMetal);
            AssignMaterial(bed, "Bed_Sheet", matSheet);
            AssignMaterial(bed, "Bed_Duvet", matDuvet);
            AssignMaterial(bed, "Bed_Pillow", matPillow);

            var col = bed.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.30f, 0);
            col.size = new Vector3(1.02f, 0.60f, 2.06f);
        }

        // ── 3. Grand Bureau Gamer & Setup ────────────────────────────
        var deskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Desk_Setup.fbx");
        if (deskPrefab != null)
        {
            var desk = (GameObject)PrefabUtility.InstantiatePrefab(deskPrefab, decorRoot);
            desk.name = "Setup_Bureau_Gamer";
            desk.transform.localPosition = new Vector3(0.20f, 0f, -1.35f);
            desk.transform.localRotation = Quaternion.identity;

            AssignMaterial(desk, "Desk_Wood", matWood);
            AssignMaterial(desk, "Desk_Metal", matMetal);
            AssignMaterial(desk, "PC_Plastic", matPlastic);
            AssignMaterial(desk, "Desk_Mousepad", matMousepad);
            AssignMaterial(desk, "PC_RGB_Cyan", matRgbCyan);
            AssignMaterial(desk, "PC_RGB_Red", matRgbRed);
            AssignMaterial(desk, "Desk_Ceramic", matCeramic);

            var deskCol = desk.AddComponent<BoxCollider>();
            deskCol.center = new Vector3(0, 0.38f, 0);
            deskCol.size = new Vector3(1.65f, 0.76f, 0.85f);

            // Chaise avec assise interactive
            var chairColGo = new GameObject("Chaise_Collider");
            chairColGo.transform.SetParent(desk.transform, false);
            var chairCol = chairColGo.AddComponent<BoxCollider>();
            chairCol.center = new Vector3(0, 0.45f, -0.58f);
            chairCol.size = new Vector3(0.55f, 0.90f, 0.55f);
            var seat = chairColGo.AddComponent<ChairSeatInteraction>();
            seat.seatedEyePosition = new Vector3(0.20f, 1.12f, -0.90f);
            seat.lookTarget = new Vector3(0.20f, 1.05f, -1.40f);
        }

        // ── 4. Décorations de la chambre ─────────────────────────────
        var decorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Room_Decor.fbx");
        if (decorPrefab != null)
        {
            var decor = (GameObject)PrefabUtility.InstantiatePrefab(decorPrefab, decorRoot);
            decor.name = "Deco_Chambre";
            decor.transform.localPosition = new Vector3(0.20f, 0f, -1.35f);
            decor.transform.localRotation = Quaternion.identity;

            AssignMaterial(decor, "Decor_Wood", matWood);
            AssignMaterial(decor, "Decor_Metal", matMetal);
            AssignMaterial(decor, "Decor_Carpet", matCarpet);
            AssignMaterial(decor, "Decor_Plant", matPlant);
            AssignMaterial(decor, "Decor_Pot", matPot);
            AssignMaterial(decor, "Decor_Book_Blue", matBookBlue);
            AssignMaterial(decor, "Decor_Book_Red", matBookRed);
            AssignMaterial(decor, "Decor_Book_Yellow", matBookYellow);
            AssignMaterial(decor, "Decor_Poster_Frame", matMetal);
            
            var matPosterTech = GetOrCreateMaterial("Room_Poster_Tech", new Color(0.08f, 0.22f, 0.45f), 0.1f, 0.6f, 0.5f, new Color(0.1f, 0.5f, 0.9f));
            AssignMaterial(decor, "Poster_Fond", matPosterTech);
        }

        // ── 5. Box Internet domestique (avec voyants LED) ─────────────
        var boxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Box_Internet.fbx");
        if (boxPrefab != null)
        {
            var boxGo = (GameObject)PrefabUtility.InstantiatePrefab(boxPrefab, decorRoot);
            boxGo.name = "Box_Internet_FTTH";
            // Posée à gauche sur le bureau à côté de la lampe
            boxGo.transform.localPosition = new Vector3(-0.45f, 0.75f, -1.18f);
            boxGo.transform.localRotation = Quaternion.Euler(0, 15f, 0);

            AssignMaterial(boxGo, "Box_White", matBoxWhite);
            AssignMaterial(boxGo, "Box_Dark", matBoxDark);
            AssignMaterial(boxGo, "Box_LED_Green", matLedGreen);
            AssignMaterial(boxGo, "Box_LED_Blue", matLedBlue);
            AssignMaterial(boxGo, "Box_LED_Orange", matLedOrange);
        }

        // ── 6. Rideaux tombants aux fenêtres ─────────────────────────
        var curtainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Room_Curtains.fbx");
        if (curtainPrefab != null)
        {
            var curtainGo = (GameObject)PrefabUtility.InstantiatePrefab(curtainPrefab, decorRoot);
            curtainGo.name = "Rideaux_Fenetre";
            // Encadre la fenêtre située sur le mur droit
            curtainGo.transform.localPosition = new Vector3(2.45f, 0f, 0.30f);
            curtainGo.transform.localRotation = Quaternion.Euler(0, -90f, 0);

            AssignMaterial(curtainGo, "Curtain_Fabric", matCurtainFabric);
            AssignMaterial(curtainGo, "Curtain_Rod", matCurtainRod);
            AssignMaterial(curtainGo, "Curtain_Ring", matCurtainRod);
        }

        // ── 7. Guirlande micro-LEDs sur l'étagère ────────────────────
        var fairyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Fairy_Lights.fbx");
        if (fairyPrefab != null)
        {
            var fairyGo = (GameObject)PrefabUtility.InstantiatePrefab(fairyPrefab, decorRoot);
            fairyGo.name = "Guirlande_LED_Etagere";
            fairyGo.transform.localPosition = new Vector3(0.20f, 1.61f, -1.33f);
            fairyGo.transform.localRotation = Quaternion.identity;

            AssignMaterial(fairyGo, "FairyLight_Wire", matMetal);
            AssignMaterial(fairyGo, "FairyLight_Bulb", matFairyBulb);
        }

        // ── 8. Panorama extérieur fenêtre (Skyline crépuscule) ────────
        var skyMat = GetOrCreateMaterial("Room_Sky_Dusk", new Color(0.85f, 0.45f, 0.20f), 0f, 0.1f, 1.2f, new Color(0.95f, 0.55f, 0.25f));
        var skyQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        skyQuad.name = "Panorama_Fenetre";
        skyQuad.transform.SetParent(decorRoot, false);
        skyQuad.transform.localPosition = new Vector3(2.65f, 1.65f, 0.30f);
        skyQuad.transform.localRotation = Quaternion.Euler(0, -90, 0);
        skyQuad.transform.localScale = new Vector3(2.4f, 2.0f, 1f);
        skyQuad.GetComponent<Renderer>().sharedMaterial = skyMat;
        UnityEngine.Object.DestroyImmediate(skyQuad.GetComponent<Collider>());

        // ── 9. Halo lumineux de l'écran (Screen Glow) ────────────────
        var glowGo = new GameObject("Lueur_Ecran_PC");
        glowGo.transform.SetParent(decorRoot, false);
        glowGo.transform.localPosition = new Vector3(0.20f, 1.05f, -1.22f);
        var screenLight = glowGo.AddComponent<Light>();
        screenLight.type = LightType.Point;
        screenLight.range = 1.8f;
        screenLight.intensity = 1.6f;
        screenLight.color = new Color(0.15f, 0.65f, 0.95f);
        screenLight.shadows = LightShadows.None;

        // ── 10. Lampe de bureau chaude 2700K ─────────────────────────
        var deskLampGo = new GameObject("Lumiere_Lampe_Bureau");
        deskLampGo.transform.SetParent(decorRoot, false);
        deskLampGo.transform.localPosition = new Vector3(-0.42f, 1.15f, -1.25f);
        var deskLight = deskLampGo.AddComponent<Light>();
        deskLight.type = LightType.Point;
        deskLight.range = 3.2f;
        deskLight.intensity = 2.2f;
        deskLight.color = new Color(1.0f, 0.72f, 0.30f); // Ambre chaud 2700K
        deskLight.shadows = LightShadows.None;

        // ── 11. Lumière douce guirlande LED ──────────────────────────
        var fairyLightGo = new GameObject("Lumiere_Guirlande_LED");
        fairyLightGo.transform.SetParent(decorRoot, false);
        fairyLightGo.transform.localPosition = new Vector3(0.20f, 1.70f, -1.30f);
        var fairyLight = fairyLightGo.AddComponent<Light>();
        fairyLight.type = LightType.Point;
        fairyLight.range = 2.4f;
        fairyLight.intensity = 1.1f;
        fairyLight.color = new Color(1.0f, 0.85f, 0.45f);
        fairyLight.shadows = LightShadows.None;

        // ── 12. Ambiance Cocooning (Contrôleur dynamique) ────────────
        var atmos = decorRoot.gameObject.AddComponent<CozyRoomAtmosphere>();
        atmos.lampeBureau = deskLight;
        atmos.guirlandeLight = fairyLight;
        atmos.lueurEcranPC = screenLight;
        atmos.lampeBureauIntensity = 2.2f;
        atmos.guirlandeIntensity = 1.1f;
        atmos.guirlandePulse = true;
        atmos.rainEnabled = true;

        // ── 13. Contrôleur d'interface OS sur l'écran ────────────────
        var screenFace = GameObject.Find("Screen_Face");
        if (screenFace != null && screenFace.GetComponent<ScreenOSController>() == null)
        {
            var os = screenFace.AddComponent<ScreenOSController>();
            os.sender = screenFace.GetComponent<PhotoSender>();
        }

        Debug.Log("[ChambreArtBuilder] Chambre Cocooning V2 complète décorée avec succès !");
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
