using UnityEngine;

public sealed partial class PhotoV2Walkthrough
{
    // Tarifs fictifs en €/MWh. Intensités arrondies d’après l’étude de cycle de vie UNECE.
    static readonly (string Name, int ProductionRate, int SpotRate, int Carbon)[] AiPowerCases =
    {
        ("Nucléaire", 55, 85, 6),
        ("Charbon", 95, 135, 900),
    };

    bool _aiPowerOpen, _aiPowerConfirmed;
    int _aiPowerChoice = -1;
    int _aiSeenRows, _aiSeenLitres;
    bool _aiSeenCooling;
    string _aiLedgerFeedback;
    float _aiLedgerKwhChange, _aiLedgerFeedbackUntil;

    internal int AiAdditionalKw => (energy.ActiveRows - PhotoV2Energy.BaseRows) * PhotoV2Energy.RowPowerKw;
    internal string AiLedgerFeedback => _aiLedgerFeedback;
    internal float AiLedgerKwhChange => _aiLedgerKwhChange;
    internal int AiConfirmedPowerChoice => _aiPowerConfirmed ? _aiPowerChoice : -1;

    internal bool PreviewAiPower(int index)
    {
        if (!_aiPowerOpen || index < 0 || index >= AiPowerCases.Length) return false;
        _aiPowerChoice = index;
        return true;
    }

    internal bool ConfirmAiPower()
    {
        if (!_aiPowerOpen || _aiPowerChoice < 0 || !grid.TrySelect(grid.substation, energy)) return false;
        _aiPowerConfirmed = true;
        CloseAiPower();
        _sound.PlayOneShot(_confirmation, .18f);
        return true;
    }

    internal void CloseAiPower()
    {
        _aiPowerOpen = false;
        SetAiModalInput(false);
    }

    internal float AiPowerCarbonKg(int index) => AiAdditionalKw * AiPowerCases[index].Carbon / 1000f;

    void DrawAiPower()
    {
        const float width = 744, height = 404;
        float scale = Mathf.Min(1, (Screen.width - 24) / width, (Screen.height - 24) / height);
        var matrix = GUI.matrix;
        var offset = new Vector3((Screen.width - width * scale) / 2, (Screen.height - height * scale) / 2);
        GUI.matrix = matrix * Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1));
        MapFill(new Rect(0, 0, width, height), MapBackground);
        GUI.Label(new Rect(20, 14, 704, 27), "POSTE ÉLECTRIQUE · COUVRIR LA HAUSSE DE CHARGE", _heading);
        GUI.Label(new Rect(20, 44, 704, 28), $"+{AiAdditionalKw} kW · {AiAdditionalKw} kWh sur une heure simulée", _instruction);
        GUI.Label(new Rect(20, 75, 704, 25),
            "Comparez deux façons de couvrir cette hausse sur le réseau, à service égal.", _small);

        for (int i = 0; i < AiPowerCases.Length; i++)
        {
            var scenario = AiPowerCases[i];
            float left = 20 + i * 358;
            bool selected = _aiPowerChoice == i;
            MapFill(new Rect(left, 111, 346, 156), selected ? MapPath : MapZone);
            GUI.Label(new Rect(left + 12, 116, 322, 25), scenario.Name, _heading);
            GUI.Label(new Rect(left + 12, 142, 322, 24), $"Coût de production · {scenario.ProductionRate} €/MWh", _small);
            GUI.Label(new Rect(left + 12, 167, 322, 24), $"Prix de gros · {scenario.SpotRate} €/MWh", _small);
            GUI.Label(new Rect(left + 12, 192, 322, 24), $"Cycle de vie · {AiPowerCarbonKg(i):0.00} kg CO₂e", _small);
            if (GUI.Button(new Rect(left + 12, 224, 322, 31), selected ? "Scénario sélectionné" : $"Choisir · {scenario.Name}"))
                PreviewAiPower(i);
        }

        var site = SelectedAiSite;
        GUI.Label(new Rect(20, 277, 704, 24),
            $"Facture du site {site.Name} · contrat fictif à {site.Rate * 1000:0} €/MWh · {AiAdditionalKw * site.Rate:0.00} € pour la hausse", _small);
        GUI.Label(new Rect(20, 305, 704, 24),
            "Le prix de gros varie avec le marché. La facture dépend aussi du contrat, du réseau et des taxes.", _small);
        GUI.Label(new Rect(20, 330, 704, 24),
            "Scénarios de production et prix illustratifs · le comparatif des sites repose sur leur mix simulé.", _small);
        if (GUI.Button(new Rect(20, 361, 118, 31), "Fermer")) CloseAiPower();
        bool enabled = GUI.enabled;
        GUI.enabled = enabled && _aiPowerChoice >= 0;
        if (GUI.Button(new Rect(483, 361, 241, 31), "Valider ce scénario")) ConfirmAiPower();
        GUI.enabled = enabled;
        GUI.matrix = matrix;
    }

    void UpdateAiLedgerFeedback()
    {
        for (int i = 0; i < _aiAssignedCount; i++)
        {
            int rowBit = 1 << i;
            if ((_aiSeenRows & rowBit) != 0 || !energy.rowLights[PhotoV2Energy.BaseRows + i].activeSelf) continue;
            _aiSeenRows |= rowBit;
            string client = AiClients[_aiAssignedClients[i]].Name;
            ShowAiLedgerChange($"+1 bloc pour {client} · +{PhotoV2Energy.RowPowerKw} kW", PhotoV2Energy.RowPowerKw);
        }
        if (cooling.Open && !_aiSeenCooling)
        {
            _aiSeenCooling = true;
            ShowAiLedgerChange($"Refroidissement en service · +{SelectedAiSite.CoolingKw} kW", SelectedAiSite.CoolingKw);
        }
        if (water.Active && water.Withdrawn != _aiSeenLitres)
        {
            int delta = water.Withdrawn - _aiSeenLitres;
            _aiSeenLitres = water.Withdrawn;
            ShowAiLedgerChange($"Pompe d’appoint · {delta:+0;-0} L sur 1 h", delta * AiPumpKwhPerLitre);
        }
    }

    void ShowAiLedgerChange(string message, float deltaKwh)
    {
        _aiLedgerFeedback = message;
        _aiLedgerKwhChange = deltaKwh;
        _aiLedgerFeedbackUntil = Time.unscaledTime + 5;
    }

    void DrawAiLedgerFeedback(float ledgerTop)
    {
        if (_aiLedgerFeedback == null || Time.unscaledTime >= _aiLedgerFeedbackUntil || _aiReturned) return;
        float width = Mathf.Min(470, Screen.width - 36);
        var frame = new Rect(Screen.width - width - 18, ledgerTop - 62, width, 55);
        MapFill(frame, MapBackground);
        MapFill(new Rect(frame.x, frame.y, 3, frame.height), MapTarget);
        GUI.Label(new Rect(frame.x + 11, frame.y + 3, width - 22, 24), _aiLedgerFeedback, _heading);
        var site = SelectedAiSite;
        float change = _aiLedgerKwhChange;
        string changeText = $"{change:+0.00;-0.00} kWh · {change * site.Rate:+0.00;-0.00} € · {change * site.Carbon:+0;-0} g CO₂e";
        GUI.Label(new Rect(frame.x + 11, frame.y + 28, width - 22, 22), changeText + " sur 1 h", _small);
    }
}
