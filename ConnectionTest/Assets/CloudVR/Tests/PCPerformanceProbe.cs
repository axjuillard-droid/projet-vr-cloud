#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>Mesure opt-in du player de développement. Captures séparées des phases chronométrées.</summary>
public class PCPerformanceProbe : MonoBehaviour
{
    [Serializable] public class Distribution
    {
        public int count;
        public double medianMs, p95Ms, meanMs;
    }
    [Serializable] public class Phase
    {
        public string mode;
        public int order, msaa, frames, timingSamples, gpuSamples, focusedFrames;
        public double seconds;
        public bool focusedThroughout, scaleUnchanged;
        public Distribution observedFrame, cpuFrame, cpuMain, cpuRender, presentWait, gpu;
    }
    [Serializable] public class Report
    {
        public bool passed, frameTimingEnabled, gpuAvailable, developmentBuild;
        public string utc, error, unity, graphics, gpu, cpu, os, protocol;
        public int width, height, gpuMemoryMB, systemMemoryMB, cpuThreads, vSync, targetFrameRate;
        public List<Phase> phases = new List<Phase>();
        public int sweepFrames;
    }
    struct Sample
    {
        public int frame;
        public double elapsed, observedMs;
        public bool fresh;
        public FrameTiming timing;
    }
    readonly Report _report = new Report();
    readonly FrameTiming[] _timings = new FrameTiming[1];
    string _folder;
    Camera _camera;
    UniversalRenderPipelineAsset _originalPipeline, _pipeline;
    int _vSync, _targetFrameRate;
    bool _runInBackground, _configured;
    Action _restoreAudit;
    readonly CultureInfo _culture = CultureInfo.InvariantCulture;

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "--cloudvr-performance") < 0) return;
        int output = Array.IndexOf(args, "--cloudvr-performance-output");
        if (output < 0 || output + 1 >= args.Length || !Path.IsPathRooted(args[output + 1]))
        {
            Debug.LogError("Fournir --cloudvr-performance-output avec un dossier absolu.");
            Application.Quit(2);
            return;
        }
        var probe = new GameObject("PCPerformanceProbe_TEMP").AddComponent<PCPerformanceProbe>();
        DontDestroyOnLoad(probe.gameObject);
        probe._folder = args[output + 1];
        probe.StartCoroutine(probe.Guard());
    }
