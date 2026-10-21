using System.Collections;
using UnityEngine;

/// <summary>
/// Ambiance cocooning de la Chambre étudiante.
/// Contrôle dynamique de l'éclairage chaud (lampe bureau, guirlande LED),
/// la pluie douce sur la vitre (son procédural) et la vapeur de café (ParticleSystem).
/// </summary>
public class CozyRoomAtmosphere : MonoBehaviour
{
    [Header("Lumières chaudes")]
    public Light lampeBureau;               // Lampe de bureau 2700K
    public Light guirlandeLight;            // Lumière ambiante guirlande
    public Light lueurEcranPC;              // Lueur cyan de l'écran (déjà existante)
    public Light lightAmbiant;              // Directional / ambient principal

    [Header("Paramètres d'éclairage")]
    [Range(0f, 5f)] public float lampeBureauIntensity = 1.8f;
    [Range(0f, 3f)] public float guirlandeIntensity   = 0.9f;
    public Color lampeBureauColor    = new Color(1.0f, 0.72f, 0.30f, 1f);   // Ambre chaud 2700K
    public Color guirlandeColor      = new Color(1.0f, 0.85f, 0.45f, 1f);   // Or doux

    [Header("Guirlande clignotante")]
    public bool  guirlandePulse  = true;
    public float pulseSpeed      = 1.2f;
    public float pulseMin        = 0.65f;
    public float pulseMax        = 1.00f;

    [Header("Pluie sonore")]
    public bool  rainEnabled  = true;
    public float rainVolume   = 0.18f;

    [Header("Vapeur café")]
    public ParticleSystem vapeurCafe;   // ParticleSystem posé sur la tasse
    [Range(0f, 10f)] public float vapeurRate = 3f;

    // Audio procédural de pluie
    private AudioSource _rainAudio;
    private AudioClip   _rainClip;

    // ─── Smoke test (build Android) ───────────────────────────────────────
    private bool _smokeMode;

    void Awake()
    {
        _smokeMode = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--cloudvr-demo-smoke") >= 0
                  || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "smoke") >= 0;
    }

    void Start()
    {
        ApplyLighting();
        if (rainEnabled && !_smokeMode) StartCoroutine(InitRain());
        if (vapeurCafe != null)
        {
            var em = vapeurCafe.emission;
            em.rateOverTime = vapeurRate;
            vapeurCafe.Play();
        }
    }

    void Update()
    {
        if (!guirlandePulse || guirlandeLight == null || _smokeMode) return;
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;   // 0..1
        guirlandeLight.intensity = Mathf.Lerp(pulseMin, pulseMax, t) * guirlandeIntensity;
    }

    // ── Éclairage ─────────────────────────────────────────────────────────
    void ApplyLighting()
    {
        if (lampeBureau != null)
        {
            lampeBureau.color     = lampeBureauColor;
            lampeBureau.intensity = lampeBureauIntensity;
            lampeBureau.type      = LightType.Point;
            lampeBureau.range     = 3.5f;
        }
        if (guirlandeLight != null)
        {
            guirlandeLight.color     = guirlandeColor;
            guirlandeLight.intensity = guirlandeIntensity;
            guirlandeLight.type      = LightType.Point;
            guirlandeLight.range     = 2.0f;
        }
        if (lightAmbiant != null)
        {
            // Diminuer la lumière directionnelle pour créer l'intimité
            lightAmbiant.intensity = 0.30f;
        }
    }

    // ── Pluie procédurale (bruit rose filtré) ────────────────────────────
    IEnumerator InitRain()
    {
        yield return null; // attendre un frame que Unity soit prêt

        int sampleRate = AudioSettings.outputSampleRate;
        int len = sampleRate * 6; // 6 secondes en boucle
        float[] data = new float[len];

        // Bruit rose (approximation : bruit blanc filtré passe-bas)
        float prev = 0f;
        System.Random rng = new System.Random(42);
        for (int i = 0; i < len; i++)
        {
            float white = (float)(rng.NextDouble() * 2.0 - 1.0);
            // Filtre passe-bas 1er ordre α=0.05
            prev = prev + 0.05f * (white - prev);
            data[i] = prev * 0.85f;
        }

        // Enveloppe douce pour la boucle (fade in / fade out)
        int fadeSamples = sampleRate / 8;
        for (int i = 0; i < fadeSamples; i++)
        {
            float t = (float)i / fadeSamples;
            data[i] *= t;
            data[len - 1 - i] *= t;
        }

        _rainClip = AudioClip.Create("RainLoop", len, 1, sampleRate, false);
        _rainClip.SetData(data, 0);

        _rainAudio = gameObject.AddComponent<AudioSource>();
        _rainAudio.clip   = _rainClip;
        _rainAudio.loop   = true;
        _rainAudio.volume = rainVolume;
        _rainAudio.spatialBlend = 0f; // son ambiant 2D
        _rainAudio.Play();
    }

    void OnDestroy()
    {
        if (_rainClip != null) Destroy(_rainClip);
    }
}
