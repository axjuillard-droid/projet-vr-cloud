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
        "Activez la borne 02 à droite : faites traiter la photo.",
        "Rejoignez la borne 03 à gauche : conservez le fichier.",
        "Borne 04 à droite : observez la chaleur, puis activez le refroidissement.",
        "Borne 05 au fond à gauche : observez le relais de l'alimentation.",
        "Borne 06 au fond à droite : transmettez l'alerte à l'équipe." };
    public static readonly string[] Lessons = {
        "Le réseau relie votre appareil au service distant. Les équipements réseau acheminent les échanges vers les machines qui hébergent le service.",
        "Un logiciel exécuté sur des serveurs reçoit la demande, vérifie ce qu'il doit faire et organise l'enregistrement. Le serveur est une machine ; le service est un logiciel.",
        "Le stockage conserve le fichier pour le retrouver après l'envoi. Des copies peuvent améliorer la disponibilité ; elles ne remplacent pas une stratégie de sauvegarde.",
        "Les équipements consomment de l'électricité et produisent de la chaleur. Ici, l'air frais traverse les baies ; l'air réchauffé repart vers le système de refroidissement.",
        "Serveurs, réseau et refroidissement dépendent d'une alimentation organisée. Ici, une batterie illustre un relais temporaire ; la continuité dépend de toute l'installation.",
        "Des outils signalent les événements et des personnes organisent les interventions. Les équipes du site et celles du service coopèrent, avec des responsabilités différentes." };

    public DemoStation[] stations;
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
    Text _objective, _body, _list, _hint, _heading;
    Image _progress;
    DemoStation _hover;
    float _nextAction;
    bool _heatShown, _mainsInterrupted;
    Camera _camera;
    int _drawnStep=-1;
    string _drawnMessage, _drawnHint;
    bool _drawnBusy;

    void Start()
    {
        _camera = Camera.main;
        BuildHud();
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
        if (Step == 3) return _heatShown ? "Activer le refroidissement" : "Révéler la chaleur";
        if (Step == 4) return _mainsInterrupted ? "Rétablir l'alimentation principale" : "Illustrer le relais batterie";
        if (Step == 5) return "Transmettre l'alerte à l'équipe";
        return "Observer le rôle de cet équipement";
    }

    public void Activate(int index)
    {
        if (Busy || Time.unscaledTime < _nextAction) return;
        if (Finished) { ReturnToRoom(); return; }
        if (index < Step) { Message = Lessons[index]; return; }
        if (index != Step) { Message = "Cette borne sera disponible ensuite. " + Goals[Step]; return; }
        Actions++;
        _nextAction = Time.unscaledTime + .5f;
        if (Step == 3 && !_heatShown)
        {
            _heatShown = true;
            foreach (var r in thermal) r.sharedMaterial = warmMaterial;
            Message = "VUE THERMIQUE SCHÉMATIQUE\nLa chaleur doit être évacuée. Activez à nouveau cette borne pour montrer un circuit de refroidissement à air.";
            return;
        }
        if (Step == 4 && !_mainsInterrupted)
        {
            _mainsInterrupted = true;
            BackupShown = true;
            Message = "RELAIS TEMPORAIRE ILLUSTRÉ\nLa batterie prend ici le relais de la source principale. Aucune autonomie ni panne réelle n'est simulée. Activez à nouveau pour rétablir la source.";
            StartCoroutine(ShowFlow(false));
            return;
        }
        if (Step == 3) { CoolingRestored = true; foreach (var r in thermal) r.sharedMaterial = coldMaterial; }
        if (Step == 5) AlertAcknowledged = true;
        Message = Lessons[Step];
        StartCoroutine(ShowFlow(true));
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
        if (_body == null) return;
        if (_drawnStep==Step && _drawnMessage==Message && _drawnHint==HoverHint && _drawnBusy==Busy) return;
        _drawnStep=Step; _drawnMessage=Message; _drawnHint=HoverHint; _drawnBusy=Busy;
        _heading.text = Finished ? "SYNTHÈSE  /  PHOTO DISPONIBLE" : "DÉCOUVERTE  " + (Step+1).ToString("00") + " / 06  •  " + Titles[Step];
        _objective.text = Finished ? "Retour chambre : Entrée ou sélectionnez une borne" : Goals[Step];
        _body.text = Message;
        _hint.text = Busy ? "Observation en cours…  Flux illustratif, non mesuré" : HoverHint;
        _progress.fillAmount = Step/6f;
        _progress.rectTransform.localScale = new Vector3(Step/6f,1,1);
        string rows = "VOTRE PARCOURS\n\n";
        for (int i=0;i<6;i++)
        {
            rows += (i < Step ? "<color=#6EE7B7>✓  " : i==Step ? "<color=#67E8F9>›  " : "<color=#91A4BE>   ") + (i+1).ToString("00") + "  " + Titles[i] + "</color>\n\n";
            if (stations[i].indicator != null) stations[i].indicator.sharedMaterial = i < Step ? coldMaterial : i==Step ? packetMaterial : warmMaterial;
            stations[i].caption.text = (i+1).ToString("00") + "  " + Titles[i] + "\n" + (i < Step ? "DÉCOUVERT • RELIRE" : i==Step ? ActionLabel() : "À DÉCOUVRIR");
        }
        _list.text = rows;
    }

    void BuildHud()
    {
        var hud = new GameObject("Demo_HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        hud.transform.SetParent(transform,false);
        var canvas = hud.GetComponent<Canvas>();
        bool vr = InputModeManager.Instance != null && InputModeManager.Instance.IsVR;
        canvas.renderMode = vr ? RenderMode.WorldSpace : RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = _camera;
        canvas.planeDistance = .65f;
        if (vr)
        {
            hud.transform.SetParent(_camera.transform,false);
            hud.transform.localPosition = new Vector3(0,-.45f,2);
            hud.transform.localScale = Vector3.one*.0016f;
            hud.GetComponent<RectTransform>().sizeDelta = new Vector2(1280,720);
        }
        else
        {
            var scale=hud.GetComponent<CanvasScaler>();
            scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution=new Vector2(1280,720);
            scale.matchWidthOrHeight=.5f;
        }
        var top=Panel(hud.transform,"Objectif",new Vector2(.025f,.85f),new Vector2(.975f,.98f));
        _heading=Label(top,"Titre",20,new Vector2(.02f,.55f),new Vector2(.98f,.98f),new Color(.4f,.9f,1));
        _objective=Label(top,"Consigne",23,new Vector2(.02f,.05f),new Vector2(.98f,.55f),Color.white);
        var left=Panel(hud.transform,"Parcours",new Vector2(.025f,.35f),new Vector2(.24f,.82f));
        _list=Label(left,"Étapes",18,new Vector2(.06f,.03f),new Vector2(.98f,.97f),Color.white);
        var bottom=Panel(hud.transform,"Narration",new Vector2(.27f,.025f),new Vector2(.975f,.28f));
        _body=Label(bottom,"Sous_titres",21,new Vector2(.025f,.22f),new Vector2(.975f,.94f),Color.white);
        _hint=Label(bottom,"Action",17,new Vector2(.025f,.02f),new Vector2(.975f,.2f),new Color(.4f,.9f,1));
        var bar=Panel(hud.transform,"Progression",new Vector2(.025f,.83f),new Vector2(.975f,.837f));
        _progress=bar.GetComponent<Image>(); _progress.color=new Color(.2f,.8f,.8f);
        _progress.type=Image.Type.Filled; _progress.fillMethod=Image.FillMethod.Horizontal;
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
