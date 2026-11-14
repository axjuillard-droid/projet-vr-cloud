using System;
using System.Collections;
using System.IO;
using UnityEngine;

public sealed class PhotoV2Smoke : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartWhenRequested()
    {
        if (Debug.isDebugBuild && Array.Exists(Environment.GetCommandLineArgs(), arg => arg == "--cloudvr-v2-smoke"))
            new GameObject("Contrôle Photo V2").AddComponent<PhotoV2Smoke>();
    }

    IEnumerator Start()
    {
        yield return null;
        var route = FindFirstObjectByType<PhotoV2Walkthrough>();
        if (route == null)
        {
            Fail("Parcours V2 introuvable.");
            yield break;
        }

        bool capture = Array.Exists(Environment.GetCommandLineArgs(), arg => arg == "--cloudvr-v2-capture");
        route.player.enabled = false;
        var controller = route.player.GetComponent<CharacterController>();
        var waypoints = new[] { 1f, 10.8f, 14.1f, 18.6f, 25.35f, 25.35f, 25.35f, 35.5f, 36.9f, 57.4f };
        bool AimAt(GameObject target) => route.TryAdvance(new Ray(route.view.transform.position,
            target.transform.position - route.view.transform.position));
        if (AimAt(route.targets[1]))
        {
            Fail("Une cible distante a avancé le parcours.");
            yield break;
        }

        var school = route.schoolRack;
        var closedDoor = school.rackDoor.localRotation;
        var insertedDrawer = school.serverBlade.localPosition;
        for (int i = 0; i < waypoints.Length; i++)
        {
            // Avance réelle par le CharacterController, sans téléportation.
            float z = waypoints[i];
            for (int move = 0; move < 400 && controller.transform.position.z < z - .02f; move++)
                controller.Move(new Vector3(0, -.05f, Mathf.Min(.08f, z - controller.transform.position.z)));
            yield return null;
            var target = route.targets[i];
            route.view.transform.rotation = Quaternion.LookRotation(target.transform.position - route.view.transform.position);
            if (capture && (i == 0 || i == 1 || i == 2 || i == 3))
                yield return Capture(route.view, i, "poste");
            if (Mathf.Abs(controller.transform.position.z - z) > .03f)
            {
                Fail($"Poste {i + 1} inaccessible depuis {controller.transform.position}.");
                yield break;
            }
            if (i == 3)
            {
                var patch = route.patchCord;
                if (!patch.ingress || !patch.ingress.useWorldSpace || patch.ingress.positionCount < 2
                    || patch.ingress.GetPosition(0).z >= 12.5f
                    || Vector3.Distance(patch.ingress.GetPosition(patch.ingress.positionCount - 1), patch.anchor.position) > .015f)
                {
                    Fail("La fibre doit arriver de l’extérieur et rejoindre l’ancrage du cordon.");
                    yield break;
                }
                if (route.GuideTarget != patch.plug || AimAt(target) || !AimAt(patch.plug.gameObject)
                    || route.GuideTarget != patch.portA || !patch.Held || route.Step != 3
                    || AimAt(patch.portB.gameObject) || !patch.Held || route.Step != 3 || !AimAt(target))
                {
                    Fail("Le cordon doit être pris avant A et B doit être refusé sans perdre le cordon.");
                    yield break;
                }
                if (!patch.Connected || route.Step != 4 || patch.CordSignalTraversed
                    || patch.signalTrace.enabled || patch.SignalDelivered || route.PhotoStatus != "En transit")
                {
                    Fail("La connexion n’a pas déclenché le signal.");
                    yield break;
                }
                if (capture)
                {
                    yield return new WaitForSecondsRealtime(.8f);
                    yield return Capture(route.view, i, "cordon");
                }
                float deadline = Time.realtimeSinceStartup + 4;
                while (!patch.SignalDelivered && Time.realtimeSinceStartup < deadline) yield return null;
                var end = patch.signalTrace.GetPosition(patch.signalTrace.positionCount - 1);
                if (!patch.CordSignalTraversed || !patch.signalTrace.enabled || !patch.SignalDelivered
                    || route.PhotoStatus != "Reçue"
                    || Vector3.Distance(patch.signal.position, end) > .015f
                    || Vector3.Distance(end, route.computeInput.position) > .015f)
                {
                    Fail("Le signal ne rejoint pas la baie école.");
                    yield break;
                }
                Debug.Log("CLOUDVR_V2_FIBER_PASS arrivée extérieure, traversée du cordon avant calcul");
                Debug.Log("CLOUDVR_V2_PATCH_PASS prise, port B refusé, port A connecté, signal arrivé");
            }
            else if (i == 7)
            {
                var storage = route.storage;
                float deadline = Time.realtimeSinceStartup + 7;
                while (!storage.ReadyToRoute && Time.realtimeSinceStartup < deadline) yield return null;
                var arrival = storage.delivery.GetPosition(storage.delivery.positionCount - 1);
                if (!storage.ReadyToRoute || !storage.packet.enabled
                    || Vector3.Distance(arrival, storage.packetCarrier.position) > .015f
                    || Vector3.Distance(arrival, storage.branch.GetPosition(0)) > .015f
                    || Vector3.Distance(storage.delivery.GetPosition(0), route.computeOutput.position) > .015f
                    || AimAt(route.targets[8]) || AimAt(storage.busyBank.gameObject)
                    || !storage.explanation.text.Contains("OCCUPÉE") || storage.Saved || route.Step != 7)
                {
                    Fail("La photo doit arriver au répartiteur et la banque occupée doit refuser l’écriture.");
                    yield break;
                }
                if (capture)
                {
                    route.view.transform.rotation = Quaternion.LookRotation(
                        arrival - route.view.transform.position);
                    yield return Capture(route.view, i, "repartiteur");
                }
                if (!AimAt(target) || route.Step != 7 || storage.Saved || route.PhotoStatus != "Traitée"
                    || AimAt(target) || AimAt(route.targets[8]))
                {
                    Fail("La banque libre doit démarrer une seule écriture et bloquer la relecture.");
                    yield break;
                }
                deadline = Time.realtimeSinceStartup + 4;
                while (route.Step == 7 && Time.realtimeSinceStartup < deadline) yield return null;
                var slot = storage.branch.GetPosition(storage.branch.positionCount - 1);
                if (route.Step != 8 || !storage.Saved || storage.Readable || storage.packet.enabled
                    || !storage.branch.enabled || Vector3.Distance(slot, storage.packetCarrier.position) > .015f
                    || route.PhotoStatus != "Enregistrée")
                {
                    Fail("La photo doit entrer dans la banque libre avant d’être enregistrée.");
                    yield break;
                }
                if (capture)
                {
                    route.view.transform.rotation = Quaternion.LookRotation(
                        target.transform.position - route.view.transform.position);
                    yield return Capture(route.view, i, "stockage");
                }
            }
            else if (i == 6)
            {
                if (route.GuideTarget != route.ramControl || AimAt(target)
                    || !AimAt(route.ramControl.gameObject) || AimAt(route.ramControl.gameObject)
                    || route.GuideTarget.gameObject != target || route.PreviewReady || !AimAt(target))
                {
                    Fail("Le CPU doit attendre le chargement de la RAM.");
                    yield break;
                }
            }
            else if (!AimAt(target))
            {
                Fail($"Cible {i + 1} inaccessible depuis {controller.transform.position}.");
                yield break;
            }
            if (i == 0)
            {
                if (!route.IsTravelling || route.Step != 0 || AimAt(target))
                {
                    Fail("La transition doit bloquer un second envoi.");
                    yield break;
                }
                if (capture)
                {
                    yield return new WaitForSecondsRealtime(2.2f);
                    yield return Capture(route.view, i, "fibre");
                }
                float deadline = Time.realtimeSinceStartup + 7;
                while (route.IsTravelling && Time.realtimeSinceStartup < deadline) yield return null;
                if (route.IsTravelling || route.Step != 1 || route.film.gameObject.activeSelf
                    || Vector3.Distance(controller.transform.position, route.arrival) > .1f)
                {
                    Fail("La transition n’a pas rejoint la cour.");
                    yield break;
                }
                if (capture)
                {
                    route.view.transform.rotation = Quaternion.LookRotation(
                        new Vector3(1.72f, 1.30f, 12.32f) - route.view.transform.position);
                    yield return Capture(route.view, i, "arrivee-fibre");
                }
            }
            if (i >= 4 && i <= 6)
            {
                if (route.Step != i || AimAt(target))
                {
                    Fail("Une seconde action ne doit pas avancer pendant le geste.");
                    yield break;
                }
                float deadline = Time.realtimeSinceStartup + 3;
                while (route.Step == i && Time.realtimeSinceStartup < deadline) yield return null;
                bool physicalResult = i switch
                {
                    4 => school.CurrentState == ServerBladeInteraction.State.DoorOpened
                        && Quaternion.Angle(closedDoor, school.rackDoor.localRotation) > 85,
                    5 => school.CurrentState == ServerBladeInteraction.State.ServerExtracted
                        && school.hardwareLabels.activeSelf
                        && Vector3.Dot(school.transform.TransformDirection(Vector3.back),
                            route.view.transform.position - school.transform.position) > 1f
                        && Vector3.Distance(insertedDrawer, school.serverBlade.localPosition) > .62f,
                    _ => route.PreviewReady && route.previewDisplay.enabled
                        && route.ramLabel.text == "RAM LIBÉRÉE"
                        && route.previewDisplay.texture.width == 24 && route.previewDisplay.texture.height == 16
                        && route.PhotoStatus == "Traitée"
                        && route.storage.packet.enabled
                        && route.storage.delivery.sharedMaterial == route.storage.deliverySignal
                        && Vector3.Distance(route.storage.serverLink.GetPosition(1), route.computeOutput.position) < .015f
                };
                if (route.Step != i + 1 || !physicalResult)
                {
                    Fail("Le geste de la baie n’a pas produit le résultat attendu à l’étape " + (i + 1));
                    yield break;
                }
                if (capture && i == 5)
                {
                    var tray = school.serverBlade.TransformPoint(new Vector3(0,.07f,.41f));
                    route.view.transform.rotation = Quaternion.LookRotation(tray - route.view.transform.position);
                    yield return Capture(route.view, i, "tiroir");
                }
                if (capture && i == 6) yield return Capture(route.view, i, "apercu");
                if (i == 6) Debug.Log("CLOUDVR_V2_COMPUTE_PASS porte, tiroir, aperçu, double action refusée");
            }
            if (i == 8)
            {
                if (!route.SamePhotoRead || route.Step != 9 || route.PhotoStatus != "Lisible")
                {
                    Fail("L’écran client doit afficher la même photo.");
                    yield break;
                }
                if (capture) yield return Capture(route.view, i, "relecture");
                Debug.Log("CLOUDVR_V2_STORAGE_PASS sortie serveur, banque occupée refusée, écriture et relecture de la même photo");
            }
            if (i == 9 && capture)
            {
                var rack = route.schoolRack.transform.position + Vector3.up * 2;
                route.view.transform.rotation = Quaternion.LookRotation(rack - route.view.transform.position);
                yield return Capture(route.view, i, "balcon");
            }
            if (i == 2 && (!route.sasEntrance.activeSelf || route.barriers[2].activeSelf || !route.badgeApproved.activeSelf))
            {
                Fail("Les portes du sas restent dans un état inattendu.");
                yield break;
            }
        }

        if (controller.transform.position.y < 3.3f)
        {
            Fail("Le balcon n’a pas été rejoint en hauteur.");
            yield break;
        }

        var energy = route.energy;
        if (!energy.isActiveAndEnabled || energy.ActiveRows != 2 || energy.PowerKw != 48)
        {
            Fail("Le pupitre doit démarrer avec deux rangées actives.");
            yield break;
        }
        var grid = route.grid;
        var departureOrigin = new Vector3(0,5.05f,58.7f);
        if (!grid.exitGate.activeSelf || grid.Stop != 0 ||
            route.TryAdvance(new Ray(departureOrigin,grid.departure.transform.position - departureOrigin)))
        {
            Fail("La sortie réseau doit attendre une hausse de demande.");
            yield break;
        }
        var initialTemperature = route.cooling.Temperature;
        for (int level = 0; level < 2; level++)
        {
            int previous = 2 + level*2;
            int requested = previous + 2;
            if (AimAt(energy.breakers[level*2].gameObject)
                || !AimAt(energy.control.gameObject) || !energy.Changing || AimAt(energy.control.gameObject))
            {
                Fail("Un départ sans demande doit être refusé. Un seul changement est permis à la fois.");
                yield break;
            }
            float deadline = Time.realtimeSinceStartup + 4;
            while (energy.Changing && Time.realtimeSinceStartup < deadline) yield return null;
            if (energy.Changing || energy.RequestedRows != requested || energy.ActiveRows != previous
                || energy.PowerKw != 18 + previous*15 || route.GuideTarget != energy.breakers[level*2] || AimAt(energy.control.gameObject)
                || route.TryAdvance(new Ray(departureOrigin,grid.departure.transform.position - departureOrigin)))
            {
                Fail("La demande seule ne doit ni alimenter les rangées ni ouvrir la sortie.");
                yield break;
            }
            var cabinet = energy.breakers[level*2].transform.position;
            route.view.transform.rotation = Quaternion.LookRotation(cabinet + Vector3.right*.2f - route.view.transform.position);
            if (capture && level == 0) yield return Capture(route.view,10,"demande");
            // Actionner les deux départs dans l’ordre inverse pour vérifier leur correspondance avec les rangées.
            for (int i = 1; i >= 0; i--)
            {
                int breaker = level*2 + i;
                float raised = energy.breakers[breaker].transform.position.y;
                if (!AimAt(energy.breakers[breaker].gameObject) || AimAt(energy.breakers[breaker].gameObject))
                {
                    Fail("Le levier demandé doit répondre une seule fois.");
                    yield break;
                }
                deadline = Time.realtimeSinceStartup + 4;
                while (energy.Changing && Time.realtimeSinceStartup < deadline) yield return null;
                if (energy.Changing || !energy.rowLights[breaker+2].activeSelf
                    || energy.feeders[breaker].sharedMaterial != energy.live
                    || energy.breakers[breaker].transform.position.y >= raised - .10f
                    || energy.ActiveRows != previous + (2-i)
                    || (i == 1 && energy.rowLights[breaker+1].activeSelf))
                {
                    Fail("Le levier doit s’abaisser et alimenter uniquement son départ et sa rangée.");
                    yield break;
                }
            }
            if (!energy.ReadyForGrid || route.GuideTarget != grid.departure || energy.PowerKw != 18 + requested*15 || energy.incoming.sharedMaterial != energy.live)
            {
                Fail("La distribution et la puissance doivent correspondre à la charge alimentée.");
                yield break;
            }
            if (capture && level == 0) yield return Capture(route.view,11,"tableau-actif");
        }
        Debug.Log("CLOUDVR_V2_ENERGY_PASS demande sans effet sur les voyants, quatre départs indépendants, puissance vérifiée");

        var stops = new[] {grid.departure,grid.substation,grid.viewpoint};
        var stopPositions = new[] {58.8f,68.6f,83.5f};
        for (int i = 0; i < stops.Length; i++)
        {
            float z = stopPositions[i];
            for (int move = 0; move < 260 && controller.transform.position.z < z - .02f; move++)
                controller.Move(new Vector3(0,-.05f,Mathf.Min(.08f,z - controller.transform.position.z)));
            yield return null;
            if (i == 2 && AimAt(route.cooling.valve.gameObject))
            {
                Fail("La vanne doit attendre le point de vue.");
                yield break;
            }
            if (Mathf.Abs(controller.transform.position.z - z) > .03f || !AimAt(stops[i].gameObject)
                || grid.Stop != i + 1 || grid.exitGate.activeSelf || !grid.routeLine.enabled)
            {
                Fail("Arrêt du parcours extérieur inaccessible : " + (i + 1));
                yield break;
            }
            if (capture && i == 1)
            {
                route.view.transform.rotation = Quaternion.LookRotation(new Vector3(6.5f,2.25f,73) - route.view.transform.position);
                yield return Capture(route.view,12,"poste");
            }
            if (capture && i == 2)
            {
                route.view.transform.rotation = Quaternion.LookRotation(new Vector3(-11,13,126) - route.view.transform.position);
                yield return Capture(route.view,13,"centrale");
            }
        }
        if (Array.Exists(grid.plumes, plume => !plume.isPlaying || plume.particleCount == 0))
        {
            Fail("Les panaches doivent être présents au point de vue.");
            yield break;
        }
        Debug.Log("CLOUDVR_V2_GRID_PASS sortie, poste, point de vue et panaches vérifiés");
        var cooling = route.cooling;
        var wheelClosed = cooling.wheel.localRotation;
        float heatingDeadline = Time.realtimeSinceStartup + 7;
        while (cooling.Temperature < initialTemperature + 7f && Time.realtimeSinceStartup < heatingDeadline) yield return null;
        float hotTemperature = cooling.Temperature;
        if (hotTemperature < initialTemperature + 7f || cooling.AuxiliaryKw != 0
            || !cooling.gate.activeSelf || !AimAt(cooling.valve.gameObject) || AimAt(cooling.valve.gameObject))
        {
            Fail("Ouverture de vanne absente ou répétée.");
            yield break;
        }
        float valveDeadline = Time.realtimeSinceStartup + 3;
        while (!cooling.Open && Time.realtimeSinceStartup < valveDeadline) yield return null;
        if (!cooling.Open || cooling.gate.activeSelf || !cooling.hotPath.enabled || !cooling.coldPath.enabled
            || energy.PowerKw != 114 || cooling.AuxiliaryKw != 6
            || Quaternion.Angle(wheelClosed,cooling.wheel.localRotation) < 110)
        {
            Fail("La vanne et les conduites ne reflètent pas l’ouverture.");
            yield break;
        }
        for (int move=0; move<260 && controller.transform.position.z<96.1f; move++)
            controller.Move(new Vector3(0,-.05f,Mathf.Min(.08f,96.1f-controller.transform.position.z)));
        yield return null;
        if (Mathf.Abs(controller.transform.position.z-96.1f) > .04f || AimAt(cooling.observation.gameObject)
            || AimAt(route.water.entry.gameObject))
        {
            Fail("L’échangeur doit être accessible et attendre le premier retour.");
            yield break;
        }
        bool sawHeatBeforeReturn=false;
        float loopDeadline=Time.realtimeSinceStartup+13;
        while (!cooling.Stabilized && Time.realtimeSinceStartup<loopDeadline)
        {
            sawHeatBeforeReturn |= cooling.HeatReached && !cooling.ReturnComplete;
            yield return null;
        }
        if (!sawHeatBeforeReturn || !cooling.Stabilized || cooling.Temperature >= hotTemperature - 5f
            || energy.ActiveRows != 6 || !AimAt(cooling.observation.gameObject)
            || !cooling.Observed || Array.Exists(cooling.airMarkers, air => !air.gameObject.activeSelf))
        {
            Fail("Le liquide doit revenir après le transfert de chaleur.");
            yield break;
        }
        if (capture)
        {
            route.view.transform.rotation=Quaternion.LookRotation(new Vector3(-6.4f,3.1f,96.7f)-route.view.transform.position);
            yield return Capture(route.view,14,"echangeur");
            route.view.transform.rotation=Quaternion.LookRotation(new Vector3(-4.3f,4.4f,83)-route.view.transform.position);
            yield return Capture(route.view,15,"conduites");
        }
        Debug.Log("CLOUDVR_V2_COOLING_PASS hausse à six rangées, renfort +6 kW, retour froid et baisse à charge constante");
        var water = route.water;
        if (route.GuideTarget != water.entry || !water.gate.activeSelf || !AimAt(water.entry.gameObject) || !water.Active
            || route.GuideTarget != water.increase
            || water.gate.activeSelf || water.workingFlows.activeSelf || water.Withdrawn != 0)
        {
            Fail("La pompe doit être arrêtée à l’arrivée.");
            yield break;
        }
        for (int move=0; move<250 && controller.transform.position.z<108.28f; move++)
            controller.Move(new Vector3(0,-.05f,Mathf.Min(.08f,108.28f-controller.transform.position.z)));
        yield return null;
        if (Mathf.Abs(controller.transform.position.z-108.28f) > .04f || !AimAt(water.load.gameObject))
        {
            Fail("Le pupitre rivière doit permettre de réduire la charge.");
            yield break;
        }
        float chargeDeadline = Time.realtimeSinceStartup + 4;
        while (energy.Changing && Time.realtimeSinceStartup < chargeDeadline) yield return null;
        if (energy.ActiveRows != 2 || !AimAt(water.load.gameObject))
        {
            Fail("Le pupitre rivière doit permettre de relancer la charge.");
            yield break;
        }
        chargeDeadline = Time.realtimeSinceStartup + 4;
        while (energy.Changing && Time.realtimeSinceStartup < chargeDeadline) yield return null;
        if (energy.ActiveRows != 4 || !AimAt(water.increase.gameObject)
            || water.PumpLevel != 1 || water.Withdrawn != 4 || !water.workingFlows.activeSelf
            || !AimAt(water.increase.gameObject) || AimAt(water.increase.gameObject)
            || water.PumpLevel != 2 || water.Withdrawn != 12 || water.RiverFlow != 100
            || water.Evaporated + water.Returned != water.Withdrawn)
        {
            Fail("La pompe doit passer par deux régimes et conserver le bilan d’eau.");
            yield break;
        }
        yield return new WaitForSecondsRealtime(.7f);
        var riverView = Quaternion.LookRotation(new Vector3(-12.9f,.6f,111.8f)-route.view.transform.position);
        route.view.transform.rotation = riverView;
        if (capture) yield return Capture(route.view,16,"debit-normal");
        if (!AimAt(water.weather.gameObject) || !water.Dry || water.RiverFlow != 30 || water.Withdrawn != 12)
        {
            Fail("L’été sec doit conserver le réglage initial de la pompe.");
            yield break;
        }
        yield return new WaitForSecondsRealtime(2.5f);
        float narrow = water.surface.localScale.x;
        if (!water.Stressed || route.GuideTarget != water.decrease || water.DeadFish != 0 || narrow >= .65f || !AimAt(water.decrease.gameObject))
        {
            Fail("Le débit fort doit réduire la largeur puis stresser les poissons avant la mortalité.");
            yield break;
        }
        yield return new WaitForSecondsRealtime(2.5f);
        if (water.Stressed || water.DeadFish != 0 || water.Withdrawn != 4 || water.surface.localScale.x <= narrow
            || !AimAt(water.increase.gameObject))
        {
            Fail("Réduire la pompe doit permettre une récupération sans mortalité.");
            yield break;
        }
        yield return new WaitForSecondsRealtime(8.5f);
        if (water.DeadFish != 3 || Quaternion.Angle(water.fish[0].localRotation,Quaternion.Euler(0,0,90)) > 1f)
        {
            Fail("Le prélèvement excessif prolongé doit produire une mortalité visible.");
            yield break;
        }
        if (capture) yield return Capture(route.view,17,"ete-sec");
        if (!AimAt(water.decrease.gameObject) || !AimAt(water.decrease.gameObject)
            || water.Withdrawn != 0 || water.workingFlows.activeSelf)
        {
            Fail("L’arrêt de la pompe doit couper les flux.");
            yield break;
        }
        yield return new WaitForSecondsRealtime(6);
        if (water.Stressed || water.DeadFish != 3 || water.surface.localScale.x <= narrow)
        {
            Fail("L’arrêt doit rendre du débit sans effacer les pertes du scénario.");
            yield break;
        }
        if (capture) yield return Capture(route.view,18,"pompe-arretee");
        Debug.Log("CLOUDVR_V2_WATER_PASS pompe, stress, récupération, mortalité persistante et bilan vérifiés");
        Debug.Log("CLOUDVR_V2_GUIDANCE_PASS cordon, tableau électrique et pompe");
        Debug.Log("CLOUDVR_V2_SMOKE_PASS 10/10 actions en ordre, marche et montée, sas, cible distante refusée");
        Application.Quit(0);
    }

    static IEnumerator Capture(Camera camera, int step, string view)
    {
        yield return new WaitForEndOfFrame();
        var previous = camera.targetTexture;
        var active = RenderTexture.active;
        var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 4);
        var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            texture.Apply();
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "V2Review");
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, $"{step:00}-{view}.png"), texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previous;
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(target);
            Destroy(texture);
        }
    }

    static void Fail(string reason)
    {
        Debug.LogError("CLOUDVR_V2_SMOKE_FAIL " + reason);
        Application.Quit(1);
    }
}
