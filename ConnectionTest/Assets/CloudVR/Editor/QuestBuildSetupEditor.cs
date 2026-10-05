#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

/// <summary>Prépare Android/Quest avec les packages installés ; aucune validation matérielle implicite.</summary>
public static class QuestBuildSetupEditor
{
    [Serializable] public class AuditResult
    {
        public string utc, unityVersion, activeTarget, backend, architecture, minSdk, targetSdk, bundleId, renderMode;
        public bool initializeXR, androidTargetActive;
        public List<string> loaders = new List<string>();
        public List<string> features = new List<string>();
        public List<string> graphics = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    [MenuItem("CloudVR/Build/Préparer Android Quest")]
    public static void Prepare()
    {
        if (Application.isPlaying || BuildPipeline.isBuildingPlayer)
            throw new InvalidOperationException("Arrêter Play et attendre la fin du build.");
        var ids = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
        if (ids.Length != 1) throw new InvalidOperationException("Une configuration XR par plateforme est attendue.");
        var perTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(AssetDatabase.GUIDToAssetPath(ids[0]));
        if (!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        var general = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
        general.InitManagerOnStart = true;
        general.Manager.automaticLoading = true;
        general.Manager.automaticRunning = true;
        if (!XRPackageMetadataStore.AssignLoader(general.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android))
            throw new InvalidOperationException("OpenXRLoader n'a pas été assigné.");

        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        if (settings == null) throw new InvalidOperationException("Settings OpenXR Android absents.");
        EnableFeature<MetaQuestFeature>(settings);
        EnableFeature<OculusTouchControllerProfile>(settings);
        EnableFeature<MetaQuestTouchProControllerProfile>(settings);
        EnableFeature<MetaQuestTouchPlusControllerProfile>(settings);
        settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
        // Les modèles exacts restent à confirmer ; cibles modernes fournies par le package.
        var meta = settings.GetFeature<MetaQuestFeature>();
        var serialized = new SerializedObject(meta);
        var devices = serialized.FindProperty("targetDevices");
        for (int i = 0; i < devices.arraySize; i++)
        {
            var device = devices.GetArrayElementAtIndex(i);
            device.FindPropertyRelative("enabled").boolValue = device.FindPropertyRelative("manifestName").stringValue != "quest";
        }
        serialized.ApplyModifiedProperties();

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.cloudvr.prototype");
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        EditorUtility.SetDirty(general);
        EditorUtility.SetDirty(general.Manager);
        EditorUtility.SetDirty(perTarget);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log("[QuestBuildSetup] Android OpenXR préparé. Build APK et casque encore à tester.");
    }

    static void EnableFeature<T>(OpenXRSettings settings) where T : OpenXRFeature
    {
        var feature = settings.GetFeature<T>();
        if (feature == null) throw new InvalidOperationException("Feature installée manquante : " + typeof(T).Name);
        feature.enabled = true;
        EditorUtility.SetDirty(feature);
    }

    public static string Audit()
    {
        var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        var result = new AuditResult {
            utc = DateTime.UtcNow.ToString("o"), unityVersion = Application.unityVersion,
            activeTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
            androidTargetActive = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android,
            backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android).ToString(),
            architecture = PlayerSettings.Android.targetArchitectures.ToString(),
            minSdk = PlayerSettings.Android.minSdkVersion.ToString(), targetSdk = PlayerSettings.Android.targetSdkVersion.ToString(),
            bundleId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android),
            initializeXR = general != null && general.InitManagerOnStart,
            renderMode = settings == null ? "Absent" : settings.renderMode.ToString()
        };
        result.graphics.AddRange(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(x => x.ToString()));
        if (general != null && general.Manager != null) result.loaders.AddRange(general.Manager.activeLoaders.Select(x => x.name));
        if (settings != null) result.features.AddRange(settings.GetFeatures<OpenXRFeature>().Where(x => x.enabled).Select(x => x.GetType().Name));
        var issues = new List<OpenXRFeature.ValidationRule>();
        var selectedGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        try
        {
            // Le prédicat des profils du package lit le groupe sélectionné, même avec l'argument Android.
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            OpenXRProjectValidation.GetCurrentValidationIssues(issues, BuildTargetGroup.Android);
        }
        finally { EditorUserBuildSettings.selectedBuildTargetGroup = selectedGroup; }
        foreach (var issue in issues) (issue.error ? result.errors : result.warnings).Add(issue.message);
        // Certains prédicats du package lisent la plateforme active : cet audit ne remplace pas le prébuild Android.
        return JsonUtility.ToJson(result, true);
    }
}
#endif
