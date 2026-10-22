using UnityEngine;
using UnityEngine.UI;

/// <summary>Viseur PC et indication de l'objet réellement visé, sans commande d'action.</summary>
public class PCCrosshair : MonoBehaviour
{
    public Color couleur=new Color(1,1,1,.85f);
    public float taille=10,epaisseur=2;
    public string ActionHint {get;private set;}="";
    public Component FocusedTarget {get;private set;}
    public bool HasTarget {get;private set;}
    public bool CueVisible => _canvas!=null&&_canvas.isActiveAndEnabled;
    Canvas _canvas;
    Text _hint;
    Image[] _bars;
    NetworkTransitionV2 _ride;
    PhotoSender _sender;
    void Awake()
    {
        _canvas=gameObject.AddComponent<Canvas>();_canvas.renderMode=RenderMode.ScreenSpaceOverlay;_canvas.sortingOrder=100;
        var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        _bars=new[]{Bar("H",new Vector2(taille*2.5f,epaisseur)),Bar("V",new Vector2(epaisseur,taille*2.5f)),Bar("Point",Vector2.one*epaisseur*1.5f)};
        var go=new GameObject("Action_Visée",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text),typeof(Shadow));go.transform.SetParent(transform,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(0,-45);rect.sizeDelta=new Vector2(600,58);
        _hint=go.GetComponent<Text>();_hint.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");_hint.fontSize=21;_hint.alignment=TextAnchor.MiddleCenter;_hint.color=Color.white;_hint.raycastTarget=false;
        go.GetComponent<Shadow>().effectColor=Color.black;go.GetComponent<Shadow>().effectDistance=new Vector2(1,-1);
        _ride=FindFirstObjectByType<NetworkTransitionV2>();_sender=FindFirstObjectByType<PhotoSender>();
    }
    Image Bar(string name,Vector2 size)
    {
        var go=new GameObject("Crosshair_"+name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(transform,false);
        var image=go.GetComponent<Image>();image.color=couleur;image.raycastTarget=false;
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=size;return image;
    }
    void LateUpdate()
    {
        FocusedTarget=null;HasTarget=false;ActionHint="";
        var camera=Camera.main;var pc=camera!=null?camera.GetComponentInParent<PCPlayerController>():null;
        bool vr=InputModeManager.Instance!=null&&InputModeManager.Instance.IsVR;
        bool film=(_ride!=null&&_ride.IsRunning)||(_sender!=null&&_sender.State!=PhotoSender.SendState.Ready);
        _canvas.enabled=!vr&&camera!=null&&!film;
        if(!_canvas.enabled)return;
        bool paused=pc!=null&&pc.InputPaused;
        foreach(var bar in _bars)bar.enabled=!paused;
        if(paused)ActionHint="EN PAUSE • Échap pour reprendre";
        else if(pc!=null&&P1LearningModule.TryPick(camera.ViewportPointToRay(new Vector3(.5f,.5f,0)),pc.transform,out var hit,5))
        {
            var data=hit.collider.GetComponentInParent<DataServiceTarget>();var p1=hit.collider.GetComponentInParent<P1ActionTarget>();var facility=hit.collider.GetComponentInParent<FacilityActionTarget>();var station=hit.collider.GetComponentInParent<DemoStation>();
            if(data!=null)
            {
                FocusedTarget=data;int step=(int)data.action<=2?0:2;
                HasTarget=hit.distance<=3.5f&&data.module.tour.Step==step&&(step==0?data.module.NetworkStarted:data.module.StorageStarted);
                ActionHint=HasTarget?DataLabel(data):Prerequisite(data.module.tour,step);
            }
            else if(p1!=null)
            {
                FocusedTarget=p1;int step=(int)p1.action<5?1:3;
                HasTarget=hit.distance<=3.5f&&p1.module.tour.Step==step&&(step==1?p1.module.ComputeStarted:p1.module.CoolingStarted);
                ActionHint=HasTarget?P1Label(p1):Prerequisite(p1.module.tour,step);
            }
            else if(facility!=null)
            {
                FocusedTarget=facility;int step=(int)facility.action<=3?4:5;
                HasTarget=hit.distance<=3.5f&&facility.module.tour.Step==step&&(step==4?facility.module.PowerStarted:facility.module.SupervisionStarted);
                ActionHint=HasTarget?FacilityLabel(facility.action):Prerequisite(facility.module.tour,step);
            }
            else if(station!=null)
            {
                FocusedTarget=station;var tour=station.GetComponentInParent<DemoTour>();HasTarget=hit.distance<=4.5f&&tour!=null&&(tour.Finished||station.index<=tour.Step);
                ActionHint=tour==null?"":tour.Finished?"Retourner dans la chambre":station.index<tour.Step?"Relire "+DemoTour.Titles[station.index]:station.index==tour.Step?"Ouvrir l'étape "+(station.index+1)+" • "+DemoTour.Titles[station.index]:"Commencez par la borne "+(tour.Step+1).ToString("00");
            }
            else
            {
                var screen=hit.collider.GetComponent<ScreenClickHandler>();
                if(screen!=null&&screen.enabled){FocusedTarget=screen;HasTarget=hit.distance<=screen.maxRayDistance;ActionHint="Envoyer la photo d'exemple";}
            }
            if(FocusedTarget!=null&&hit.distance>(FocusedTarget is DemoStation?4.5f:FocusedTarget is ScreenClickHandler s?s.maxRayDistance:3.5f))ActionHint="Rapprochez-vous pour sélectionner cet objet";
        }
        var tint=HasTarget?new Color(.2f,1,.8f):couleur;foreach(var bar in _bars)bar.color=tint;
        _hint.text=HasTarget?(FocusedTarget is ScreenClickHandler?"Clic • ":"Clic / E • ")+ActionHint:ActionHint;
        _hint.enabled=ActionHint.Length>0;
    }
    static string Prerequisite(DemoTour tour,int step)
    {return tour.Step>step?"Étape déjà observée • poursuivez la visite":"Commencez par la borne "+(step+1).ToString("00");}
    static string DataLabel(DataServiceTarget target)
    {
        switch(target.action)
        {
            case DataServiceTarget.Action.IsolateLink:return "Illustrer la perte du lien A";
            case DataServiceTarget.Action.ReadResponse:return "Lire la réponse du service";
            case DataServiceTarget.Action.RestoreLink:return "Rétablir le lien A";
            case DataServiceTarget.Action.Replicate:return "Préparer la réplique B";
            case DataServiceTarget.Action.RecoveryPoint:return "Conserver le point C";
            case DataServiceTarget.Action.FailPrimary:return "Illustrer la panne de A";
            case DataServiceTarget.Action.ReadPhoto:return "Lire la photo disponible";
            case DataServiceTarget.Action.DeleteCurrent:return "Observer une suppression propagée";
            default:return "Restaurer le point C sur B";
        }
    }
    static string P1Label(P1ActionTarget target)
    {
        switch(target.action)
        {
            case P1ActionTarget.Action.Door:return "Ouvrir la baie de démonstration";
            case P1ActionTarget.Action.Drawer:return target.module.rack.CurrentState==ServerBladeInteraction.State.ServerExtracted?"Réinsérer le serveur et refermer":"Extraire le serveur";
            case P1ActionTarget.Action.Cpu:return "Observer les processeurs";
            case P1ActionTarget.Action.Ram:return "Observer la mémoire RAM";
            case P1ActionTarget.Action.Storage:return "Observer le stockage NVMe";
            case P1ActionTarget.Action.ResetAlarm:return "Acquitter l'alarme";
            case P1ActionTarget.Action.BoostFans:return "Observer une ventilation accrue";
            default:return "Fermer le confinement";
        }
    }
    static string FacilityLabel(FacilityActionTarget.Action action)
    {
        switch(action)
        {
            case FacilityActionTarget.Action.InspectUPS:return "Observer l'UPS / onduleur";
            case FacilityActionTarget.Action.StartGenerator:return "Observer le démarrage du groupe";
            case FacilityActionTarget.Action.Transfer:return "Observer le transfert de charge";
            case FacilityActionTarget.Action.Replay:return "Rejouer après épuisement";
            case FacilityActionTarget.Action.ReadIncident:return "Lire l'incident";
            case FacilityActionTarget.Action.RouteNetwork:return "Préparer le relais vers réseau";
            default:return "Préparer le relais vers énergie";
        }
    }
}
