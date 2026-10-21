using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Six étapes narratives illustratives ; aucune mesure de réseau réel.</summary>
public class NetworkTransitionV2 : MonoBehaviour
{
    public GameObject hudPanel;
    public Text hudTitle, hudSubtitle, hudProgress;
    public Image progressBar;
    public StepTransition stepTransition;
    public Transform tourPC, boxInternet;
    public float stepDuration = 1.8f;
    public bool IsRunning { get; private set; }
    public int CurrentStep { get; private set; }
    public static int CompletedStages { get; private set; }
    public static float CompletedDuration { get; private set; }
    private PCPlayerController _controller;
    private bool _controllerWasEnabled;
    private ChairSeatInteraction _seat;
    private bool _seatWasEnabled;

    private static readonly string[] Titles = {
        "Préparation de la photo", "Connexion à la box Internet", "Accès fibre optique",
        "Transit chez les opérateurs", "Arrivée au data center", "Réception de la requête" };
    private static readonly string[] Subtitles = {
        "Les données sont préparées et la connexion sécurisée.",
        "Le réseau local rejoint le fournisseur d’accès.",
        "Le signal rejoint le réseau de collecte de l’opérateur.",
        "Les routeurs acheminent les paquets vers le service.",
        "Le réseau du site reçoit la requête destinée aux serveurs.",
        "Découvrez maintenant les équipements physiques du service." };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEvidence() { CompletedStages = 0; CompletedDuration = 0; }

    public void StartNetworkRide()
    {
        if (IsRunning || stepTransition == null || stepTransition.IsTriggered) return;
        IsRunning = true;
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        // Résoudre la caméra active après le choix PC/VR, jamais la caméra PC inactive.
        var camera = Camera.main;
        _controller = camera != null ? camera.GetComponentInParent<PCPlayerController>() : null;
        _controllerWasEnabled = _controller != null && _controller.enabled;
        if (_controller != null) _controller.enabled = false;
        _seat = FindFirstObjectByType<ChairSeatInteraction>();
        _seatWasEnabled = _seat != null && _seat.enabled;
        if (_seat != null) _seat.enabled = false;
        var canvas = hudPanel.GetComponent<Canvas>();
        bool vr = InputModeManager.Instance != null && InputModeManager.Instance.IsVR;
        canvas.worldCamera = camera;
        canvas.renderMode = vr ? RenderMode.WorldSpace : RenderMode.ScreenSpaceCamera;
        canvas.planeDistance = .5f;
        if (vr && camera != null)
        {
            // Garder le tracking : aucun déplacement artificiel de la caméra XR.
            hudPanel.transform.SetParent(camera.transform, false);
            hudPanel.transform.localPosition = new Vector3(0, 0, 2);
            hudPanel.transform.localScale = Vector3.one * .0018f;
            hudPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1280, 720);
        }
        hudPanel.SetActive(true);
        CompletedStages = 0;
        float started = Time.unscaledTime;
        Vector3 cameraPosition = camera != null ? camera.transform.position : Vector3.zero;
        Quaternion cameraRotation = camera != null ? camera.transform.rotation : Quaternion.identity;
        for (int i = 0; i < Titles.Length; i++)
        {
            CurrentStep = i + 1;
            hudTitle.text = Titles[i]; hudSubtitle.text = Subtitles[i];
            hudProgress.text = $"Étape {CurrentStep} / 6 — trajet illustratif, non mesuré";
            Debug.Log($"[NetworkTransitionV2] Étape {CurrentStep}/6 : {Titles[i]}");
            float elapsed = 0;
            var focus = i == 0 ? tourPC : i == 1 ? boxInternet : null;
            Quaternion look = focus != null && camera != null
                ? Quaternion.LookRotation(focus.position + Vector3.up * (i == 0 ? 1f : .12f) - cameraPosition)
                : cameraRotation;
            Quaternion from = camera != null ? camera.transform.rotation : Quaternion.identity;
            while (elapsed < stepDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = (i + Mathf.Clamp01(elapsed / stepDuration)) / 6f;
                progressBar.fillAmount = progress;
                progressBar.rectTransform.localScale = new Vector3(progress, 1, 1);
                if (!vr && camera != null)
                    camera.transform.rotation = Quaternion.Slerp(from, look, Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / .6f)));
                yield return null;
            }
            CompletedStages++;
        }
        CompletedDuration = Time.unscaledTime - started;
        if (!vr && camera != null) camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
        Debug.Log($"[NetworkTransitionV2] Six étapes terminées en {CompletedDuration:F2}s.");
        // Le chargement asynchrone reste centralisé dans StepTransition.
        stepTransition.TriggerTransition();
    }

    void OnDisable()
    {
        if (_controller != null) _controller.enabled = _controllerWasEnabled;
        if (_seat != null) _seat.enabled = _seatWasEnabled;
        IsRunning = false;
    }
}
