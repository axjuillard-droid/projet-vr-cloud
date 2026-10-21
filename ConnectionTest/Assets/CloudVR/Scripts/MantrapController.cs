using System.Collections;
using UnityEngine;

/// <summary>
/// Contrôleur du Sas Mantrap biométrique unitaire.
/// 3 phases séquentielles :
///   1. Joueur entre → porte arrière se ferme (verrouillage magnétique)
///   2. Analyse biométrique animée (scan rétinien + pesée)
///   3. Autorisation → porte avant s'ouvre pour accéder à la salle blanche
///
/// Compatible avec le smoke test (--cloudvr-demo-smoke).
/// </summary>
public class MantrapController : MonoBehaviour
{
    public enum MantrapState { Idle, PlayerInside, Scanning, Authorized, Denied }

    [Header("Portes (pivots)")]
    public Transform doorEntry;          // Porte d'entrée (pivot charnière)
    public Transform doorExit;           // Porte de sortie (pivot charnière)

    [Header("Rotation des portes")]
    public float doorOpenAngle  = 85f;   // Angle d'ouverture (degrés, axe Y)
    public float doorSpeed      = 60f;   // Degrés / seconde

    [Header("LED de statut")]
    public Renderer ledEntry;            // Renderer de la diode de la porte entrée
    public Renderer ledExit;             // Renderer de la diode de la porte sortie
    public Renderer ledScanner;          // LED du terminal biométrique

    [Header("Matériaux LED")]
    public Material matLedGreen;
    public Material matLedRed;
    public Material matLedScanning;      // Bleu clignotant

    [Header("Timings")]
    public float scanDuration    = 3.5f; // Durée de l'analyse biométrique
    public float holdOpenSeconds = 5.0f; // Temps pendant lequel la porte reste ouverte

    [Header("Audio")]
    public AudioSource audioSource;

    private AudioClip _clipRfid;
    private AudioClip _clipHydraulic;
    private AudioClip _clipHandle;

    // ── État ──────────────────────────────────────────────────────────────
    public MantrapState State { get; private set; } = MantrapState.Idle;
    private bool _smokeMode;
    private float _entryClosedAngle;
    private float _exitClosedAngle;

    // ── Init ──────────────────────────────────────────────────────────────
    void Awake()
    {
        _smokeMode = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--cloudvr-demo-smoke") >= 0
                  || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "smoke") >= 0;

        if (doorEntry != null) _entryClosedAngle = doorEntry.localEulerAngles.y;
        if (doorExit  != null) _exitClosedAngle  = doorExit.localEulerAngles.y;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        _clipRfid = ProceduralAudioService.CreateRfidAccessBeep();
        _clipHydraulic = ProceduralAudioService.CreateHydraulicDoorSound();
        _clipHandle = ProceduralAudioService.CreateHandleTurnSound();

        SetLED(ledEntry,   matLedGreen);   // Entrée verte par défaut (libre)
        SetLED(ledExit,    matLedRed);     // Sortie rouge (bloquée)
        SetLED(ledScanner, matLedRed);
    }

    // ── API publique ──────────────────────────────────────────────────────
    /// <summary>Déclencher l'analyse quand le joueur est dans le sas.</summary>
    public void PlayerEnteredSas()
    {
        if (State != MantrapState.Idle) return;
        StartCoroutine(_smokeMode ? SmokeSequence() : FullSequence());
    }

    // ── Smoke : ouvrir immédiatement sans animation ───────────────────────
    IEnumerator SmokeSequence()
    {
        yield return new WaitForSeconds(0.05f);
        if (doorExit != null)
        {
            var e = doorExit.localEulerAngles;
            doorExit.localEulerAngles = new Vector3(e.x, _exitClosedAngle + doorOpenAngle, e.z);
        }
        State = MantrapState.Authorized;
    }

    // ── Séquence complète ─────────────────────────────────────────────────
    IEnumerator FullSequence()
    {
        State = MantrapState.PlayerInside;

        // 1. Fermer la porte d'entrée (joueur piégé dans le sas)
        SetLED(ledEntry, matLedRed);
        if (audioSource && _clipHandle) audioSource.PlayOneShot(_clipHandle);
        yield return StartCoroutine(RotateDoor(doorEntry, _entryClosedAngle, _entryClosedAngle - doorOpenAngle));

        // Petit temps de verrouillage
        yield return new WaitForSeconds(0.4f);

        // 2. Analyse biométrique
        State = MantrapState.Scanning;
        SetLED(ledScanner, matLedScanning);
        if (audioSource && _clipRfid) audioSource.PlayOneShot(_clipRfid);
        yield return StartCoroutine(ScanningEffect());

        // 3. Autorisation accordée
        State = MantrapState.Authorized;
        SetLED(ledScanner, matLedGreen);
        SetLED(ledExit,    matLedGreen);
        if (audioSource && _clipRfid) audioSource.PlayOneShot(_clipRfid);

        yield return new WaitForSeconds(0.6f);

        // 4. Ouvrir la porte de sortie
        if (audioSource && _clipHydraulic) audioSource.PlayOneShot(_clipHydraulic);
        yield return StartCoroutine(RotateDoor(doorExit, _exitClosedAngle, _exitClosedAngle + doorOpenAngle));

        // Laisser ouverte quelques secondes
        yield return new WaitForSeconds(holdOpenSeconds);

        // 5. Refermer et réinitialiser
        if (audioSource && _clipHydraulic) audioSource.PlayOneShot(_clipHydraulic);
        yield return StartCoroutine(RotateDoor(doorExit, _exitClosedAngle + doorOpenAngle, _exitClosedAngle));

        // Rouvrir l'entrée
        if (audioSource && _clipHandle) audioSource.PlayOneShot(_clipHandle);
        yield return StartCoroutine(RotateDoor(doorEntry, _entryClosedAngle - doorOpenAngle, _entryClosedAngle));

        SetLED(ledEntry,   matLedGreen);
        SetLED(ledExit,    matLedRed);
        SetLED(ledScanner, matLedRed);
        State = MantrapState.Idle;
    }

    // ── Rotation animée d'une porte ───────────────────────────────────────
    IEnumerator RotateDoor(Transform door, float fromY, float toY)
    {
        if (door == null) yield break;
        float elapsed  = 0f;
        float duration = Mathf.Abs(toY - fromY) / doorSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            var   e  = door.localEulerAngles;
            door.localEulerAngles = new Vector3(e.x, Mathf.LerpAngle(fromY, toY, t), e.z);
            yield return null;
        }
        var ef = door.localEulerAngles;
        door.localEulerAngles = new Vector3(ef.x, toY, ef.z);
    }

    // ── Effet de scanning biométrique (clignotement du scanner) ──────────
    IEnumerator ScanningEffect()
    {
        float elapsed = 0f;
        bool  tog     = false;
        while (elapsed < scanDuration)
        {
            elapsed += 0.25f;
            tog      = !tog;
            SetLED(ledScanner, tog ? matLedScanning : matLedRed);
            yield return new WaitForSeconds(0.25f);
        }
        SetLED(ledScanner, matLedScanning);
    }

    // ── Utilitaire LED ─────────────────────────────────────────────────────
    void SetLED(Renderer r, Material m)
    {
        if (r == null || m == null) return;
        r.material = m;
    }

    // ── Trigger Unity ─────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) PlayerEnteredSas();
    }
}
