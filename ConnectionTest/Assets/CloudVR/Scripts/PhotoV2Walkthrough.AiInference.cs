using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public sealed partial class PhotoV2Walkthrough
{
    internal enum AiInferencePhase { AwaitingQuestion, Prepared, Context, Generation, Complete }

    static readonly Color AiMemoryColor = new(.35f, .88f, 1);
    static readonly Color AiComputeColor = new(1, .72f, .39f);
    const int DemonstrationTokens = 5;
    GameObject _aiGpuIndicator;
    GameObject _aiMemoryIndicator;
    int _aiGeneratedTokens;

    internal AiInferencePhase InferencePhase { get; private set; }
    internal int InferenceTokensShown => _aiGeneratedTokens;
    internal bool AiCutawayVisible => _aiGpuIndicator && _aiGpuIndicator.transform.parent.gameObject.activeInHierarchy;
    internal Vector3 AiCutawayPosition => _aiGpuIndicator.transform.parent.position;

    void InstallAiCutaway()
    {
        var cutaway = new GameObject("Coupe pédagogique GPU").transform;
        cutaway.SetParent(schoolRack.hardwareLabels.transform, false);
        cutaway.localPosition = new Vector3(0, .17f, .58f);
        var casing = targets[PreviewStep].GetComponent<MeshRenderer>().sharedMaterial;
        var memory = targets[ComputeStep].GetComponent<MeshRenderer>().sharedMaterial;
        var compute = computeOutput.GetComponent<MeshRenderer>().sharedMaterial;
        CutawayBlock(cutaway, "Carte GPU", Vector3.zero, new Vector3(.31f, .02f, .23f), casing);
        CutawayBlock(cutaway, "Puce de calcul", new Vector3(.02f, .022f, .015f),
            new Vector3(.106f, .024f, .106f), casing);
        _aiGpuIndicator = CutawayBlock(cutaway, "Activité du GPU", new Vector3(.02f, .039f, .015f),
            new Vector3(.084f, .014f, .084f), compute);
        _aiMemoryIndicator = new GameObject("Mémoire du GPU");
        _aiMemoryIndicator.transform.SetParent(cutaway, false);
        for (int i = 0; i < 4; i++)
        {
            float z = -.082f + i * .052f;
            CutawayBlock(_aiMemoryIndicator.transform, "Module mémoire", new Vector3(-.105f, .027f, z),
                new Vector3(.041f, .024f, .037f), memory);
        }
        _aiGpuIndicator.SetActive(false);
        _aiMemoryIndicator.SetActive(false);

        foreach (var label in schoolRack.hardwareLabels.GetComponentsInChildren<Text>(true))
        {
            if (label.text == "CPU · traitement")
            {
                label.text = "GPU · calcul";
                label.color = AiComputeColor;
            }
            else if (label.text == "RAM · mémoire de travail")
            {
                label.text = "VRAM · mémoire du GPU";
                label.color = AiMemoryColor;
            }
        }
    }

    static GameObject CutawayBlock(Transform parent, string label, Vector3 position, Vector3 size, Material material)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = label;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = position;
        block.transform.localScale = size;
        var obstacle = block.GetComponent<Collider>();
        obstacle.enabled = false;
        Destroy(obstacle);
        var renderer = block.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        return block;
    }

    void PrepareAiInference()
    {
        InferencePhase = AiInferencePhase.Prepared;
        ramLabel.text = "QUESTION · PRÉPARÉE";
        previewLabel.text = "GPU · LANCER";
        _aiMemoryIndicator.SetActive(true);
        _sound.PlayOneShot(_confirmation, .18f);
    }

    IEnumerator DemonstrateAiInference()
    {
        _computeBusy = true;
        InferencePhase = AiInferencePhase.Context;
        _aiGpuIndicator.SetActive(true);
        ramLabel.text = "MÉMOIRE GPU · MODÈLE + CONTEXTE";
        previewLabel.text = "GPU · CONTEXTE";
        yield return new WaitForSeconds(1.65f);

        InferencePhase = AiInferencePhase.Generation;
        previewLabel.text = "GPU · GÉNÉRATION";
        for (int index = 1; index <= DemonstrationTokens; index++)
        {
            _aiGeneratedTokens = index;
            yield return new WaitForSeconds(.46f);
        }
        InferencePhase = AiInferencePhase.Complete;
        _aiGpuIndicator.SetActive(false);
        previewLabel.text = "DÉMONSTRATION TERMINÉE";
        _sound.PlayOneShot(_confirmation, .18f);
        yield return new WaitForSeconds(1.4f);

        CompleteStep();
        // Le retour IA a lieu dans la chambre, après l’exploitation du site.
        CompleteStep();
        CompleteStep();
        _computeBusy = false;
        Debug.Log("CLOUDVR_AI_INFERENCE_VISITED");
    }

    string AiInferenceInstruction() => InferencePhase switch
    {
        AiInferencePhase.AwaitingQuestion => "Viser PRÉPARER LA QUESTION",
        AiInferencePhase.Prepared => "Viser GPU · LANCER",
        AiInferencePhase.Context => "Le GPU traite le contexte",
        AiInferencePhase.Generation => "La réponse se construit token par token",
        _ => "Lire la réponse au retour dans la chambre"
    };

    void DrawAiInference()
    {
        float width = Mathf.Min(585, Screen.width - 36);
        var panel = new Rect(18, Screen.height - 238, width, 202);
        MapFill(panel, MapBackground);
        GUI.Label(new Rect(panel.x + 14, panel.y + 8, width - 28, 25),
            "Dans le serveur · démonstration", _heading);
        float column = (width - 28) / 3;
        int phaseIndex = InferencePhase >= AiInferencePhase.Generation ? 2 :
            InferencePhase == AiInferencePhase.Context ? 1 : 0;
        DrawAiInferenceCell(panel.x + 14, panel.y + 41, column - 5, "Préparation", phaseIndex == 0, AiMemoryColor);
        DrawAiInferenceCell(panel.x + 14 + column, panel.y + 41, column - 5, "Contexte", phaseIndex == 1, AiComputeColor);
        DrawAiInferenceCell(panel.x + 14 + 2 * column, panel.y + 41, column - 5, "Génération", phaseIndex == 2, AiComputeColor);

        string detail = InferencePhase switch
        {
            AiInferencePhase.AwaitingQuestion => "La question arrive sous forme de texte. Préparez son découpage en tokens.",
            AiInferencePhase.Prepared => "Le texte est découpé en unités appelées tokens. Le modèle est déjà chargé dans la mémoire du GPU.",
            AiInferencePhase.Context => "Le GPU traite les tokens de la question et du contexte. Sa mémoire conserve des calculs utiles pour la suite.",
            AiInferencePhase.Generation => "Le modèle choisit un token, puis s’appuie aussi sur ce token pour calculer le suivant.",
            _ => "Le texte de démonstration est terminé. La réponse réelle sera révélée au PC de départ."
        };
        GUI.Label(new Rect(panel.x + 14, panel.y + 73, width - 28, 72), detail, _instruction);
        if (InferencePhase == AiInferencePhase.Prepared)
        {
            GUI.Label(new Rect(panel.x + 14, panel.y + 150, width - 28, 32),
                "Exemple de découpage  ·  data  |  center  |  ?", _small);
        }
        else if (InferencePhase >= AiInferencePhase.Generation)
        {
            for (int i = 0; i < DemonstrationTokens; i++)
            {
                float tokenWidth = Mathf.Min(91, (width - 30) / DemonstrationTokens);
                var token = new Rect(panel.x + 14 + i * tokenWidth, panel.y + 148, tokenWidth - 5, 28);
                MapFill(token, i < _aiGeneratedTokens ? new Color(.23f, .36f, .39f) : MapZone);
                if (i < _aiGeneratedTokens) GUI.Label(token, $" Token {i + 1}", _small);
            }
        }
    }

    void DrawAiInferenceCell(float x, float y, float width, string title, bool current, Color accent)
    {
        MapFill(new Rect(x, y, width, 22), MapZone);
        if (current) MapFill(new Rect(x, y, 3, 22), accent);
        GUI.Label(new Rect(x + 8, y, width - 8, 22), title, _small);
    }
}
