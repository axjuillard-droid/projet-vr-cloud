#if UNITY_EDITOR
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

/// <summary>Retire les déclarations eye tracking ajoutées par OpenXR 1.14 au prototype sans regard.</summary>
public sealed class QuestManifestSanitizer : IPostGenerateGradleAndroidProject
{
    public int callbackOrder { get { return 1000; } }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        if (settings == null) return;
        var meta = settings.GetFeature<MetaQuestFeature>();
        var gaze = settings.GetFeature<EyeGazeInteraction>();
        if (meta == null || !meta.enabled || (gaze != null && gaze.enabled)) return;

        int removed = 0;
        foreach (var relativePath in new[] { "src/main/AndroidManifest.xml", "xrmanifest.androidlib/AndroidManifest.xml" })
        {
            var manifestPath = Path.Combine(path, relativePath);
            if (!File.Exists(manifestPath)) continue;
            var document = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
            document.Load(manifestPath);
            var nodes = document.SelectNodes("/manifest/uses-feature | /manifest/uses-permission");
            int changed = 0;
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = (XmlElement)nodes[i];
                var name = node.GetAttribute("name", "http://schemas.android.com/apk/res/android");
                if ((node.Name == "uses-feature" && name == "oculus.software.eye_tracking") ||
                    (node.Name == "uses-permission" && name == "com.oculus.permission.EYE_TRACKING"))
                {
                    node.ParentNode.RemoveChild(node);
                    changed++;
                }
            }
            if (changed != 0) document.Save(manifestPath);
            removed += changed;
        }
        Debug.Log("[CloudVR Manifest] Eye Gaze désactivé : " + removed + " déclarations eye tracking retirées.");
    }
}
#endif
