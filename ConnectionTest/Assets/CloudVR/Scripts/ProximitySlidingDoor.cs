using UnityEngine;

/// <summary>Porte de circulation : ouvre les deux vantaux avant l'arrivée du visiteur.</summary>
public class ProximitySlidingDoor : MonoBehaviour
{
    public Transform left, right;
    public float halfTravel = 1.15f;
    public bool manualOverride;
    [Range(0,1)] public float forcedOpening;
    public float OpenAmount => _amount;
    Vector3 _leftClosed, _rightClosed;
    float _amount;
    void Awake() { _leftClosed = left.localPosition; _rightClosed = right.localPosition; }
    void Update()
    {
        var mode = InputModeManager.Instance;
        if (mode == null) return;
        var player = (mode.IsVR ? mode.vrRig : mode.pcRig).transform;
        var p = transform.InverseTransformPoint(player.position);
        bool approach = Mathf.Abs(p.x) < 1.8f && Mathf.Abs(p.z) < 2.2f;
        float wanted=manualOverride ? Mathf.Clamp01(forcedOpening) : approach ? 1 : 0;
        _amount = Mathf.MoveTowards(_amount, wanted, Time.deltaTime * 2);
        left.localPosition = _leftClosed + Vector3.left * (halfTravel * _amount);
        right.localPosition = _rightClosed + Vector3.right * (halfTravel * _amount);
    }
}
