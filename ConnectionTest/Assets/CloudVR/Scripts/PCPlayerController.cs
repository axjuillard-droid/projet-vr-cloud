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
        Vector2 delta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        float mouseX = delta.x * mouseSensitivity * 0.05f;
        float mouseY = delta.y * mouseSensitivity * 0.05f;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMove()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        float h = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
            - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
        float v = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
            - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);

        Vector3 move = transform.right * h + transform.forward * v;
        _cc.Move(move * moveSpeed * Time.deltaTime);

        // Gravité simple
        if (!_cc.isGrounded)
            _cc.Move(Physics.gravity * Time.deltaTime);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && (InputModeManager.Instance == null || InputModeManager.Instance.IsPC))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
