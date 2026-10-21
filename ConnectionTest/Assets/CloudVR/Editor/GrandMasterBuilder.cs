#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Constructeur Maître Global :
/// Décore et assemble de manière 100% automatisée et reproductible :
/// 1. La Chambre Cocooning (Box Internet, rideaux, guirlande LED, ambiance ambrée 2700K, pluie, vapeur café)
/// 2. La Transition Réseau
/// 3. Le Data Center Monumental Tier IV (Campus extérieur, Sas Mantrap cylindrique biométrique, Grande Salle Blanche 40x18m, Local TGBT/UPS, Serveur rack 2U extractible)
/// 4. Le parcours pédagogique guidé (6 bornes XR interactives)
/// </summary>
public static class GrandMasterBuilder
{
    private const string ChambrePath = "Assets/Scenes/Chambre.unity";
    private const string DataCenterPath = "Assets/Scenes/DataCenter.unity";

    [MenuItem("CloudVR/Build Scene/★ DÉCORATION MONUMENTALE COMPLÈTE ★")]
    public static void BuildAll()
    {
        Debug.Log("[GrandMasterBuilder] === DÉBUT DE LA CONSTRUCTION MONUMENTALE ===");

        // ── 1. Chambre étudiante Cocooning ────────────────────────────
        var roomScene = EditorSceneManager.OpenScene(ChambrePath);
        var roomRoot = GameObject.Find("Chambre_Root");
        if (roomRoot != null)
        {
            ChambreArtBuilder.Decorate(roomRoot.transform);
        }
        else
        {
            Debug.LogWarning("[GrandMasterBuilder] Chambre_Root absent, construction des primitives...");
            ChambreSceneBuilder.BuildChambre();
            roomRoot = GameObject.Find("Chambre_Root");
            if (roomRoot != null)
            {
                ChambreArtBuilder.Decorate(roomRoot.transform);
            }
        }
        EditorSceneManager.MarkSceneDirty(roomScene);
        EditorSceneManager.SaveScene(roomScene);
        Debug.Log("[GrandMasterBuilder] [OK] Chambre Cocooning configurée et enregistrée.");

        // ── 2. Data Center Monumental Tier IV ─────────────────────────
        var dcScene = EditorSceneManager.OpenScene(DataCenterPath);
        var dcRoot = GameObject.Find("DataCenter_Root");
        if (dcRoot == null)
        {
            Debug.LogWarning("[GrandMasterBuilder] DataCenter_Root absent, construction des primitives...");
            DataCenterSceneBuilder.Build();
            dcRoot = GameObject.Find("DataCenter_Root");
        }

        if (dcRoot != null)
        {
            // Décorer avec le Campus extérieur, Sas Mantrap, Grande Salle Blanche, Local Électrique
            DatacenterMonumentalBuilder.DecorateDataCenter(dcRoot.transform);

            // Installer le serveur extractible dans Baie_01
            try
            {
                ServerBladeBuilder.InstallBladeInRack();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GrandMasterBuilder] Blade installer notice: {ex.Message}");
            }
        }

        EditorSceneManager.MarkSceneDirty(dcScene);
        EditorSceneManager.SaveScene(dcScene);
        Debug.Log("[GrandMasterBuilder] [OK] Data Center Monumental configuré et enregistré.");

        // ── 3. Démo guidée complète (bornes + flux) ───────────────────
        DemoTourBuilder.Build();
        Debug.Log("[GrandMasterBuilder] [OK] Démo guidée intégrée.");

        AssetDatabase.SaveAssets();
        Debug.Log("[GrandMasterBuilder] === TOUTE LA SCÉNOGRAPHIE MONUMENTALE EST PRÊTE ET SAUVEGARDÉE ===");
    }
}
#endif
