using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Helper copié uniquement dans le projet batch isolé.</summary>
public static class DemoCopyBuild
{
    [Serializable] class Report { public string utc,result,unity; public int errors,warnings; public double seconds; public ulong bytes; }
    public static void Run()
    {
        Directory.CreateDirectory("Builds/Demo");
        try
        {
            GrandMasterBuilder.BuildAll();
            P0SpatialValidation.Validate("Builds/Demo/p0-layout-audit.json");
            P1LearningValidation.Validate("Builds/Demo/p1-learning-audit.json");
            NetworkRideValidation.Validate("Builds/Demo/p2-network-audit.json");
            FacilityOperationsValidation.Validate("Builds/Demo/p3-facility-audit.json");
            PlayerSettings.runInBackground=true;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Scenes/Chambre.unity","Assets/Scenes/DataCenter.unity"}, locationPathName="Builds/Demo/CloudVR-PC.exe",
                target=BuildTarget.StandaloneWindows64, options=BuildOptions.Development | BuildOptions.CompressWithLz4 | BuildOptions.DetailedBuildReport | BuildOptions.ForceOptimizeScriptCompilation });
            var summary=report.summary;
            var result=new Report{utc=DateTime.UtcNow.ToString("o"),result=summary.result.ToString(),unity=Application.unityVersion,errors=(int)summary.totalErrors,warnings=(int)summary.totalWarnings,seconds=summary.totalTime.TotalSeconds,bytes=summary.totalSize};
            File.WriteAllText("Builds/Demo/build-result.json",JsonUtility.ToJson(result,true));
            if(summary.result!=BuildResult.Succeeded) EditorApplication.Exit(1);
        }
        catch(Exception e) { File.WriteAllText("Builds/Demo/build-error.txt",e.ToString()); Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
