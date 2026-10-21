using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Action guidée commune au raycast PC et à la sélection XRI.</summary>
public class P1ActionTarget : MonoBehaviour
{
    public enum Action { Door, Drawer, Cpu, Ram, Storage, ResetAlarm, BoostFans, SealContainment }
    public Action action;
    public P1LearningModule module;
    XRSimpleInteractable _xr;
    public void Activate() { if(module!=null)module.Activate(action); }
    void Start()
    {
        _xr=GetComponent<XRSimpleInteractable>();
        if(_xr!=null)_xr.selectEntered.AddListener(Selected);
    }
    void Selected(SelectEnterEventArgs args) { Activate(); }
    void OnDestroy() { if(_xr!=null)_xr.selectEntered.RemoveListener(Selected); }
}