#endif
    IEnumerator Guard()
    {
        var exercise = Exercise();
        while (true)
        {
            object current;
            try { if (!exercise.MoveNext()) break; current = exercise.Current; }
            catch (Exception e) { Finish(false, e.ToString()); yield break; }
            yield return current;
        }
        Finish(true, null);
    }
    IEnumerator Exercise()
    {
        Directory.CreateDirectory(_folder);
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) throw new InvalidOperationException("GPU requis.");
        _vSync = QualitySettings.vSyncCount; _targetFrameRate = Application.targetFrameRate; _runInBackground = Application.runInBackground;
        _originalPipeline = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (_originalPipeline == null) throw new InvalidOperationException("Profil URP de qualité requis.");
        _pipeline = Instantiate(_originalPipeline);
        QualitySettings.renderPipeline = _pipeline; QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; Application.runInBackground = true; _configured = true;
        InputModeManager.Instance.SetMode(InputModeManager.InputMode.PC); InputModeManager.Instance.PrepareNextScene();
        yield return SceneManager.LoadSceneAsync("DataCenter");
        yield return null;
        _camera = Camera.main;
        var controller = _camera.GetComponentInParent<PCPlayerController>(); controller.enabled = false;
        controller.GetComponent<CharacterController>().enabled = false;
        controller.transform.SetPositionAndRotation(new Vector3(0,.05f,8),Quaternion.identity);
        _camera.transform.position = new Vector3(0,1.68f,8);
        Cursor.lockState = CursorLockMode.None; Cursor.visible = false;
        _report.utc = DateTime.UtcNow.ToString("o"); _report.unity = Application.unityVersion; _report.developmentBuild = Debug.isDebugBuild;
        _report.graphics = SystemInfo.graphicsDeviceType.ToString(); _report.gpu = SystemInfo.graphicsDeviceName; _report.cpu = SystemInfo.processorType;
        _report.os = SystemInfo.operatingSystem; _report.cpuThreads = SystemInfo.processorCount; _report.gpuMemoryMB = SystemInfo.graphicsMemorySize; _report.systemMemoryMB = SystemInfo.systemMemorySize;
        _report.width = Screen.width; _report.height = Screen.height; _report.vSync = QualitySettings.vSyncCount; _report.targetFrameRate = Application.targetFrameRate;
        _report.frameTimingEnabled = FrameTimingManager.IsFeatureEnabled();
        _report.protocol = "Development Windows D3D11, windowed 1280x720, uncapped requested, no deep profiler; ABBA 1x/4x/4x/1x, warm-up 2 s and measurement 8 s per phase. Same camera: (0,1.68,8), FOV 75, pitch -8, yaw -12..12 at 3 degrees/s. Scene idle, player controller disabled. Captures only after measurements. Background apps not controlled; no Quest inference.";
        if (_report.width != 1280 || _report.height != 720 || Mathf.Abs(_camera.fieldOfView-75)>.01f) throw new InvalidOperationException("Résolution ou FOV inattendu.");
        bool audit = Array.IndexOf(Environment.GetCommandLineArgs(), "--cloudvr-pass-audit") >= 0;
        bool shadows = Array.IndexOf(Environment.GetCommandLineArgs(), "--cloudvr-shadow-comparison") >= 0;
        bool ao = Array.IndexOf(Environment.GetCommandLineArgs(), "--cloudvr-ao-comparison") >= 0;
        if ((audit && (shadows || ao)) || (shadows && ao))
            throw new InvalidOperationException("Choisir un seul protocole.");
        string[] modes = audit
            ? new[] {"reference","ao-half","no-ao","no-shadows","no-copies","no-glass","short-frustum","reference"}
            : shadows ? new[] {"four-cascades","two-cascades","two-cascades","four-cascades"}
            : ao ? new[] {"ao-on","ao-off","ao-off","ao-on"} : new[] {"1x","4x","4x","1x"};
        Action<string> setMode = audit ? PrepareAudit() : shadows
            ? mode => _pipeline.shadowCascadeCount = mode == "four-cascades" ? 4 : 2
            : ao ? PrepareAoComparison() : mode => _pipeline.msaaSampleCount = mode == "1x" ? 1 : 4;
        if (audit)
            _report.protocol = "PC 4x throughout, isolated interventions bracketed by reference; 2 s warm-up + 8 s moving per phase; same pose and yaw sweep as T24. Deltas include interactions between URP passes, not isolated GPU marker timings.";
        if (shadows || ao)
        {
            if (_pipeline.msaaSampleCount != 4)
                throw new InvalidOperationException("Comparaison attendue : profil PC avec MSAA 4x.");
            string variants = shadows ? "four/two/two/four cascades" : "SSAO on/off/off/on";
            _report.protocol = "PC MSAA 4x throughout; ABBA " + variants
                + ", same resolution, pose and yaw as T24. 6 s initial warm-up, then 2 s warm-up + 8 s moving per phase. Captures after timing.";
            setMode(modes[0]);
            double initial = Time.realtimeSinceStartupAsDouble;
            while (Time.realtimeSinceStartupAsDouble - initial < 6)
            {
                _camera.transform.rotation = Quaternion.Euler(-8, -12, 0);
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
            }
        }
        for (int i=0;i<modes.Length;i++)
        {
            setMode(modes[i]);
            double warm = Time.realtimeSinceStartupAsDouble;
            while (Time.realtimeSinceStartupAsDouble-warm<2) { _camera.transform.rotation=Quaternion.Euler(-8,-12,0); FrameTimingManager.CaptureFrameTimings(); yield return null; }
            var samples = new List<Sample>(8192); var phase = new Phase {order=i,mode=modes[i],msaa=_pipeline.msaaSampleCount,focusedThroughout=true,scaleUnchanged=true};
            double start = Time.realtimeSinceStartupAsDouble, previous = start; ulong last=0;
            while (Time.realtimeSinceStartupAsDouble-start<8)
            {
                double elapsed=Time.realtimeSinceStartupAsDouble-start;
                _camera.transform.rotation=Quaternion.Euler(-8,-12+(float)elapsed*3,0);
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
                double now=Time.realtimeSinceStartupAsDouble;
                var sample=new Sample {frame=Time.frameCount,elapsed=now-start,observedMs=(now-previous)*1000}; previous=now;
                if(FrameTimingManager.GetLatestTimings(1,_timings)>0&&_timings[0].frameStartTimestamp!=0&&_timings[0].frameStartTimestamp!=last)
                {sample.fresh=true;sample.timing=_timings[0];last=sample.timing.frameStartTimestamp;}
                if(samples.Count<5)sample.fresh=false; // Écarter le retard des timings issus du warm-up.
                samples.Add(sample); if(Application.isFocused)phase.focusedFrames++;phase.focusedThroughout&=Application.isFocused;phase.scaleUnchanged&=Mathf.Approximately(Time.timeScale,1);
            }
            phase.seconds=Time.realtimeSinceStartupAsDouble-start;phase.frames=samples.Count;
            Summarize(phase,samples);_report.phases.Add(phase);WriteSamples(i,samples);
            if(samples.Count<30||!phase.scaleUnchanged)throw new InvalidOperationException("Phase insuffisante ou temps de jeu modifié.");
        }
        _report.gpuAvailable=_report.phases.TrueForAll(p=>p.gpuSamples>0);
        if (shadows || ao)
        {
            foreach (string mode in new[] {modes[0], modes[1]})
            {
                setMode(mode);
                _camera.transform.rotation = Quaternion.Euler(-8, 0, 0);
                yield return new WaitForEndOfFrame();
                CaptureAuditView(shadows ? "shadow-" + mode + ".png" : mode + ".png");
            }
            yield break;
        }
        if (audit)
        {
            // Les vues de comparaison viennent après toutes les fenêtres chronométrées.
            for (int i = 0; i < modes.Length - 1; i++)
            {
                setMode(modes[i]);
                _camera.transform.rotation = Quaternion.Euler(-8, 0, 0);
                yield return new WaitForEndOfFrame();
                CaptureAuditView("audit-" + modes[i] + ".png");
            }
            // Même FOV et orientation, observation du tableau à trois distances.
            setMode("reference");
            _camera.transform.rotation = Quaternion.identity;
            foreach (int z in new[] {8, 25, 36})
            {
                _camera.transform.position = new Vector3(0, 1.68f, z);
                yield return new WaitForEndOfFrame();
                CaptureAuditView("supervision-z" + z + ".png");
            }
            yield break;
        }
        // Revue séparée : images hors écran à angles imposés, aucune capture complète Overlay.
        foreach(int msaa in new[]{1,4})
        {
            _pipeline.msaaSampleCount=msaa;yield return null;
            var folder=Path.Combine(_folder,"sweep-"+msaa+"x");Directory.CreateDirectory(folder);
            var target=RenderTexture.GetTemporary(1280,720,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,msaa);
            var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);var previous=_camera.targetTexture;var active=RenderTexture.active;
            try
            {
                for(int frame=0;frame<25;frame++)
                {
                    _camera.transform.rotation=Quaternion.Euler(-8,-3+frame*.25f,0);
                    yield return new WaitForEndOfFrame();
                    _camera.targetTexture=target;_camera.Render();RenderTexture.active=target;
                    pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
                    File.WriteAllBytes(Path.Combine(folder,frame.ToString("00")+".png"),pixels.EncodeToPNG());_report.sweepFrames++;
                    _camera.targetTexture=previous;RenderTexture.active=active;
                }
            }
            finally{_camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);Destroy(pixels);}
        }
    }
    ScriptableRendererFeature GetActiveAo()
    {
        var features = _pipeline.rendererDataList[0].rendererFeatures.FindAll(f => f is ScreenSpaceAmbientOcclusion);
        if (features.Count != 1 || !features[0].isActive || _pipeline.msaaSampleCount != 4)
            throw new InvalidOperationException("Reconstruire le profil PC de référence avec MSAA 4x et un SSAO actif.");
        return features[0];
    }

    Action<string> PrepareAoComparison()
    {
        var ao = GetActiveAo();
        _restoreAudit = () => ao.SetActive(true);
        return mode => ao.SetActive(mode == "ao-on");
    }

    Action<string> PrepareAudit()
    {
        var ao = GetActiveAo();
        // URP 17 ne publie pas le réglage Downsample : utilisé uniquement dans le probe opt-in.
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var settings = ao.GetType().GetField("m_Settings", fields)?.GetValue(ao);
        var downsample = settings?.GetType().GetField("Downsample", fields);
        if (downsample == null || downsample.FieldType != typeof(bool))
            throw new InvalidOperationException("Réglage SSAO Downsample introuvable.");
        bool originalDownsample = (bool)downsample.GetValue(settings);
        var data = _camera.GetUniversalAdditionalCameraData();
        bool shadows = data.renderShadows;
        var color = data.requiresColorOption;
        var depth = data.requiresDepthOption;
        float far = _camera.farClipPlane;
        var glass = new List<Renderer>();
        foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!renderer.enabled) continue;
            foreach (var material in renderer.sharedMaterials)
                if (material && material.name == "WS_CAC_Glass") { glass.Add(renderer); break; }
        }
        if (glass.Count == 0 || !shadows || !data.requiresColorTexture || !data.requiresDepthTexture || far <= 26)
            throw new InvalidOperationException("Référence incomplète pour les variantes de rendu.");
        _restoreAudit = () =>
        {
            ao.SetActive(true);
            downsample.SetValue(settings, originalDownsample);
            data.renderShadows = shadows;
            data.requiresColorOption = color;
            data.requiresDepthOption = depth;
            _camera.farClipPlane = far;
            foreach (var renderer in glass) renderer.enabled = true;
        };
        return mode =>
        {
            _restoreAudit();
            switch (mode)
            {
                case "ao-half": downsample.SetValue(settings, true); break;
                case "no-ao": ao.SetActive(false); break;
                case "no-shadows": data.renderShadows = false; break;
                case "no-copies":
                    data.requiresColorOption = CameraOverrideOption.Off;
                    data.requiresDepthOption = CameraOverrideOption.Off;
                    break;
                case "no-glass": foreach (var renderer in glass) renderer.enabled = false; break;
                case "short-frustum": _camera.farClipPlane = 26; break;
            }
        };
    }
    void CaptureAuditView(string filename)
    {
        var previous = _camera.targetTexture;
        var active = RenderTexture.active;
        var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 4);
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            _camera.targetTexture = target;
            _camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(Path.Combine(_folder, filename), pixels.EncodeToPNG());
        }
        finally
        {
            _camera.targetTexture = previous;
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(target);
            Destroy(pixels);
        }
    }
    static Distribution Stats(List<double> values)
    {
        if(values.Count==0)return new Distribution();values.Sort();double sum=0;foreach(double value in values)sum+=value;
        int count=values.Count;return new Distribution {count=count,medianMs=count%2==0?(values[count/2-1]+values[count/2])/2:values[count/2],p95Ms=values[Math.Max(0,(int)Math.Ceiling(count*.95)-1)],meanMs=sum/count};
    }
    void Summarize(Phase phase,List<Sample> samples)
    {
        var observed=new List<double>();var cpu=new List<double>();var main=new List<double>();var render=new List<double>();var wait=new List<double>();var gpu=new List<double>();
        foreach(var sample in samples)
        {
            observed.Add(sample.observedMs);if(!sample.fresh)continue;phase.timingSamples++;
            var timing=sample.timing;
            if(timing.cpuFrameTime>0)cpu.Add(timing.cpuFrameTime);if(timing.cpuMainThreadFrameTime>0)main.Add(timing.cpuMainThreadFrameTime);
            if(timing.cpuRenderThreadFrameTime>0)render.Add(timing.cpuRenderThreadFrameTime);if(timing.cpuMainThreadPresentWaitTime>=0)wait.Add(timing.cpuMainThreadPresentWaitTime);
            if(timing.gpuFrameTime>0)gpu.Add(timing.gpuFrameTime);
        }
        phase.gpuSamples=gpu.Count;phase.observedFrame=Stats(observed);phase.cpuFrame=Stats(cpu);phase.cpuMain=Stats(main);phase.cpuRender=Stats(render);phase.presentWait=Stats(wait);phase.gpu=Stats(gpu);
    }
    void WriteSamples(int phase,List<Sample> samples)
    {
        var csv=new StringBuilder("frame,elapsedSeconds,observedMs,fresh,timestamp,cpuFrameMs,cpuMainMs,cpuRenderMs,presentWaitMs,gpuMs\n");
        foreach(var s in samples)
        {
            csv.Append(s.frame).Append(',').Append(s.elapsed.ToString("R",_culture)).Append(',').Append(s.observedMs.ToString("R",_culture)).Append(',').Append(s.fresh?1:0).Append(',').Append(s.fresh?s.timing.frameStartTimestamp:0);
            foreach(double value in new[]{s.timing.cpuFrameTime,s.timing.cpuMainThreadFrameTime,s.timing.cpuRenderThreadFrameTime,s.timing.cpuMainThreadPresentWaitTime,s.timing.gpuFrameTime})csv.Append(',').Append(s.fresh?value.ToString("R",_culture):"");
            csv.Append('\n');
        }
        File.WriteAllText(Path.Combine(_folder,"phase-"+phase+".csv"),csv.ToString());
    }
    void Finish(bool passed,string error)
    {
        _report.passed=passed;_report.error=error;
        _restoreAudit?.Invoke();
        if(_configured){QualitySettings.renderPipeline=_originalPipeline;QualitySettings.vSyncCount=_vSync;Application.targetFrameRate=_targetFrameRate;Application.runInBackground=_runInBackground;Destroy(_pipeline);_configured=false;}
        if(_folder!=null)File.WriteAllText(Path.Combine(_folder,"performance-result.json"),JsonUtility.ToJson(_report,true));
        Debug.Log(passed?"PC_PERFORMANCE_DONE":"PC_PERFORMANCE_FAIL "+error);
#if !UNITY_EDITOR
        Application.Quit(passed?0:1);
#endif
    }
}
#endif
