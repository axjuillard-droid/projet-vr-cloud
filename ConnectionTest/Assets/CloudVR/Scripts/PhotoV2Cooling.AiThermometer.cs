using UnityEngine;
using UnityEngine.UI;

public sealed partial class PhotoV2Cooling
{
    const float AiNormalC = 23f;
    const float AiAlertC = 27f;
    const float AiGaugeMaxC = 32f;
    const float GaugeWidth = 1300;
    static readonly Color AiThermalBlue = new(.41f, .89f, 1f);
    static readonly Color AiThermalAmber = new(1f, .67f, .31f);

    RectTransform _aiTemperatureBar;
    Image _aiTemperatureFill;
    Text _aiThermalLegend, _aiThermalPeak;
    float _aiPeakTemperature;
    string _aiDisplayedPhase;

    internal string AiThermalPhase => Stabilized ? "Retour refroidi · température stabilisée" :
        ReturnComplete ? "Retour refroidi · température en baisse" :
        Open ? "Débit renforcé · attente du retour refroidi" :
        _turning ? "Ouverture de la vanne" :
        Temperature >= AiAlertC ? "Alerte · température élevée" :
        energy.ActiveRows > PhotoV2Energy.BaseRows ? "Température en hausse" : "Débit normal";

    internal string AiThermalSummary => $"Boucle thermique · {Temperature:0.0} °C · {AiThermalPhase}";
    internal float AiThermalPeak => _aiPeakTemperature;
    internal bool AiThermometerVisible => _aiTemperatureBar && _aiTemperatureBar.gameObject.activeInHierarchy;

    internal void InstallAiThermometer()
    {
        var panel = (RectTransform)status.transform.parent;
        panel.name = "Thermomètre de la boucle IA";
        panel.position += Vector3.up * .37f;
        panel.localScale = Vector3.one * .00090f;
        panel.sizeDelta = new Vector2(1460, 650);

        Rectangle(panel, "Fond", new Rect(0, 0, 1460, 650), new Color(.025f, .057f, .081f))
            .transform.SetAsFirstSibling();
        Rectangle(panel, "Séparateur", new Rect(80, 118, 1300, 3), new Color(.22f, .35f, .40f));
        Label(panel, "BOUCLE DE REFROIDISSEMENT", new Rect(80, 33, 1300, 72), 61, Color.white);

        SetBounds(status.rectTransform, new Rect(80, 134, 760, 155));
        status.fontSize = 127;
        status.fontStyle = FontStyle.Bold;
        status.alignment = TextAnchor.MiddleLeft;
        Label(panel, "TEMPÉRATURE\nSIMULÉE", new Rect(890, 148, 490, 129), 54, AiThermalBlue);
        _aiThermalLegend = Label(panel, "", new Rect(80, 307, 1300, 87), 61, Color.white);

        Rectangle(panel, "Échelle", new Rect(80, 433, GaugeWidth, 48), new Color(.12f, .22f, .27f));
        _aiTemperatureFill = Rectangle(panel, "Température", new Rect(80, 433, 0, 48), AiThermalBlue);
        _aiTemperatureBar = _aiTemperatureFill.rectTransform;
        float tick = 80 + GaugeWidth * Mathf.InverseLerp(AiNormalC, AiGaugeMaxC, AiAlertC);
        Rectangle(panel, "Repère d’alerte", new Rect(tick, 414, 5, 81), Color.white);
        Label(panel, "23 °C", new Rect(80, 497, 235, 60), 49, Color.white);
        Label(panel, "Alerte 27 °C", new Rect(tick - 130, 497, 340, 60), 49, Color.white);
        _aiThermalPeak = Label(panel, "", new Rect(80, 570, 1300, 60), 50, AiThermalBlue);
        _aiPeakTemperature = Temperature;
        _lastDisplayed = -1;
        UpdateAiThermometer();
    }

    void UpdateAiThermometer()
    {
        _aiPeakTemperature = Mathf.Max(Temperature, _aiPeakTemperature);
        int displayed = Mathf.RoundToInt(Temperature * 10f);
        string phase = AiThermalPhase;
        if (displayed == _lastDisplayed && phase == _aiDisplayedPhase) return;
        _lastDisplayed = displayed;
        _aiDisplayedPhase = phase;
        bool hot = Temperature >= AiAlertC;
        Color color = hot ? AiThermalAmber : AiThermalBlue;
        status.text = $"{Temperature:0.0} °C";
        status.color = color;
        _aiThermalLegend.text = phase;
        _aiThermalPeak.text = $"Pic depuis la mise en charge   {_aiPeakTemperature:0.0} °C";
        _aiTemperatureFill.color = color;
        _aiTemperatureBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
            GaugeWidth * Mathf.InverseLerp(AiNormalC, AiGaugeMaxC, Temperature));
    }

    Text Label(RectTransform panel, string value, Rect bounds, int size, Color color)
    {
        var label = new GameObject(value, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(panel, false);
        SetBounds(label.rectTransform, bounds);
        label.font = status.font;
        label.fontSize = size;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = color;
        label.text = value;
        label.raycastTarget = false;
        return label;
    }

    static Image Rectangle(RectTransform panel, string name, Rect bounds, Color color)
    {
        var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(panel, false);
        SetBounds(image.rectTransform, bounds);
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static void SetBounds(RectTransform rect, Rect bounds)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(bounds.x, -bounds.y);
        rect.sizeDelta = bounds.size;
    }
}
