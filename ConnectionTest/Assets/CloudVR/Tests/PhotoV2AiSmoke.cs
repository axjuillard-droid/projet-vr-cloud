using System;
using System.Collections;
using UnityEngine;

public sealed class PhotoV2AiSmoke : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartWhenRequested()
    {
        if (Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(), "--cloudvr-ai-smoke") >= 0)
            new GameObject("Contrôle IA V2").AddComponent<PhotoV2AiSmoke>();
    }

    IEnumerator Start()
    {
        yield return null;
        var route = FindFirstObjectByType<PhotoV2Walkthrough>();
        if (!Check(route != null, "Scène")) yield break;
        var controller = route.player.GetComponent<CharacterController>();
        Vector3 home = controller.transform.position;
        var targets = route.targets;
        var selfRay = new Ray(home + new Vector3(-.6f, .8f, 0), Vector3.right);
        if (!Check(Physics.Raycast(selfRay, out var selfHit, 1.2f) && selfHit.collider == controller
            && !route.TryPickInteraction(selfRay, out _, 1.2f),
            "Capsule du joueur ignorée par la visée")) yield break;

        bool Aim(GameObject target)
        {
            Vector3 delta = target.transform.position - route.view.transform.position;
            route.view.transform.rotation = Quaternion.LookRotation(delta);
            return route.TryAdvance(new Ray(route.view.transform.position, delta));
        }

        IEnumerator Walk(float z)
        {
            for (int move = 0; move < 520 && controller.transform.position.z < z - .02f; move++)
                controller.Move(new Vector3(0, -.05f, Mathf.Min(.08f, z-controller.transform.position.z)));
            yield return null;
        }

        if (!Check(!route.SubmitAiQuestion("inaccessible", 0) && Aim(targets[0])
            && !route.SubmitAiQuestion("  ", 0) && !route.SubmitAiQuestion("test", 3)
            && route.SubmitAiQuestion("Pourquoi une tour de refroidissement a besoin d’eau d’appoint ?", 1)
            && !route.SubmitAiQuestion("deuxième envoi", 1) && !route.AiAnswerVisible, "Saisie et soumission unique")) yield break;
        var capturePath = Environment.GetEnvironmentVariable("CLOUDVR_ROUTING_CAPTURE");
        bool captured = false;
        int legs = 0;
        float travelDeadline = Time.realtimeSinceStartup + 16;
        while (route.IsTravelling && Time.realtimeSinceStartup < travelDeadline)
        {
            legs |= 1 << route.AiRouteLeg;
            if (!captured && !string.IsNullOrEmpty(capturePath) && route.AiRouteLeg == 4
                && route.AiTransportProgress >= .57f)
            {
                ScreenCapture.CaptureScreenshot(capturePath);
                captured = true;
            }
            yield return null;
        }
        if (!Check(legs == 0b111111 && route.AiTransportMapLoaded, "Six étapes et fond géographique")) yield break;
        route.player.enabled = false;
        if (!Check(route.Step == 1 && route.water.AiRequiredLitres == 100 && !route.water.weather.enabled,
            "Site sélectionné et météo désactivée")) yield break;
        yield return Walk(10.8f);
        if (!Check(Aim(targets[1]), "Seuil")) yield break;
        yield return Walk(14.1f);
        if (!Check(Aim(targets[2]), "Badge")) yield break;
        yield return Walk(18.6f);
        if (!Check(Aim(route.patchCord.plug.gameObject) && !Aim(route.patchCord.portB.gameObject)
            && Aim(targets[3]), "Brassage")) yield break;
        yield return WaitFor(() => route.patchCord.SignalDelivered, 18);
        if (!Check(route.patchCord.SignalDelivered, "Signal reçu à la baie")) yield break;
        yield return Walk(25.35f);
        for (int step = 4; step <= 6; step++)
        {
            if (step == 6 && !Check(route.AiCutawayVisible && route.GuideTarget == route.ramControl
                && !Aim(targets[step]) && Aim(route.ramControl.gameObject)
                && !Aim(route.ramControl.gameObject)
                && route.InferencePhase == PhotoV2Walkthrough.AiInferencePhase.Prepared,
                "Question préparée, lancement prématuré et double préparation refusés")) yield break;
            var target = targets[step];
            Vector3 origin = route.view.transform.position;
            var ray = new Ray(origin, target.transform.position - origin);
            string hit = route.TryPickInteraction(ray, out var obstacle) ? obstacle.collider.name : "aucune cible";
            if (!Check(Aim(target), $"Baie, étape {step} · Z={controller.transform.position.z:0.00} · visée {hit}"))
                yield break;
            int previous = step;
            if (step == 6)
            {
                if (!Check(route.InferencePhase == PhotoV2Walkthrough.AiInferencePhase.Context
                    && !Aim(target) && !route.AiAnswerVisible, "Contexte actif, double lancement refusé")) yield break;
                yield return WaitFor(() => route.InferenceTokensShown > 0, 7);
                if (!Check(route.InferencePhase == PhotoV2Walkthrough.AiInferencePhase.Generation
                    && !route.AiAnswerVisible, "Génération progressive, réponse masquée")) yield break;
                var gpuCapture = Environment.GetEnvironmentVariable("CLOUDVR_GPU_CAPTURE");
                if (!string.IsNullOrEmpty(gpuCapture))
                {
                    var previousRotation = route.view.transform.rotation;
                    route.view.transform.rotation = Quaternion.LookRotation(
                        route.AiCutawayPosition - route.view.transform.position);
                    yield return PhotoV2Smoke.Capture(route.view, 6, "gpu", gpuCapture);
                    route.view.transform.rotation = previousRotation;
                }
            }
            yield return WaitFor(() => route.Step > previous, 10);
            if (step == 6 && !Check(route.InferencePhase == PhotoV2Walkthrough.AiInferencePhase.Complete
                && route.InferenceTokensShown == 5, "Les cinq tokens de démonstration ont été produits")) yield break;
            if (!Check(route.Step == (step == 6 ? 9 : step + 1) && !route.AiAnswerVisible,
                "Progression de la baie, réponse masquée")) yield break;
        }
        yield return Walk(57.4f);
        if (!Check(Aim(targets[9]) && !Aim(route.energy.breakers[0].gameObject) && Aim(route.energy.control.gameObject)
            && !route.player.enabled && route.energy.RequestedRows == 2 && !route.ConfirmAiAllocation()
            && !route.ReleaseAiBlock(0), "Le pupitre précède la mise sous tension")) yield break;
        if (!Check(route.ReserveAiBlock(1) && !route.ReserveAiBlock(1) && !route.ConfirmAiAllocation()
            && route.ReserveAiBlock(2) && !route.ReserveAiBlock(0)
            && route.AiReservedBlocks(1) == 2 && route.AiServedBlocks(1) == 1,
            "Réservations limitées par la demande du client et la capacité totale")) yield break;
        route.CloseAiAllocation();
        if (!Check(route.player.enabled && !route.ConfirmAiAllocation() && Aim(route.energy.control.gameObject)
            && route.AiReservedBlocks(2) == 1 && route.ReleaseAiBlock(1) && !route.ReleaseAiBlock(1)
            && route.ReserveAiBlock(0) && route.AiReservedBlocks(0) == 2 && route.AiReservedBlocks(1) == 1,
            "Répartition modifiable avant validation, brouillon conservé")) yield break;
        string allocationCapture = Environment.GetEnvironmentVariable("CLOUDVR_ALLOCATION_CAPTURE");
        if (!string.IsNullOrEmpty(allocationCapture))
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(allocationCapture);
            yield return null;
        }
        if (!Check(route.ConfirmAiAllocation() && !route.ReserveAiBlock(0) && route.player.enabled
            && route.energy.RequestedRows == 4 && route.energy.ActiveRows == 2
            && route.energy.breakers[0].name.EndsWith("05") && route.energy.breakers[1].name.EndsWith("06"),
            "Répartition validée sur les rangées 05 et 06")) yield break;
        route.player.enabled = false;
        yield return WaitFor(() => !route.energy.Changing, 5);
        if (!Check(Aim(route.energy.breakers[1].gameObject), "Rangée 06 activée en premier")) yield break;
        yield return WaitFor(() => !route.energy.Changing, 5);
        if (!Check(route.AiLedgerFeedback?.Contains("Assistant IA") == true && route.AiLedgerKwhChange == 15,
            "Le compteur crédite le client alimenté et ajoute 15 kWh")) yield break;
        if (!Check(route.AiServedBlocks(0) == 2 && route.AiServedBlocks(2) == 0
            && Aim(route.energy.breakers[0].gameObject), "Seule la rangée alimentée sert son client")) yield break;
        yield return WaitFor(() => !route.energy.Changing, 5);
        if (!Check(route.AiServedBlocks(0) == 2 && route.AiServedBlocks(1) == 1 && route.AiServedBlocks(2) == 1
            && route.energy.PowerKw == 78, "Quatre blocs servis après les deux départs électriques")) yield break;
        if (!Check(route.energy.ActiveRows == 4 && !Aim(route.energy.control.gameObject), "Limite de la mission")) yield break;
        yield return Walk(58.8f);
        if (!Check(Aim(route.grid.departure.gameObject), "Sortie")) yield break;
        yield return Walk(68.6f);
        if (!Check(!route.ConfirmAiPower() && Aim(route.grid.substation.gameObject) && route.grid.Stop == 1
            && !route.player.enabled && !route.ConfirmAiPower() && !route.PreviewAiPower(-1),
            "Poste électrique accessible, validation refusée sans choix")) yield break;
        if (!Check(route.AiAdditionalKw == 30 && Mathf.Approximately(route.AiPowerCarbonKg(0), .18f)
            && Mathf.Approximately(route.AiPowerCarbonKg(1), 27f)
            && route.PreviewAiPower(1) && route.AiConfirmedPowerChoice == -1,
            "Deux scénarios à 30 kWh, 0,18 et 27 kg CO₂e")) yield break;
        route.CloseAiPower();
        if (!Check(route.player.enabled && route.grid.Stop == 1 && Aim(route.grid.substation.gameObject)
            && route.PreviewAiPower(0) && route.ConfirmAiPower() && route.grid.Stop == 2
            && route.AiConfirmedPowerChoice == 0 && !route.ConfirmAiPower(),
            "Choix révisable, validation unique et reprise de la visite")) yield break;
        route.player.enabled = false;
        yield return Walk(83.5f);
        if (!Check(Aim(route.grid.viewpoint.gameObject), "Point de vue sur la centrale")) yield break;
        yield return WaitFor(() => route.cooling.Temperature >= 28.9f, 6);
        if (!Check(route.cooling.AiThermometerVisible && !route.cooling.Open
            && route.cooling.Temperature >= 28.9f && route.cooling.AiThermalPhase.Contains("Alerte")
            && route.cooling.status.text.Contains("°C"), "La nouvelle charge déclenche l’alerte thermique dans le décor")) yield break;
        string thermalCapture = Environment.GetEnvironmentVariable("CLOUDVR_THERMAL_CAPTURE");
        if (!string.IsNullOrEmpty(thermalCapture))
        {
            route.view.transform.rotation = Quaternion.LookRotation(route.cooling.status.transform.parent.position - route.view.transform.position);
            yield return PhotoV2Smoke.Capture(route.view, 1, "alerte", thermalCapture);
        }
        if (!Check(Aim(route.cooling.valve.gameObject), "Intervention sur la vanne")) yield break;
        yield return WaitFor(() => route.cooling.Open, 5);
        yield return null;
        if (!Check(route.cooling.Open && !route.cooling.ReturnComplete && route.cooling.Temperature >= 28.9f
            && route.cooling.AiThermalPhase.Contains("attente"), "Le thermomètre attend le retour refroidi après l’ouverture")) yield break;
        if (!Check(route.AiLedgerFeedback?.Contains("Refroidissement") == true && route.AiLedgerKwhChange == 15,
            "Le compteur ajoute le refroidissement du site")) yield break;
        yield return WaitFor(() => route.cooling.Temperature < 28.5f, 10);
        if (!Check(route.cooling.ReturnComplete && route.cooling.Temperature < 28.5f
            && route.cooling.AiThermalPhase.Contains("baisse"), "La température baisse au retour du liquide refroidi")) yield break;
        if (!string.IsNullOrEmpty(thermalCapture))
        {
            yield return WaitFor(() => route.cooling.Stabilized, 7);
            route.view.transform.rotation = Quaternion.LookRotation(route.cooling.status.transform.parent.position - route.view.transform.position);
            yield return PhotoV2Smoke.Capture(route.view, 2, "stabilisee", thermalCapture);
        }
        yield return Walk(96.1f);
        yield return WaitFor(() => route.cooling.Stabilized, 16);
        if (!Check(route.cooling.Stabilized && route.cooling.Temperature <= 24f
            && route.cooling.AiThermalPeak >= 28.9f && route.cooling.AiThermalPhase.Contains("stabilisée"),
            "Température stabilisée avec le pic conservé")) yield break;
        if (!Check(route.cooling.AuxiliaryKw == 15 && Aim(route.cooling.observation.gameObject)
            && Aim(route.water.entry.gameObject), "Refroidissement du site et rivière")) yield break;
        yield return Walk(108.5f);
        if (!Check(!Aim(route.water.load.gameObject), "Retour refusé sans appoint")) yield break;
        for (int step = 0; step < 5; step++)
        {
            bool raised = Aim(route.water.increase.gameObject);
            yield return null;
            if (!Check(raised && route.AiLedgerFeedback?.Contains("+25") == true
                && Mathf.Approximately(route.AiLedgerKwhChange, .1f), "Augmentation de l’appoint et du compteur")) yield break;
        }
        if (!Check(route.water.Withdrawn == 125 && !Aim(route.water.load.gameObject)
            && Aim(route.water.decrease.gameObject), "Excès refusé, correction possible")) yield break;
        yield return WaitFor(() => Mathf.Approximately(route.AiLedgerKwhChange, -.1f), 2);
        if (!Check(route.AiLedgerFeedback?.Contains("-25") == true && Mathf.Approximately(route.AiLedgerKwhChange, -.1f),
            "Le compteur déduit un palier d’appoint à la baisse")) yield break;
        yield return WaitFor(() => route.water.MissionCompleted, 6);
        bool requireReply = Array.IndexOf(Environment.GetCommandLineArgs(), "--cloudvr-ai-offline") < 0;
        yield return WaitFor(() => !route.AiRequestPending, 90);
        if (!Check(route.water.Withdrawn == 100 && !route.AiAnswerVisible && route.AiHasReply == requireReply
            && (!requireReply || route.AiReplyStatus == "ok") && Aim(route.water.load.gameObject),
            "Appoint équilibré, réponse toujours masquée")) yield break;
        yield return WaitFor(() => route.AiReturned, 6);
        if (!Check(route.AiReturned && route.AiAnswerVisible == requireReply && route.AiTerminalTab == 0
            && route.AiServedBlocks(2) == 1
            && Vector3.Distance(home, controller.transform.position) < .1f, "Réponse décodée au PC de départ")) yield break;
        var france = route.AiCompareSite(0);
        var usa = route.AiCompareSite(1);
        var china = route.AiCompareSite(2);
        if (!Check(france.DelayMs == 845 && usa.DelayMs == 925 && china.DelayMs == 1010,
            "Délai total des trois sites, calcul et file identiques")) yield break;
        if (!Check(france.WaterLitres == 75 && usa.WaterLitres == 100 && china.WaterLitres == 125
            && Mathf.Approximately(france.Kwh, 87.3f) && Mathf.Approximately(usa.Kwh, 93.4f)
            && Mathf.Approximately(china.Kwh, 96.5f), "Même charge informatique, refroidissement et appoint propres au site")) yield break;
        if (!Check(Mathf.Approximately(france.Cost, 8.73f) && Mathf.Approximately(usa.Cost, 7.472f)
            && Mathf.Approximately(china.Cost, 8.685f) && Mathf.Approximately(france.CarbonKg, 4.365f)
            && Mathf.Approximately(usa.CarbonKg, 35.492f) && Mathf.Approximately(china.CarbonKg, 51.145f),
            "Coût et CO₂e calculés avec les trois profils, scénario du poste indépendant")) yield break;
        route.AiTerminalTab = 1;
        yield return new WaitForEndOfFrame();
        var comparisonCapture = Environment.GetEnvironmentVariable("CLOUDVR_COMPARISON_CAPTURE");
        if (!string.IsNullOrEmpty(comparisonCapture))
        {
            ScreenCapture.CaptureScreenshot(comparisonCapture);
            yield return null;
        }
        route.AiTerminalTab = 0;
        yield return new WaitForEndOfFrame();
        if (!Check(route.AiAnswerVisible == requireReply && route.AiTerminalTab == 0,
            "Réponse encore disponible après affichage du comparatif")) yield break;
        Debug.Log($"CLOUDVR_AI_SMOKE_PASS réponse réelle : {requireReply}");
        Application.Quit(0);
    }

    static bool Check(bool ok, string description)
    {
        if (ok)
        {
            Debug.Log("CLOUDVR_AI_CHECK " + description);
            return true;
        }
        Debug.LogError("CLOUDVR_AI_FAIL " + description);
        Application.Quit(1);
        return false;
    }

    static IEnumerator WaitFor(Func<bool> condition, float seconds)
    {
        float deadline = Time.realtimeSinceStartup + seconds;
        while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
    }
}
