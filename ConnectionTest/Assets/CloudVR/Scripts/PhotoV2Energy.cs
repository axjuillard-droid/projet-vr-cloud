using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PhotoV2Energy : MonoBehaviour
{
    public Collider control;
    public Transform handle;
    public Text meter;
    public GameObject[] rowLights;

    const int BaseRows = 2;
    const int SiteOverheadKw = 18;
    const int RowPowerKw = 15;
    public const int LimitKw = 120;

    public int ActiveRows { get; private set; } = BaseRows;
    public int RequestedRows { get; private set; } = BaseRows;
    public bool Changing { get; private set; }
    public bool HasRaisedDemand { get; private set; }
    public int PowerKw => SiteOverheadKw + RowPowerKw * ActiveRows;
    public string Reading => $"{PowerKw} / {LimitKw} kW  ·  {ActiveRows} / {rowLights.Length} rangées";

    void OnEnable()
    {
        UpdateMeter();
    }

    public bool TrySelect(Collider target)
    {
        if (!isActiveAndEnabled || target != control || Changing) return false;
        RequestedRows = ActiveRows == rowLights.Length ? BaseRows : ActiveRows + 2;
        StartCoroutine(ChangeDemand());
        return true;
    }

    IEnumerator ChangeDemand()
    {
        Changing = true;
        var initial = handle.localRotation;
        float notch = (RequestedRows - BaseRows) / 2f;
        var next = Quaternion.Euler(-24 + notch*24,0,0);
        for (float t = 0; t < .24f; t += Time.deltaTime)
        {
            handle.localRotation = Quaternion.Slerp(initial,next,t/.24f);
            yield return null;
        }
        handle.localRotation = next;
        while (ActiveRows != RequestedRows)
        {
            if (ActiveRows < RequestedRows)
                rowLights[ActiveRows++].SetActive(true);
            else
                rowLights[--ActiveRows].SetActive(false);
            UpdateMeter();
            yield return new WaitForSeconds(.32f);
        }
        Changing = false;
        if (ActiveRows > BaseRows) HasRaisedDemand = true;
        Debug.Log($"CLOUDVR_V2_ENERGY {ActiveRows}/{rowLights.Length} rangées, {PowerKw} kW simulés");
    }

    void UpdateMeter()
    {
        meter.text = Reading + "\nScénario illustratif";
    }
}
