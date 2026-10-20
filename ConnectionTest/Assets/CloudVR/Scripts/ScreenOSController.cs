using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère l'affichage moderne du système d'exploitation sur l'écran du PC :
/// barre de progression fluide, étapes techniques détaillées (chiffrement, TLS, paquets, stockage).
/// Compatible avec PhotoSender et tous les tests automatisés (DemoSmoke, JourneySmoke).
/// </summary>
public class ScreenOSController : MonoBehaviour
{
    [Header("Références PhotoSender")]
    public PhotoSender sender;

    [Header("Éléments UI")]
    public Text sendingStatusText;
    public Image progressBarFill;
    public Text progressPercentText;

    private Coroutine _progressCoroutine;

    private void Awake()
    {
        if (sender == null) sender = GetComponent<PhotoSender>();
        EnsureUIElements();
    }

    private void Start()
    {
        if (sender == null) sender = GetComponent<PhotoSender>();
        if (sender != null)
        {
            sender.onSendStarted.AddListener(OnSendStarted);
            sender.onSendCompleted.AddListener(OnSendCompleted);
        }
    }

    public void EnsureUIElements()
    {
        var canvas = transform.Find("Photo_Canvas");
        if (canvas == null) return;

        var sending = canvas.Find("Photo_Sending");
        if (sending != null)
        {
            var status = sending.Find("Status");
            if (status != null)
            {
                sendingStatusText = status.GetComponent<Text>();
                var statusRect = status.GetComponent<RectTransform>();
                if (statusRect != null)
                {
                    statusRect.anchoredPosition = new Vector2(0, 30);
                    statusRect.sizeDelta = new Vector2(540, 80);
                }
            }

            // Création de la barre de progression si non présente
            var barBg = sending.Find("ProgressBar_Bg");
            if (barBg == null)
            {
                var bgGo = new GameObject("ProgressBar_Bg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bgGo.transform.SetParent(sending, false);
                var bgRect = bgGo.GetComponent<RectTransform>();
                bgRect.anchoredPosition = new Vector2(0, -50);
                bgRect.sizeDelta = new Vector2(420, 22);
                var bgImg = bgGo.GetComponent<Image>();
                bgImg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);
                bgImg.raycastTarget = false;

                var fillGo = new GameObject("ProgressBar_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fillGo.transform.SetParent(bgGo.transform, false);
                var fillRect = fillGo.GetComponent<RectTransform>();
                fillRect.anchorMin = new Vector2(0, 0);
                fillRect.anchorMax = new Vector2(0, 1);
                fillRect.pivot = new Vector2(0, 0.5f);
                fillRect.anchoredPosition = Vector2.zero;
                fillRect.sizeDelta = new Vector2(0, 0);
                var fillImg = fillGo.GetComponent<Image>();
                fillImg.color = new Color(0.05f, 0.85f, 0.95f); // Cyan néon
                fillImg.raycastTarget = false;
                progressBarFill = fillImg;

                var percentGo = new GameObject("ProgressPercent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                percentGo.transform.SetParent(sending, false);
                var percentRect = percentGo.GetComponent<RectTransform>();
                percentRect.anchoredPosition = new Vector2(0, -90);
                percentRect.sizeDelta = new Vector2(400, 30);
                var percentTxt = percentGo.GetComponent<Text>();
                percentTxt.font = sendingStatusText != null ? sendingStatusText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                percentTxt.fontSize = 22;
                percentTxt.alignment = TextAnchor.MiddleCenter;
                percentTxt.color = new Color(0.80f, 0.92f, 1.0f);
                percentTxt.raycastTarget = false;
                progressPercentText = percentTxt;
            }
            else
            {
                var fill = barBg.Find("ProgressBar_Fill");
                if (fill != null) progressBarFill = fill.GetComponent<Image>();
                var percent = sending.Find("ProgressPercent");
                if (percent != null) progressPercentText = percent.GetComponent<Text>();
            }
        }
    }

    private void OnSendStarted()
    {
        EnsureUIElements();
        if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
        _progressCoroutine = StartCoroutine(AnimateProgress());
    }

    private IEnumerator AnimateProgress()
    {
        float duration = sender != null ? sender.sendDuration : 2.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (progressBarFill != null)
            {
                var parentRect = progressBarFill.transform.parent.GetComponent<RectTransform>();
                float maxWidth = parentRect != null ? parentRect.sizeDelta.x : 420f;
                var fillRect = progressBarFill.rectTransform;
                fillRect.sizeDelta = new Vector2(maxWidth * t, 0);
            }

            if (progressPercentText != null)
            {
                progressPercentText.text = Mathf.RoundToInt(t * 100f) + " %";
            }

            if (sendingStatusText != null)
            {
                if (t < 0.25f)
                    sendingStatusText.text = "Chiffrement AES-256...";
                else if (t < 0.55f)
                    sendingStatusText.text = "Handshake TLS 1.3...";
                else if (t < 0.85f)
                    sendingStatusText.text = "Routage paquets IP...";
                else
                    sendingStatusText.text = "Arrivée switch frontal DC...";
            }
            yield return null;
        }

        if (progressBarFill != null)
        {
            var parentRect = progressBarFill.transform.parent.GetComponent<RectTransform>();
            float maxWidth = parentRect != null ? parentRect.sizeDelta.x : 420f;
            progressBarFill.rectTransform.sizeDelta = new Vector2(maxWidth, 0);
        }
        if (progressPercentText != null) progressPercentText.text = "100 %";
        if (sendingStatusText != null) sendingStatusText.text = "✓ Paquets délivrés au Data Center !";
    }

    private void OnSendCompleted()
    {
        var canvas = transform.Find("Photo_Canvas");
        if (canvas != null)
        {
            var completed = canvas.Find("Photo_Completed");
            if (completed != null)
            {
                var status = completed.Find("Status");
                if (status != null)
                {
                    var txt = status.GetComponent<Text>();
                    if (txt != null) txt.text = "✓ Fichier persisté au Data Center\nConnexion en cours...";
                }
            }
        }
    }
}
