#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

/// <summary>Parcours des six bornes par raycast et souris synthétique ; poses contrôlées signalées.</summary>
public class DemoSmoke : MonoBehaviour
{
    [Serializable] public class Result { public bool passed; public string utc, error, platform, graphics; public Vector3 arrivalPosition,walkPosition; public string cursorAfterWalk; public List<string> checks=new List<string>(); }
    readonly Result _result=new Result();
    Mouse _mouse;
    Keyboard _keyboard;
    InputSettings.BackgroundBehavior _background;
    bool _oldRun;
    string _folder;

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--cloudvr-demo-smoke")>=0) Run();
    }
#endif
    public static void Run()
    {
        var probe=new GameObject("DemoSmoke_TEMP").AddComponent<DemoSmoke>(); DontDestroyOnLoad(probe.gameObject); probe.StartCoroutine(probe.Guard());
    }
    IEnumerator Guard()
    {
        var iterator=Exercise();
        while(true)
        {
            object step=null; bool next;
            try { next=iterator.MoveNext(); if(next) step=iterator.Current; }
            catch(Exception e) { End(false,e.ToString()); yield break; }
            if(!next) { End(true,null); yield break; }
            yield return step;
        }
    }
    void Check(bool value,string message) { if(!value) throw new InvalidOperationException(message); _result.checks.Add(message); }
    void Button(bool down)
    {
        _mouse.MakeCurrent();
        Cursor.lockState=CursorLockMode.Locked;
        InputSystem.QueueStateEvent(_mouse,new MouseState {
            position=new Vector2(Screen.width*.5f,Screen.height*.5f), buttons=(ushort)(down ? 1 : 0) });
    }
    IEnumerator Exercise()
    {
        _result.utc=DateTime.UtcNow.ToString("o"); _result.platform=Application.platform.ToString(); _result.graphics=SystemInfo.graphicsDeviceType.ToString();
        var args=Environment.GetCommandLineArgs(); int arg=Array.IndexOf(args,"--cloudvr-smoke-output");
        _folder=arg>=0 ? args[arg+1] : Path.GetFullPath(Path.Combine(Application.dataPath,"../../tmp/unity/demo-editor")); Directory.CreateDirectory(_folder);
        _background=InputSystem.settings.backgroundBehavior; _oldRun=Application.runInBackground;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; Application.runInBackground=true;
        _mouse=InputSystem.AddDevice<Mouse>("DemoSmokeMouse"); _keyboard=InputSystem.AddDevice<Keyboard>("DemoSmokeKeyboard");
        _mouse.MakeCurrent(); _keyboard.MakeCurrent();
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        yield return new WaitForSecondsRealtime(1);
        Check(SceneManager.GetActiveScene().name=="Chambre","Départ dans Chambre");
        InputModeManager.Instance.SetMode(InputModeManager.InputMode.PC);
        var sender=FindFirstObjectByType<PhotoSender>(); var cam=Camera.main;
        var controller=cam.GetComponentInParent<PCPlayerController>();
        controller.enabled=false; cam.transform.LookAt(sender.transform.position);
        Cursor.lockState=CursorLockMode.Locked;
        yield return new WaitForSecondsRealtime(.2f); Button(true); yield return new WaitForSecondsRealtime(.2f); Button(false);
        Check(sender.State==PhotoSender.SendState.Sending,"Clic synthétique sur écran déclenche l'envoi");
        float until=Time.realtimeSinceStartup+20;
        while(SceneManager.GetActiveScene().name!="DataCenter" && Time.realtimeSinceStartup<until) yield return null;
        yield return new WaitForSecondsRealtime(.8f);
        var tour=FindFirstObjectByType<DemoTour>();
        Check(tour!=null && tour.Step==0 && tour.stations.Length==6,"Six bornes initialisées sans étape sautée");
        Check(FindObjectsByType<Camera>(FindObjectsSortMode.None).Length==1,"Une seule caméra active");
        yield return new WaitForEndOfFrame(); Capture("demo-arrival.png");
        controller=Camera.main.GetComponentInParent<PCPlayerController>();
        _result.arrivalPosition=controller.transform.position;
        float startZ=controller.transform.position.z;
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.W));
        until=Time.realtimeSinceStartup+10;
        while(controller.transform.position.z<startZ+1 && Time.realtimeSinceStartup<until) yield return null;
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState()); yield return new WaitForSecondsRealtime(.1f);
        _result.walkPosition=controller.transform.position; _result.cursorAfterWalk=Cursor.lockState.ToString();
        Check(controller.transform.position.z>=startZ+1 && controller.transform.position.y>-.1f,"Avancée initiale au clavier synthétique et maintien au sol");
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape)); yield return new WaitForSecondsRealtime(.2f);
        var pausedPosition=controller.transform.position;
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.W)); yield return new WaitForSecondsRealtime(.2f);
        Check(controller.InputPaused && Vector3.Distance(pausedPosition,controller.transform.position)<.01f,"Échap suspend réellement le déplacement malgré le clavier");
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape)); yield return new WaitForSecondsRealtime(.2f);
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState()); yield return new WaitForSecondsRealtime(.1f);
        Check(!controller.InputPaused,"Échap reprend les commandes PC");
        // Une mauvaise borne ne doit pas permettre de sauter une notion.
        tour.Activate(5); Check(tour.Step==0 && tour.Actions==0,"Borne future ne saute pas la progression");
        yield return new WaitForEndOfFrame(); Capture("demo-arrival.png");
        for(int i=0;i<6;i++)
        {
            cam=Camera.main; controller=cam.GetComponentInParent<PCPlayerController>(); controller.enabled=false;
            var rig=controller.transform; var cc=rig.GetComponent<CharacterController>(); cc.enabled=false;
            var station=tour.stations[i];
            // Offset vers le centre de l'allée : bornes gauches (x<0) → joueur à droite ; bornes droites (x>0) → joueur à gauche.
            rig.position=station.transform.position + (i%2==0 ? Vector3.right : Vector3.left)*1.8f + Vector3.up*.08f;
            cc.enabled=true; cam.transform.LookAt(station.transform.position+Vector3.up*1.6f);
            Cursor.lockState=CursorLockMode.Locked;
            yield return new WaitForSecondsRealtime(.25f);
            Check(Physics.Raycast(cam.ViewportPointToRay(new Vector3(.5f,.5f,0)),out var hit,4.5f) && hit.collider.GetComponentInParent<DemoStation>()==station,"Raycast atteint borne "+i);
            Button(true); yield return new WaitForSecondsRealtime(.15f); Button(false);
            yield return new WaitForSecondsRealtime(.65f);
            if(i==3)
            {
                Check(tour.Step==3 && !tour.CoolingRestored,"Chaleur révélée sans sauter l'action de refroidissement");
                yield return new WaitForEndOfFrame(); Capture("demo-heat.png");
                Button(true); yield return new WaitForSecondsRealtime(.15f); Button(false);
            }
            if(i==4)
            {
                Check(tour.BackupShown && tour.Step==4,"Relais batterie illustré sans valider trop tôt");
                yield return new WaitForSecondsRealtime(3);
                Button(true); yield return new WaitForSecondsRealtime(.15f); Button(false);
            }
            if(i==0) { yield return new WaitForEndOfFrame(); Capture("demo-network.png"); }
            until=Time.realtimeSinceStartup+8;
            while(tour.Step==i && Time.realtimeSinceStartup<until) yield return null;
            Check(tour.Step==i+1,"Borne "+(i+1)+" validée après animation par clic");
        }
        Check(tour.Finished && tour.CoolingRestored && tour.BackupShown && tour.AlertAcknowledged && tour.Actions==8,"Six notions et huit actions terminées");
        yield return new WaitForEndOfFrame(); Capture("demo-summary.png");
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Enter));
        yield return new WaitForSecondsRealtime(.2f); InputSystem.QueueStateEvent(_keyboard,new KeyboardState());
        until=Time.realtimeSinceStartup+15;
        while(SceneManager.GetActiveScene().name!="Chambre" && Time.realtimeSinceStartup<until) yield return null;
        yield return new WaitForSecondsRealtime(.6f);
        Check(SceneManager.GetActiveScene().name=="Chambre" && DemoSession.Completed,"Retour Chambre par Entrée et état de conclusion conservé");
        Check(FindFirstObjectByType<PhotoSender>().photoObject.activeSelf,"Photo d'exemple de nouveau visible");
        yield return new WaitForEndOfFrame(); Capture("demo-room-complete.png");
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.R)); yield return new WaitForSecondsRealtime(.2f); InputSystem.QueueStateEvent(_keyboard,new KeyboardState());
        yield return new WaitForSecondsRealtime(1);
        Check(!DemoSession.Completed && FindFirstObjectByType<PhotoSender>().State==PhotoSender.SendState.Ready,"Recommencer remet la démo à zéro");
    }
    void Capture(string file)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        Canvas.ForceUpdateCanvases();
        var camera=Camera.main;
        var target=RenderTexture.GetTemporary(1280,720,24);
        var previous=camera.targetTexture; var active=RenderTexture.active;
        var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
            tex.ReadPixels(new Rect(0,0,1280,720),0,0); tex.Apply();
            File.WriteAllBytes(Path.Combine(_folder,file),tex.EncodeToPNG());
        }
        finally { camera.targetTexture=previous; RenderTexture.active=active; RenderTexture.ReleaseTemporary(target); Destroy(tex); }
    }
    void End(bool passed,string error)
    {
        _result.passed=passed; _result.error=error;
        if(_folder!=null) File.WriteAllText(Path.Combine(_folder,"demo-smoke-result.json"),JsonUtility.ToJson(_result,true));
        if(_mouse!=null) InputSystem.RemoveDevice(_mouse); if(_keyboard!=null) InputSystem.RemoveDevice(_keyboard);
        InputSystem.settings.backgroundBehavior=_background; Application.runInBackground=_oldRun;
        Debug.Log(passed ? "DEMO_SMOKE_PASS" : "DEMO_SMOKE_FAIL "+error);
#if !UNITY_EDITOR
        Application.Quit(passed ? 0 : 1);
#else
        Destroy(gameObject);
#endif
    }
}
#endif
