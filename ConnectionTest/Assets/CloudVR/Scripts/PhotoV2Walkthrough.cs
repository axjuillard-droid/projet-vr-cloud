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

    void Update()
    {
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
        if (_step >= targets.Length || !Physics.Raycast(ray, out var hit, 3.1f)
            || hit.collider.gameObject != targets[_step]) return false;
        if (targets[_step] == badgeReader)
        {
            // Présenter le badge depuis l’intérieur, après avoir dégagé la porte.
            if (player.transform.position.z < sasEntrance.transform.position.z + .6f) return false;
            sasEntrance.SetActive(true);
        }
        var barrier = barriers[_step];
        if (barrier) barrier.SetActive(false);
        targets[_step].SetActive(false);
        _step++;
        ShowGuide();
        Debug.Log($"CLOUDVR_V2_STEP {_step}/{targets.Length}");
        return true;
    }

    void ShowGuide()
    {
        for (int i = 0; i < guides.Length; i++) guides[i].SetActive(i == _step);
    }

    void OnDestroy()
    {
        if (_photo) Destroy(_photo);
    }

    void OnGUI()
    {
        _heading ??= new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
        _heading.normal.textColor = new Color(.45f, .94f, 1f);
        _instruction ??= new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
        _small ??= new GUIStyle(GUI.skin.label) { fontSize = 14 };
        string action = player.InputPaused ? "Pause · Échap pour reprendre" :
            _step < Instructions.Length ? Instructions[_step] : "Retournez-vous pour retrouver la baie école.";
        GUI.Box(new Rect(18, 18, Mathf.Min(480, Screen.width - 36), 95), GUIContent.none);
        GUI.Label(new Rect(30, 26, 430, 28), "CLOUDVR V2 · PARCOURS PHOTO", _heading);
        GUI.Label(new Rect(30, 54, Mathf.Min(452, Screen.width - 60), 58), action, _instruction);
        GUI.Label(new Rect(Screen.width * .5f - 5, Screen.height * .5f - 10, 14, 24), "+", _small);
        GUI.Box(new Rect(18, Screen.height - 66, 217, 46), GUIContent.none);
        if (_photo) GUI.DrawTexture(new Rect(26, Screen.height - 59, 45, 32), _photo);
        GUI.Label(new Rect(80, Screen.height - 58, 151, 34), $"Photo · {Status[_step]}", _small);
        GUI.Label(new Rect(18, Screen.height - 20, 425, 20), "ZQSD / flèches · souris · clic ou E · Échap", _small);
    }
}
