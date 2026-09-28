#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Script éditeur : construit la scène Chambre avec des primitives Unity.
/// Menu : CloudVR → Build Scene → Chambre
/// Aucun asset externe requis. Supprime et recrée les objets à chaque appel.
/// </summary>
public static class ChambreSceneBuilder
{
    private const string ROOT_NAME = "Chambre_Root";

    [MenuItem("CloudVR/Build Scene/Chambre (primitives)")]
    public static void BuildChambre()
    {
        // ── 0. Nettoyage ──────────────────────────────────────────────
        var existing = GameObject.Find(ROOT_NAME);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
            Debug.Log("[ChambreBuilder] Ancien Chambre_Root supprimé.");
        }

        var root = new GameObject(ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(root, "Build Chambre Scene");

        // ── 1. Pièce (sol + murs + plafond) ──────────────────────────
        BuildRoom(root.transform);

        // ── 2. Mobilier (bureau + PC) ─────────────────────────────────
        BuildDesk(root.transform);

        // ── 3. Écran PC interactif ─────────────────────────────────────
        BuildScreen(root.transform);

        // ── 4. Fenêtre (lumière naturelle) ────────────────────────────
        BuildWindow(root.transform);

        // ── 5. Éclairage intérieur ────────────────────────────────────
        BuildLight(root.transform);

        Debug.Log("[ChambreBuilder] Scène Chambre construite avec succès.");
        EditorUtility.SetDirty(root);
    }

    // ══════════════════════════════════════════════════════════════════
    // Pièce 5m x 3m x 4m
    // ══════════════════════════════════════════════════════════════════
    static void BuildRoom(Transform parent)
    {
        var roomParent = new GameObject("Room").transform;
        roomParent.SetParent(parent, false);

        // Sol
        CreatePlane(roomParent, "Sol",
            pos: new Vector3(0, 0, 0),
            scale: new Vector3(5f, 1f, 4f),
            color: new Color(0.65f, 0.55f, 0.45f));   // parquet chaud

        // Plafond
        CreatePlane(roomParent, "Plafond",
            pos: new Vector3(0, 3f, 0),
            scale: new Vector3(5f, 1f, 4f),
            color: new Color(0.95f, 0.95f, 0.93f),
            flipY: true);

        // Mur arrière (fond)
        CreateWall(roomParent, "Mur_Fond",
            pos: new Vector3(0, 1.5f, -2f),
            scale: new Vector3(5f, 3f, 0.1f),
            color: new Color(0.88f, 0.85f, 0.80f));

        // Mur avant (entrée)
        CreateWall(roomParent, "Mur_Entree",
            pos: new Vector3(0, 1.5f, 2f),
            scale: new Vector3(5f, 3f, 0.1f),
            color: new Color(0.88f, 0.85f, 0.80f));

        // Mur gauche
        CreateWall(roomParent, "Mur_Gauche",
            pos: new Vector3(-2.5f, 1.5f, 0),
            scale: new Vector3(0.1f, 3f, 4f),
            color: new Color(0.85f, 0.83f, 0.78f));

        // Mur droit (avec fenêtre — laissé ouvert visuellement)
        CreateWall(roomParent, "Mur_Droit",
            pos: new Vector3(2.5f, 1.5f, 0),
            scale: new Vector3(0.1f, 3f, 4f),
            color: new Color(0.85f, 0.83f, 0.78f));
    }

    // ══════════════════════════════════════════════════════════════════
    // Bureau + chaise + tour PC
    // ══════════════════════════════════════════════════════════════════
    static void BuildDesk(Transform parent)
    {
        var deskParent = new GameObject("Desk").transform;
        deskParent.SetParent(parent, false);

        // Plateau du bureau
        CreateBox(deskParent, "Bureau_Plateau",
            pos: new Vector3(0, 0.75f, -1.4f),
            scale: new Vector3(1.6f, 0.06f, 0.8f),
            color: new Color(0.45f, 0.30f, 0.15f));

        // Pieds (x4 simplifiés en 2 panneaux)
        CreateBox(deskParent, "Bureau_Pied_G",
            pos: new Vector3(-0.75f, 0.37f, -1.4f),
            scale: new Vector3(0.06f, 0.75f, 0.6f),
            color: new Color(0.40f, 0.27f, 0.13f));
        CreateBox(deskParent, "Bureau_Pied_D",
            pos: new Vector3(0.75f, 0.37f, -1.4f),
            scale: new Vector3(0.06f, 0.75f, 0.6f),
            color: new Color(0.40f, 0.27f, 0.13f));

        // Chaise (assise + dossier)
        CreateBox(deskParent, "Chaise_Assise",
            pos: new Vector3(0, 0.5f, -0.8f),
            scale: new Vector3(0.5f, 0.05f, 0.5f),
            color: new Color(0.20f, 0.20f, 0.22f));
        CreateBox(deskParent, "Chaise_Dossier",
            pos: new Vector3(0, 0.85f, -0.58f),
            scale: new Vector3(0.5f, 0.65f, 0.05f),
            color: new Color(0.20f, 0.20f, 0.22f));

        // Tour PC (sous le bureau, à gauche)
        CreateBox(deskParent, "PC_Tour",
            pos: new Vector3(-0.55f, 0.2f, -1.65f),
            scale: new Vector3(0.18f, 0.40f, 0.40f),
            color: new Color(0.12f, 0.12f, 0.14f));
    }

