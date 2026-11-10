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
        var waypoints = new[] { 1f, 10.8f, 14.1f, 18.6f, 23.5f, 23.5f, 23.5f, 35.2f, 36.9f, 57.4f };
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
                if (AimAt(target) || !AimAt(patch.plug.gameObject) || !patch.Held || route.Step != 3
                    || AimAt(patch.portB.gameObject) || !patch.Held || route.Step != 3 || !AimAt(target))
                {
                    Fail("Le cordon doit être pris avant A et B doit être refusé sans perdre le cordon.");
                    yield break;
                }
                if (!patch.Connected || route.Step != 4 || !patch.signalTrace.enabled || route.PhotoStatus != "En transit")
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
                if (!patch.SignalDelivered || route.PhotoStatus != "Reçue"
                    || Vector3.Distance(patch.signal.position, end) > .015f)
                {
                    Fail("Le signal ne rejoint pas la baie école.");
                    yield break;
                }
                Debug.Log("CLOUDVR_V2_PATCH_PASS prise, port B refusé, port A connecté, signal arrivé");
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
                        && Vector3.Distance(insertedDrawer, school.serverBlade.localPosition) > .62f,
                    _ => route.PreviewReady && route.previewDisplay.enabled
                        && route.previewDisplay.texture.width == 24 && route.previewDisplay.texture.height == 16
                        && route.PhotoStatus == "Reçue"
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
