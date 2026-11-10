using System.Collections;
using UnityEngine;

public sealed class PhotoV2PatchCord : MonoBehaviour
{
    public Camera view;
    public Collider plug;
    public Collider portA;
    public Collider portB;
    public LineRenderer cord;
    public LineRenderer signalTrace;
    public Transform signal;
    public Transform anchor;

    readonly Vector3[] _cordPoints = new Vector3[5];
    Vector3 _cradlePosition;
    Quaternion _cradleRotation;
    bool _wrongPort;

    public bool Held { get; private set; }
    public bool Connected { get; private set; }
    public bool SignalDelivered { get; private set; }
    public string Instruction => !Held ? "Prendre le cordon sur son support" :
        _wrongPort ? "Port B de réserve · brancher sur A" : "Brancher le cordon sur le port A";

    void Start()
    {
        _cradlePosition = plug.transform.position;
        _cradleRotation = plug.transform.rotation;
        DrawCord();
    }

    public bool TrySelect(Collider target)
    {
        if (Connected) return false;
        if (!Held && target == plug)
        {
            Held = true;
            _wrongPort = false;
            plug.enabled = false;
            return true;
        }
        if (Held && target == portB)
        {
            _wrongPort = true;
            return false;
        }
        if (!Held || target != portA) return false;

        Held = false;
        Connected = true;
        plug.transform.SetPositionAndRotation(portA.transform.position + Vector3.back * .067f, Quaternion.identity);
        DrawCord();
        StartCoroutine(TraceSignal());
        return true;
    }

    void LateUpdate()
    {
        if (!Held) return;
        if ((view.transform.position - _cradlePosition).sqrMagnitude > 3.2f * 3.2f)
        {
            Held = false;
            plug.transform.SetPositionAndRotation(_cradlePosition, _cradleRotation);
            plug.enabled = true;
        }
        else
        {
            plug.transform.SetPositionAndRotation(view.transform.TransformPoint(new Vector3(.25f,-.20f,.65f)),
                view.transform.rotation);
        }
        DrawCord();
    }

    void DrawCord()
    {
        var start = anchor.position;
        var end = plug.transform.position;
        for (int i = 0; i < _cordPoints.Length; i++)
        {
            float t = i / (float)(_cordPoints.Length - 1);
            _cordPoints[i] = Vector3.Lerp(start, end, t) + Vector3.down * (.23f * Mathf.Sin(t * Mathf.PI));
        }
        cord.SetPositions(_cordPoints);
    }

    IEnumerator TraceSignal()
    {
        signalTrace.enabled = true;
        signal.gameObject.SetActive(true);
        for (int i = 1; i < signalTrace.positionCount; i++)
        {
            var start = signalTrace.GetPosition(i - 1);
            var end = signalTrace.GetPosition(i);
            float duration = Vector3.Distance(start, end) / 4.5f;
            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                signal.position = Vector3.Lerp(start, end, time / duration);
                yield return null;
            }
            signal.position = end;
        }
        SignalDelivered = true;
        Debug.Log("CLOUDVR_V2_NETWORK_SIGNAL_DELIVERED");
    }
}
