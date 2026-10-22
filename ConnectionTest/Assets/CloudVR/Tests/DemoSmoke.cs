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
public partial class DemoSmoke : MonoBehaviour
{
    [Serializable] public class Result { public bool passed; public string utc, error, platform, graphics; public Vector3 arrivalPosition,walkPosition; public string cursorAfterWalk; public float walkRealSeconds,walkGameSeconds; public int walkFrames; public List<string> movement=new List<string>(); public List<string> checks=new List<string>(); }
    readonly Result _result=new Result();
    Mouse _mouse;
    Keyboard _keyboard;
    InputSettings.BackgroundBehavior _background;
    bool _oldRun;
    InputSettings.EditorInputBehaviorInPlayMode _editorInput;
    bool _holdForward;
    Key _movementKey=Key.W;
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
        var iterators=new Stack<IEnumerator>();iterators.Push(Exercise());
        while(iterators.Count>0)
        {
            object step=null; bool next;
            try { var iterator=iterators.Peek();next=iterator.MoveNext();if(next)step=iterator.Current; }
            catch(Exception e) { End(false,e.ToString()); yield break; }
            if(!next){iterators.Pop();continue;}
            if(step is IEnumerator nested){iterators.Push(nested);continue;}
            yield return step;
        }
        End(true,null);
    }
    void Check(bool value,string message) { if(!value) throw new InvalidOperationException(message); _result.checks.Add(message); }
    void Button(bool down)
    {
        _mouse.MakeCurrent();
        Cursor.lockState=CursorLockMode.Locked;
        InputSystem.QueueStateEvent(_mouse,new MouseState {
            position=new Vector2(Screen.width*.5f,Screen.height*.5f), buttons=(ushort)(down ? 1 : 0) });
    }
    void MaintainMovement()
    {
        // Réaffirmer le périphérique et l'état à chaque mise à jour d'entrée.
        // Laisser le contrôleur et les collisions réels traiter cette intention.
        if (!_holdForward || _keyboard == null || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        _keyboard.MakeCurrent();
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(_movementKey));
    }
    void Forward(bool held)
    {
        _holdForward=held; _keyboard.MakeCurrent();
        InputSystem.QueueStateEvent(_keyboard,held ? new KeyboardState(_movementKey) : new KeyboardState());
    }
    IEnumerator Exercise()
    {
        _result.utc=DateTime.UtcNow.ToString("o"); _result.platform=Application.platform.ToString(); _result.graphics=SystemInfo.graphicsDeviceType.ToString();
        var args=Environment.GetCommandLineArgs(); int arg=Array.IndexOf(args,"--cloudvr-smoke-output");
        _folder=arg>=0 ? args[arg+1] : Path.GetFullPath(Path.Combine(Application.dataPath,"../../tmp/unity/demo-editor")); Directory.CreateDirectory(_folder);
        _background=InputSystem.settings.backgroundBehavior; _oldRun=Application.runInBackground;
        _editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; Application.runInBackground=true;
        _mouse=InputSystem.AddDevice<Mouse>("DemoSmokeMouse"); _keyboard=InputSystem.AddDevice<Keyboard>("DemoSmokeKeyboard");
        _mouse.MakeCurrent(); _keyboard.MakeCurrent();
        InputSystem.onBeforeUpdate+=MaintainMovement;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        yield return new WaitForSecondsRealtime(1);
        Check(SceneManager.GetActiveScene().name=="Chambre","Départ dans Chambre");
        InputModeManager.Instance.SetMode(InputModeManager.InputMode.PC);
        var sender=FindFirstObjectByType<PhotoSender>(); var cam=Camera.main;
        var controller=cam.GetComponentInParent<PCPlayerController>();
        controller.enabled=false; cam.transform.LookAt(sender.transform.position);
        yield return new WaitForEndOfFrame(); Capture("demo-room-start.png");
        Cursor.lockState=CursorLockMode.Locked;
        yield return new WaitForSecondsRealtime(.2f); Button(true); yield return new WaitForSecondsRealtime(.2f); Button(false);
        Check(sender.State==PhotoSender.SendState.Sending,"Clic synthétique sur écran déclenche l'envoi");
        float until=Time.realtimeSinceStartup+20;
        bool capturedTransition=false,duplicateChecked=false,earlyStatusChecked=false;
        var stageCaptured=new bool[7];var cameraSamples=new List<Vector3>();var packetSamples=new List<Vector3>();
        while(SceneManager.GetActiveScene().name!="DataCenter" && Time.realtimeSinceStartup<until)
        {
            var ride=FindFirstObjectByType<NetworkTransitionV2>();
            if(ride!=null&&ride.IsRunning&&!earlyStatusChecked)
            {
                bool premature=false;
                foreach(var text in FindObjectsByType<UnityEngine.UI.Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(text.text.Contains("Fichier persisté"))premature=true;
                Check(!premature&&!sender.confirmationIndicator.activeInHierarchy,"Départ réseau : aucun stockage annoncé et ancienne confirmation masquée");earlyStatusChecked=true;
            }
            if(ride!=null&&ride.IsRunning&&ride.CurrentStep>=2&&!duplicateChecked)
            {
                ride.StartNetworkRide();sender.Send();
                Check(NetworkTransitionV2.Starts==1&&!ride.stepTransition.IsTriggered,"Double déclenchement ignoré et aucun chargement avant la fin du trajet");duplicateChecked=true;
            }
            if(ride!=null&&(ride.CurrentStep==2||ride.CurrentStep==3||ride.CurrentStep==4||ride.CurrentStep==6)&&!stageCaptured[ride.CurrentStep])
            {
                int stage=ride.CurrentStep;stageCaptured[stage]=true;
                yield return new WaitForSecondsRealtime(.35f);
                cameraSamples.Add(Camera.main.transform.position);packetSamples.Add(ride.visuals.packets[0].position);
                yield return new WaitForEndOfFrame();Capture("demo-network-stage-"+stage+".png");
                if(stage==3){Capture("demo-network-transition.png");capturedTransition=true;}
            }
            yield return null;
        }
        yield return new WaitForSecondsRealtime(.8f);
        Check(capturedTransition&&stageCaptured[6]&&NetworkTransitionV2.CompletedStages==6&&NetworkTransitionV2.CompletedDuration>=7.4f&&NetworkTransitionV2.CompletedDuration<8.5f,
            "Transition 3D complète : six étapes et arrivée campus, durée réelle 7,5 s environ, aucun bypass smoke");
        Check(cameraSamples.Count==4&&Vector3.Distance(cameraSamples[0],cameraSamples[2])>20&&NetworkTransitionV2.TravelDistance>50,
            "La caméra parcourt réellement le décor 3D sur PC, au-delà de 50 unités fictives");
        Check(Vector3.Distance(packetSamples[0],packetSamples[1])>1&&!NetworkTransitionV2.UsedComfortScreen,
            "Signaux animés dans le tunnel, vue PC immersive distincte du mode confort VR");
        var tour=FindFirstObjectByType<DemoTour>();
        Check(tour!=null && tour.Step==0 && tour.stations.Length==6,"Six bornes initialisées sans étape sautée");
        Check(FindObjectsByType<Camera>(FindObjectsSortMode.None).Length==1,"Une seule caméra active");
        yield return new WaitForEndOfFrame(); Capture("demo-arrival.png");
        controller=Camera.main.GetComponentInParent<PCPlayerController>();
        _result.arrivalPosition=controller.transform.position;
        Check(Mathf.Abs(controller.transform.position.z+28)<.3f,"Apparition dans la cour à Z=-28 m");
        // Régression du seuil qui annulait les déplacements à cadence élevée.
        var capsule=controller.GetComponent<CharacterController>();
        float beforeSmallMoves=controller.transform.position.z;
        for(int n=0;n<100;n++) capsule.Move(Vector3.forward*.0005f);
        Check(Mathf.Abs(controller.transform.position.z-beforeSmallMoves-.05f)<.003f,"Cent commandes de 0,5 mm produisent 5 cm, sans perte liée au seuil du CharacterController");
        float startZ=controller.transform.position.z;
        Forward(true);
        until=Time.realtimeSinceStartup+10;
        while(controller.transform.position.z<startZ+1 && Time.realtimeSinceStartup<until) yield return null;
        Forward(false); yield return new WaitForSecondsRealtime(.1f);
        _result.walkPosition=controller.transform.position; _result.cursorAfterWalk=Cursor.lockState.ToString();
        Check(controller.transform.position.z>=startZ+1 && controller.transform.position.y>-.1f,"Avancée initiale au clavier synthétique et maintien au sol");
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape)); yield return new WaitForSecondsRealtime(.2f);
        var pausedPosition=controller.transform.position;
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.W)); yield return new WaitForSecondsRealtime(.2f);
        Check(controller.InputPaused && Vector3.Distance(pausedPosition,controller.transform.position)<.01f,"Échap suspend réellement le déplacement malgré le clavier");
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape)); yield return new WaitForSecondsRealtime(.2f);
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState()); yield return new WaitForSecondsRealtime(.1f);
        Check(!controller.InputPaused,"Échap reprend les commandes PC");
        // Vrai déplacement avec CharacterController : ne pas téléporter au-delà des murs ou du sas.
        var mantrap=FindFirstObjectByType<MantrapController>();
        bool sawScan=false, interlock=true;
        Forward(true);
        float walkStart=Time.realtimeSinceStartup,gameStart=Time.time,nextSample=walkStart;
        int firstFrame=Time.frameCount;
        until=Time.realtimeSinceStartup+35;
        while(controller.transform.position.z<39f && Time.realtimeSinceStartup<until)
        {
            if(Time.realtimeSinceStartup>=nextSample)
            {
                _result.movement.Add("t="+(Time.realtimeSinceStartup-walkStart).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+
                    " z="+controller.transform.position.z.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+
                    " keyboard="+Keyboard.current.name+" W="+_keyboard.wKey.isPressed+" sas="+mantrap.State);
                nextSample=Time.realtimeSinceStartup+1;
            }
            if(mantrap.State==MantrapController.MantrapState.Scanning)
            {
                if(!sawScan) { yield return new WaitForEndOfFrame(); Capture("demo-mantrap-scan.png"); }
                sawScan=true; interlock &= !mantrap.EntryOpen && !mantrap.ExitOpen;
            }
            yield return null;
        }
        Forward(false);
        _result.walkRealSeconds=Time.realtimeSinceStartup-walkStart;
        _result.walkGameSeconds=Time.time-gameStart;
        _result.walkFrames=Time.frameCount-firstFrame;
        yield return new WaitForSecondsRealtime(.1f);
        _result.walkPosition=controller.transform.position;
        var arrivalRotation=Camera.main.transform.rotation;
        Camera.main.transform.rotation=Quaternion.LookRotation(Vector3.back);
        yield return new WaitForEndOfFrame(); Capture("demo-hall-end.png");
        Camera.main.transform.rotation=arrivalRotation;
        Check(sawScan && interlock && MantrapController.ScanCompleted,"Sas traversé : fermeture des deux portes, scan réel de 2 s, autorisation");
        Check(controller.transform.position.z>=39 && controller.transform.position.y>-.1f,"Marche continue de la cour au fond à Z=39 m, sans traverser le sol ni téléporter : "+controller.transform.position);
        Check(GameObject.Find("Demo_HUD")==null,"Informations sur consoles 3D, sans HUD massif du data center");
        _movementKey=Key.S;Forward(true);until=Time.realtimeSinceStartup+6;
        while(controller.transform.position.z>30&&Time.realtimeSinceStartup<until)yield return null;
        Forward(false);yield return new WaitForSecondsRealtime(.1f);
        Check(Mathf.Abs(controller.transform.position.z-30)<.2f,"Retour à la bifurcation électrique par déplacement réel");
        _movementKey=Key.A;Forward(true);until=Time.realtimeSinceStartup+8;
        while(controller.transform.position.x>-12&&Time.realtimeSinceStartup<until)yield return null;
        Forward(false);_movementKey=Key.W;yield return new WaitForSecondsRealtime(.1f);
        Check(controller.transform.position.x < -12,"Porte TGBT traversée au clavier, entrée dans le local");
        Camera.main.transform.LookAt(new Vector3(-19,1.4f,25.5f));
        yield return new WaitForEndOfFrame();Capture("demo-electrical.png");
        // Comparaison visuelle ciblée du mur : poses contrôlées après la marche vérifiée.
        // Le bilan des contrôles reste distinct de l'inspection de ces images.
        controller.enabled=false;
        var wallRigPosition=controller.transform.position;
        var wallCameraLocalPosition=Camera.main.transform.localPosition;
        var wallCameraRotation=Camera.main.transform.rotation;
        capsule.enabled=false;controller.transform.position=new Vector3(-7.6f,.08f,12.4f);capsule.enabled=true;
        Camera.main.transform.position=new Vector3(-7.6f,1.65f,12.4f);
        foreach(int yaw in new[]{195,205,220,235,250,265,280})
        {
            Camera.main.transform.rotation=Quaternion.Euler(0,yaw,0);
            yield return new WaitForEndOfFrame();Capture("demo-west-wall-"+yaw+".png");
        }
        capsule.enabled=false;controller.transform.position=wallRigPosition;capsule.enabled=true;
        Camera.main.transform.localPosition=wallCameraLocalPosition;Camera.main.transform.rotation=wallCameraRotation;
        controller.enabled=true;
        for(int i=0;i<6;i++) Check(Mathf.Abs(tour.stations[i].transform.position.z-(8+i*6))<.01f,"Borne "+(i+1)+" à Z="+(8+i*6)+" m");
        // Une mauvaise borne ne doit pas permettre de sauter une notion.
        tour.Activate(5); Check(tour.Step==0 && tour.Actions==0,"Borne future ne saute pas la progression");
        for(int i=0;i<6;i++)
        {
            cam=Camera.main; controller=cam.GetComponentInParent<PCPlayerController>(); controller.enabled=false;
            var rig=controller.transform; var cc=rig.GetComponent<CharacterController>(); cc.enabled=false;
            var station=tour.stations[i];
            // Offset vers le centre de l'allée : bornes gauches (x<0) → joueur à droite ; bornes droites (x>0) → joueur à gauche.
            rig.position=station.transform.position + (station.transform.position.x<0 ? Vector3.right : Vector3.left)*1.8f + Vector3.up*.08f;
            cc.enabled=true; cam.transform.LookAt(station.transform.position+Vector3.up*1.6f);
            Cursor.lockState=CursorLockMode.Locked;
            yield return new WaitForSecondsRealtime(.25f);
            Check(Physics.Raycast(cam.ViewportPointToRay(new Vector3(.5f,.5f,0)),out var hit,4.5f) && hit.collider.GetComponentInParent<DemoStation>()==station,"Raycast atteint borne "+i);
            Button(true); yield return new WaitForSecondsRealtime(.15f); Button(false);
            yield return new WaitForSecondsRealtime(.65f);
            if(i==0)yield return ExerciseDataNetwork(tour);
            if(i==2)yield return ExerciseDataStorage(tour);
            if(i==1)
            {
                var p1=tour.learning;var blade=p1.rack;
                Check(p1.ComputeStarted&&tour.Step==1,"La borne 02 attend une manipulation, sans valider par un flux automatique");
                AimPhysical(p1,P1ActionTarget.Action.Door,14);
                InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape));yield return new WaitForSecondsRealtime(.15f);
                InputSystem.QueueStateEvent(_keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.1f);
                Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);
                Check(controller.InputPaused&&blade.CurrentState==ServerBladeInteraction.State.DoorClosed,"Pause : clic sur poignée sans ouverture de baie");
                InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape));yield return new WaitForSecondsRealtime(.15f);
                InputSystem.QueueStateEvent(_keyboard,new KeyboardState());
                var drawerTarget=Array.Find(p1.targets,t=>t.action==P1ActionTarget.Action.Drawer);
                cam.transform.LookAt(drawerTarget.transform.position);
                Check(P1LearningModule.TryPick(cam.ViewportPointToRay(new Vector3(.5f,.5f,0)),controller.transform,out var closedHit)&&closedHit.collider.transform.IsChildOf(blade.rackDoor),"La porte fermée reste un obstacle de visée, même lorsque la capsule du visiteur est ignorée");
                cam.transform.LookAt(Array.Find(p1.targets,t=>t.action==P1ActionTarget.Action.Door).transform.position);
                Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);
                yield return new WaitForSecondsRealtime(.15f);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);
                Check(blade.IsAnimating&&blade.CurrentState!=ServerBladeInteraction.State.ServerExtracted,"Double clic pendant ouverture ne saute pas à l'extraction");
                yield return new WaitForSecondsRealtime(1);
                Check(blade.CurrentState==ServerBladeInteraction.State.DoorOpened,"Poignée sélectionnée par raycast : porte ouverte sur charnière");
                var inserted=blade.serverBlade.localPosition;
                AimPhysical(p1,P1ActionTarget.Action.Drawer,14);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);
                yield return new WaitForSecondsRealtime(1);
                Check(blade.CurrentState==ServerBladeInteraction.State.ServerExtracted&&Mathf.Abs(Vector3.Distance(inserted,blade.serverBlade.localPosition)-.64f)<.01f,"Tiroir extrait réellement de 0,64 m, composants exposés");
                Camera.main.transform.LookAt(blade.serverBlade.TransformPoint(new Vector3(0,.14f,.4f)));
                yield return new WaitForEndOfFrame();Capture("demo-server-extracted.png");
                AimPhysical(p1,P1ActionTarget.Action.Drawer,14);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);yield return new WaitForSecondsRealtime(.3f);
                Check(blade.CurrentState==ServerBladeInteraction.State.ServerExtracted&&tour.Step==1,"Réinsertion prématurée refusée avant inspection des trois composants");
                foreach(var part in new[]{P1ActionTarget.Action.Cpu,P1ActionTarget.Action.Ram,P1ActionTarget.Action.Storage})
                {
                    AimPhysical(p1,part,14);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);yield return new WaitForSecondsRealtime(.3f);
                }
                Check(p1.InspectedParts==7&&tour.Step==1,"CPU, RAM et NVMe identifiés par trois sélections physiques distinctes");
                tour.Activate(1);Check(tour.Step==1,"La console ne termine pas l'inspection à la place de la remise en place");
                AimPhysical(p1,P1ActionTarget.Action.Drawer,14);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);yield return new WaitForSecondsRealtime(1.8f);
                Check(blade.CurrentState==ServerBladeInteraction.State.DoorClosed&&p1.ComputeCompleted&&Vector3.Distance(inserted,blade.serverBlade.localPosition)<.01f,"Serveur remis en place et porte fermée avant validation du traitement");
            }
            if(i==3)
            {
                var p1=tour.learning;
                Check(tour.Step==3&&!tour.CoolingRestored&&p1.CoolingActive&&p1.containment.OpenAmount>.25f,"Anomalie de confinement : porte entrouverte, refroidissement déjà actif");
                AimPhysical(p1,P1ActionTarget.Action.ResetAlarm,26);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);yield return new WaitForSecondsRealtime(.3f);
                Check(tour.Step==3&&!p1.LeakSealed&&p1.InletTemperature>31,"Acquitter l'alarme ne répare pas la fuite ni la température");
                float aux=p1.AuxiliaryKW;
                AimPhysical(p1,P1ActionTarget.Action.BoostFans,26);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);yield return new WaitForSecondsRealtime(.6f);
                Check(tour.Step==3&&!p1.LeakSealed&&p1.AuxiliaryKW>aux&&p1.InletTemperature>29,"Ventilation maximale : auxiliaires en hausse, fuite et progression non résolues");
                Camera.main.transform.LookAt(p1.thermalDisplay.transform.position);yield return new WaitForEndOfFrame();Capture("demo-heat.png");
                AimPhysical(p1,P1ActionTarget.Action.SealContainment,26);Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);yield return new WaitForSecondsRealtime(.25f);
                Check(p1.LeakSealed&&tour.Step==3&&!tour.CoolingRestored,"Poignée de confinement fermée : attendre la baisse thermique, sans validation instantanée");
                until=Time.realtimeSinceStartup+8;while(tour.Step==3&&Time.realtimeSinceStartup<until)yield return null;
                Check(p1.CoolingRecovered&&p1.InletTemperature<24&&p1.CoolingActive&&p1.AuxiliaryKW==3,"Température stabilisée, refroidissement continu et auxiliaires revenus au scénario nominal");
                Camera.main.transform.LookAt(p1.thermalDisplay.transform.position);yield return new WaitForEndOfFrame();Capture("demo-cooling-recovered.png");
            }
            if(i==4)yield return ExerciseFacilityPower(tour);
            if(i==5)yield return ExerciseFacilitySupervision(tour);
            if(i==0) { yield return new WaitForEndOfFrame(); Capture("demo-network.png"); }
            until=Time.realtimeSinceStartup+8;
            while(tour.Step==i && Time.realtimeSinceStartup<until) yield return null;
            Check(tour.Step==i+1,"Borne "+(i+1)+" validée après interaction");
        }
        Check(tour.Finished && tour.CoolingRestored && tour.BackupShown && tour.AlertAcknowledged && tour.Actions==6,"Six notions terminées, six activations de console et manipulations physiques requises");
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
    void AimPhysical(P1LearningModule module,P1ActionTarget.Action action,float z)
    {
        var camera=Camera.main;var controller=camera.GetComponentInParent<PCPlayerController>();controller.enabled=false;
        var cc=controller.GetComponent<CharacterController>();cc.enabled=false;controller.transform.position=new Vector3(0,.08f,z);cc.enabled=true;
        var target=Array.Find(module.targets,t=>t.action==action);camera.transform.LookAt(target.transform.position);Physics.SyncTransforms();
        var ray=camera.ViewportPointToRay(new Vector3(.5f,.5f,0));
        bool reached=P1LearningModule.TryPick(ray,controller.transform,out var hit)&&hit.collider.GetComponentInParent<P1ActionTarget>()==target;
        Check(reached,"Raycast physique atteint "+action+" ; contact="+(hit.collider!=null?hit.collider.name:"aucun")+" ; oeil="+camera.transform.position+" ; cible="+target.transform.position);
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
        _holdForward=false;
        InputSystem.onBeforeUpdate-=MaintainMovement;
        if(_folder!=null) File.WriteAllText(Path.Combine(_folder,"demo-smoke-result.json"),JsonUtility.ToJson(_result,true));
        if(_mouse!=null) InputSystem.RemoveDevice(_mouse); if(_keyboard!=null) InputSystem.RemoveDevice(_keyboard);
        InputSystem.settings.backgroundBehavior=_background; Application.runInBackground=_oldRun;
        InputSystem.settings.editorInputBehaviorInPlayMode=_editorInput;
        Debug.Log(passed ? "DEMO_SMOKE_PASS" : "DEMO_SMOKE_FAIL "+error);
#if !UNITY_EDITOR
        Application.Quit(passed ? 0 : 1);
#else
        Destroy(gameObject);
#endif
    }
}
#endif
