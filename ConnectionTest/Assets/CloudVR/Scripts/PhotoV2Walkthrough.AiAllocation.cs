using System;
using UnityEngine;

public sealed partial class PhotoV2Walkthrough
{
    static readonly (string Name, string Work, int Base, int Demand)[] AiClients =
    {
        ("Assistant IA", "Des utilisateurs attendent une réponse", 1, 3),
        ("Studio vidéo", "Une diffusion est programmée", 1, 2),
        ("Recherche produit", "Un lot de calculs peut être reporté", 0, 2),
    };

    // Les deux premiers départs du tableau alimentent les rangées physiques 05 et 06.
    static readonly string[] AiExtraRows = { "05", "06" };
    readonly int[] _aiAssignedClients = new int[AiExtraRows.Length];
    int _aiAssignedCount;
    bool _aiAllocationOpen, _aiAllocationConfirmed;

    void OpenAiAllocation()
    {
        _aiAllocationOpen = true;
        SetAiModalInput(true);
    }

    internal void CloseAiAllocation()
    {
        if (!_aiAllocationOpen) return;
        _aiAllocationOpen = false;
        SetAiModalInput(false);
    }

    internal int AiReservedBlocks(int client)
    {
        int total = AiClients[client].Base;
        for (int i = 0; i < _aiAssignedCount; i++)
            if (_aiAssignedClients[i] == client) total++;
        return total;
    }

    internal int AiServedBlocks(int client)
    {
        int total = AiClients[client].Base;
        if (!_aiAllocationConfirmed) return total;
        for (int i = 0; i < _aiAssignedCount; i++)
            if (_aiAssignedClients[i] == client && energy.rowLights[PhotoV2Energy.BaseRows + i].activeSelf) total++;
        return total;
    }

    bool CanReserveAiBlock(int client) =>
        _aiAssignedCount < _aiAssignedClients.Length && AiReservedBlocks(client) < AiClients[client].Demand;

    internal bool ReserveAiBlock(int client)
    {
        if (!_aiAllocationOpen || !CanReserveAiBlock(client)) return false;
        _aiAssignedClients[_aiAssignedCount++] = client;
        return true;
    }

    internal bool ReleaseAiBlock(int client)
    {
        if (!_aiAllocationOpen || _aiAssignedCount == 0) return false;
        int index = Array.LastIndexOf(_aiAssignedClients, client, _aiAssignedCount - 1, _aiAssignedCount);
        if (index < 0) return false;
        _aiAssignedCount--;
        Array.Copy(_aiAssignedClients, index + 1, _aiAssignedClients, index, _aiAssignedCount - index);
        return true;
    }

    internal bool ConfirmAiAllocation()
    {
        if (!_aiAllocationOpen || _aiAssignedCount != _aiAssignedClients.Length || !energy.TrySelect(energy.control))
            return false;
        _aiAllocationConfirmed = true;
        CloseAiAllocation();
        return true;
    }

    string AiRowAssignment(int index) =>
        $"Rangée {AiExtraRows[index]} · {(index < _aiAssignedCount ? AiClients[_aiAssignedClients[index]].Name : "À attribuer")}";

    string AiClientResult(int client)
    {
        int reserved = AiReservedBlocks(client);
        return $"{AiClients[client].Name} · {AiServedBlocks(client)} en service / {reserved} réservés · "
            + $"{AiClients[client].Demand - reserved} en attente de capacité";
    }

    void DrawAiAllocation()
    {
        const float width = 754, height = 399;
        float scale = Mathf.Min(1, (Screen.width - 24) / width, (Screen.height - 24) / height);
        var matrix = GUI.matrix;
        var offset = new Vector3((Screen.width - width * scale) / 2, (Screen.height - height * scale) / 2);
        GUI.matrix = matrix * Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1));
        bool enabled = GUI.enabled;
        MapFill(new Rect(0, 0, width, height), MapBackground);
        GUI.Label(new Rect(22, 14, 710, 28), "Répartir les blocs de calcul", _heading);
        GUI.Label(new Rect(22, 46, 710, 24),
            "Le data center héberge les serveurs. Le fournisseur cloud répartit le calcul.", _small);
        GUI.Label(new Rect(22, 75, 710, 28),
            $"Blocs à attribuer · {_aiAssignedClients.Length - _aiAssignedCount} / {_aiAssignedClients.Length}", _instruction);

        GUI.Label(new Rect(344, 111, 88, 22), "Demandés", _small);
        GUI.Label(new Rect(439, 111, 86, 22), "Réservés", _small);
        GUI.Label(new Rect(520, 111, 113, 22), "En attente", _small);
        for (int i = 0; i < AiClients.Length; i++)
        {
            float top = 138 + i * 50;
            var client = AiClients[i];
            int reserved = AiReservedBlocks(i);
            MapFill(new Rect(22, top, 710, 46), MapZone);
            GUI.Label(new Rect(32, top + 1, 302, 23), client.Name, _heading);
            GUI.Label(new Rect(32, top + 23, 302, 22), client.Work, _small);
            GUI.Label(new Rect(371, top + 11, 39, 24), client.Demand.ToString(), _small);
            GUI.Label(new Rect(463, top + 11, 39, 24), reserved.ToString(), _small);
            GUI.Label(new Rect(546, top + 11, 39, 24), (client.Demand - reserved).ToString(), _small);
            GUI.enabled = enabled && reserved > client.Base;
            if (GUI.Button(new Rect(628, top + 7, 41, 32), "−")) ReleaseAiBlock(i);
            GUI.enabled = enabled && CanReserveAiBlock(i);
            if (GUI.Button(new Rect(680, top + 7, 41, 32), "+")) ReserveAiBlock(i);
        }

        GUI.enabled = enabled;
        GUI.Label(new Rect(22, 299, 346, 24), AiRowAssignment(0), _small);
        GUI.Label(new Rect(386, 299, 346, 24), AiRowAssignment(1), _small);
        GUI.Label(new Rect(22, 327, 710, 23), "Chaque nouveau bloc sera en service une fois sa rangée alimentée.", _small);
        if (GUI.Button(new Rect(22, 354, 122, 31), "Fermer")) CloseAiAllocation();
        GUI.enabled = enabled && _aiAssignedCount == _aiAssignedClients.Length;
        if (GUI.Button(new Rect(497, 354, 235, 31), "Valider la répartition")) ConfirmAiAllocation();
        GUI.enabled = enabled;
        GUI.matrix = matrix;
    }

    void DrawAiAllocationStatus()
    {
        float width = Mathf.Min(518, Screen.width - 36);
        GUI.Box(new Rect(18, 190, width, 123), GUIContent.none);
        GUI.Label(new Rect(30, 195, width - 24, 25), "BLOCS DE CALCUL PAR CLIENT", _heading);
        for (int client = 0; client < AiClients.Length; client++)
            GUI.Label(new Rect(30, 220 + client * 21, width - 24, 23), AiClientResult(client), _small);
        string nextRow = energy.PendingBreaker == energy.breakers[0] ? AiRowAssignment(0) : AiRowAssignment(1);
        if (!energy.Changing && !energy.ReadyForGrid)
            GUI.Label(new Rect(30, 283, width - 24, 25), $"Prochain départ · {nextRow}", _small);
    }
}
