using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Cibles de maquette partagées par le clic PC et la sélection XRI.</summary>
public class DataServiceTarget : MonoBehaviour
{
    public enum Action { IsolateLink, ReadResponse, RestoreLink, Replicate, RecoveryPoint, FailPrimary, ReadPhoto, DeleteCurrent, RestorePhoto }
    public Action action;
    public DataServiceLearning module;
    XRSimpleInteractable _xr;
    void Start(){_xr=GetComponent<XRSimpleInteractable>();if(_xr!=null)_xr.selectEntered.AddListener(Select);}
    void Select(SelectEnterEventArgs args){Activate();}
    public void Activate(){if(module!=null)module.Activate(action);}
    void OnDestroy(){if(_xr!=null)_xr.selectEntered.RemoveListener(Select);}
}
