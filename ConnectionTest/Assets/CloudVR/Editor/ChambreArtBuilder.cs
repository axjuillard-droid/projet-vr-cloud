#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Décorateur artistique de la Chambre étudiante :
/// Instancie le mobilier modélisé (lit, bureau gamer, chaise, étagère, livres, tapis)
/// et applique les matériaux URP chaleureux.
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

    public static void Decorate(Transform roomRoot)
    {
        var decorRoot = new GameObject("Mobilier_Et_Ambiance").transform;
        decorRoot.SetParent(roomRoot, false);

        // ── 1. Matériaux ──────────────────────────────────────────────
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

        // ── 2. Lit étudiant ──────────────────────────────────────────
        var bedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Bed_Student.fbx");
        if (bedPrefab != null)
        {
            var bed = (GameObject)PrefabUtility.InstantiatePrefab(bedPrefab, decorRoot);
            bed.name = "Lit_Etudiant";
            // Placé le long du mur gauche, orienté dans l'axe de la chambre
            bed.transform.localPosition = new Vector3(-1.95f, 0f, 0.50f);
            bed.transform.localRotation = Quaternion.Euler(0, 0, 0);

            AssignMaterial(bed, "Bed_Wood", matWood);
            AssignMaterial(bed, "Bed_Metal", matMetal);
            AssignMaterial(bed, "Bed_Sheet", matSheet);
            AssignMaterial(bed, "Bed_Duvet", matDuvet);
            AssignMaterial(bed, "Bed_Pillow", matPillow);

            // Collider pour ne pas traverser le lit
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

            // Collider pour le bureau
            var deskCol = desk.AddComponent<BoxCollider>();
            deskCol.center = new Vector3(0, 0.38f, 0);
            deskCol.size = new Vector3(1.65f, 0.76f, 0.85f);

            // Collider pour la chaise
            // Collider pour la chaise + interaction pour s'asseoir
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
            
            // Poster tech avec visuel bleu profond émissif
            var matPosterTech = GetOrCreateMaterial("Room_Poster_Tech", new Color(0.08f, 0.22f, 0.45f), 0.1f, 0.6f, 0.5f, new Color(0.1f, 0.5f, 0.9f));
            AssignMaterial(decor, "Poster_Fond", matPosterTech);
        }

        // ── 5. Vue extérieure par la fenêtre (Skyline crépuscule) ─────
        var skyMat = GetOrCreateMaterial("Room_Sky_Dusk", new Color(0.85f, 0.45f, 0.20f), 0f, 0.1f, 1.2f, new Color(0.95f, 0.55f, 0.25f));
        var skyQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        skyQuad.name = "Panorama_Fenetre";
        skyQuad.transform.SetParent(decorRoot, false);
        skyQuad.transform.localPosition = new Vector3(2.65f, 1.65f, 0.30f);
        skyQuad.transform.localRotation = Quaternion.Euler(0, -90, 0);
        skyQuad.transform.localScale = new Vector3(2.4f, 2.0f, 1f);
        skyQuad.GetComponent<Renderer>().sharedMaterial = skyMat;
        UnityEngine.Object.DestroyImmediate(skyQuad.GetComponent<Collider>());

        // ── 6. Halo lumineux de l'écran (Screen Glow) ────────────────
        var glowGo = new GameObject("Lueur_Ecran_PC");
        glowGo.transform.SetParent(decorRoot, false);
        glowGo.transform.localPosition = new Vector3(0.20f, 1.05f, -1.22f);
        var screenLight = glowGo.AddComponent<Light>();
        screenLight.type = LightType.Point;
        screenLight.range = 1.8f;
        screenLight.intensity = 1.6f;
        screenLight.color = new Color(0.15f, 0.65f, 0.95f); // lueur cyan douce
        screenLight.shadows = LightShadows.None;

        // ── 7. Contrôleur d'interface OS sur l'écran ─────────────────
        var screenFace = GameObject.Find("Screen_Face");
        if (screenFace != null && screenFace.GetComponent<ScreenOSController>() == null)
        {
            var os = screenFace.AddComponent<ScreenOSController>();
            os.sender = screenFace.GetComponent<PhotoSender>();
        }

        Debug.Log("[ChambreArtBuilder] Décoration de la chambre appliquée avec succès !");
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
