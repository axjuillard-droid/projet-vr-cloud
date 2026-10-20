using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Permet au joueur de s'asseoir confortablement sur la chaise de bureau face au PC :
/// - En mode PC : viser la chaise/bureau et appuyer sur E (ou cliquer) pour s'asseoir.
///   Une fois assis, le curseur souris est libéré pour cliquer sur l'écran.
///   Appuyer sur E, Échap ou une touche de déplacement relève le joueur.
/// - En mode VR : téléportation / sélection XR vers la chaise.
/// </summary>
public class ChairSeatInteraction : MonoBehaviour
{
    [Header("Position assise")]
    [Tooltip("Position cible des yeux du joueur une fois assis")]
    public Vector3 seatedEyePosition = new Vector3(0.20f, 1.12f, -0.90f);
    public Vector3 lookTarget = new Vector3(0.20f, 1.05f, -1.40f);

    [Header("Transition")]
    public float transitionSpeed = 4f;

    public bool IsSeated { get; private set; } = false;

    private PCPlayerController _pcController;
    private CharacterController _characterController;
    private Camera _cam;
    private Vector3 _standingPosition;
    private Quaternion _standingRotation;
    private bool _isTransitioning = false;

    private void Start()
    {
        _cam = Camera.main;
        if (_cam != null)
        {
            _pcController = _cam.GetComponentInParent<PCPlayerController>();
            if (_pcController != null)
            {
                _characterController = _pcController.GetComponent<CharacterController>();
            }
        }
    }

    private void Update()
    {
        if (InputModeManager.Instance != null && InputModeManager.Instance.IsVR) return;
        var keyboard = Keyboard.current;
        if (keyboard == null || _isTransitioning) return;

        // Si assis, appuyer sur E ou Échap ou une touche de mouvement relève le joueur
        if (IsSeated)
        {
            bool wantsStand = keyboard.eKey.wasPressedThisFrame ||
                              keyboard.escapeKey.wasPressedThisFrame ||
                              keyboard.wKey.wasPressedThisFrame ||
                              keyboard.zKey.wasPressedThisFrame ||
                              keyboard.sKey.wasPressedThisFrame ||
                              keyboard.aKey.wasPressedThisFrame ||
                              keyboard.qKey.wasPressedThisFrame ||
                              keyboard.dKey.wasPressedThisFrame;

            if (wantsStand)
            {
                StandUp();
            }
            return;
        }

        // Si debout, appuyer sur E en visant la chaise/bureau assied le joueur
        if (keyboard.eKey.wasPressedThisFrame)
        {
            TrySitDownFromRaycast();
        }
    }

    private void TrySitDownFromRaycast()
    {
        if (_cam == null) return;
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 3.5f))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                SitDown();
            }
        }
    }

    public void SitDown()
    {
        if (IsSeated || _isTransitioning || _pcController == null) return;
        StartCoroutine(TransitionRoutine(true));
    }

    public void StandUp()
    {
        if (!IsSeated || _isTransitioning || _pcController == null) return;
        StartCoroutine(TransitionRoutine(false));
    }

    private IEnumerator TransitionRoutine(bool toSeated)
    {
        _isTransitioning = true;
        if (toSeated)
        {
            _standingPosition = _pcController.transform.position;
            _standingRotation = _pcController.transform.rotation;
            if (_characterController != null) _characterController.enabled = false;
            _pcController.enabled = false;
        }

        Vector3 startPos = _cam.transform.position;
        Quaternion startRot = _cam.transform.rotation;
        Vector3 targetPos = toSeated ? seatedEyePosition : _standingPosition + Vector3.up * 1.6f;
        Quaternion targetRot = toSeated ? Quaternion.LookRotation((lookTarget - seatedEyePosition).normalized) : _standingRotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            _cam.transform.position = Vector3.Lerp(startPos, targetPos, smooth);
            _cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, smooth);
            yield return null;
        }

        IsSeated = toSeated;
        if (!toSeated)
        {
            _cam.transform.localPosition = new Vector3(0, 1.6f, 0);
            if (_characterController != null) _characterController.enabled = true;
            _pcController.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        _isTransitioning = false;
    }
}
