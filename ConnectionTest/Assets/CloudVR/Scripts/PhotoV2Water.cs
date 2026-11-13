using UnityEngine;
using UnityEngine.UI;

public sealed class PhotoV2Water : MonoBehaviour
{
    public Collider entry;
    public Collider weather;
    public Collider load;
    public Collider increase;
    public Collider decrease;
    public GameObject gate;
    public GameObject guide;
    public GameObject workingFlows;
    public Transform surface;
    public Text weatherLabel;
    public Text loadLabel;
    public Text pumpStatus;
    public LineRenderer intake;
    public LineRenderer discharge;
    public ParticleSystem evaporation;
    public Transform[] fish;
    public Renderer[] fishBodies;
    public Material deadFishMaterial;
    public Transform[] ripples;
    public Transform[] intakeMarkers;
    public PhotoV2Energy energy;

    const float StressThreshold = 2f;
    float _exposure;
    public bool Active { get; private set; }
    public bool Dry { get; private set; }
    public int PumpLevel { get; private set; }
    public int DeadFish { get; private set; }
    public bool Stressed => _exposure >= StressThreshold;
    public int RiverFlow => Dry ? 30 : 100;
    public int Withdrawn => energy.ActiveRows * (PumpLevel == 2 ? 3 : PumpLevel);
    public int Evaporated => Withdrawn * 2 / 3;
    public int Returned => Withdrawn - Evaporated;
    public int Share => Mathf.RoundToInt(100f * Withdrawn / RiverFlow);
    public string PumpMode => PumpLevel == 0 ? "Arrêt" : PumpLevel == 1 ? "Réduit" : "Fort";
    public string RiverStatus => DeadFish > 0 ? $"{DeadFish} poissons morts · bilan du scénario" :
        Stressed ? "Poissons en stress · oxygène insuffisant" : "Poissons actifs";
    public string Instruction => !Active ? "Ouvrir l’accès au point d’observation de la rivière" :
        Stressed ? "Réduire le pompage ou la charge pour protéger la rivière" :
        PumpLevel == 0 ? "Démarrer la pompe avec + pour alimenter la tour" :
        !Dry ? "Comparer le prélèvement en été sec" : "Ajuster la pompe avec + ou − et observer la rivière";

    public bool TrySelect(Collider target)
    {
        if (!Active)
        {
            if (target != entry) return false;
            Active = true;
            entry.enabled = false;
            gate.SetActive(false);
            guide.SetActive(true);
            Debug.Log("CLOUDVR_V2_WATER_OPEN");
            return true;
        }
        if (energy.Changing) return false;
        if (target == load) return energy.TryRequestComparisonLoad(energy.ActiveRows < 4);
        if (target == increase || target == decrease)
        {
            int level = PumpLevel + (target == increase ? 1 : -1);
            if (level < 0 || level > 2) return false;
            PumpLevel = level;
            if (level == 0) evaporation.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            workingFlows.SetActive(level > 0);
            if (level > 0 && !evaporation.isPlaying) evaporation.Play();
        }
        else if (target == weather) Dry = !Dry;
        else return false;

        guide.SetActive(false);
        return true;
    }

    void Update()
    {
        bool lowOxygen = Dry && Withdrawn * 3 > RiverFlow;
        _exposure = Mathf.Clamp(_exposure + (lowOxygen ? 1f : -1.5f) * Time.deltaTime,0,9);
        int casualties = Mathf.Clamp(Mathf.FloorToInt(_exposure) - 5,0,3);
        while (DeadFish < casualties)
            fishBodies[DeadFish++].sharedMaterial = deadFishMaterial;
        float level = Mathf.Clamp01((RiverFlow - Withdrawn) / 100f);
        var scale = surface.localScale;
        scale.x = Mathf.MoveTowards(scale.x,Mathf.Lerp(.25f,1f,Mathf.Sqrt(level)),Time.deltaTime*.35f);
        surface.localScale = scale;
        for (int i=0;i<fish.Length;i++)
        {
            var animal = fish[i];
            if (i < DeadFish)
            {
                animal.localPosition = new Vector3(animal.localPosition.x,.17f,animal.localPosition.z);
                animal.localRotation = Quaternion.Euler(0,0,90);
                continue;
            }
            float t = Time.time*(Stressed ? 1.6f : .37f)+i*.84f;
            float offset = (i%3-1)*2.35f + Mathf.Sin(t)*.72f;
            animal.localPosition = new Vector3(offset*scale.x,.16f,3.2f+i*1.43f+Mathf.Sin(t*.42f)*.42f);
            animal.localRotation = Quaternion.Euler(0,Mathf.Cos(t)*21,0);
        }
        for (int i=0;i<ripples.Length;i++)
        {
            float z = Mathf.Repeat(Time.time*(Dry ? .45f : 1.1f)+i*1.21f,36);
            ripples[i].localPosition = new Vector3(((i%5)-2)*2.28f*scale.x,.122f,z);
        }
        if (!Active) return;

        weatherLabel.text = Dry ? "DÉBIT NORMAL" : "ÉTÉ SEC";
        loadLabel.text = energy.ActiveRows < 4 ? "AUGMENTER LA CHARGE" : "RÉDUIRE LA CHARGE";
        pumpStatus.text = PumpMode.ToUpperInvariant();
        intake.widthMultiplier = Withdrawn * .016f;
        discharge.widthMultiplier = Returned * .018f;
        var emission = evaporation.emission;
        emission.rateOverTime = Evaporated;
        if (PumpLevel == 0) return;

        for (int i=0;i<intakeMarkers.Length;i++)
        {
            float position = Mathf.Repeat(Time.time * (.45f + .35f * PumpLevel) + i/(float)intakeMarkers.Length,1f)
                * (intake.positionCount-1);
            int segment = Mathf.Min(Mathf.FloorToInt(position),intake.positionCount-2);
            intakeMarkers[i].position = Vector3.Lerp(intake.GetPosition(segment),intake.GetPosition(segment+1),position-segment);
        }
    }
}
