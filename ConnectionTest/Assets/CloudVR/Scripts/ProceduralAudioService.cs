using UnityEngine;

/// <summary>
/// Synthétiseur audio procédural pur C# pour CloudVR :
/// Zéro dépendance, zéro asset externe, 100% reproductible, compatible PC et Meta Quest.
/// Génère des clips audio de haute fidélité pour l'ambiance industrielle et les interactions.
/// </summary>
public static class ProceduralAudioService
{
    private const int SampleRate = 44100;

    /// <summary>
    /// Grondement sourd et immersif d'allée de serveurs (turbines de ventilation + transformateurs 60Hz).
    /// </summary>
    public static AudioClip CreateDataCenterTurbineDrone(float duration = 4.0f)
    {
        int numSamples = (int)(SampleRate * duration);
        float[] samples = new float[numSamples];

        System.Random rng = new System.Random(42);
        float filterState = 0f;

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / SampleRate;

            // Harmoniques fondamentales du réseau électrique et des moteurs (60Hz, 120Hz, 180Hz)
            float hum = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.25f
                      + Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.18f
                      + Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.08f;

            // Sifflement aigu des ventilateurs centrifuges haute vitesse (~980Hz et ~1450Hz avec battements)
            float whine = Mathf.Sin(2f * Mathf.PI * 980f * t) * 0.035f
                        + Mathf.Sin(2f * Mathf.PI * 1450f * t) * 0.025f;

            // Souffle aéraulique (bruit blanc filtré passe-bas pour imiter l'air propulsé dans les racks)
            float whiteNoise = ((float)rng.NextDouble() * 2f - 1f) * 0.12f;
            filterState += (whiteNoise - filterState) * 0.04f; // Filtre passe-bas simple

            float sample = hum + whine + filterState;

            // Fondu transparent en début et fin pour boucle parfaite
            float fadeLength = 0.05f * numSamples;
            if (i < fadeLength) sample *= (i / fadeLength);
            else if (i > numSamples - fadeLength) sample *= ((numSamples - i) / fadeLength);

            samples[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("DC_Turbine_Drone", numSamples, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Bip cristallin de badgeuse biométrique RFID ("ACCESS GRANTED").
    /// </summary>
    public static AudioClip CreateRfidAccessBeep()
    {
        float duration = 0.35f;
        int numSamples = (int)(SampleRate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / SampleRate;
            float freq = t < 0.12f ? 1500f : 2200f; // Deux tons aigus
            float envelope = Mathf.Exp(-t * 14f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("RFID_Beep", numSamples, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Bruit de vérin pneumatique / décompression et glissement de portes lourdes blindées.
    /// </summary>
    public static AudioClip CreateHydraulicDoorSound()
    {
        float duration = 1.4f;
        int numSamples = (int)(SampleRate * duration);
        float[] samples = new float[numSamples];

        System.Random rng = new System.Random(1337);
        float filter = 0f;

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / SampleRate;

            // Décompression initiale d'air comprimé (pssshh)
            float airRelease = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 4f) * 0.3f;

            // Glissement mécanique lourd basse fréquence
            float motorHum = Mathf.Sin(2f * Mathf.PI * 45f * t) * (1f - Mathf.Abs(t - 0.7f) / 0.7f) * 0.22f;

            filter += (airRelease - filter) * 0.15f;
            samples[i] = Mathf.Clamp(filter + motorHum, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Hydraulic_Door", numSamples, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Vortex de données / souffle optique lors du voyage dans la fibre.
    /// </summary>
    public static AudioClip CreateFiberTransitSound()
    {
        float duration = 2.4f;
        int numSamples = (int)(SampleRate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;

            // Modulation de fréquence montante puis descendante
            float freq = 300f + Mathf.Sin(progress * Mathf.PI) * 1200f;
            float envelope = Mathf.Sin(progress * Mathf.PI);

            float s1 = Mathf.Sin(2f * Mathf.PI * freq * t);
            float s2 = Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * t) * 0.5f;

            samples[i] = (s1 + s2) * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("Fiber_Transit", numSamples, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
