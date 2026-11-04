#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        int[] order = {1,4,4,1};
        for (int i=0;i<order.Length;i++)
        {
            _pipeline.msaaSampleCount = order[i];
            double warm = Time.realtimeSinceStartupAsDouble;
            while (Time.realtimeSinceStartupAsDouble-warm<2) { _camera.transform.rotation=Quaternion.Euler(-8,-12,0); FrameTimingManager.CaptureFrameTimings(); yield return null; }
            var samples = new List<Sample>(8192); var phase = new Phase {order=i,msaa=order[i],focusedThroughout=true,scaleUnchanged=true};
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
        if(_configured){QualitySettings.renderPipeline=_originalPipeline;QualitySettings.vSyncCount=_vSync;Application.targetFrameRate=_targetFrameRate;Application.runInBackground=_runInBackground;Destroy(_pipeline);_configured=false;}
        if(_folder!=null)File.WriteAllText(Path.Combine(_folder,"performance-result.json"),JsonUtility.ToJson(_report,true));
        Debug.Log(passed?"PC_PERFORMANCE_DONE":"PC_PERFORMANCE_FAIL "+error);
#if !UNITY_EDITOR
        Application.Quit(passed?0:1);
#endif
    }
}
#endif
