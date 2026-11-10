using UnityEngine;

public sealed class PhotoV2GridRoute : MonoBehaviour
{
    public Collider departure;
    public Collider substation;
    public Collider viewpoint;
    public GameObject exitGate;
    public GameObject[] guides;
    public Transform[] flowMarkers;
    public LineRenderer routeLine;
    public ParticleSystem[] plumes;

    public int Stop { get; private set; }
    public string Instruction => Stop switch
    {
        0 => "Ouvrir l’accès au parcours extérieur",
        1 => "Suivre la passerelle jusqu’au poste électrique",
        2 => "Suivre les lignes jusqu’au point de vue",
        _ => "Le panache blanc illustre le refroidissement de la centrale."
    };
    public string Detail => Stop switch
    {
        1 => "Le poste adapte la tension pour le site.",
        2 => "Le tracé représente l’alimentation du site par le réseau.",
        3 => "Centrale illustrative · une source parmi d’autres sur le réseau.",
        _ => ""
    };

    public bool TrySelect(Collider target, PhotoV2Energy energy)
    {
        if (Stop == 0 && target == departure && energy.HasRaisedDemand && !energy.Changing)
        {
            exitGate.SetActive(false);
            departure.enabled = false;
            guides[0].SetActive(true);
            routeLine.enabled = true;
            foreach (var marker in flowMarkers) marker.gameObject.SetActive(true);
            Stop = 1;
        }
        else if (Stop == 1 && target == substation)
        {
            guides[0].SetActive(false);
            guides[1].SetActive(true);
            Stop = 2;
        }
        else if (Stop == 2 && target == viewpoint)
        {
            guides[1].SetActive(false);
            Stop = 3;
        }
        else return false;
        Debug.Log($"CLOUDVR_V2_GRID_STOP {Stop}/3");
        return true;
    }

    void Update()
    {
        if (Stop == 0) return;
        for (int i = 0; i < flowMarkers.Length; i++)
        {
            // Le visiteur remonte le trajet. Les repères suivent l’alimentation vers le site.
            float point = (1 - Mathf.Repeat(Time.time*.17f + i/(float)flowMarkers.Length,1))
                * (routeLine.positionCount - 1);
            int segment = Mathf.Min(Mathf.FloorToInt(point),routeLine.positionCount - 2);
            flowMarkers[i].position = Vector3.Lerp(routeLine.GetPosition(segment),
                routeLine.GetPosition(segment + 1),point - segment);
        }
    }
}
