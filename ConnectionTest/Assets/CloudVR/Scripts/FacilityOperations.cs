using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Secours électrique et supervision de démonstration, sans équipement réel piloté.</summary>
public class FacilityOperations : MonoBehaviour
{
    public enum PowerPhase { Normal, Battery, Starting, Ready, Transferring, Generator, Exhausted }
    public DemoTour tour;
    public P1LearningModule cooling;
    public FacilityActionTarget[] targets;
    public Text powerDisplay,powerFeedback,metricsDisplay,eventDisplay,limitsDisplay,supervisionFeedback;
    public Renderer[] sourceIndicators;
    public Material off,cold,warm;
    public AudioSource generatorAudio;
    AudioClip _generatorClip;
    public PowerPhase Phase { get; private set; }
    public bool PowerStarted { get; private set; }
    public bool UPSInspected { get; private set; }
    public bool PowerCompleted { get; private set; }
    public bool SupervisionStarted { get; private set; }
    public bool IncidentRead { get; private set; }
    public bool ReportPrepared { get; private set; }
    public int RefusedChoices { get; private set; }
    public float BatteryPercent { get; private set; }=100;
    public float SiteEnergyKWh { get; private set; }
    public float SitePowerKW => cooling.ITLoadKW+cooling.AuxiliaryKW;
    public float ReserveSeconds => BatteryPercent*.01f*.5f/SitePowerKW*3600;
    public bool OnBattery => PowerStarted&&Phase!=PowerPhase.Generator&&Phase!=PowerPhase.Exhausted;
    public bool ServiceAvailable => Phase!=PowerPhase.Exhausted;
    public bool GeneratorReady => Phase==PowerPhase.Ready||Phase==PowerPhase.Transferring||Phase==PowerPhase.Generator;
    readonly List<string> _events=new List<string>();
    float _phaseTime,_nextAction,_nextDisplay,_start;
    public bool InputPaused
    {
        get {var pc=Camera.main!=null?Camera.main.GetComponentInParent<PCPlayerController>():null;return pc!=null&&pc.InputPaused;}
    }
    void Start()
    {
        _start=Time.time;Log("Supervision initialisée");
        _generatorClip=ProceduralAudioService.CreateDataCenterTurbineDrone();generatorAudio.clip=_generatorClip;generatorAudio.loop=true;generatorAudio.volume=0;generatorAudio.Play();
        powerFeedback.text="Borne 05 : observation accompagnée d'un secours électrique.\nAucune manœuvre d'installation réelle.";
        supervisionFeedback.text="Borne 06 : reconstituer un incident et préparer son relais humain.";
        limitsDisplay.text="INDICATEURS À DOCUMENTER\nPUE annuel : non calculé\nÉnergies annuelles absentes\nEau / WUE : non renseignés\nAucun compteur d'eau\nCO2e : non calculé\nFacteurs énergie absents\n\nDONNÉES À PROTÉGER\nLocalisation du service,\ndroits d'accès et clés :\ndes questions distinctes.";
        Draw();
    }
    void Update()
    {
        var camera=Camera.main;var pc=camera!=null?camera.GetComponentInParent<PCPlayerController>():null;
        if(InputPaused)return;
        bool vr=InputModeManager.Instance!=null&&InputModeManager.Instance.IsVR;
        if(!vr&&pc!=null&&((Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)||(Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame))&&
            P1LearningModule.TryPick(camera.ViewportPointToRay(new Vector3(.5f,.5f,0)),pc.transform,out var hit))
            hit.collider.GetComponentInParent<FacilityActionTarget>()?.Activate();
        AdvancePower(Time.deltaTime);
        if(Time.unscaledTime>=_nextDisplay){_nextDisplay=Time.unscaledTime+.15f;Draw();}
    }
    // Pas de simulation matériel : permet aussi de contrôler les bornes du modèle en temps explicite.
    public void AdvancePower(float deltaSeconds)
    {
        deltaSeconds=Mathf.Max(0,deltaSeconds);
        float deliveredSeconds=!ServiceAvailable?0:OnBattery?Mathf.Min(deltaSeconds,ReserveSeconds):deltaSeconds;
        SiteEnergyKWh+=SitePowerKW*deliveredSeconds/3600;
        if(OnBattery)
        {
            BatteryPercent=Mathf.Max(0,BatteryPercent-SitePowerKW*deltaSeconds/3600/.5f*100);
            if(BatteryPercent<=0)
            {
                Phase=PowerPhase.Exhausted;Log("Réserve fictive épuisée, scénario à rejouer");
                powerFeedback.text="RÉSERVE ÉPUISÉE DANS CE SCÉNARIO\nRejouez l'incident depuis le pupitre. Aucun temps d'autonomie réel n'est prédit.";
            }
        }
        _phaseTime+=deltaSeconds;
        if(Phase==PowerPhase.Starting&&_phaseTime>=4)
        {Phase=PowerPhase.Ready;Log("Groupe stable (automate fictif)");powerFeedback.text="GROUPE PRÊT\nL'automate peut transférer la charge. Sélectionnez VOIR TRANSFERT pour observer cette étape ; l'UPS soutient encore la charge.";}
        if(Phase==PowerPhase.Transferring&&_phaseTime>=2)
        {Phase=PowerPhase.Generator;Log("Charge alimentée par groupe");powerFeedback.text="SOURCE GROUPE ÉTABLIE\nLa batterie ne se décharge plus ; IT et refroidissement restent alimentés. Inspectez aussi l'UPS.";}
        if(PowerStarted&&!PowerCompleted&&Phase==PowerPhase.Generator&&UPSInspected)
        {PowerCompleted=true;tour.CompleteFacilityStep(4,"Secours observé : batterie de l'UPS pendant le démarrage, puis groupe et distribution.\nLe retour du secteur reste une intervention à suivre.");}
        generatorAudio.volume=Phase==PowerPhase.Starting?Mathf.Clamp01(_phaseTime/4)*.18f:GeneratorReady ? .18f : 0;
    }
    public void BeginPower()
    {
        if(PowerStarted)return;
        PowerStarted=true;Phase=PowerPhase.Battery;_phaseTime=0;BatteryPercent=100;
        Log("PERTE SECTEUR / UPS sur batterie");
        powerFeedback.text="PERTE SECTEUR • BATTERIE DE L'UPS\nSecours normalement automatiques ; ici, observez les étapes à votre rythme.\nInspectez l'UPS, puis GROUPE et TRANSFERT.";
        Draw();
    }
    public void BeginSupervision()
    {
        if(SupervisionStarted)return;SupervisionStarted=true;
        supervisionFeedback.text="Lisez la ligne PERTE SECTEUR, puis choisissez l'équipe concernée. Le service est maintenu sur secours, la cause reste à traiter.";Draw();
    }
    public void Activate(FacilityActionTarget.Action action)
    {
        if(InputPaused||Time.unscaledTime<_nextAction)return;_nextAction=Time.unscaledTime+.25f;
        if((int)action<=3)
        {
            if(!PowerStarted||PowerCompleted)return;
            if(action==FacilityActionTarget.Action.Replay)
            {
                if(Phase!=PowerPhase.Exhausted){powerFeedback.text="La réserve soutient encore le scénario ; observez puis transférez vers le groupe prêt.";return;}
                PowerStarted=false;UPSInspected=false;BeginPower();return;
            }
            if(Phase==PowerPhase.Exhausted)return;
            if(action==FacilityActionTarget.Action.InspectUPS)
            {UPSInspected=true;Log("UPS observé : réserve temporaire");powerFeedback.text="UPS / ONDULEUR\nIl maintient la charge pendant la transition. Le groupe prend ensuite le relais ; une réserve limitée ne remplace pas une alimentation durable.";}
            if(action==FacilityActionTarget.Action.StartGenerator&&Phase==PowerPhase.Battery)
            {Phase=PowerPhase.Starting;_phaseTime=0;Log("Séquence automatique : démarrage groupe");powerFeedback.text="SUITE DU SECOURS AUTOMATIQUE\n4 s fictives de démarrage. Source à stabiliser, batterie toujours en soutien ; IT et refroidissement alimentés.";}
            if(action==FacilityActionTarget.Action.Transfer)
            {
                if(Phase!=PowerPhase.Ready){RefusedChoices++;powerFeedback.text="TRANSFERT REFUSÉ\nUne source non prête ne doit pas être sélectionnée. Observez son démarrage et attendez l'état stable.";return;}
                Phase=PowerPhase.Transferring;_phaseTime=0;Log("Transfert autorisé");
                powerFeedback.text="TRANSFERT EN COURS • 2 s FICTIVES\nLe relais est illustré par l'automate ; aucune coupure de charge représentée.";
            }
        }
        else
        {
            if(!SupervisionStarted||ReportPrepared)return;
            if(action==FacilityActionTarget.Action.ReadIncident)
            {IncidentRead=true;supervisionFeedback.text="CAUSE : PERTE DU SECTEUR\nUPS puis groupe ont maintenu le service. Le retour de la source normale reste à suivre par l'équipe énergie.";}
            if(action==FacilityActionTarget.Action.RouteNetwork)
            {RefusedChoices++;supervisionFeedback.text="L'ÉQUIPE RÉSEAU NE TRAITE PAS LA SOURCE ÉLECTRIQUE\nAucun défaut réseau n'est signalé dans ce scénario. Relisez l'incident, puis choisissez l'exploitation énergie.";}
            if(action==FacilityActionTarget.Action.RouteEnergy)
            {
                if(!IncidentRead){RefusedChoices++;supervisionFeedback.text="LISEZ D'ABORD L'INCIDENT\nUn rapport doit relier cause observée, source de secours et état du service.";return;}
                ReportPrepared=true;Log("Rapport énergie préparé");
                supervisionFeedback.text="RAPPORT PRÉPARÉ • SIMULATION\nPerte secteur ; UPS puis groupe ; service maintenu ; retour secteur à suivre. Aucun message externe envoyé.";
                tour.CompleteFacilityStep(5,"RAPPORT PÉDAGOGIQUE PRÉPARÉ\nLe service dépend d'équipements, de ressources et d'équipes. Les données fictives ne remplacent pas des mesures du site.");
            }
        }
        Draw();
    }
    void Log(string message)
    {
        _nextDisplay=0;
        _events.Add("+"+(Time.time-_start).ToString("F0")+" s • "+message);
        if(_events.Count>7)_events.RemoveAt(0);
    }
    void Draw()
    {
        string source=!PowerStarted?"secteur":Phase==PowerPhase.Generator?"groupe":Phase==PowerPhase.Exhausted?"aucune (réserve épuisée)":"batterie de l'UPS";
        powerDisplay.text="SECOURS ÉLECTRIQUE • DÉMONSTRATION\nSource de charge : "+source+"\nCharge : "+SitePowerKW.ToString("F1")+" kW • IT + auxiliaires\nRéserve fictive : "+BatteryPercent.ToString("F1")+" % (0,50 kWh)\nEstimation à charge constante : "+ReserveSeconds.ToString("F0")+" s\nGroupe : "+(GeneratorReady?"prêt":Phase==PowerPhase.Starting?"démarrage":"à demander")+" • service "+(ServiceAvailable?"alimenté":"interrompu dans le scénario");
        metricsDisplay.text="ÉNERGIE / AIR • FICTIF\nIT : "+(ServiceAvailable?cooling.ITLoadKW:0).ToString("F1")+" kW\nAuxiliaires : "+(ServiceAvailable?cooling.AuxiliaryKW:0).ToString("F1")+" kW\nTotal fourni : "+(ServiceAvailable?SitePowerKW:0).ToString("F1")+" kW\nÉnergie de session :\n"+SiteEnergyKWh.ToString("F3")+" kWh\nPtot / PIT : "+(ServiceAvailable?(SitePowerKW/cooling.ITLoadKW).ToString("F2"):"N/D")+"\nPas un PUE annuel mesuré\nEntrée baies : "+cooling.InletTemperature.ToString("F1")+" °C\nSource : "+source+"\nRefroidissement : "+(cooling.CoolingActive?"alimenté":"arrêté");
        eventDisplay.text="CHRONOLOGIE DU SCÉNARIO\n"+string.Join("\n",_events)+"\n\n"+(IncidentRead?"PERTE SECTEUR identifiée":"Sélectionnez LIRE INCIDENT");
        for(int i=0;i<sourceIndicators.Length;i++)sourceIndicators[i].sharedMaterial=i==0&&!PowerStarted?cold:i==1&&OnBattery?warm:i==2&&Phase==PowerPhase.Generator?cold:off;
    }
    void OnDestroy(){if(_generatorClip!=null)Destroy(_generatorClip);}
}
