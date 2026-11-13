using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PhotoV2Energy : MonoBehaviour
{
    public Collider control;
    public Transform handle;
    public Text meter;
    public Text cabinetMeter;
    public GameObject[] rowLights;
    public Collider[] breakers;
    public Transform[] breakerHandles;
    public Text[] breakerLabels;
    public LineRenderer[] feeders;
    public LineRenderer incoming;
    public Material live;
    public Material idle;
    public PhotoV2Cooling cooling;

    public const int BaseRows = 2;
    const int SiteOverheadKw = 18;
    const int RowPowerKw = 15;
    public const int LimitKw = 120;
    readonly bool[] _authorized = new bool[4];

    public int ActiveRows { get; private set; } = BaseRows;
    public int RequestedRows { get; private set; } = BaseRows;
    public bool Changing { get; private set; }
    public bool ReadyForGrid => !Changing && ActiveRows == RequestedRows && ActiveRows > BaseRows;
    public int PowerKw => SiteOverheadKw + RowPowerKw * ActiveRows + cooling.AuxiliaryKw;
    public string Reading => $"{ActiveRows} actives / {RequestedRows} demandées · {PowerKw} / {LimitKw} kW";
    public string Instruction => Changing ? "Observer les départs et les voyants des rangées" :
        ActiveRows < RequestedRows ? "Au tableau électrique, abaisser les leviers marqués DEMANDÉ" :
        ActiveRows > BaseRows ? "Augmenter la demande ou ouvrir VERS LE RÉSEAU" :
        "Actionner la manette DEMANDE DE CALCUL";

    void OnEnable() => Refresh();

    public bool TrySelect(Collider target)
    {
        if (Changing) return false;
        if (target == control)
        {
            if (RequestedRows != ActiveRows) return false;
            bool reset = RequestedRows == rowLights.Length;
            if (reset) System.Array.Clear(_authorized, 0, _authorized.Length);
            return RequestRows(reset ? BaseRows : RequestedRows + 2);
        }

        int index = System.Array.IndexOf(breakers, target);
        if (index < 0 || index + BaseRows >= RequestedRows || _authorized[index]
            || PowerKw + RowPowerKw > LimitKw) return false;
        _authorized[index] = true;
        StartCoroutine(ApplyLoad(breakerHandles[index], Quaternion.Euler(95,0,0)));
        return true;
    }

    public bool TryRequestComparisonLoad(bool higher)
    {
        // La comparaison en rivière suspend ou relance les deux départs déjà autorisés.
        if (higher && (!_authorized[0] || !_authorized[1])) return false;
        return RequestRows(higher ? BaseRows + 2 : BaseRows);
    }

    bool RequestRows(int rows)
    {
        if (!isActiveAndEnabled || Changing || rows == RequestedRows) return false;
        RequestedRows = rows;
        float notch = (RequestedRows - BaseRows) / 2f;
        StartCoroutine(ApplyLoad(handle, Quaternion.Euler(-24 + notch*24,0,0)));
        return true;
    }

    IEnumerator ApplyLoad(Transform lever, Quaternion target)
    {
        Changing = true;
        Refresh();
        var initial = lever.localRotation;
        for (float t = 0; t < .24f; t += Time.deltaTime)
        {
            lever.localRotation = Quaternion.Slerp(initial,target,t/.24f);
            yield return null;
        }
        lever.localRotation = target;
        for (int i = 0; i < breakers.Length; i++)
        {
            int row = i + BaseRows;
            bool active = row < RequestedRows && _authorized[i];
            if (!_authorized[i]) breakerHandles[i].localRotation = Quaternion.identity;
            if (rowLights[row].activeSelf == active) continue;
            rowLights[row].SetActive(active);
            ActiveRows += active ? 1 : -1;
            feeders[i].sharedMaterial = active ? live : idle;
            Refresh();
            yield return new WaitForSeconds(.32f);
        }
        Changing = false;
        Refresh();
        Debug.Log($"CLOUDVR_V2_ENERGY {ActiveRows} actives / {RequestedRows} demandées, {PowerKw} kW simulés");
    }

    public void Refresh()
    {
        meter.text = $"{ActiveRows} / {RequestedRows} rangées\n{PowerKw} / {LimitKw} kW";
        cabinetMeter.text = $"{ActiveRows} ACTIVES / {RequestedRows} DEMANDÉES";
        for (int i = 0; i < breakers.Length; i++)
        {
            int row = i + BaseRows;
            breakerLabels[i].text = rowLights[row].activeSelf ? "EN SERVICE" :
                row < RequestedRows ? "DEMANDÉ" : _authorized[i] ? "EN VEILLE" : "EN ATTENTE";
            breakerLabels[i].color = row < RequestedRows && !_authorized[i]
                ? new Color(1,.69f,.36f) : Color.white;
        }
        incoming.sharedMaterial = ActiveRows > BaseRows ? live : idle;
    }
}
