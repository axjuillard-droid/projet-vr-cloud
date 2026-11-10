using UnityEngine;
using UnityEngine.UI;

public sealed class PhotoV2Water : MonoBehaviour
{
    public Collider entry;
    public Collider weather;
    public Collider load;
    public GameObject gate;
    public GameObject guide;
    public GameObject workingFlows;
    public Transform surface;
    public Text weatherLabel;
    public Text loadLabel;
    public LineRenderer intake;
    public LineRenderer discharge;
    public ParticleSystem evaporation;
    public Transform[] fish;
    public Transform[] ripples;
    public PhotoV2Energy energy;

    public bool Active { get; private set; }
    public bool Dry { get; private set; }
    public bool Completed { get; private set; }
    public int RiverFlow => Dry ? 30 : 100;
    public int Withdrawn => energy.ActiveRows * 3;
    public int Evaporated => energy.ActiveRows * 2;
    public int Returned => Withdrawn - Evaporated;
    public int Share => Mathf.RoundToInt(100f*Withdrawn/RiverFlow);
    public string Instruction => !Active ? "Ouvrir l’accès au point d’observation de la rivière" :
        energy.Changing ? "Observer le changement du prélèvement" :
        Completed ? "Comparer à nouveau le débit ou la charge" :
        energy.ActiveRows < 4 && !Dry ? "Augmenter la demande pour préparer la comparaison" :
        Dry ? "Réduire la demande de calcul" : "Passer en été sec à demande identique";

    public bool TrySelect(Collider target)
    {
        if (!Active)
        {
            if (target != entry) return false;
            Active = true;
            entry.enabled = false;
            gate.SetActive(false);
            guide.SetActive(true);
            workingFlows.SetActive(true);
            Debug.Log("CLOUDVR_V2_WATER_OPEN");
            return true;
        }
        if (energy.Changing) return false;
        if (target == load) return energy.TryRequestComparisonLoad(energy.ActiveRows < 4);
        if (target != weather || (!Dry && !Completed && energy.ActiveRows < 4)) return false;
        Dry = !Dry;
        guide.SetActive(false);
        Debug.Log($"CLOUDVR_V2_WATER_WEATHER {RiverFlow} unités de débit, {Withdrawn} prélevées");
        return true;
    }

    void Update()
    {
        var scale = surface.localScale;
        scale.x = Mathf.MoveTowards(scale.x,Dry ? .47f : 1f,Time.deltaTime*.65f);
        surface.localScale = scale;
        for(int i=0;i<fish.Length;i++)
        {
            float t = Time.time*.37f+i*.84f;
            float offset = (i%3-1)*2.35f + Mathf.Sin(t)*.72f;
            fish[i].localPosition = new Vector3(offset*scale.x,.16f,3.2f+i*1.43f+Mathf.Sin(t*.42f)*.42f);
            fish[i].localRotation = Quaternion.Euler(0,Mathf.Cos(t)*21,0);
        }
        for(int i=0;i<ripples.Length;i++)
        {
            float z = Mathf.Repeat(Time.time*(Dry ? .45f : 1.1f)+i*1.21f,36);
            ripples[i].localPosition = new Vector3(((i%5)-2)*2.28f*scale.x,.122f,z);
        }
        if (!Active) return;

        weatherLabel.text = Dry ? "DÉBIT NORMAL" : "ÉTÉ SEC";
        loadLabel.text = energy.ActiveRows < 4 ? "AUGMENTER LA CHARGE" : "RÉDUIRE LA CHARGE";
        intake.widthMultiplier = Withdrawn * .016f;
        discharge.widthMultiplier = Returned * .018f;
        var emission = evaporation.emission;
        emission.rateOverTime = Evaporated;
        if (!Completed && Dry && !energy.Changing && energy.ActiveRows == 2)
        {
            Completed = true;
            Debug.Log("CLOUDVR_V2_WATER_DEMAND_REDUCED");
        }
    }
}
