#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Balayages de caméra reproductibles pour comparer les sols avant/après.
/// Rendu hors Play ; ne mesure ni le confort ni la cadence interactive du player.</summary>
public static class FloorRenderCapture
{
    public static void Capture(string outputFolder)
    {
        if (Application.isPlaying) throw new InvalidOperationException("Arrêter Play avant la capture.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Enregistrer les scènes avant la capture.");
        var setup = EditorSceneManager.GetSceneManagerSetup();
        var hiddenRoots = new List<GameObject>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).path != "Assets/Scenes/DataCenter.unity")
                foreach (var root in SceneManager.GetSceneAt(i).GetRootGameObjects())
                    if (root.activeSelf) hiddenRoots.Add(root);
        var previousTarget = RenderTexture.active;
        var target = new RenderTexture(960, 540, 24);
        var pixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        var cameraObject = new GameObject("Validation_FloorCamera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.nearClipPlane = .05f;
        camera.farClipPlane = 1000;
        camera.fieldOfView = 65;
        camera.targetTexture = target;
        // La caméra de capture survit aux changements de scène, sans être sauvegardée.
        cameraObject.hideFlags = HideFlags.HideAndDontSave;
        Directory.CreateDirectory(outputFolder);
        try
        {
            foreach (var root in hiddenRoots) root.SetActive(false);
            var dc = SceneManager.GetSceneByPath("Assets/Scenes/DataCenter.unity");
            bool opened = !dc.isLoaded;
            if (opened) dc = EditorSceneManager.OpenScene("Assets/Scenes/DataCenter.unity", OpenSceneMode.Additive);
            SceneManager.SetActiveScene(dc);
            CaptureSweep(camera, target, pixels, outputFolder, "cour", new Vector3(0, 1.68f, -10), 35);
            CaptureSweep(camera, target, pixels, outputFolder, "sas", new Vector3(0, 1.68f, -2), 55);
            CaptureSweep(camera, target, pixels, outputFolder, "salle", new Vector3(0, 1.68f, 17), 40);
            CaptureSweep(camera, target, pixels, outputFolder, "rasant", new Vector3(0, 1.68f, 8), 8);
            if (opened) EditorSceneManager.CloseScene(dc, true);
            File.WriteAllText(Path.Combine(outputFolder, "capture.json"),
                "{\"width\":960,\"height\":540,\"framesPerSweep\":31,\"yawStart\":-30,\"yawEnd\":30,\"source\":\"Editor Camera.Render, outside Play; no frame-rate measurement\"}");
        }
        finally
        {
            foreach (var root in hiddenRoots) if (root != null) root.SetActive(true);
            RenderTexture.active = previousTarget;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(pixels);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }

    static void CaptureSweep(Camera camera, RenderTexture target, Texture2D pixels, string folder,
        string name, Vector3 position, float pitch)
    {
        var sweepFolder = Path.Combine(folder, name);
        Directory.CreateDirectory(sweepFolder);
        for (int i = 0; i < 31; i++)
        {
            camera.transform.SetPositionAndRotation(position, Quaternion.Euler(pitch, -30 + i * 2, 0));
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(Path.Combine(sweepFolder, i.ToString("00") + ".png"), pixels.EncodeToPNG());
        }
    }
}
#endif
