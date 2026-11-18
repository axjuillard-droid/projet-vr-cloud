using UnityEngine;

public sealed partial class PhotoV2Walkthrough
{
    const int AiQueueMs = 120;
    const int AiComputeMs = 700;
    const float AiComparisonMinWidth = 490;
    static readonly string[] AiTerminalTabs = { "Réponse IA", "Comparer les sites" };
    static readonly Color AiVisitedColumn = new(.13f, .27f, .30f);
    Vector2 _aiComparisonScroll;
    GUIStyle _aiTableCell, _aiTableValue, _aiTableHeader, _aiTableCaption;
    bool _aiComparisonDetails;

    internal int AiTerminalTab { get; set; }

    internal (int DelayMs, float Kwh, float Cost, float CarbonKg, int WaterLitres) AiCompareSite(int index)
    {
        var site = AiSites[index];
        float kwh = SiteEnergy(energy.ActiveRows, site.CoolingKw, site.WaterLitres);
        return (site.NetworkMs + AiQueueMs + AiComputeMs, kwh, kwh * site.Rate,
            kwh * site.Carbon / 1000f, site.WaterLitres);
    }

    void DrawAiSiteComparison()
    {
        GUILayout.Label("Comparaison à charge égale", _heading);
        GUILayout.Label($"{energy.ActiveRows} rangées en service · bilan du site sur 1 h · délai par requête · valeurs simulées",
            _instruction);
        _aiComparisonScroll = GUILayout.BeginScrollView(_aiComparisonScroll, GUILayout.ExpandHeight(true));
        _aiTableCell ??= new GUIStyle(_small) { alignment = TextAnchor.MiddleLeft };
        _aiTableValue ??= new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        _aiTableHeader ??= new GUIStyle(_heading) { alignment = TextAnchor.MiddleCenter };
        _aiTableCaption ??= new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };

        var france = AiCompareSite(0);
        var usa = AiCompareSite(1);
        var china = AiCompareSite(2);
        DrawAiComparisonRow("Site", AiSiteNames[0], AiSiteNames[1], AiSiteNames[2], true);
        DrawAiComparisonRow("Délai total", $"{france.DelayMs} ms", $"{usa.DelayMs} ms", $"{china.DelayMs} ms");
        DrawAiComparisonRow("Électricité", $"{france.Kwh:0.00} kWh", $"{usa.Kwh:0.00} kWh", $"{china.Kwh:0.00} kWh");
        DrawAiComparisonRow("Coût électrique", $"{france.Cost:0.00} €", $"{usa.Cost:0.00} €", $"{china.Cost:0.00} €");
        DrawAiComparisonRow("Émissions", $"{france.CarbonKg:0.00} kg CO₂e", $"{usa.CarbonKg:0.00} kg CO₂e", $"{china.CarbonKg:0.00} kg CO₂e");
        DrawAiComparisonRow("Eau d’appoint", $"{france.WaterLitres} L", $"{usa.WaterLitres} L", $"{china.WaterLitres} L");

        GUILayout.Space(10);
        if (GUILayout.Button(_aiComparisonDetails ? "Masquer le détail" : "Voir le détail du calcul", GUILayout.Height(29)))
            _aiComparisonDetails = !_aiComparisonDetails;
        if (_aiComparisonDetails) DrawAiComparisonDetails();
        if (_aiAllocationConfirmed)
        {
            GUILayout.Space(12);
            GUILayout.Label("Répartition au belvédère", _heading);
            for (int client = 0; client < AiClients.Length; client++)
                GUILayout.Label(AiClientResult(client), _small);
        }
        if (_aiPowerConfirmed)
        {
            GUILayout.Space(12);
            GUILayout.Label("Scénario exploré au poste électrique", _heading);
            GUILayout.Label($"{AiPowerCases[_aiPowerChoice].Name} · +{AiAdditionalKw} kW · "
                + $"{AiPowerCarbonKg(_aiPowerChoice):0.00} kg CO₂e sur 1 h", _small);
        }
        GUILayout.EndScrollView();
    }

    void DrawAiComparisonDetails()
    {
        DrawAiComparisonRow("Climat du profil", AiSites[0].Climate, AiSites[1].Climate, AiSites[2].Climate);
        DrawAiComparisonRow("Aller-retour réseau", $"{AiSites[0].NetworkMs} ms", $"{AiSites[1].NetworkMs} ms", $"{AiSites[2].NetworkMs} ms");
        DrawAiComparisonRow("Refroidissement", $"{AiSites[0].CoolingKw} kWh", $"{AiSites[1].CoolingKw} kWh", $"{AiSites[2].CoolingKw} kWh");
        DrawAiComparisonRow("Tarif électrique", $"{AiSites[0].Rate:0.00} €/kWh", $"{AiSites[1].Rate:0.00} €/kWh", $"{AiSites[2].Rate:0.00} €/kWh");
        DrawAiComparisonRow("Intensité carbone", $"{AiSites[0].Carbon} g/kWh", $"{AiSites[1].Carbon} g/kWh", $"{AiSites[2].Carbon} g/kWh");
        GUILayout.Space(7);
        GUILayout.Label($"Délai total · aller-retour réseau + {AiQueueMs} ms de file + {AiComputeMs} ms de calcul.", _instruction);
        GUILayout.Label($"Électricité du site · {BaseSiteEnergy(energy.ActiveRows):0} kWh bâtiment + "
            + $"{energy.ActiveRows * PhotoV2Energy.RowPowerKw} kWh serveurs + refroidissement + pompe "
            + $"({AiPumpKwhPerLitre:0.000} kWh/L d’appoint).", _instruction);
        GUILayout.Label("Le coût et les émissions s’obtiennent en appliquant le tarif et l’intensité du site à son électricité.", _instruction);
    }

    void DrawAiComparisonRow(string label, string france, string usa, string china, bool header = false)
    {
        var row = GUILayoutUtility.GetRect(AiComparisonMinWidth, header ? 54 : 32, GUILayout.ExpandWidth(true));
        if (Event.current.type != EventType.Repaint) return;
        const float labelWidth = 146;
        const float gap = 3;
        float width = (row.width - labelWidth - 3 * gap) / 3;
        MapFill(row, MapZone);
        GUI.Label(new Rect(row.x + 10, row.y, labelWidth - 10, row.height), label, _aiTableCell);
        for (int index = 0; index < AiSites.Length; index++)
        {
            var cell = new Rect(row.x + labelWidth + gap + index * (width + gap), row.y, width, row.height);
            bool visited = index == _aiSiteIndex;
            if (visited) MapFill(cell, AiVisitedColumn);
            string value = index == 0 ? france : index == 1 ? usa : china;
            if (header)
            {
                GUI.Label(new Rect(cell.x, cell.y + 4, cell.width, 26), value, _aiTableHeader);
                if (visited) GUI.Label(new Rect(cell.x, cell.y + 29, cell.width, 20), "Site visité", _aiTableCaption);
            }
            else GUI.Label(cell, value, _aiTableValue);
        }
    }
}
