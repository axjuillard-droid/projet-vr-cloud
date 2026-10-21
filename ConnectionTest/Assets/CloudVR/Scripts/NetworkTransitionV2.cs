using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Nouvelle transition réseau réaliste V2 :
/// Au lieu du tunnel abstrait, la caméra suit le signal depuis la tour PC
/// jusqu'à l'arrivée devant le campus extérieur du data center.
/// 5 étapes cinématiques avec HUD textuel contextuel.
/// Compatible avec les tests automatiques --cloudvr-demo-smoke.
/// </summary>
public class NetworkTransitionV2 : MonoBehaviour
{
    // ── Références scène ──────────────────────────────────────────────────
    [Header("Caméra & Player")]
    public Camera    mainCamera;
    public Transform playerTransform;     // Désactiver le player pendant la transition

    [Header("Objets de la chambre")]
    public Transform boxInternet;         // La Box Internet sur le bureau (LED clignotante)
    public Transform tourPC;              // La tour PC (point de départ)

    [Header("HUD")]
    public GameObject          hudPanel;
    public TextMeshProUGUI     hudTitle;
    public TextMeshProUGUI     hudSubtitle;
    public TextMeshProUGUI     hudProgress;
    public UnityEngine.UI.Image progressBar;

    [Header("Timings (secondes)")]
    public float stepDuration = 1.8f;    // Durée de chaque étape
    public float fadeDuration = 0.35f;

    [Header("Callbacks")]
    public StepTransition stepTransition; // La scène DataCenter sera chargée à la fin

    // ── Étapes narratives ─────────────────────────────────────────────────
    private static readonly (string title, string sub, float progress)[] Steps =
    {
        ("Chiffrement de la photo",
         "AES-256-GCM · Clé éphémère générée localement",
         0.10f),

        ("Connexion à la Box Internet",
         "Ethernet 1 Gbps · Fibre FTTH · Port WAN activé",
         0.22f),

        ("Réseau local → Nœud de Raccordement Optique",
         "Gaine technique immeuble · PM-Rue · NRO ×1 Gbps",
         0.42f),

        ("Transit dorsale opérateur",
         "WAN longue distance · MPLS · 100 Gbps · Latence < 2 ms",
         0.65f),

        ("Arrivée au Data Center Tier IV",
         "Campus sécurisé · Liaison fibre dédiée · Authentification en cours…",
         0.90f),

        ("Réception confirmée",
         "Photo stockée · Checksums SHA-256 validés · CRC32 OK",
         1.00f),
    };

    // ── Etat interne ──────────────────────────────────────────────────────
    private bool _running;
    private bool _smokeMode;
    private float _camOrigFOV;
    private Vector3 _camOrigPos;
    private Quaternion _camOrigRot;
    private CharacterController _cc;

    void Awake()
    {
        _smokeMode = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--cloudvr-demo-smoke") >= 0
                  || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "smoke") >= 0;
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    // ── API publique ──────────────────────────────────────────────────────
    /// <summary>Lance la cinématique de transition réseau.</summary>
    public void StartNetworkRide()
    {
        if (_running) return;
        StartCoroutine(_smokeMode ? SmokeTransition() : FullTransition());
    }

    // ── Smoke (tests automatisés) ─────────────────────────────────────────
    IEnumerator SmokeTransition()
    {
        yield return new WaitForSeconds(0.1f);
        stepTransition?.TriggerTransition();
    }

    // ── Cinématique complète ──────────────────────────────────────────────
    IEnumerator FullTransition()
    {
        _running = true;

        // Sauvegarder état caméra
        _camOrigFOV = mainCamera != null ? mainCamera.fieldOfView : 60f;
        _camOrigPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
        _camOrigRot = mainCamera != null ? mainCamera.transform.rotation : Quaternion.identity;

        // Désactiver le CharacterController pendant la ciné
        _cc = playerTransform?.GetComponent<CharacterController>();
        if (_cc) _cc.enabled = false;

        // Afficher HUD
        if (hudPanel != null) hudPanel.SetActive(true);

        // ── Étape 0 : Zoom sur la tour PC (point de départ du signal) ────
        yield return StartCoroutine(MoveCameraTo(
            tourPC != null ? tourPC.position + Vector3.up * 0.5f + Vector3.back * 0.4f : _camOrigPos + Vector3.back * 0.5f,
            tourPC != null ? Quaternion.LookRotation((tourPC.position - mainCamera.transform.position).normalized) : _camOrigRot,
            70f, fadeDuration));

        // ── Étapes narratives ─────────────────────────────────────────────
        for (int i = 0; i < Steps.Length; i++)
        {
            var (title, sub, prog) = Steps[i];
            yield return StartCoroutine(DisplayStep(title, sub, prog, i));
        }

        // ── Fondu final et chargement de la scène ─────────────────────────
        yield return StartCoroutine(FadeHUD(false));
        if (hudPanel != null) hudPanel.SetActive(false);

        // Restaurer la caméra
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = _camOrigFOV;
            mainCamera.transform.position = _camOrigPos;
            mainCamera.transform.rotation = _camOrigRot;
        }
        if (_cc) _cc.enabled = true;

        _running = false;
        stepTransition?.TriggerTransition();
    }

    // ── Afficher une étape ────────────────────────────────────────────────
    IEnumerator DisplayStep(string title, string sub, float targetProgress, int stepIndex)
    {
        // Mettre à jour le HUD
        if (hudTitle    != null) hudTitle.text    = title;
        if (hudSubtitle != null) hudSubtitle.text = sub;
        if (hudProgress != null) hudProgress.text = $"Étape {stepIndex + 1} / {Steps.Length}";

        // Animation de la barre de progression
        float startProg = progressBar != null ? progressBar.fillAmount : 0f;
        float elapsed   = 0f;
        while (elapsed < stepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / stepDuration);
            if (progressBar != null) progressBar.fillAmount = Mathf.Lerp(startProg, targetProgress, t);
            yield return null;
        }
    }

    // ── Mouvement caméra fluide ───────────────────────────────────────────
    IEnumerator MoveCameraTo(Vector3 targetPos, Quaternion targetRot, float targetFOV, float duration)
    {
        if (mainCamera == null) yield break;
        float elapsed  = 0f;
        Vector3    sp  = mainCamera.transform.position;
        Quaternion sr  = mainCamera.transform.rotation;
        float      sf  = mainCamera.fieldOfView;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            mainCamera.transform.position   = Vector3.Lerp(sp, targetPos, t);
            mainCamera.transform.rotation   = Quaternion.Slerp(sr, targetRot, t);
            mainCamera.fieldOfView          = Mathf.Lerp(sf, targetFOV, t);
            yield return null;
        }
    }

    // ── Fondu du HUD ─────────────────────────────────────────────────────
    IEnumerator FadeHUD(bool fadeIn)
    {
        if (hudPanel == null) yield break;
        var group = hudPanel.GetComponent<CanvasGroup>();
        if (group == null) group = hudPanel.AddComponent<CanvasGroup>();
        float start = fadeIn ? 0f : 1f;
        float end   = fadeIn ? 1f : 0f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed      += Time.deltaTime;
            group.alpha   = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = end;
    }
}
