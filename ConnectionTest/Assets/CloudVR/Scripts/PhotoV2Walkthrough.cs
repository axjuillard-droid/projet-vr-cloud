using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed partial class PhotoV2Walkthrough : MonoBehaviour
{
    public Camera view;
    public PCPlayerController player;
    public GameObject[] targets;
    public GameObject[] barriers;
    public GameObject[] guides;
    public GameObject sasEntrance;
    public GameObject badgeReader;
    public GameObject badgeApproved;
    public PhotoV2PatchCord patchCord;
    public ServerBladeInteraction schoolRack;
    public Transform computeInput;
    public Transform computeOutput;
    public RawImage previewDisplay;
    public RawImage sendImage;
    public Collider ramControl;
    public Text ramLabel;
    Texture2D _pixelsInMemory;
    public PhotoV2Storage storage;
    public PhotoV2Energy energy;
    public PhotoV2GridRoute grid;
    public PhotoV2Cooling cooling;
    public PhotoV2Water water;
    public Text previewLabel;
    public NetworkRideVisuals film;
    public Vector3 arrival;

    public bool IsTravelling { get; private set; }
    public int Step => _step;
    public string PhotoStatus => SignalInTransit ? "En transit" : Status[_step];
    const float InteractionRange = 3.1f;
    const int NetworkStep = 3;
    const int ComputeStep = 4;
    const int PreviewStep = 6;
    const int StorageStep = 7;
    const int ReadStep = 8;
    bool _saving;
    bool _reading;
    public bool SamePhotoRead => storage && storage.Shows(_photo);
    bool _computeBusy;
    Texture2D _preview;
    public bool PreviewReady => _preview != null;
    const float ScreenSeconds = 1.35f;
    const float FibreSeconds = 2.75f;
    const float ArrivalSeconds = 1.85f;
    const float TravelSeconds = ScreenSeconds + FibreSeconds + ArrivalSeconds;
    AudioSource _sound;
    AudioClip _transit;
    AudioClip _confirmation;
    float _travelProgress;

    static readonly string[] Instructions =
    {
        "Envoyer la photo",
        "Ouvrir le sas à l’entrée du bâtiment",
        "Entrer dans le sas et viser le lecteur de badge",
        "Connecter le port réseau",
        "Ouvrir la baie école",
        "Extraire le tiroir",
        "Créer l’aperçu",
        "Sélectionner la banque de stockage libre",
        "Relire la banque B sur l’écran client",
        "Suivre la rampe et viser le repère BELVÉDÈRE"
    };
    static readonly string[] Status =
    {
        "Prête", "En attente", "En attente", "En attente", "Reçue",
        "Reçue", "Reçue", "Traitée", "Enregistrée", "Lisible", "Lisible"
    };

    int _step;
    Texture2D _photo;
    GUIStyle _heading;
    GUIStyle _instruction;
    GUIStyle _small;
    GUIStyle _marker;

    void Start()
    {
        _sound = gameObject.AddComponent<AudioSource>();
        _sound.playOnAwake = false;
        _transit = ProceduralAudioService.CreateFiberTransitSound();
        _confirmation = ProceduralAudioService.CreateRfidAccessBeep();
        _photo = CreateSamplePhoto();
        sendImage.texture = _photo;
        ShowGuide();
        Debug.Log("CLOUDVR_V2_READY");
    }

    bool SignalInTransit => _step == ComputeStep && patchCord && !patchCord.SignalDelivered;
    string Instruction
    {
        get
        {
            if (_step == NetworkStep && patchCord) return patchCord.Instruction;
            if (SignalInTransit) return "Suivre le signal vers la baie école";
            if (_step == PreviewStep)
            {
                if (_computeBusy) return "Le CPU regroupe les pixels pour créer l’aperçu";
                return _pixelsInMemory ? "RAM chargée · lancez le calcul sur le CPU" :
                    "Chargez la photo en RAM, la mémoire de travail";
            }
            if (_saving) return "Écriture en cours dans la banque B";
            if (_step == StorageStep && storage && !storage.ReadyToRoute)
                return "Aperçu créé. Suivez l’original vers le stockage.";
            if (_step == ReadStep && storage)
            {
                if (_reading) return "Suivez l’original de la banque vers l’écran";
                if (storage.Readable) return "Comparez les tailles et confirmez le fichier original";
            }
            return Instructions[_step];
        }
    }

    void Update()
    {
        if (IsTravelling) return;
        var keyboard = Keyboard.current;
        if (keyboard?.escapeKey.wasPressedThisFrame == true) player.TogglePause();
        if (player.InputPaused || player.SiteMapOpen || keyboard?.mKey.wasReleasedThisFrame == true) return;
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
                Cursor.lockState = CursorLockMode.Locked;
            return;
        }
        if (keyboard?.eKey.wasPressedThisFrame == true || Mouse.current?.leftButton.wasPressedThisFrame == true)
            TryAdvance(view.ViewportPointToRay(new Vector3(.5f, .5f)));
    }

    public bool TryAdvance(Ray ray)
    {
        if (IsTravelling || player.InputPaused || player.SiteMapOpen || SignalInTransit || _computeBusy || _saving || _reading || !Physics.Raycast(ray, out var hit, InteractionRange))
            return false;
        if (_step == targets.Length)
        {
            if (!energy) return false;
            if ((!grid || grid.Stop == 0) && energy.TrySelect(hit.collider)) return true;
            bool selected = grid && grid.TrySelect(hit.collider,energy);
            if (!selected && AtCooling) selected = cooling.TrySelect(hit.collider);
            if (!selected && AtWater) selected = water.TrySelect(hit.collider);
            if (!selected) return false;
            _sound.PlayOneShot(_confirmation,.18f);
            return true;
        }
        if (_step == NetworkStep && patchCord)
        {
            if (!patchCord.TrySelect(hit.collider)) return false;
            _sound.PlayOneShot(_confirmation, .18f);
            if (patchCord.Connected) CompleteStep();
            return true;
        }
        if (_step == StorageStep && storage)
        {
            if (!storage.ReadyToRoute) return false;
            if (hit.collider == storage.busyBank)
            {
                storage.ReportBusy();
                return false;
            }
            if (hit.collider.gameObject != targets[_step]) return false;
            StartCoroutine(RecordPhoto());
            return true;
        }
        if (_step == PreviewStep && ramControl && !_pixelsInMemory)
        {
            if (hit.collider != ramControl) return false;
            _pixelsInMemory = _photo;
            ramLabel.text = $"RAM · {_pixelsInMemory.width} × {_pixelsInMemory.height}";
            previewLabel.text = "CPU · CRÉER L’APERÇU";
            _sound.PlayOneShot(_confirmation,.18f);
            return true;
        }
        if (_step == ReadStep && storage)
        {
            if (storage.Readable)
            {
                if (!storage.ConfirmOriginal(hit.collider)) return false;
                _sound.PlayOneShot(_confirmation,.18f);
                CompleteStep();
                return true;
            }
            if (hit.collider.gameObject != targets[_step] || !storage.Saved) return false;
            StartCoroutine(ReadStoredPhoto());
            return true;
        }
        if (hit.collider.gameObject != targets[_step]) return false;
        if (schoolRack && _step >= ComputeStep && _step <= PreviewStep)
        {
            StartCoroutine(AdvanceCompute());
            return true;
        }
        if (targets[_step] == badgeReader)
        {
            // Présenter le badge depuis l’intérieur, après avoir dégagé la porte.
            if (player.transform.position.z < sasEntrance.transform.position.z + .6f) return false;
            sasEntrance.SetActive(true);
            badgeApproved.SetActive(true);
        }
        if (_step == 0)
        {
            StartCoroutine(PlayTransit());
            return true;
        }
        _sound.PlayOneShot(_confirmation, .18f);
        CompleteStep();
        return true;
    }

    void CompleteStep()
    {
        var barrier = barriers[_step];
        if (barrier) barrier.SetActive(false);
        var target = targets[_step];
        if (target == badgeReader || (_step == NetworkStep && patchCord) || (_step == PreviewStep && schoolRack) || (_step == ReadStep && storage)
            || (_step == StorageStep && storage) || _step == targets.Length - 1)
            target.GetComponent<Collider>().enabled = false;
        else
            target.SetActive(false);
        _step++;
        ShowGuide();
        if (_step == targets.Length && energy) energy.gameObject.SetActive(true);
        Debug.Log($"CLOUDVR_V2_STEP {_step}/{targets.Length}");
    }

    IEnumerator RecordPhoto()
    {
        _saving = true;
        yield return storage.Save();
        _sound.PlayOneShot(_confirmation,.18f);
        CompleteStep();
        _saving = false;
    }

    IEnumerator ReadStoredPhoto()
    {
        _reading = true;
        yield return storage.Read(_preview);
        _reading = false;
    }

    IEnumerator AdvanceCompute()
    {
        _computeBusy = true;
        if (_step == PreviewStep)
        {
            previewLabel.text = "CALCUL EN COURS";
            yield return new WaitForSeconds(.7f);
            // Réduction par moyenne de blocs de 4 × 4 pixels de la même image.
            var sourcePixels = _pixelsInMemory ? _pixelsInMemory : _photo;
            _preview = new Texture2D(sourcePixels.width / 4, sourcePixels.height / 4);
            for (int y = 0; y < _preview.height; y++)
                for (int x = 0; x < _preview.width; x++)
                {
                    Color sum = Color.clear;
                    for (int dy = 0; dy < 4; dy++)
                        for (int dx = 0; dx < 4; dx++) sum += sourcePixels.GetPixel(x * 4 + dx, y * 4 + dy);
                    _preview.SetPixel(x, y, sum / 16);
                }
            _preview.Apply();
            _pixelsInMemory = null;
            if (ramLabel) ramLabel.text = "RAM LIBÉRÉE";
            previewDisplay.texture = _preview;
            previewDisplay.enabled = true;
            previewLabel.text = "";
            _sound.PlayOneShot(_confirmation,.18f);
            if (storage)
            {
                var source = schoolRack.serverBlade.TransformPoint(new Vector3(0,.11f,.42f));
                StartCoroutine(storage.Deliver(_photo, source));
            }
            Debug.Log("CLOUDVR_V2_PREVIEW_READY");
        }
        else
        {
            schoolRack.Interact();
            while (schoolRack.IsAnimating) yield return null;
        }
        CompleteStep();
        _computeBusy = false;
    }

    static Texture2D CreateSamplePhoto()
    {
        const int width = 96, height = 64;
        var image = new Texture2D(width,height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float u = x / (float)width;
                float v = y / (float)height;
                float ridge = .37f + .22f * Mathf.Sin(u * 3.14f) + .07f * Mathf.Sin(u * 15);
                var sky = Color.Lerp(new Color(.86f,.72f,.53f),new Color(.16f,.38f,.62f),v);
                var land = v < .27f + .06f * Mathf.Sin(u * 12) ? new Color(.16f,.33f,.30f) :
                    Color.Lerp(new Color(.14f,.24f,.29f),new Color(.46f,.52f,.55f),v);
                image.SetPixel(x,y,v > ridge ? sky : land);
            }
        image.Apply();
        return image;
    }

    IEnumerator PlayTransit()
    {
        IsTravelling = true;
        Debug.Log("CLOUDVR_V2_TRANSIT_START");
        bool wasEnabled = player.enabled;
        player.enabled = false;
        var originalMask = view.cullingMask;
        var originalBackground = view.backgroundColor;
        var localPosition = view.transform.localPosition;
        var cameraStart = view.transform.position;
        var rotationStart = view.transform.rotation;
        var photoSize = sendImage.rectTransform.sizeDelta;
        var screenPoint = sendImage.transform.position;
        var screenNear = screenPoint + Vector3.back*.38f;
        var stopInCourtyard = arrival + localPosition;
        var fibreEntry = patchCord.ingress.GetPosition(0);
        var fibreFoot = patchCord.ingress.GetPosition(1);
        var fibreTop = patchCord.ingress.GetPosition(2);
        var arrivalStart = stopInCourtyard + new Vector3(.85f,.18f,.65f);
        var arrivalLook = Quaternion.LookRotation(Vector3.Lerp(fibreFoot,fibreTop,.32f)-arrivalStart);
        var signal = patchCord.signal;
        var signalHome = signal.position;

        _sound.PlayOneShot(_transit, .26f);
        float started = Time.unscaledTime;
        while (Time.unscaledTime-started < ScreenSeconds)
        {
            float elapsed = Time.unscaledTime-started;
            _travelProgress = elapsed/TravelSeconds;
            float t = Mathf.SmoothStep(0,1,elapsed/ScreenSeconds);
            float convert = Mathf.SmoothStep(0,1,Mathf.InverseLerp(.84f,ScreenSeconds,elapsed));
            sendImage.rectTransform.sizeDelta = Vector2.Lerp(photoSize,new Vector2(20,7),convert);
            sendImage.color = Color.Lerp(Color.white,new Color(.27f,.87f,1f),convert);
            view.transform.SetPositionAndRotation(Vector3.Lerp(cameraStart,screenNear,t),
                Quaternion.Slerp(rotationStart,Quaternion.identity,t));
            yield return null;
        }

        sendImage.enabled = false;
        film.gameObject.SetActive(true);
        view.cullingMask = 1 << NetworkRideVisuals.Layer;
        view.backgroundColor = new Color(.012f,.022f,.038f);
        started = Time.unscaledTime;
        while (Time.unscaledTime-started < FibreSeconds)
        {
            float elapsed = Time.unscaledTime-started;
            _travelProgress = (ScreenSeconds+elapsed)/TravelSeconds;
            float t = elapsed/FibreSeconds;
            view.transform.SetPositionAndRotation(film.transform.TransformPoint(new Vector3(0,3.6f,2+42*t)),
                film.transform.rotation);
            film.Animate(elapsed);
            yield return null;
        }

        film.gameObject.SetActive(false);
        view.cullingMask = originalMask;
        view.backgroundColor = originalBackground;
        signal.position = fibreEntry;
        signal.gameObject.SetActive(true);
        started = Time.unscaledTime;
        while (Time.unscaledTime-started < ArrivalSeconds)
        {
            float elapsed = Time.unscaledTime-started;
            _travelProgress = (ScreenSeconds+FibreSeconds+elapsed)/TravelSeconds;
            float t = elapsed/ArrivalSeconds;
            signal.position = t < .4f ? Vector3.Lerp(fibreEntry,fibreFoot,t/.4f) :
                Vector3.Lerp(fibreFoot,fibreTop,Mathf.InverseLerp(.4f,1,t));
            float settle = Mathf.SmoothStep(0,1,Mathf.InverseLerp(.52f,1,t));
            view.transform.SetPositionAndRotation(Vector3.Lerp(arrivalStart,stopInCourtyard,settle),
                Quaternion.Slerp(arrivalLook,Quaternion.identity,settle));
            yield return null;
        }

        signal.gameObject.SetActive(false);
        signal.position = signalHome;
        var capsule = player.GetComponent<CharacterController>();
        capsule.enabled = false;
        player.transform.SetPositionAndRotation(arrival,Quaternion.identity);
        view.transform.localPosition = localPosition;
        player.ResetLook();
        capsule.enabled = true;
        CompleteStep();
        player.enabled = wasEnabled;
        IsTravelling = false;
        _sound.PlayOneShot(_confirmation, .18f);
        Debug.Log("CLOUDVR_V2_TRANSIT_COMPLETE");
    }

    void ShowGuide()
    {
        for (int i = 0; i < guides.Length; i++) guides[i].SetActive(i == _step);
    }

    void OnDestroy()
    {
        if (_photo) Destroy(_photo);
        if (_preview) Destroy(_preview);
        if (_transit) Destroy(_transit);
        if (_confirmation) Destroy(_confirmation);
    }

    public Collider GuideTarget
    {
        get
        {
            if (IsTravelling || player.InputPaused || SignalInTransit || _computeBusy || _saving || _reading) return null;
            if (_step < targets.Length)
            {
                if (_step == NetworkStep && patchCord) return patchCord.Held ? patchCord.portA : patchCord.plug;
                if (_step == PreviewStep && ramControl && !_pixelsInMemory) return ramControl;
                if (_step == StorageStep && storage && !storage.ReadyToRoute) return null;
                if (_step == ReadStep && storage && storage.Readable) return null;
                return targets[_step].GetComponent<Collider>();
            }

            if (AtWater) return water.NextControl;
            if (AtCooling)
            {
                if (!cooling.Open) return cooling.valve;
                return cooling.Stabilized ? cooling.observation : null;
            }
            if (!grid || grid.Stop == 0)
            {
                if (!energy || energy.Changing) return null;
                if (energy.ReadyForGrid && grid) return grid.departure;
                return energy.RequestedRows > energy.ActiveRows ? energy.PendingBreaker : energy.control;
            }
            return grid.Stop == 1 ? grid.substation : grid.viewpoint;
        }
    }

    void DrawGuide()
    {
        Collider target = GuideTarget;
        if (!target || !target.enabled) return;

        Vector3 point = target.bounds.center;
        Vector3 screen = view.WorldToScreenPoint(point);
        Vector3 offset = point - view.transform.position;
        float centerX = Screen.width * .5f;
        float centerY = Screen.height * .5f;
        float y = Screen.height-screen.y;
        float panelWidth = _step == targets.Length ? (AtWater ? 545 : 518) : 480;
        float panelBottom = _step == targets.Length ? (AtWater ? 247 : 181) : 113;
        bool inPanel = screen.x < panelWidth + 34 && y < panelBottom + 34;
        bool onScreen = screen.z > 0 && screen.x > 32 && screen.x < Screen.width - 32
            && y > 32 && y < Screen.height - 32 && !inPanel;
        if (!onScreen)
        {
            float horizontal = Vector3.Dot(offset,view.transform.right);
            float vertical = Vector3.Dot(offset,view.transform.up);
            bool sideways = screen.z <= 0 || Mathf.Abs(horizontal) > Mathf.Abs(vertical);
            Rect arrow;
            string symbol;
            if (sideways)
            {
                bool right = horizontal >= 0;
                arrow = new Rect(right ? Screen.width-68 : 16,centerY-16,52,32);
                symbol = right ? "▶" : "◀";
            }
            else
            {
                bool up = vertical >= 0;
                float safeX = Mathf.Clamp(screen.x,16,Screen.width-68);
                arrow = new Rect(safeX,up ? (inPanel ? panelBottom+6 : 16) : Screen.height-54,52,32);
                symbol = up ? "▲" : "▼";
            }
            GUI.Box(arrow,GUIContent.none);
            GUI.Label(arrow,symbol,_marker);
            return;
        }

        // Respecter les obstacles et supprimer le repère si la cible est masquée.
        if (!Physics.Raycast(view.transform.position,offset.normalized,out var hit,offset.magnitude+.05f)
            || hit.collider != target) return;
        bool aimed = Physics.Raycast(view.ViewportPointToRay(new Vector3(.5f,.5f)),out var centerHit,InteractionRange)
            && centerHit.collider == target;
        if (aimed)
        {
            GUI.Box(new Rect(centerX-54,centerY+22,108,32),GUIContent.none);
            GUI.Label(new Rect(centerX-54,centerY+22,108,32),"Clic ou E",_marker);
        }
        else
            GUI.Label(new Rect(screen.x-14,y-31,28,24),"▼",_marker);
    }

    void OnGUI()
    {
        _heading ??= new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
        _heading.normal.textColor = new Color(.45f, .94f, 1f);
        _instruction ??= new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
        _small ??= new GUIStyle(GUI.skin.label) { fontSize = 15 };
        _marker ??= new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        _marker.normal.textColor = _heading.normal.textColor;
        if (IsTravelling)
        {
            DrawTransit();
            return;
        }
        if (player.SiteMapOpen)
        {
            DrawSiteMap();
            return;
        }
        DrawMovementKeys();
        if (_step == targets.Length && energy)
        {
            DrawEnergy();
            DrawGuide();
            return;
        }
        string action = player.InputPaused ? "Pause · Échap pour reprendre" :
            _step < Instructions.Length ? Instruction : "Retournez-vous pour retrouver la baie école.";
        GUI.Box(new Rect(18, 18, Mathf.Min(480, Screen.width - 36), 95), GUIContent.none);
        GUI.Label(new Rect(30, 26, 430, 28), "CLOUDVR V2 · PARCOURS PHOTO", _heading);
        GUI.Label(new Rect(30, 54, Mathf.Min(452, Screen.width - 60), 58), action, _instruction);
        GUI.Label(new Rect(Screen.width * .5f - 5, Screen.height * .5f - 10, 14, 24), "+", _small);
        GUI.Box(new Rect(18, Screen.height - 66, 217, 46), GUIContent.none);
        if (_photo) GUI.DrawTexture(new Rect(26, Screen.height - 59, 45, 32), _photo);
        GUI.Label(new Rect(80, Screen.height - 58, 151, 34), $"Photo · {PhotoStatus}", _small);
        DrawGuide();
    }

    bool AtCooling => grid && grid.Stop == 3 && cooling;
    bool AtWater => AtCooling && cooling.Observed && water;

    void DrawEnergy()
    {
        if (AtWater)
        {
            DrawWater();
            return;
        }
        GUI.Box(new Rect(18,18,Mathf.Min(518,Screen.width - 36),163),GUIContent.none);
        GUI.Label(new Rect(30,26,425,27),AtCooling ? "CLOUDVR V2 · REFROIDISSEMENT" : "CLOUDVR V2 · ÉNERGIE",_heading);
        string action = player.InputPaused ? "Pause · Échap pour reprendre" :
            AtCooling && cooling.Stabilized && !cooling.Observed ? "Viser ÉCHANGEUR · REJET À L’AIR" :
            AtCooling ? cooling.Instruction :
            grid && grid.Stop > 0 ? grid.Instruction :
            energy.Instruction;
        GUI.Label(new Rect(30,52,480,48),action,_instruction);
        if (AtCooling)
        {
            GUI.Label(new Rect(30,108,482,24),cooling.Reading + " · simulation",_small);
            GUI.Label(new Rect(30,134,482,26),cooling.Detail,_small);
        }
        else if (grid && grid.Stop > 0)
            GUI.Label(new Rect(30,106,482,33),grid.Detail,_small);
        else if (grid && energy.ReadyForGrid)
            GUI.Label(new Rect(30,134,482,24),"Ou augmentez la demande au pupitre",_small);
        if (!grid || grid.Stop == 0)
            GUI.Label(new Rect(30,100,482,24),energy.Reading + " · valeurs simulées",_small);
        GUI.Label(new Rect(Screen.width*.5f - 5,Screen.height*.5f - 10,14,24),"+",_small);
    }

    void DrawWater()
    {
        float width = Mathf.Min(545,Screen.width-36);
        GUI.Box(new Rect(18,18,width,229),GUIContent.none);
        GUI.Label(new Rect(30,26,width-24,27),"CLOUDVR V2 · EAU",_heading);
        GUI.Label(new Rect(30,52,width-24,55),player.InputPaused ? "Pause · Échap pour reprendre" : water.Instruction,_instruction);
        if (water.Active)
        {
            GUI.Label(new Rect(30,107,width-24,24),$"{water.WeatherName} · {water.ServedRows} / {water.TargetRows} rangées servies · {energy.ActiveRows} alimentées",_small);
            GUI.Label(new Rect(30,132,width-24,24),$"Refroidissement · capacité {water.CoolingRows} rangées · pompe {water.PumpMode}",_small);
            GUI.Label(new Rect(30,157,width-24,24),$"Rivière · {water.Withdrawn} / {water.RiverFlow} unités prélevées ({water.Share} %)",_small);
            GUI.Label(new Rect(30,182,width-24,24),$"{water.Evaporated} évaporées · {water.Returned} restituées · valeurs fictives",_small);
            GUI.Label(new Rect(30,207,width-24,24),water.RiverStatus,_small);
        }
        else GUI.Label(new Rect(30,112,width-24,34),"Viser VERS LA RIVIÈRE sur la passerelle.",_small);
        GUI.Label(new Rect(Screen.width*.5f - 5,Screen.height*.5f - 10,14,24),"+",_small);
    }

    void DrawTransit()
    {
        float elapsed = _travelProgress * TravelSeconds;
        float fade = Mathf.Max(
            1 - Mathf.Clamp01(Mathf.Abs(elapsed-ScreenSeconds)/.17f),
            1 - Mathf.Clamp01(Mathf.Abs(elapsed-ScreenSeconds-FibreSeconds)/.18f));
        var color = GUI.color;
        GUI.color = new Color(.015f,.024f,.04f,fade);
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
        GUI.color = color;
        float width = Mathf.Min(580, Screen.width - 48);
        var panel = new Rect((Screen.width - width) * .5f, Screen.height - 125, width, 98);
        GUI.Box(panel, GUIContent.none);
        string title = elapsed < ScreenSeconds ? "01 / ENVOI DEPUIS LE POSTE" :
            elapsed < ScreenSeconds + FibreSeconds ? "02 / SIGNAL DANS LA FIBRE" : "03 / ARRIVÉE AU DATA CENTER";
        float left = panel.x + 19;
        if (_photo) GUI.DrawTexture(new Rect(left, panel.y + 19, 52, 38), _photo);
        GUI.Label(new Rect(left + 65, panel.y + 14, width - 100, 29), title, _heading);
        GUI.Label(new Rect(left + 65, panel.y + 42, width - 100, 25), "Photo en transit · trajet illustratif", _small);
        GUI.color = new Color(.21f, .75f, .87f);
        GUI.DrawTexture(new Rect(left, panel.y + 77, (width - 38) * _travelProgress, 3), Texture2D.whiteTexture);
        GUI.color = color;
    }
}
