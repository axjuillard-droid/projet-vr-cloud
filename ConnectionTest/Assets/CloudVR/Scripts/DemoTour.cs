using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Parcours pédagogique hors ligne. Les flux sont des illustrations, pas des mesures.</summary>
public class DemoTour : MonoBehaviour
{
    public static readonly string[] Titles = { "RÉSEAU", "TRAITEMENT", "STOCKAGE", "REFROIDISSEMENT", "ÉLECTRICITÉ", "SUPERVISION" };
    public static readonly string[] Goals = {
        "Activez la borne 01 à gauche : faites entrer la requête.",
        "Borne 02 : inspectez la baie de démonstration à droite, puis remettez-la en place.",
        "Rejoignez la borne 03 à gauche : conservez le fichier.",
        "Borne 04 : diagnostiquez une entrée d'air trop chaude et rétablissez le confinement.",
        "Borne 05 : rejoignez le local électrique et observez le secours UPS / groupe.",
        "Borne 06 : lisez l'incident au mur de supervision et préparez son relais humain." };
    public static readonly string[] Lessons = {
        "Le réseau relie votre appareil au service distant. Les équipements réseau acheminent les échanges vers les machines qui hébergent le service.",
        "Un logiciel exécuté sur des serveurs reçoit la demande, vérifie ce qu'il doit faire et organise l'enregistrement. Le serveur est une machine ; le service est un logiciel.",
        "Le stockage conserve le fichier pour le retrouver après l'envoi. Des copies peuvent améliorer la disponibilité ; elles ne remplacent pas une stratégie de sauvegarde.",
        "Le refroidissement fonctionne en continu. Une fuite de confinement favorise les mélanges d'air : corriger la cause évite de compenser uniquement par davantage de ventilation.",
        "Serveurs, réseau et refroidissement dépendent d'une alimentation organisée. Ici, une batterie illustre un relais temporaire ; la continuité dépend de toute l'installation.",
        "Des outils signalent les événements et des personnes organisent les interventions. Les équipes du site et celles du service coopèrent, avec des responsabilités différentes." };

    public DemoStation[] stations;
    public P1LearningModule learning;
    public FacilityOperations facility;
    public int Step { get; private set; }
    public bool Busy { get; private set; }
    public bool Finished => Step >= Titles.Length;
    public int Actions { get; private set; }
    public bool CoolingRestored { get; private set; }
    public bool BackupShown { get; private set; }
    public bool AlertAcknowledged { get; private set; }
    public string Message { get; private set; }
    public string HoverHint { get; private set; }
    public Transform flowRoot;
    public Transform packet;
    public Renderer[] thermal;
    public Material coldMaterial, warmMaterial, packetMaterial;
    DemoStation _hover;
    float _nextAction;
    Camera _camera;
    int _drawnStep=-1;
    string _drawnMessage, _drawnHint;
    bool _drawnBusy;

    void Start()
    {
        _camera = Camera.main;
        Message = "Votre photo devient notre fil conducteur. Explorez les six bornes : elles montrent les ressources physiques nécessaires au service.";
        UpdateHud();
    }

