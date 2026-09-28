using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gère le mode d'entrée courant : PC (clavier/souris) ou VR (Quest).
/// Permet aux autres scripts de connaître le contexte sans dépendre d'une plateforme spécifique.
/// </summary>
public class InputModeManager : MonoBehaviour
{
    public enum InputMode { PC, VR }

    [Header("Mode actif")]
    [Tooltip("Détecté automatiquement au démarrage ; modifiable dans l'inspecteur pour les tests.")]
    public InputMode currentMode = InputMode.PC;

    public static InputModeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DetectMode();
    }

    private void DetectMode()
    {
        // Détection simple : si Unity XR est actif, on est en VR
        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            currentMode = InputMode.VR;
            Debug.Log("[InputModeManager] Mode VR détecté.");
        }
        else
        {
            currentMode = InputMode.PC;
            Debug.Log("[InputModeManager] Mode PC détecté.");
        }
    }

    public bool IsVR => currentMode == InputMode.VR;
    public bool IsPC => currentMode == InputMode.PC;
}
