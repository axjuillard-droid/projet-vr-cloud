using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PhotoV2Walkthrough : MonoBehaviour
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
    public NetworkRideVisuals film;
    public Vector3 arrival;

    public bool IsTravelling { get; private set; }
    public int Step => _step;
    public string PhotoStatus => SignalInTransit ? "En transit" : Status[_step];
    const int NetworkStep = 3;
    const int ComputeStep = 4;
    const float TravelSeconds = 5f;
    AudioSource _sound;
    AudioClip _transit;
    AudioClip _confirmation;
    float _travelProgress;

    static readonly string[] Instructions =
    {
        "Envoyer la photo",
        "Rejoindre la façade",
        "Présenter le badge",
        "Connecter le port réseau",
        "Ouvrir la baie école",
        "Extraire le tiroir",
        "Générer l’aperçu",
        "Identifier le stockage de la photo",
        "Ouvrir la photo sur l’écran client",
        "Monter au balcon"
    };
    static readonly string[] Status =
    {
        "Prête", "En attente", "En attente", "En attente", "Reçue",
        "Reçue", "Reçue", "Reçue", "Enregistrée", "Lisible", "Lisible"
    };

    int _step;
    Texture2D _photo;
    GUIStyle _heading;
    GUIStyle _instruction;
    GUIStyle _small;

    void Start()
    {
        _sound = gameObject.AddComponent<AudioSource>();
        _sound.playOnAwake = false;
        _transit = ProceduralAudioService.CreateFiberTransitSound();
        _confirmation = ProceduralAudioService.CreateRfidAccessBeep();
        _photo = new Texture2D(4, 3) { filterMode = FilterMode.Point };
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 4; x++)
                _photo.SetPixel(x, y, y == 0 ? new Color(.16f, .40f, .23f)
                    : y == 1 && x != 0 && x != 3 ? new Color(.40f, .49f, .57f)
                    : new Color(.38f, .70f, .89f));
        _photo.Apply();
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
            return Instructions[_step];
        }
    }

    void Update()
    {
        if (IsTravelling) return;
        var keyboard = Keyboard.current;
        if (keyboard?.escapeKey.wasPressedThisFrame == true) player.TogglePause();
        if (player.InputPaused) return;
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
        if (IsTravelling || player.InputPaused || SignalInTransit || _step >= targets.Length || !Physics.Raycast(ray, out var hit, 3.1f))
            return false;
        if (_step == NetworkStep && patchCord)
        {
            if (!patchCord.TrySelect(hit.collider)) return false;
            _sound.PlayOneShot(_confirmation, .18f);
            if (patchCord.Connected) CompleteStep();
            return true;
        }
        if (hit.collider.gameObject != targets[_step]) return false;
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
        if (target == badgeReader || (_step == NetworkStep && patchCord))
            target.GetComponent<Collider>().enabled = false;
        else
            target.SetActive(false);
        _step++;
        ShowGuide();
        Debug.Log($"CLOUDVR_V2_STEP {_step}/{targets.Length}");
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
        var localRotation = view.transform.localRotation;
        _sound.PlayOneShot(_transit, .26f);
        _travelProgress = 0;
        float started = Time.unscaledTime;
        bool enteredFilm = false;

        while (_travelProgress < 1)
        {
            _travelProgress = Mathf.Clamp01((Time.unscaledTime - started) / TravelSeconds);
            if (_travelProgress >= .1f)
            {
                if (!enteredFilm)
                {
                    film.gameObject.SetActive(true);
                    view.cullingMask = 1 << NetworkRideVisuals.Layer;
                    view.backgroundColor = new Color(.012f, .022f, .038f);
                    enteredFilm = true;
                }
                float t = Mathf.InverseLerp(.1f, 1, _travelProgress);
                view.transform.SetPositionAndRotation(film.transform.TransformPoint(new Vector3(0, 3.6f, 2 + t * 53)),
                    film.transform.rotation);
                film.Animate(Time.unscaledTime - started);
            }
            yield return null;
        }

        film.gameObject.SetActive(false);
        view.cullingMask = originalMask;
        view.backgroundColor = originalBackground;
        var capsule = player.GetComponent<CharacterController>();
        capsule.enabled = false;
        player.transform.SetPositionAndRotation(arrival, Quaternion.identity);
        view.transform.SetLocalPositionAndRotation(localPosition, localRotation);
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
        if (_transit) Destroy(_transit);
        if (_confirmation) Destroy(_confirmation);
    }

    void OnGUI()
    {
        _heading ??= new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
        _heading.normal.textColor = new Color(.45f, .94f, 1f);
        _instruction ??= new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
        _small ??= new GUIStyle(GUI.skin.label) { fontSize = 14 };
        if (IsTravelling)
        {
            DrawTransit();
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
        GUI.Label(new Rect(18, Screen.height - 20, 425, 20), "ZQSD / flèches · souris · clic ou E · Échap", _small);
    }

    void DrawTransit()
    {
        float fade = _travelProgress < .1f ? Mathf.InverseLerp(.02f, .1f, _travelProgress) :
            Mathf.Max(1 - Mathf.InverseLerp(.1f, .25f, _travelProgress), Mathf.InverseLerp(.88f, 1, _travelProgress));
        var color = GUI.color;
        GUI.color = new Color(.015f, .024f, .04f, fade);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = color;
        float width = Mathf.Min(580, Screen.width - 48);
        var panel = new Rect((Screen.width - width) * .5f, Screen.height - 125, width, 98);
        GUI.Box(panel, GUIContent.none);
        string title = _travelProgress < .30f ? "01 / DÉPART DU CLIENT" :
            _travelProgress < .72f ? "02 / TRANSPORT SUR LE RÉSEAU" : "03 / APPROCHE DU SITE";
        float left = panel.x + 19;
        if (_photo) GUI.DrawTexture(new Rect(left, panel.y + 19, 52, 38), _photo);
        GUI.Label(new Rect(left + 65, panel.y + 14, width - 100, 29), title, _heading);
        GUI.Label(new Rect(left + 65, panel.y + 42, width - 100, 25), "Photo en transit · parcours illustratif", _small);
        GUI.color = new Color(.21f, .75f, .87f);
        GUI.DrawTexture(new Rect(left, panel.y + 77, (width - 38) * _travelProgress, 3), Texture2D.whiteTexture);
        GUI.color = color;
    }
}
