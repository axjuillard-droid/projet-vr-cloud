using UnityEngine;

/// <summary>
/// Gère le clic souris sur l'écran PC (mode PC standalone).
/// Raycast depuis la caméra principale ; appelle PhotoSender.Send() au clic.
/// En VR, ce composant est ignoré (XR Interactor prend le relais).
/// </summary>
[RequireComponent(typeof(PhotoSender))]
[RequireComponent(typeof(Collider))]
public class ScreenClickHandler : MonoBehaviour
{
    [Tooltip("Distance maximale de détection du raycast (en mètres)")]
    public float maxRayDistance = 3f;

    [Tooltip("Feedback visuel au survol (optionnel)")]
    public Renderer hoverRenderer;

    private PhotoSender _sender;
    private Material _originalMat;
    private Material _hoverMat;
    private bool _isHovered = false;

    private void Awake()
    {
        _sender = GetComponent<PhotoSender>();

        if (hoverRenderer != null)
        {
            _originalMat = hoverRenderer.sharedMaterial;
            _hoverMat = new Material(_originalMat);
            _hoverMat.color = new Color(0.15f, 0.25f, 0.60f); // bleu clair au survol
            _hoverMat.EnableKeyword("_EMISSION");
            _hoverMat.SetColor("_EmissionColor", _hoverMat.color * 0.4f);
        }
    }

    private void Update()
    {
        // Ignorer si en VR
        if (InputModeManager.Instance != null && InputModeManager.Instance.IsVR) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitThis = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            hitThis = (hit.collider.gameObject == gameObject);
        }

        // Feedback survol
        if (hitThis != _isHovered)
        {
            _isHovered = hitThis;
            if (hoverRenderer != null)
                hoverRenderer.sharedMaterial = _isHovered ? _hoverMat : _originalMat;

            Cursor.visible = !_isHovered; // cache le curseur quand on survole l'écran
        }

        // Clic gauche sur l'écran
        if (_isHovered && Input.GetMouseButtonDown(0))
        {
            _sender.Send();
        }
    }

    private void OnDestroy()
    {
        if (_hoverMat != null) Destroy(_hoverMat);
    }
}