    // ══════════════════════════════════════════════════════════════════
    // Écran PC — objet interactif principal de l'étape 1
    // ══════════════════════════════════════════════════════════════════
    static void BuildScreen(Transform parent)
    {
        var screenParent = new GameObject("PC_Screen_Interactive").transform;
        screenParent.SetParent(parent, false);

        // Pied de l'écran
        CreateBox(screenParent, "Screen_Pied",
            pos: new Vector3(0.2f, 0.79f, -1.4f),
            scale: new Vector3(0.04f, 0.08f, 0.15f),
            color: new Color(0.15f, 0.15f, 0.15f));

        // Cadre écran
        var screenGo = CreateBox(screenParent, "Screen_Face",
            pos: new Vector3(0.2f, 1.05f, -1.4f),
            scale: new Vector3(0.64f, 0.40f, 0.03f),
            color: new Color(0.08f, 0.08f, 0.10f));

        // Surface affichée (légèrement en avant du cadre, couleur "veille")
        var displayGo = CreateBox(screenParent, "Screen_Display",
            pos: new Vector3(0.2f, 1.05f, -1.37f),
            scale: new Vector3(0.60f, 0.36f, 0.005f),
            color: new Color(0.05f, 0.10f, 0.30f),   // bleu nuit au repos
            emissive: true);

        // Tag pour que PhotoSender puisse le trouver
        screenGo.tag = "Untagged";
        screenGo.name = "Screen_Face";

        Debug.Log("[ChambreBuilder] Écran PC créé. Attacher PhotoSender à PC_Screen_Interactive dans l'inspecteur.");
    }

    // ══════════════════════════════════════════════════════════════════
    // Fenêtre (ouverture lumineuse dans le mur droit)
    // ══════════════════════════════════════════════════════════════════
    static void BuildWindow(Transform parent)
    {
        // Cadre fenêtre (4 barres autour du vide)
        var winParent = new GameObject("Fenetre").transform;
        winParent.SetParent(parent, false);

        // Haut et bas
        CreateBox(winParent, "Win_Haut",
            pos: new Vector3(2.45f, 2.1f, 0.3f),
            scale: new Vector3(0.1f, 0.06f, 0.9f),
            color: new Color(0.90f, 0.88f, 0.84f));
        CreateBox(winParent, "Win_Bas",
            pos: new Vector3(2.45f, 1.2f, 0.3f),
            scale: new Vector3(0.1f, 0.06f, 0.9f),
            color: new Color(0.90f, 0.88f, 0.84f));
        // Côtés
        CreateBox(winParent, "Win_Gauche",
            pos: new Vector3(2.45f, 1.65f, -0.15f),
            scale: new Vector3(0.1f, 0.9f, 0.06f),
            color: new Color(0.90f, 0.88f, 0.84f));
        CreateBox(winParent, "Win_Droit",
            pos: new Vector3(2.45f, 1.65f, 0.75f),
            scale: new Vector3(0.1f, 0.9f, 0.06f),
            color: new Color(0.90f, 0.88f, 0.84f));

        // Vitre (semi-transparente si URP le permet)
        var vitre = CreateBox(winParent, "Vitre",
            pos: new Vector3(2.48f, 1.65f, 0.30f),
            scale: new Vector3(0.02f, 0.84f, 0.84f),
            color: new Color(0.7f, 0.85f, 1.0f, 0.3f),
            transparent: true);
    }

    // ══════════════════════════════════════════════════════════════════
    // Éclairage : point light au plafond
    // ══════════════════════════════════════════════════════════════════
    static void BuildLight(Transform parent)
    {
        var lightGo = new GameObject("Lampe_Plafond");
        lightGo.transform.SetParent(parent, false);
        lightGo.transform.localPosition = new Vector3(0, 2.9f, -0.5f);

        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 6f;
        light.intensity = 1.5f;
        light.color = new Color(1.0f, 0.96f, 0.88f);  // lumière chaude
        light.shadows = LightShadows.Soft;
    }

    // ══════════════════════════════════════════════════════════════════
    // Utilitaires de création de primitives
    // ══════════════════════════════════════════════════════════════════

    static GameObject CreateBox(Transform parent, string name, Vector3 pos, Vector3 scale,
        Color color, bool emissive = false, bool transparent = false)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;

        var mat = new Material(GetURPLit());
        mat.name = name + "_Mat";
        mat.color = color;

        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.5f);
        }

        if (transparent)
        {
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject CreatePlane(Transform parent, string name, Vector3 pos, Vector3 scale,
        Color color, bool flipY = false)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale / 10f;  // Plane natif = 10 unités

        if (flipY)
            go.transform.localRotation = Quaternion.Euler(180, 0, 0);

        var mat = new Material(GetURPLit());
        mat.name = name + "_Mat";
        mat.color = color;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        => CreateBox(parent, name, pos, scale, color);

    static Shader GetURPLit()
    {
        var s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }
}
#endif
