using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôle PC simple : WASD/flèches pour se déplacer, souris pour regarder.
/// Actif uniquement en mode PC (désactivé automatiquement si VR détecté).
/// Utilisé pendant le développement et pour le livrable PC standalone.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PCPlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 3f;

    [Header("Regard souris")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController _cc;
    private float _xRotation = 0f;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // Désactiver si on est en VR
        if (InputModeManager.Instance != null && InputModeManager.Instance.IsVR)
        {
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        _cc.Move(move * moveSpeed * Time.deltaTime);

        // Gravité simple
        if (!_cc.isGrounded)
            _cc.Move(Physics.gravity * Time.deltaTime);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
