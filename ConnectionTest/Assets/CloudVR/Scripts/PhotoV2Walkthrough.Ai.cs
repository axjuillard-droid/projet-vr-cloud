using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public sealed partial class PhotoV2Walkthrough
{
    sealed class AiSite
    {
        public readonly string Name, Climate;
        public readonly float Rate;
        public readonly int Carbon, NetworkMs, CoolingKw, WaterLitres;

        public AiSite(string name, string climate, float rate, int carbon, int networkMs, int coolingKw, int waterLitres)
        {
            Name = name;
            Climate = climate;
            Rate = rate;
            Carbon = carbon;
            NetworkMs = networkMs;
            CoolingKw = coolingKw;
            WaterLitres = waterLitres;
        }
    }

    static readonly AiSite[] AiSites =
    {
        new("France", "18 °C · 65 % HR", .10f, 50, 25, 9, 75),
        new("États-Unis", "29 °C · 35 % HR", .08f, 380, 105, 15, 100),
        new("Chine", "26 °C · 75 % HR", .09f, 530, 190, 18, 125)
    };
    static readonly string[] AiSiteNames = { "France", "États-Unis", "Chine" };

    [Serializable] sealed class AiQuestion { public string question; }
    [Serializable] sealed class AiSource { public string title; public string url; }
    [Serializable] sealed class AiReply
    {
        public string status;
        public string answer;
        public AiSource[] sources;
        public long generation_ms;
    }

    const string RagEndpoint = "http://127.0.0.1:8765/question";
    bool _aiMode, _aiEntryOpen, _aiAnswerOpen, _aiReturned, _aiReturning, _aiPending;
    int _aiSiteIndex;
    string _aiQuestion = "";
    string _aiError;
    AiReply _aiReply;
    Vector3 _aiHome;
    Quaternion _aiHomeRotation;
    Vector2 _aiScroll;
    Text _aiMonitor;
    UnityWebRequest _aiRequest;
    AiSite SelectedAiSite => AiSites[_aiSiteIndex];
    bool AiModalOpen => _aiEntryOpen || _aiAnswerOpen || _aiReturning || _aiAllocationOpen || _aiPowerOpen;
    internal bool AiAnswerVisible => _aiAnswerOpen && _aiReturned && _aiReply != null;
    internal bool AiHasReply => _aiReply != null;
    internal string AiReplyStatus => _aiReply?.status;
    internal bool AiRequestPending => _aiPending;
    internal bool AiReturned => _aiReturned;

    void SetUpAiMode()
    {
        _aiMode = Array.Exists(Environment.GetCommandLineArgs(), arg => arg == "--cloudvr-ai");
        if (!_aiMode) return;

        _aiLand = Resources.Load<Texture2D>("CloudVR/AiRouteLand");
        _aiHome = player.transform.position;
        _aiHomeRotation = player.transform.rotation;
        _aiMonitor = targets[0].GetComponentInChildren<Text>();
        if (_aiMonitor) _aiMonitor.text = "POSER UNE QUESTION";
        water.ConfigureAi(SelectedAiSite.WaterLitres);
        cooling.AiAuxiliaryKw = SelectedAiSite.CoolingKw;
        energy.AiDemandLimit = PhotoV2Energy.BaseRows + _aiAssignedClients.Length;
        foreach (var label in FindObjectsByType<Text>(FindObjectsSortMode.None))
        {
            if (label.text == "MÉTÉO SUIVANTE") label.text = "APPOINT EN EAU";
        }
        ramLabel.text = "PRÉPARER LA QUESTION";
        previewLabel.text = "GPU · EN ATTENTE";
        InstallAiCutaway();
        Debug.Log("CLOUDVR_AI_READY");
    }

    void OpenAiTerminal()
    {
        _aiEntryOpen = !_aiReturned;
        _aiAnswerOpen = _aiReturned;
        if (_aiAnswerOpen)
        {
            AiTerminalTab = 0;
            _aiScroll = Vector2.zero;
            _aiComparisonScroll = Vector2.zero;
        }
        SetAiModalInput(true);
    }

    void CloseAiTerminal()
    {
        _aiEntryOpen = false;
        _aiAnswerOpen = false;
        SetAiModalInput(false);
    }

    void SetAiModalInput(bool open)
    {
        player.enabled = !open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
    }

    internal bool SubmitAiQuestion(string question, int site)
    {
        if (!_aiEntryOpen || _step != 0 || _aiPending || string.IsNullOrWhiteSpace(question)
            || question.Trim().Length > 500 || site < 0 || site >= AiSites.Length)
            return false;

        _aiQuestion = question.Trim();
        _aiSiteIndex = site;
        water.ConfigureAi(SelectedAiSite.WaterLitres);
        cooling.AiAuxiliaryKw = SelectedAiSite.CoolingKw;
        CloseAiTerminal();
        StartCoroutine(AskAi());
        StartCoroutine(PlayTransit());
        return true;
    }

    IEnumerator AskAi()
    {
        _aiError = null;
        _aiReply = null;
        _aiPending = true;
        using var request = new UnityWebRequest(RagEndpoint, UnityWebRequest.kHttpVerbPOST);
        _aiRequest = request;
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(
            JsonUtility.ToJson(new AiQuestion { question = _aiQuestion })));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        request.timeout = 85;
        yield return request.SendWebRequest();
        _aiRequest = null;
        _aiPending = false;
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var reply = JsonUtility.FromJson<AiReply>(request.downloadHandler.text);
                if (reply != null && !string.IsNullOrWhiteSpace(reply.answer)
                    && (reply.status == "ok" || reply.status == "hors_corpus"))
                    _aiReply = reply;
            }
            catch (ArgumentException)
            {
                _aiError = "Le service a renvoyé une réponse illisible.";
            }
        }
        if (_aiReply == null && _aiError == null)
            _aiError = "Service local indisponible. Démarrez le RAG et Ollama, puis réessayez.";
        Debug.Log(_aiReply != null ? "CLOUDVR_AI_REPLY_BUFFERED" : "CLOUDVR_AI_REPLY_UNAVAILABLE");
    }

    IEnumerator ReturnAiToRoom()
    {
        _aiReturning = true;
        player.enabled = false;
        Debug.Log("CLOUDVR_AI_RETURN_START");
        float started = Time.unscaledTime;
        while (Time.unscaledTime - started < AiReturnSeconds)
        {
            _aiReturnProgress = (Time.unscaledTime - started) / AiReturnSeconds;
            yield return null;
        }
        var capsule = player.GetComponent<CharacterController>();
        capsule.enabled = false;
        player.transform.SetPositionAndRotation(_aiHome, _aiHomeRotation);
        player.ResetLook();
        capsule.enabled = true;
        _aiReturned = true;
        _aiReturning = false;
        targets[0].GetComponent<Collider>().enabled = true;
        if (_aiMonitor) _aiMonitor.text = "CONSULTER LA RÉPONSE";
        Debug.Log("CLOUDVR_AI_RETURNED");
        OpenAiTerminal();
    }

    void DrawAiTerminal()
    {
        if (_aiReturning)
        {
            DrawAiRouteMap(_aiReturnProgress, true);
            return;
        }
        float width = Mathf.Min(_aiReturned ? 920 : 690, Screen.width - 32);
        float height = Mathf.Min(_aiReturned ? 620 : 370, Screen.height - 24);
        var panel = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
        if (_aiReturned) MapFill(panel, MapBackground);
        else GUI.Box(panel, GUIContent.none);
        GUILayout.BeginArea(new Rect(panel.x + 22, panel.y + 16, width - 44, height - 32));
        if (_aiEntryOpen)
        {
            GUILayout.Label("Poser une question", _heading);
            GUILayout.Label("Question sur le cloud, les réseaux, l’électricité ou le refroidissement", _instruction);
            GUI.SetNextControlName("CloudVrQuestion");
            _aiQuestion = GUILayout.TextArea(_aiQuestion, 500, GUILayout.Height(73));
            GUILayout.Space(12);
            GUILayout.Label("Choisir le site simulé", _small);
            _aiSiteIndex = GUILayout.SelectionGrid(_aiSiteIndex, AiSiteNames, 3, GUILayout.Height(32));
            GUILayout.Label(SelectedAiSite.Climate + " · génération réelle sur ce PC", _small);
            GUILayout.FlexibleSpace();
            bool enabledBefore = GUI.enabled;
            GUI.enabled = !string.IsNullOrWhiteSpace(_aiQuestion);
            if (GUILayout.Button("Envoyer la question", GUILayout.Height(32)))
                SubmitAiQuestion(_aiQuestion, _aiSiteIndex);
            GUI.enabled = enabledBefore;
            if (GUILayout.Button("Fermer")) CloseAiTerminal();
        }
        else
        {
            AiTerminalTab = GUILayout.Toolbar(AiTerminalTab, AiTerminalTabs, GUILayout.Height(34));
            GUILayout.Space(12);
            if (AiTerminalTab == 0) DrawAiAnswerPane();
            else DrawAiSiteComparison();
            if (GUILayout.Button("Fermer")) CloseAiTerminal();
        }
        GUILayout.EndArea();
    }

    void DrawAiAnswerPane()
    {
        GUILayout.Label("Réponse à ta question", _heading);
        _aiScroll = GUILayout.BeginScrollView(_aiScroll, GUILayout.ExpandHeight(true));
        DrawAiLiteral(_aiQuestion, _instruction);
        GUILayout.Space(8);
        if (AiAnswerVisible)
        {
            DrawAiLiteral(_aiReply.answer, _instruction);
            if (_aiReply.sources?.Length > 0)
            {
                GUILayout.Space(8);
                GUILayout.Label("Sources consultées", _heading);
                foreach (var source in _aiReply.sources)
                {
                    if (source == null) continue;
                    DrawAiLiteral(source.title, _instruction);
                    DrawAiLiteral(source.url, _instruction);
                }
            }
        }
        else GUILayout.Label(_aiPending ? "Génération locale en cours…" : _aiError, _instruction);
        GUILayout.EndScrollView();
        if (!_aiPending && _aiReply == null && GUILayout.Button("Réessayer la même question"))
            StartCoroutine(AskAi());
    }

    static void DrawAiLiteral(string value, GUIStyle style)
    {
        bool richText = style.richText;
        style.richText = false;
        GUILayout.Label(value ?? "", style);
        style.richText = richText;
    }

    string AiInstruction()
    {
        if (_aiReturned) return "Viser le PC pour lire la réponse";
        if (_step == 0) return "Viser le PC pour poser une question";
        if (_step == PreviewStep) return AiInferenceInstruction();
        return Instruction;
    }

    void DrawAiLedger()
    {
        var site = SelectedAiSite;
        int rows = _step >= StorageStep ? energy.ActiveRows : 0;
        float auxiliary = cooling.Open ? site.CoolingKw : 0;
        int litres = water.Active ? water.Withdrawn : 0;
        float total = SiteEnergy(rows, auxiliary, litres);
        float width = Mathf.Min(430, Screen.width - 36);
        float left = Screen.width - width - 18;
        float top = Screen.height - 149;
        GUI.Box(new Rect(left, top, width, 132), GUIContent.none);
        GUI.Label(new Rect(left + 10, top + 6, width - 20, 27), site.Name + " · bilan fictif sur 1 h", _heading);
        GUI.Label(new Rect(left + 10, top + 34, width - 20, 22),
            "Joueur · PC et box · 0,07 kWh · 0,015 € · 3,5 g CO₂e", _small);
        GUI.Label(new Rect(left + 10, top + 56, width - 20, 22),
            $"Site · {total:0.00} kWh · {total * site.Rate:0.00} € · {total * site.Carbon:0} g CO₂e", _small);
        GUI.Label(new Rect(left + 10, top + 78, width - 20, 22),
            $"Bâtiment {BaseSiteEnergy(rows):0} · serveurs {rows * PhotoV2Energy.RowPowerKw} · froid {auxiliary:0} · pompe {litres * AiPumpKwhPerLitre:0.00} kWh", _small);
        GUI.Label(new Rect(left + 10, top + 100, width - 20, 22),
            $"Aller-retour réseau {site.NetworkMs} ms · file {AiQueueMs} ms · calcul {AiComputeMs} ms", _small);
        DrawAiLedgerFeedback(top);
    }

    const float AiPumpKwhPerLitre = .004f;

    static float SiteEnergy(int rows, float auxiliaryKw, int litres) =>
        BaseSiteEnergy(rows) + PhotoV2Energy.RowPowerKw * rows + auxiliaryKw + litres * AiPumpKwhPerLitre;

    static float BaseSiteEnergy(int rows) => rows > 0 ? PhotoV2Energy.SiteOverheadKw : 0;
}
