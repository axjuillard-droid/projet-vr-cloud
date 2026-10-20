using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Contrôle l'interaction physique avec la baie serveur et le tiroir extractible 2U :
/// - Clic 1 : ouverture de la porte de baie sur charnières avec clic de poignée quart de tour.
/// - Clic 2 : extraction du serveur 2U sur glissières télescopiques avec mise en valeur des CPU/RAM/NVMe.
/// - Clic 3 : réinsertion du serveur et fermeture de la baie.
/// Compatible PC (survol/clic raycast) et Meta Quest (XRSimpleInteractable).
/// </summary>
public class ServerBladeInteraction : MonoBehaviour
{
    public enum State { DoorClosed, DoorOpened, ServerExtracted }
    public State CurrentState { get; private set; } = State.DoorClosed;

    [Header("Composants")]
    public Transform rackDoor;
    public Transform serverBlade;
    public Light inspectionLight;
    public GameObject hardwareLabels;

    [Header("Paramètres d'animation")]
    public float doorOpenAngle = 95f;
    public float serverSlideDistance = 0.52f;
    public float animDuration = 0.9f;

    [Header("Audio")]
    public AudioSource audioSource;

    private bool _isAnimating = false;
    private Quaternion _doorClosedRot;
    private Vector3 _bladeClosedPos;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0.85f;
            audioSource.maxDistance = 10f;
        }

        if (rackDoor != null) _doorClosedRot = rackDoor.localRotation;
        if (serverBlade != null) _bladeClosedPos = serverBlade.localPosition;

        if (inspectionLight != null) inspectionLight.enabled = false;
        if (hardwareLabels != null) hardwareLabels.SetActive(false);

        // Configuration pour XR / VR
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(_ => Interact());
        }
    }

    private void Update()
    {
        // En mode PC : détection de clic gauche si visé
        if (InputModeManager.Instance != null && InputModeManager.Instance.IsVR) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 3.5f))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    Interact();
                }
            }
        }
    }

    public void Interact()
    {
        if (_isAnimating) return;

        switch (CurrentState)
        {
            case State.DoorClosed:
                StartCoroutine(OpenDoorRoutine());
                break;
            case State.DoorOpened:
                StartCoroutine(ExtractServerRoutine());
                break;
            case State.ServerExtracted:
                StartCoroutine(CloseAllRoutine());
                break;
        }
    }

    private IEnumerator OpenDoorRoutine()
    {
        _isAnimating = true;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(ProceduralAudioService.CreateHandleTurnSound(), 0.7f);
        }

        Quaternion startRot = rackDoor != null ? rackDoor.localRotation : Quaternion.identity;
        Quaternion targetRot = _doorClosedRot * Quaternion.Euler(0, -doorOpenAngle, 0);

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            if (rackDoor != null) rackDoor.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        if (rackDoor != null) rackDoor.localRotation = targetRot;
        CurrentState = State.DoorOpened;
        _isAnimating = false;
    }

    private IEnumerator ExtractServerRoutine()
    {
        _isAnimating = true;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(ProceduralAudioService.CreateSlideRailSound(), 0.8f);
        }

        Vector3 startPos = serverBlade != null ? serverBlade.localPosition : Vector3.zero;
        Vector3 targetPos = _bladeClosedPos + Vector3.back * serverSlideDistance;

        if (inspectionLight != null) inspectionLight.enabled = true;
        if (hardwareLabels != null) hardwareLabels.SetActive(true);

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            if (serverBlade != null) serverBlade.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (serverBlade != null) serverBlade.localPosition = targetPos;
        CurrentState = State.ServerExtracted;
        _isAnimating = false;
    }

    private IEnumerator CloseAllRoutine()
    {
        _isAnimating = true;

        // 1. Rétracter le serveur
        if (audioSource != null)
        {
            audioSource.PlayOneShot(ProceduralAudioService.CreateSlideRailSound(), 0.6f);
        }

        Vector3 startPos = serverBlade != null ? serverBlade.localPosition : Vector3.zero;
        float elapsed = 0f;
        while (elapsed < animDuration * 0.7f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / (animDuration * 0.7f));
            if (serverBlade != null) serverBlade.localPosition = Vector3.Lerp(startPos, _bladeClosedPos, t);
            yield return null;
        }
        if (serverBlade != null) serverBlade.localPosition = _bladeClosedPos;

        if (inspectionLight != null) inspectionLight.enabled = false;
        if (hardwareLabels != null) hardwareLabels.SetActive(false);

        yield return new WaitForSeconds(0.15f);

        // 2. Refermer la porte
        if (audioSource != null)
        {
            audioSource.PlayOneShot(ProceduralAudioService.CreateHandleTurnSound(), 0.6f);
        }

        Quaternion startRot = rackDoor != null ? rackDoor.localRotation : Quaternion.identity;
        elapsed = 0f;
        while (elapsed < animDuration * 0.8f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / (animDuration * 0.8f));
            if (rackDoor != null) rackDoor.localRotation = Quaternion.Slerp(startRot, _doorClosedRot, t);
            yield return null;
        }
        if (rackDoor != null) rackDoor.localRotation = _doorClosedRot;

        CurrentState = State.DoorClosed;
        _isAnimating = false;
    }
}
