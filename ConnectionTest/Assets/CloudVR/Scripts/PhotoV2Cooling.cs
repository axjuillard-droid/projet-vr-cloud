using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PhotoV2Cooling : MonoBehaviour
{
    public Collider valve;
    public Collider observation;
    public Transform wheel;
    public GameObject gate;
    public GameObject guide;
    public LineRenderer hotPath;
    public LineRenderer coldPath;
    public Transform[] hotMarkers;
    public Transform[] coldMarkers;
    public Transform[] airMarkers;
    public Transform[] fans;
    public PhotoV2Energy energy;
    public Text status;

    public float Temperature { get; private set; } = 23f;
    public int AuxiliaryKw => Open ? 6 : 0;
    public bool Stabilized => ReturnComplete && Temperature <= 24f;
    public string Reading => $"{Temperature:0.0} °C · débit {(Open ? "renforcé" : "normal")} · +{AuxiliaryKw} kW";

    float _distance;
    float _hotLength;
    float _coldLength;
    bool _turning;
    int _lastDisplayed = -1;
    public bool Open { get; private set; }
    public bool HeatReached { get; private set; }
    public bool ReturnComplete { get; private set; }
    public bool Observed { get; private set; }

    public string Instruction => Observed ? "Le liquide refroidi repart vers les serveurs." :
        _turning ? "La vanne s’ouvre" :
        Open ? "Suivre les conduites puis attendre la stabilisation" : "Tourner la vanne pour augmenter le débit";
    public string Detail => Observed ? "La chaleur passe à l’air extérieur. Le liquide recircule." :
        Open ? "Bleu : départ froid vers les baies. Orange : retour chaud." :
        "Boucle de démonstration du data center";

    void Awake()
    {
        _hotLength = PathLength(hotPath);
        _coldLength = PathLength(coldPath);
    }

    public bool TrySelect(Collider target)
    {
        if (target == valve && !Open && !_turning)
        {
            StartCoroutine(OpenValve());
            return true;
        }
        if (target != observation || !Stabilized || Observed) return false;
        Observed = true;
        guide.SetActive(false);
        Debug.Log("CLOUDVR_V2_COOLING_OBSERVED");
        return true;
    }

    IEnumerator OpenValve()
    {
        _turning = true;
        var initial = wheel.localRotation;
        for (float t=0; t<.5f; t+=Time.deltaTime)
        {
            wheel.localRotation = initial * Quaternion.Euler(0,0,120*t/.5f);
            yield return null;
        }
        wheel.localRotation = initial * Quaternion.Euler(0,0,120);
        gate.SetActive(false);
        guide.SetActive(true);
        hotPath.enabled = true;
        coldPath.enabled = true;
        Open = true;
        _lastDisplayed = -1;
        energy.Refresh();
        _turning = false;
        Debug.Log("CLOUDVR_V2_COOLING_OPEN");
    }

    void Update()
    {
        if (!energy.isActiveAndEnabled) return;
        float heat = (energy.ActiveRows - PhotoV2Energy.BaseRows) * 3f;
        float target = 23f + Mathf.Max(0,heat - (ReturnComplete ? 12f : 0f));
        Temperature = Mathf.MoveTowards(Temperature,target,Time.deltaTime * 2.6f);
        int displayed = Mathf.RoundToInt(Temperature * 10f);
        if (displayed != _lastDisplayed)
        {
            _lastDisplayed = displayed;
            status.text = Reading;
        }
        if (!Open) return;
        _distance += 31f * Time.deltaTime;
        MoveMarkers(hotPath,hotMarkers,_distance,_hotLength);
        if (_distance < _hotLength) return;

        if (!HeatReached)
        {
            HeatReached = true;
            Debug.Log("CLOUDVR_V2_COOLING_HEAT_REACHED");
        }
        float returnDistance = _distance - _hotLength;
        MoveMarkers(coldPath,coldMarkers,returnDistance,_coldLength);
        for (int i=0; i<fans.Length; i++)
            fans[i].Rotate(Vector3.up,480 * Time.deltaTime,Space.Self);
        for (int i=0; i<airMarkers.Length; i++)
        {
            var air = airMarkers[i];
            air.gameObject.SetActive(true);
            float rise = Mathf.Repeat(returnDistance*.022f + i*.2f,1);
            air.localPosition = Vector3.up * (rise * 2.4f);
            air.localScale = new Vector3(.16f,.48f,.16f) * Mathf.Sin(rise*Mathf.PI);
        }
        if (!ReturnComplete && returnDistance >= _coldLength)
        {
            ReturnComplete = true;
            Debug.Log("CLOUDVR_V2_COOLING_RETURN_COMPLETE");
        }
    }

    static float PathLength(LineRenderer path)
    {
        float length=0;
        for (int i=1; i<path.positionCount; i++)
            length += Vector3.Distance(path.GetPosition(i-1),path.GetPosition(i));
        return length;
    }

    static void MoveMarkers(LineRenderer path,Transform[] markers,float distance,float length)
    {
        for (int j=0; j<markers.Length; j++)
        {
            var marker = markers[j];
            float along = distance - j*3.7f;
            marker.gameObject.SetActive(along >= 0);
            if (along < 0) continue;
            along = Mathf.Repeat(along,length);
            for (int i=1; i<path.positionCount; i++)
            {
                var start = path.GetPosition(i-1);
                var end = path.GetPosition(i);
                float segment = Vector3.Distance(start,end);
                if (along <= segment)
                {
                    marker.position = Vector3.Lerp(start,end,along/segment);
                    break;
                }
                along -= segment;
            }
        }
    }
}
