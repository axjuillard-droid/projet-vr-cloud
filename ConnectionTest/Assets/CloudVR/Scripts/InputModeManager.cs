using UnityEngine;

/// <summary>
/// Gère le mode d'entrée courant : PC (clavier/souris) ou VR (Quest).
/// Permet aux autres scripts de connaître le contexte sans dépendre d'une plateforme spécifique.
/// </summary>
[DefaultExecutionOrder(-100)]
public class InputModeManager : MonoBehaviour
{
    public enum InputMode { PC, VR }

    public enum StartupMode { Auto, PC, VR }
    public StartupMode startupMode = StartupMode.Auto;
    public GameObject pcRig;
    public GameObject vrRig;
    public GameObject pcCrosshair;
    [SerializeField] private InputMode currentMode = InputMode.PC;
    public InputMode CurrentMode => currentMode;
    private bool _autoDetect;
    private static InputMode? _nextSceneMode;
    private static bool _nextSceneAuto;

    public static InputModeManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Instance = null;
        _nextSceneMode = null;
    }

    public void PrepareNextScene()
    {
        _nextSceneMode = currentMode;
        _nextSceneAuto = _autoDetect;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Références propres à cette scène : ne pas les conserver entre scènes.
        _autoDetect = startupMode == StartupMode.Auto;
        bool androidPlayer = Application.platform == RuntimePlatform.Android && !Application.isEditor;
        var initial = startupMode == StartupMode.VR || (_autoDetect &&
            (androidPlayer || UnityEngine.XR.XRSettings.isDeviceActive)) ? InputMode.VR : InputMode.PC;
        if (_nextSceneMode.HasValue)
        {
            initial = _nextSceneMode.Value;
            _autoDetect = _nextSceneAuto;
            _nextSceneMode = null;
        }
        ApplyMode(initial);
    }

    private void Update()
    {
        // OpenXR peut devenir disponible après Awake. Conserver VR si le tracking se perd.
        if (_autoDetect && IsPC && UnityEngine.XR.XRSettings.isDeviceActive)
            ApplyMode(InputMode.VR);
    }

    public void SetMode(InputMode mode)
    {
        _autoDetect = false;
        ApplyMode(mode);
    }

    private void ApplyMode(InputMode mode)
    {
        if (mode == InputMode.VR && vrRig == null)
        {
            Debug.LogWarning("[InputModeManager] Rig VR absent : retour au mode PC.");
            mode = InputMode.PC;
        }
        currentMode = mode;
        // Désactiver l'autre caméra avant d'activer le nouveau rig.
        if (IsVR) { if (pcRig != null) pcRig.SetActive(false); }
        else { if (vrRig != null) vrRig.SetActive(false); }
        if (pcCrosshair != null) pcCrosshair.SetActive(IsPC);
        if (IsVR) vrRig.SetActive(true);
        else if (pcRig != null) pcRig.SetActive(true);
        Cursor.lockState = IsPC ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = false;
        Debug.Log("[InputModeManager] Mode " + currentMode + ".");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool IsVR => currentMode == InputMode.VR;
    public bool IsPC => currentMode == InputMode.PC;
}
