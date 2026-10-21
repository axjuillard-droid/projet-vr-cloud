using System.Collections;
using UnityEngine;

/// <summary>Sas illustratif : approche, portes coulissantes interverrouillées, pesée/scan, sortie.</summary>
public class MantrapController : MonoBehaviour
{
    public enum MantrapState { Idle, PlayerInside, Scanning, Authorized, Denied }
    public Transform doorEntry, doorExit;
    public Renderer ledEntry, ledExit, ledScanner;
    public Material matLedGreen, matLedRed, matLedScanning;
    public float scanDuration = 2f;
    public float holdOpenSeconds = 5f;
    public AudioSource audioSource;
    public MantrapState State { get; private set; }
    public bool EntryOpen => _entryAmount > .99f;
    public bool ExitOpen => _exitAmount > .99f;
    public static bool ScanCompleted { get; private set; }
    Vector3 _entryClosed, _exitClosed;
    float _entryAmount, _exitAmount;
    bool _reverse;
    AudioClip _beep;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEvidence() { ScanCompleted = false; }

    void Awake()
    {
        _entryClosed = doorEntry.localPosition; _exitClosed = doorExit.localPosition;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1; audioSource.maxDistance = 8;
        _beep = ProceduralAudioService.CreateRfidAccessBeep();
        SetLED(ledScanner, matLedRed);
    }

    Transform Player()
    {
        var mode = InputModeManager.Instance;
        return mode == null ? null : (mode.IsVR ? mode.vrRig : mode.pcRig).transform;
    }

    void Update()
    {
        var player = Player(); if (player == null) return;
        Vector3 p = player.position;
        bool nearby = Mathf.Abs(p.x - transform.position.x) < 1.1f;
        if (State == MantrapState.Idle) _reverse = p.z > transform.position.z;
        bool approach = nearby && Mathf.Abs(p.z - transform.position.z) < 5f;
        bool entryWanted = State == MantrapState.Authorized ? _reverse : State == MantrapState.Idle && !_reverse && approach;
        bool exitWanted = State == MantrapState.Authorized ? !_reverse : State == MantrapState.Idle && _reverse && approach;
        _entryAmount = Mathf.MoveTowards(_entryAmount, entryWanted ? 1 : 0, Time.deltaTime * 1.6f);
        _exitAmount = Mathf.MoveTowards(_exitAmount, exitWanted ? 1 : 0, Time.deltaTime * 1.6f);
        doorEntry.localPosition = _entryClosed + Vector3.right * (_entryAmount * 1.1f);
        doorExit.localPosition = _exitClosed + Vector3.right * (_exitAmount * 1.1f);
        SetLED(ledEntry, entryWanted ? matLedGreen : matLedRed);
        SetLED(ledExit, exitWanted ? matLedGreen : matLedRed);
        if (State == MantrapState.Idle && nearby && Mathf.Abs(p.z - transform.position.z) < .55f)
            PlayerEnteredSas();
        if (State == MantrapState.Authorized && (_reverse ? p.z < transform.position.z - 3.2f : p.z > transform.position.z + 3.2f))
        {
            State = MantrapState.Idle; SetLED(ledScanner, matLedRed);
        }
    }

    public void PlayerEnteredSas()
    {
        if (State != MantrapState.Idle) return;
        State = MantrapState.PlayerInside;
        StartCoroutine(Scan());
    }

    IEnumerator Scan()
    {
        // Attendre la fermeture complète avant de compter le scan.
        while (_entryAmount > .001f || _exitAmount > .001f) yield return null;
        State = MantrapState.Scanning;
        SetLED(ledScanner, matLedScanning);
        audioSource.PlayOneShot(_beep);
        yield return new WaitForSeconds(scanDuration);
        SetLED(ledScanner, matLedGreen);
        ScanCompleted = true;
        State = MantrapState.Authorized;
        audioSource.PlayOneShot(_beep);
        Debug.Log("[Mantrap] Pesée/scan illustratifs terminés ; sortie autorisée.");
    }

    void SetLED(Renderer renderer, Material material)
    {
        if (renderer != null && material != null && renderer.sharedMaterial != material) renderer.sharedMaterial = material;
    }
}
