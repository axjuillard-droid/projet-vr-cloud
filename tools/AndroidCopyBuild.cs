#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Copié uniquement dans le dossier Editor de la copie de compilation ASCII.
public static class AndroidCopyBuild
{
    [Serializable] public class Result
    {
        public string utc, unityVersion, project, output, result;
        public double durationSeconds;
        public ulong bytes;
        public int errors, warnings;
    }

    public static void Run()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            throw new InvalidOperationException("La copie doit démarrer avec -buildTarget Android.");
        var project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        foreach (char c in project)
            if (c > 127) throw new InvalidOperationException("Chemin de compilation non ASCII.");
        var folder = Path.Combine(project, "Builds/Android");
        Directory.CreateDirectory(folder);
        var auditJson = QuestBuildSetupEditor.Audit();
        File.WriteAllText(Path.Combine(folder, "android-audit.json"), auditJson);
        var audit = JsonUtility.FromJson<QuestBuildSetupEditor.AuditResult>(auditJson);
        if (audit.errors.Count != 0) throw new InvalidOperationException("Audit Android bloquant : " + auditJson);
        EditorUserBuildSettings.buildAppBundle = false;
        var output = Path.Combine(folder, "CloudVR-Quest.apk");
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (!scenes.Contains("Assets/Scenes/Chambre.unity") || !scenes.Contains("Assets/Scenes/DataCenter.unity"))
            throw new InvalidOperationException("Chambre et DataCenter doivent être incluses dans les scènes de build.");
        var started = DateTime.UtcNow;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = scenes,
            target = BuildTarget.Android,
            locationPathName = output,
            options = BuildOptions.Development | BuildOptions.CompressWithLz4 | BuildOptions.DetailedBuildReport
        });
        var summary = report.summary;
        var result = new Result {
            utc = DateTime.UtcNow.ToString("o"), unityVersion = Application.unityVersion,
            project = project, output = output, result = summary.result.ToString(),
            durationSeconds = (DateTime.UtcNow - started).TotalSeconds,
            bytes = summary.totalSize, errors = summary.totalErrors, warnings = summary.totalWarnings
        };
        File.WriteAllText(Path.Combine(folder, "build-result.json"), JsonUtility.ToJson(result, true));
        if (summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("Build Android : " + summary.result);
    }
}
#endif
