using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Même action pédagogique au clic PC ou à la sélection XRI.</summary>
public class DemoStation : MonoBehaviour
{
    public int index;
    public Renderer indicator;
    public UnityEngine.UI.Text caption;
    public UnityEngine.UI.Text information;
    DemoTour _tour;
    XRSimpleInteractable _xr;
    void Start()
    {
        _tour = FindFirstObjectByType<DemoTour>();
        _xr = GetComponent<XRSimpleInteractable>();
        if (_xr != null) _xr.selectEntered.AddListener(OnSelect);
    }
    void OnSelect(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args) { Activate(); }
    public void Activate() { if (_tour != null) _tour.Activate(index); }
    void OnDestroy() { if (_xr != null) _xr.selectEntered.RemoveListener(OnSelect); }
}
