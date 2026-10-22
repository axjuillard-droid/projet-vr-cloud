using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Maintenance pédagogique et modèle thermique fictif à temps accéléré, pas un outil d'exploitation.</summary>
public class P1LearningModule : MonoBehaviour
{
    public DemoTour tour;
    public ServerBladeInteraction rack;
    public ProximitySlidingDoor containment;
    public P1ActionTarget[] targets;
    public GameObject inspectionTargets, thermalVisuals;
    public Text rackDisplay, thermalDisplay, feedback;
    public Renderer[] temperatureMarkers;
    public Renderer[] inspectionMarkers;
    public Material cold, warm;
    public Transform[] airMarkers;
    public LineRenderer leakPath;
    public Transform fan;
    public bool ComputeStarted { get; private set; }
    public bool ComputeCompleted { get; private set; }
    public int InspectedParts { get; private set; }
    public bool CoolingStarted { get; private set; }
    public bool CoolingRecovered { get; private set; }
    public bool LeakSealed { get; private set; }
    public bool FansBoosted { get; private set; }
    public bool CoolingActive => tour.facility==null||tour.facility.ServiceAvailable;
    public float InletTemperature { get; private set; }=22;
    public float ITLoadKW => 12;
    public float AuxiliaryKW => !CoolingStarted||LeakSealed ? 3 : FansBoosted ? 5 : 4;
    public float PUE => (ITLoadKW+AuxiliaryKW)/ITLoadKW;
    public int UnsuccessfulChoices { get; private set; }
    bool _extracted;
    bool _automaticRestored;
    float _nextInput, _stable, _nextDisplay;
    string _rackMessage="Baie de démonstration hors service.\nCommencez par la borne 02 : inspection guidée, sans intervention sur une machine en production.";
    AudioSource _fans;
    AudioClip _fanClip;
    void Start()
    {
        _fans=gameObject.AddComponent<AudioSource>();
        _fans.spatialBlend=1;_fans.minDistance=1;_fans.maxDistance=12;_fans.loop=true;
        _fanClip=ProceduralAudioService.CreateDataCenterTurbineDrone();_fans.clip=_fanClip;
        _fans.volume=.08f;_fans.Play();
        UpdateDisplays();
    }
    void Update()
    {
        var camera=Camera.main;
        var pc=camera!=null?camera.GetComponentInParent<PCPlayerController>():null;
        bool vr=InputModeManager.Instance!=null&&InputModeManager.Instance.IsVR;
        if(!vr&&camera!=null&&pc!=null&&!pc.InputPaused&&
            ((Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)||(Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame))&&
            TryPick(camera.ViewportPointToRay(new Vector3(.5f,.5f,0)),pc.transform,out var hit))
            hit.collider.GetComponentInParent<P1ActionTarget>()?.Activate();

        bool extracted=rack.CurrentState==ServerBladeInteraction.State.ServerExtracted&&!rack.IsAnimating;
        if(extracted)_extracted=true;
        inspectionTargets.SetActive(extracted);
        if(ComputeStarted&&!ComputeCompleted&&_extracted&&InspectedParts==7&&
            rack.CurrentState==ServerBladeInteraction.State.DoorClosed&&!rack.IsAnimating)
        {
            ComputeCompleted=true;
            _rackMessage="INSPECTION TERMINÉE\nCPU : exécuter le logiciel. RAM : mémoire de travail volatile. NVMe : stockage persistant.\nServeur réinséré, porte refermée.";
            tour.CompletePhysicalStep(1,_rackMessage);
        }
        if(CoolingStarted)
        {
            float target=!CoolingActive?40:LeakSealed?22:FansBoosted?30:32;
            InletTemperature=Mathf.Lerp(InletTemperature,target,1-Mathf.Exp(-Time.deltaTime/1.5f));
            _stable=LeakSealed&&containment.OpenAmount<.02f&&InletTemperature<24?_stable+Time.deltaTime:0;
            if(!CoolingRecovered&&_stable>1)
            {
                CoolingRecovered=true;
                feedback.text="CONFINEMENT RÉTABLI\nLa fuite est fermée et l'entrée des baies revient à sa cible. Le refroidissement fonctionnait pendant toute l'intervention.";
                tour.CompletePhysicalStep(3,"Fuite corrigée : l'air froid reste dirigé vers les entrées des baies.\nVentiler davantage ou acquitter l'alarme ne répare pas une porte ouverte.");
            }
            foreach(var r in temperatureMarkers)r.sharedMaterial=InletTemperature<24?cold:warm;
            leakPath.gameObject.SetActive(!LeakSealed);
            var mode=InputModeManager.Instance;var rig=mode!=null?(mode.IsVR?mode.vrRig:mode.pcRig):null;
            if(CoolingRecovered&&!_automaticRestored&&rig!=null&&Mathf.Abs(rig.transform.position.z-containment.transform.position.z)>3.2f)
            {containment.manualOverride=false;_automaticRestored=true;}
        }
        if(_fans!=null){_fans.volume=!CoolingActive?0:CoolingStarted&&!LeakSealed?(FansBoosted ? .27f : .18f):.08f;_fans.pitch=FansBoosted&&!LeakSealed?1.15f:.85f;}
        if(fan!=null&&CoolingActive)fan.Rotate(Vector3.forward,(FansBoosted&&!LeakSealed?900:360)*Time.deltaTime);
        for(int i=0;i<airMarkers.Length;i++)
        {
            var line=airMarkers[i].parent.GetComponent<LineRenderer>();
            float p=Mathf.Repeat(Time.time*.45f+i*.33f,1)*(line.positionCount-1);
            int segment=Mathf.Min(line.positionCount-2,(int)p);
            airMarkers[i].position=Vector3.Lerp(line.GetPosition(segment),line.GetPosition(segment+1),p-segment);
        }
        if(Time.unscaledTime>=_nextDisplay){_nextDisplay=Time.unscaledTime+.1f;UpdateDisplays();}
    }
    public static bool TryPick(Ray ray,Transform observer,out RaycastHit nearest)
    {
        // Une visée plongeante peut entrer dans la capsule du visiteur : ignorer seulement son propre rig.
        // Conserver le premier obstacle du décor, sans sélectionner au travers d'une porte ou d'une baie.
        nearest=default;float distance=float.PositiveInfinity;
        foreach(var h in Physics.RaycastAll(ray,3.5f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide))
        {
            if(observer!=null&&h.collider.transform.IsChildOf(observer))continue;
            if(h.distance<distance){distance=h.distance;nearest=h;}
        }
        return distance<float.PositiveInfinity;
    }
    public void BeginCompute()
    {
        if(ComputeStarted)return;
        ComputeStarted=true;
        _rackMessage="MAINTENANCE SUR BAIE DE DÉMONSTRATION\n1. Tournez la poignée de porte.\n2. Tirez le serveur sur ses rails.\n3. Visez CPU, RAM et SSD.\n4. Réinsérez le tiroir pour refermer.";
        UpdateDisplays();
    }
    public void BeginCooling()
    {
        if(CoolingStarted)return;
        CoolingStarted=true;InletTemperature=32;
        containment.manualOverride=true;containment.forcedOpening=.32f;
        thermalVisuals.SetActive(true);
        feedback.text="ALARME : ENTRÉE DES BAIES TROP CHAUDE\nCherchez la cause. Refroidissement en fonctionnement ; porte de confinement entrouverte.";
        UpdateDisplays();
    }
    public void Activate(P1ActionTarget.Action action)
    {
        var pc=Camera.main!=null?Camera.main.GetComponentInParent<PCPlayerController>():null;
        if(pc!=null&&pc.InputPaused)return;
        if(Time.unscaledTime<_nextInput)return;
        _nextInput=Time.unscaledTime+.25f;
        if((int)action<5)
        {
            if(!ComputeStarted||ComputeCompleted){_rackMessage="Commencez l'inspection à la borne 02.";return;}
            if(rack.IsAnimating)return;
            if(action==P1ActionTarget.Action.Door||action==P1ActionTarget.Action.Drawer)
            {
                if(action==P1ActionTarget.Action.Door&&rack.CurrentState!=ServerBladeInteraction.State.DoorClosed)return;
                if(action==P1ActionTarget.Action.Drawer&&rack.CurrentState==ServerBladeInteraction.State.DoorClosed)return;
                if(rack.CurrentState==ServerBladeInteraction.State.ServerExtracted&&InspectedParts!=7)
                {_rackMessage="Avant de réinsérer, identifiez les trois repères : CPU, RAM et SSD NVMe.";return;}
                rack.Interact();
            }
            else if(rack.CurrentState==ServerBladeInteraction.State.ServerExtracted)
            {
                int bit=1<<((int)action-2);InspectedParts|=bit;
                inspectionMarkers[(int)action-2].sharedMaterial=cold;
                _rackMessage=action==P1ActionTarget.Action.Cpu?"CPU / TRAITEMENT\nLes processeurs exécutent les instructions du logiciel. Les radiateurs transmettent leur chaleur à l'air.":
                    action==P1ActionTarget.Action.Ram?"RAM / MÉMOIRE DE TRAVAIL\nLes données en cours de traitement y sont accessibles rapidement. Cette mémoire est volatile ; elle ne remplace pas le stockage.":
                    "SSD NVMe / STOCKAGE\nLe stockage conserve des données après extinction. Redondance et sauvegarde répondent à des risques différents.";
                _rackMessage+="\nRepères : "+CountParts()+" / 3"+(InspectedParts==7?" • Réinsérez le tiroir.":"");
            }
        }
        else
        {
            if(!CoolingStarted||CoolingRecovered)return;
            if(action==P1ActionTarget.Action.ResetAlarm)
            {UnsuccessfulChoices++;feedback.text="ACQUITTEMENT SANS RÉPARATION\nLe signal est reconnu, mais la porte reste entrouverte. La température ne revient pas à sa cible.";}
            if(action==P1ActionTarget.Action.BoostFans)
            {UnsuccessfulChoices++;FansBoosted=true;feedback.text="PLUS D'ÉNERGIE, FUITE TOUJOURS PRÉSENTE\nLa ventilation augmente, mais l'air se mélange encore. Fermez le confinement plutôt que compenser la fuite.";}
            if(action==P1ActionTarget.Action.SealContainment)
            {
                var rig=InputModeManager.Instance;var player=rig!=null?(rig.IsVR?rig.vrRig:rig.pcRig):null;
                if(player!=null&&Mathf.Abs(player.transform.position.z-containment.transform.position.z)<.65f&&Mathf.Abs(player.transform.position.x)<1.3f)
                {feedback.text="ÉLOIGNEZ-VOUS DU PASSAGE\nFermez la porte depuis l'extérieur du seuil.";return;}
                LeakSealed=true;FansBoosted=false;containment.forcedOpening=0;
                feedback.text="FERMETURE DU CONFINEMENT\nObservez le retour de la température. Le refroidissement reste actif ; aucun effet instantané sur l'air.";
            }
        }
        UpdateDisplays();
    }
    int CountParts() { return ((InspectedParts&1)!=0?1:0)+((InspectedParts&2)!=0?1:0)+((InspectedParts&4)!=0?1:0); }
    void UpdateDisplays()
    {
        if(rackDisplay!=null)rackDisplay.text=_rackMessage;
        if(thermalDisplay!=null)thermalDisplay.text="AIR / ÉNERGIE • SCÉNARIO FICTIF\nEntrée : "+InletTemperature.ToString("F1")+" °C • cible 22 °C\nDemande IT : 12 kW • auxiliaires : "+AuxiliaryKW.ToString("F1")+" kW\nPtot / PIT : "+PUE.ToString("F2")+" • PUE indicatif\n(régime constant, temps accéléré)\nRefroidissement : "+(CoolingActive?"alimenté":"arrêté, réserve épuisée")+"\nPorte : "+(LeakSealed?"fermée":CoolingStarted?"entrouverte":"automatique");
    }
    void OnDestroy(){if(_fanClip!=null)Destroy(_fanClip);}
}
