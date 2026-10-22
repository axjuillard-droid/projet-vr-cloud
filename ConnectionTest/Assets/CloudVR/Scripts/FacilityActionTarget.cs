using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Commandes pédagogiques, communes au pointeur PC et à la sélection XRI.</summary>
public class FacilityActionTarget : MonoBehaviour
{
    public enum Action { InspectUPS, StartGenerator, Transfer, Replay, ReadIncident, RouteNetwork, RouteEnergy }
    public Action action;
    public FacilityOperations module;
    XRSimpleInteractable _xr;
    void Start(){_xr=GetComponent<XRSimpleInteractable>();if(_xr!=null)_xr.selectEntered.AddListener(Select);}
    void Select(SelectEnterEventArgs args){Activate();}
    public void Activate(){if(module!=null)module.Activate(action);}
    void OnDestroy(){if(_xr!=null)_xr.selectEntered.RemoveListener(Select);}
}