    void Update()
    {
        if (_camera == null) _camera = Camera.main;
        _hover = null;
        bool pc = InputModeManager.Instance == null || InputModeManager.Instance.IsPC;
        if (pc && _camera != null && Physics.Raycast(_camera.ViewportPointToRay(new Vector3(.5f,.5f,0)), out var hit, 4.5f))
            _hover = hit.collider.GetComponentInParent<DemoStation>();
        var keyboard = Keyboard.current;
        var controller = _camera != null ? _camera.GetComponentInParent<PCPlayerController>() : null;
        if (pc && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            controller?.TogglePause();
        }
        if (pc && controller != null && !controller.InputPaused && _hover != null &&
            ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) || (keyboard != null && keyboard.eKey.wasPressedThisFrame)))
            _hover.Activate();
        if (pc && Finished && keyboard != null && keyboard.enterKey.wasPressedThisFrame) ReturnToRoom();
        HoverHint = _hover == null ? "Visez une borne • clic gauche ou E • Échap : libérer la souris" :
            Finished ? "Clic / E : retourner dans la chambre" : _hover.index == Step ? "CLIC / E — " + ActionLabel() :
            _hover.index < Step ? "CLIC / E — relire cette étape" : "Suivez d'abord l'objectif affiché";
        UpdateHud();
    }

    string ActionLabel()
    {
        if (Step == 1) return learning!=null&&learning.ComputeStarted?"Manipuler la baie à droite":"Commencer l'inspection de baie";
        if (Step == 3) return learning!=null&&learning.CoolingStarted?"Corriger la fuite à la porte":"Diagnostiquer l'anomalie d'air";
        if (Step == 4) return facility!=null&&facility.PowerStarted?"Rejoindre le local électrique":"Observer la perte du secteur";
        if (Step == 5) return facility!=null&&facility.SupervisionStarted?"Lire le mur de supervision":"Ouvrir le suivi d'incident";
        return "Observer le rôle de cet équipement";
    }

    public void Activate(int index)
    {
        if (Busy || Time.unscaledTime < _nextAction) return;
        if (Finished) { ReturnToRoom(); return; }
        if (index < Step) { Message = Lessons[index]; return; }
        if (index != Step) { Message = "Cette borne sera disponible ensuite. " + Goals[Step]; return; }
        if(Step==1&&learning.ComputeStarted||Step==3&&learning.CoolingStarted)return;
        if(Step==4&&facility.PowerStarted||Step==5&&facility.SupervisionStarted)return;
        Actions++;
        _nextAction = Time.unscaledTime + .5f;
        if (Step == 1)
        {
            learning.BeginCompute();
            Message="Baie 01 à droite : poignée, tiroir, repères CPU / RAM / NVMe. Inspectez puis réinsérez pour continuer.\nDémonstration hors production, sans maintenance sur un service réel.";
            return;
        }
        if(Step==3)
        {
            learning.BeginCooling();
            Message="Porte de confinement à Z=24, juste avant cette console. Lisez l'afficheur à droite, comparez les actions puis fermez la porte avec sa poignée verte.\nScénario fictif, temps accéléré.";
            return;
        }
        if (Step == 4)
        {
            facility.BeginPower();BackupShown=true;
            Message="PERTE SECTEUR • UPS AUTOMATIQUEMENT EN SOUTIEN\nLocal électrique à gauche, porte à Z=30. Observez l'UPS et utilisez le pupitre de démonstration. Aucun appareil réel piloté.";
            return;
        }
        if(Step==5)
        {
            facility.BeginSupervision();Message="Mur de supervision au fond : chronologie, énergie et air. Lisez l'incident puis préparez un rapport pour l'équipe concernée. Données fictives, aucune communication externe.";return;
        }
        Message = Lessons[Step];
        StartCoroutine(ShowFlow(true));
    }

    public void CompletePhysicalStep(int expected,string explanation)
    {
        if(Busy||Step!=expected)return;
        if(expected==1&&!learning.ComputeCompleted||expected==3&&!learning.CoolingRecovered)return;
        if(expected==3)CoolingRestored=true;
        Step++;Message=explanation;UpdateHud();
    }

    public void CompleteFacilityStep(int expected,string explanation)
    {
        if(Busy||Step!=expected)return;
        if(expected==4&&!facility.PowerCompleted||expected==5&&!facility.ReportPrepared)return;
        if(expected==5)AlertAcknowledged=true;
        Step++;Message=explanation;UpdateHud();
    }

    IEnumerator ShowFlow(bool advance)
    {
        Busy = true;
        flowRoot.gameObject.SetActive(true);
        int stage = Step;
        var origin = stations[stage].transform.position + Vector3.up * 1.5f;
        var rack = GameObject.Find("Baie_01").transform;
        var target = rack.position + rack.rotation * new Vector3(0, stage == 0 ? 1.65f : stage == 2 ? .55f : 1.15f, -.59f);
        var line = flowRoot.GetComponent<LineRenderer>();
        line.sharedMaterial = stage==3 ? coldMaterial : stage==4 ? warmMaterial : packetMaterial;
        packet.GetComponent<Renderer>().sharedMaterial = line.sharedMaterial;
        line.SetPositions(new[] {origin, new Vector3(origin.x,3.1f,origin.z), new Vector3(target.x,3.1f,target.z),target});
        var start = Time.unscaledTime;
        while (Time.unscaledTime - start < 2.8f)
        {
            float travel = Mathf.Clamp01((Time.unscaledTime-start)/2.8f)*3;
            int segment = Mathf.Min(2,(int)travel);
            packet.position = Vector3.Lerp(line.GetPosition(segment),line.GetPosition(segment+1),travel-segment);
            yield return null;
        }
        flowRoot.gameObject.SetActive(false);
        if (advance) Step++;
        Busy = false;
        if (Finished) Message = "PHOTO DISPONIBLE — ALERTE TRANSMISE\nLe service dépend du réseau, de serveurs, du stockage, d'énergie, de refroidissement et du travail humain. Revenez dans la chambre pour retrouver la photo.";
        UpdateHud();
    }

    public void ReturnToRoom()
    {
        if (!Finished || Busy) return;
        DemoSession.Completed = true;
        InputModeManager.Instance?.PrepareNextScene();
        SceneManager.LoadSceneAsync("Chambre");
        Busy = true;
    }

    void UpdateHud()
    {
        if (_drawnStep==Step && _drawnMessage==Message && _drawnHint==HoverHint && _drawnBusy==Busy) return;
        _drawnStep=Step; _drawnMessage=Message; _drawnHint=HoverHint; _drawnBusy=Busy;
        for(int i=0;i<stations.Length;i++)
        {
            var station=stations[i];
            station.indicator.sharedMaterial=i<Step?coldMaterial:i==Step?packetMaterial:warmMaterial;
            station.caption.text=(i+1).ToString("00")+"  "+Titles[i]+"\n"+(i<Step?"DÉCOUVERT • RELIRE":i==Step?ActionLabel():"À DÉCOUVRIR");
            if(station.information!=null) station.information.text=Finished ? Message+"\nClic / E : retour chambre" :
                i==Step ? Goals[Step]+"\n\n"+Message+"\n\n"+(Busy?"Flux illustratif en cours…":"Visez cet écran • clic ou E") :
                i<Step ? Lessons[i] : "Suivez les consoles numérotées.\n\n"+Titles[i]+"\nÉtape "+(i+1)+" / 6";
        }
    }

    public static RectTransform Panel(Transform parent,string name,Vector2 min,Vector2 max)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)); go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
        var image=go.GetComponent<Image>(); image.color=new Color(.025f,.045f,.08f,.94f); image.raycastTarget=false;
        return rect;
    }
    public static Text Label(Transform parent,string name,int size,Vector2 min,Vector2 max,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text)); go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
        var text=go.GetComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=size;
        text.color=color; text.alignment=TextAnchor.MiddleLeft; text.raycastTarget=false; text.supportRichText=true;
        return text;
    }
}

public static class DemoSession
{
    public static bool Completed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { Completed=false; }
}
