using System.Collections;
using UnityEngine;

/// <summary>
/// Anime l'ouverture mécanique du sas de sécurité blindé du Data Center :
/// - Gyrophare d'alerte orange rotatif
/// - Bip d'autorisation RFID avec voyant vert
/// - Bruitage de vérin hydraulique / décompression
/// - Coulissement fluide des deux portes blindées
/// - Diffusion de l'ambiance sonore industrielle (turbines des serveurs)
/// </summary>
public class AirlockDoorController : MonoBehaviour
{
    [Header("Composants de porte")]
    public Transform doorLeft;
    public Transform doorRight;
    public Transform beaconLight;

    [Header("Paramètres d'animation")]
    public float slideDistance = 0.85f;
    public float openDuration = 1.4f;
    public bool autoOpenOnStart = true;
    public float startDelay = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource ambientDroneSource;

    private bool _isOpen = false;
    private Vector3 _doorLeftClosedPos;
    private Vector3 _doorRightClosedPos;

    private void Awake()
    {
        LocateDoorParts();
        SetupAudioSources();
    }

    private void Start()
    {
        // En mode batch de smoke test, ouvrir immédiatement sans animation lente
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (var a in args)
        {
            if (a.Contains("smoke"))
            {
                OpenInstant();
                return;
            }
        }

        if (autoOpenOnStart)
        {
            StartCoroutine(SequenceRoutine());
        }
    }

    private void Update()
    {
        // Rotation continue de la balise gyrophare
        if (beaconLight != null)
        {
            beaconLight.Rotate(0, 180f * Time.deltaTime, 0, Space.Self);
        }
    }

    public void LocateDoorParts()
    {
        if (doorLeft == null) doorLeft = transform.Find("Airlock_Door_Left");
        if (doorRight == null) doorRight = transform.Find("Airlock_Door_Right");
        if (beaconLight == null) beaconLight = transform.Find("Lumiere_Gyrophare_Sas");

        if (doorLeft != null) _doorLeftClosedPos = doorLeft.localPosition;
        if (doorRight != null) _doorRightClosedPos = doorRight.localPosition;
    }

    private void SetupAudioSources()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.8f;
            audioSource.maxDistance = 15f;
        }

        if (ambientDroneSource == null)
        {
            ambientDroneSource = gameObject.AddComponent<AudioSource>();
            ambientDroneSource.loop = true;
            ambientDroneSource.playOnAwake = false;
            ambientDroneSource.spatialBlend = 0.3f;
            ambientDroneSource.volume = 0.45f;
            ambientDroneSource.clip = ProceduralAudioService.CreateDataCenterTurbineDrone();
            ambientDroneSource.Play();
        }
    }

    private IEnumerator SequenceRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        // 1. Bip d'autorisation RFID
        if (audioSource != null)
        {
            audioSource.PlayOneShot(ProceduralAudioService.CreateRfidAccessBeep(), 0.6f);
        }

        yield return new WaitForSeconds(0.35f);

        // 2. Décompression hydraulique et ouverture mécanique des portes
        if (audioSource != null)
        {
            audioSource.PlayOneShot(ProceduralAudioService.CreateHydraulicDoorSound(), 0.7f);
        }

        yield return StartCoroutine(SlideDoorsRoutine(true));
    }

    private IEnumerator SlideDoorsRoutine(bool open)
    {
        _isOpen = open;
        if (doorLeft == null || doorRight == null) yield break;

        Vector3 targetLeft = _doorLeftClosedPos + (open ? Vector3.left * slideDistance : Vector3.zero);
        Vector3 targetRight = _doorRightClosedPos + (open ? Vector3.right * slideDistance : Vector3.zero);

        Vector3 startLeft = doorLeft.localPosition;
        Vector3 startRight = doorRight.localPosition;

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            doorLeft.localPosition = Vector3.Lerp(startLeft, targetLeft, smooth);
            doorRight.localPosition = Vector3.Lerp(startRight, targetRight, smooth);

            yield return null;
        }

        doorLeft.localPosition = targetLeft;
        doorRight.localPosition = targetRight;
    }

    public void OpenInstant()
    {
        LocateDoorParts();
        if (doorLeft != null) doorLeft.localPosition = _doorLeftClosedPos + Vector3.left * slideDistance;
        if (doorRight != null) doorRight.localPosition = _doorRightClosedPos + Vector3.right * slideDistance;
        _isOpen = true;
    }
}
