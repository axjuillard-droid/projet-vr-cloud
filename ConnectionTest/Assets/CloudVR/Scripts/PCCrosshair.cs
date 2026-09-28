using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche un viseur (croix) au centre de l'écran en mode PC.
/// S'efface automatiquement en mode VR.
/// S'ajoute via CloudVR → Setup → Viseur PC, ou automatiquement par PlayerSetupEditor.
/// </summary>
public class PCCrosshair : MonoBehaviour
{
    [Header("Apparence")]
    public Color couleur = new Color(1f, 1f, 1f, 0.85f);
    public float taille = 10f;
    public float epaisseur = 2f;

    private Canvas _canvas;

    private void Awake()
    {
        if (InputModeManager.Instance != null && InputModeManager.Instance.IsVR)
        {
            gameObject.SetActive(false);
            return;
        }
        BuildCrosshair();
    }

    private void BuildCrosshair()
    {
        // Canvas Screen Space Overlay
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Barre horizontale
        CreateBar("H", new Vector2(taille * 2.5f, epaisseur));
        // Barre verticale
        CreateBar("V", new Vector2(epaisseur, taille * 2.5f));
        // Point central
        CreateBar("Dot", new Vector2(epaisseur * 1.5f, epaisseur * 1.5f));
    }

    private void CreateBar(string nom, Vector2 size)
    {
        var go = new GameObject("Crosshair_" + nom);
        go.transform.SetParent(transform, false);
        var img = go.AddComponent<Image>();
        img.color = couleur;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }
}
