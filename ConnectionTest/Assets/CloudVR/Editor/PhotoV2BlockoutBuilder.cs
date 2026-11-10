#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class PhotoV2BlockoutBuilder
{
    const string Folder = "Assets/Scenes/CloudVRV2";
    const string ScenePath = Folder + "/PhotoBlockout.unity";
    const string Output = "Builds/Windows-V2/CloudVR-V2.exe";
    static readonly Color Neutral = new(.44f, .51f, .56f);
    static readonly Color Warm = new(.69f, .49f, .36f);
    static readonly Color Dark = new(.14f, .21f, .29f);
    static readonly Color Signal = new(.14f, .82f, .97f);
    static Material _neutral, _warm, _dark, _signal;

    [MenuItem("CloudVR/V2/Générer le parcours Photo")]
    public static void Generate()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Scenes", "CloudVRV2");
        _neutral = Palette("Béton", Neutral);
        _warm = Palette("Chambre", Warm);
        _dark = Palette("Baies", Dark);
        _signal = Palette("Repère", Signal);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.49f, .56f, .64f);
        var light = new GameObject("Éclairage de travail").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(47, -28, 0);

        Box("Sol chambre", new(0, -.15f, 2.5f), new(8, .3f, 5), Palette("V2 Parquet", new(.22f,.13f,.08f)));
        Box("Sol cour et bâtiment", new(0, -.15f, 23.5f), new(20, .3f, 37), _neutral);
        WallPair("Chambre", 4, 0, 5, 3.1f, _warm);
        Box("Mur derrière le joueur", new(0, 1.55f, 0), new(8, 3.1f, .3f), _warm);
        DressOpening();
        Box("Façade gauche", new(-5.8f, 4, 13), new(8, 8, .55f), _neutral);
        Box("Façade droite", new(5.8f, 4, 13), new(8, 8, .55f), _neutral);
        Box("Linteau façade", new(0, 5.5f, 13), new(3.6f, 5, .55f), _neutral);
        WallPair("Sas", 1.75f, 13, 17, 3, _neutral);
        WallPair("Bord de cour", 9.85f, 5, 13, 1.1f, _neutral);
        WallPair("Couloir vers le balcon", 1.5f, 40, 41.15f, 1.1f, _dark);
        Box("Limite gauche de la salle", new(-5.7f, .55f, 41.3f), new(8.2f, 1.1f, .2f), _dark);
        Box("Limite droite de la salle", new(5.7f, .55f, 41.3f), new(8.2f, 1.1f, .2f), _dark);
        WallPair("Réseau", 4, 17, 22, 4, _neutral);
        WallPair("Salle des baies", 9.8f, 22, 40, 6, _neutral);
        Box("Plafond de salle", new(0, 6, 31), new(20, .25f, 18), _neutral);
        DressRackRows();

        var rowLights = new GameObject[6];
        for (int row=0; row<rowLights.Length; row++) rowLights[row] = CreateRowLights(row);

        // La rampe remplace provisoirement l'escalier ou l'ascenseur du balcon.
        Vector3 foot = new(0, 0, 41), landing = new(0, 3.4f, 55);
        var ramp = Box("Rampe provisoire", (foot + landing) * .5f - Vector3.up * .15f,
            new(3, .3f, Vector3.Distance(foot, landing) + .25f), _neutral);
        ramp.transform.rotation = Quaternion.Euler(-Mathf.Atan2(3.4f, 14) * Mathf.Rad2Deg, 0, 0);
        Box("Balcon", new(0, 3.25f, 58), new(7, .3f, 6), _neutral);
        Box("Garde-corps gauche", new(-3.45f, 3.98f, 58), new(.18f, 1.15f, 6), _dark);
        Box("Garde-corps droit", new(3.45f, 3.98f, 58), new(.18f, 1.15f, 6), _dark);
        Box("Garde-corps rampe gauche", new(-1.5f, 2.28f, 48), new(.12f, 1.1f, 14.4f), _dark)
            .transform.rotation = ramp.transform.rotation;
        Box("Garde-corps rampe droit", new(1.5f, 2.28f, 48), new(.12f, 1.1f, 14.4f), _dark)
            .transform.rotation = ramp.transform.rotation;
        DressHallAndLookout();
        Box("Repère haut de la baie école", new(-1.3f, 2.25f, 25.05f), new(.78f, .21f, 1.1f), _signal);

        var rig = new GameObject("Joueur PC");
        rig.transform.position = new(0, .03f, 1);
        var capsule = rig.AddComponent<CharacterController>();
        capsule.height = 1.75f;
        capsule.center = new(0, .875f, 0);
        capsule.radius = .32f;
        var camera = new GameObject("Caméra PC").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.SetParent(rig.transform, false);
        camera.transform.localPosition = new(0, 1.65f, 0);
        camera.gameObject.AddComponent<AudioListener>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new(.22f, .29f, .38f);
        var player = rig.AddComponent<PCPlayerController>();
        player.cameraTransform = camera.transform;
        var route = new GameObject("Parcours Photo V2").AddComponent<PhotoV2Walkthrough>();
        route.view = camera;
        route.player = player;
        route.film = NetworkRideBuilder.BuildVisuals(route.transform);
        route.arrival = new Vector3(0, .035f, 7.3f);
        route.targets = new[]
        {
            Target("Écran Envoyer", 1.4f, 1.15f, 3.46f),
            Target("Seuil de la façade", 1.25f, 1.25f, 12.65f),
            Target("Lecteur de badge", 1.25f, 1.25f, 15.4f),
            Target("Panneau de brassage", 1.25f, 1.25f, 20.1f),
            Target("Porte de la baie école", -1.3f, 1.34f, 24.48f),
            Target("Tiroir école", -1.3f, 1.34f, 24.68f),
            Target("Commande d’aperçu", -1.3f, 1.34f, 24.88f),
            Target("Repère stockage", 1.3f, 1.35f, 36.4f),
            Target("Écran de relecture", 1.3f, 1.35f, 38.1f),
            Target("Balcon · baie école", 0, 4.75f, 58.65f)
        };
        route.barriers = new GameObject[route.targets.Length];
        route.barriers[0] = Gate("Sortie chambre", 4.85f, 7.7f);
        route.barriers[1] = Gate("Porte extérieure", 13, 3.5f);
        route.sasEntrance = route.barriers[1];
        route.badgeReader = route.targets[2];
        DressOpeningTargets(route.targets);
        InstallSchoolRack(route);
        InstallAccessAndPatch(route);
        InstallStorageAndBalcony(route);
        InstallEnergy(route, rowLights);
        InstallGridRoute(route);
        InstallCoolingRoute(route);
        InstallWaterRoute(route);
        route.barriers[2] = Gate("Porte intérieure", 16.85f, 3.5f);
        route.guides = new GameObject[route.targets.Length];
        float previous = 1;
        for (int i = 0; i < route.targets.Length; i++)
        {
            var position = route.targets[i].transform.position;
            var guide = new GameObject("Jalon " + (i + 1));
            route.guides[i] = guide;
            for (float z = previous + .6f; z < position.z; z += .75f)
            {
                float y = z < 41 ? .018f : Mathf.Clamp01((z - 41) / 14) * 3.4f + .025f;
                var mark = Box("Marque cyan", new(0, y, z), new(.10f, .012f, .37f), _signal);
                mark.transform.SetParent(guide.transform);
                UnityEngine.Object.DestroyImmediate(mark.GetComponent<Collider>());
            }
            guide.SetActive(i == 0);
            previous = i == 0 ? route.arrival.z : position.z;
        }
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Scène V2 non enregistrée.");
        AssetDatabase.SaveAssets();
    }

    [MenuItem("CloudVR/V2/Construire le player Windows")]
    public static void BuildWindows()
    {
        Generate();
        Directory.CreateDirectory(Path.GetDirectoryName(Output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = Output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"Build V2 : {report.summary.result}");
        Debug.Log($"CLOUDVR_V2_BUILD {report.summary.totalErrors} erreurs, {report.summary.totalWarnings} avertissements");
    }

    static GameObject Target(string name, float x, float y, float z) =>
        Box(name, new(x, y, z), new(.55f, .4f, .08f), _signal);

    static GameObject Gate(string name, float z, float width) =>
        Box(name, new(0, 1.5f, z), new(width, 3, .15f), _dark);

    static void WallPair(string name, float x, float start, float end, float height, Material material)
    {
        foreach (int side in new[] { -1, 1 })
            Box(name + " · paroi " + side, new(side * x, height / 2, (start + end) / 2),
                new(.25f, height, end - start), material);
    }

    static GameObject Box(string name, Vector3 position, Vector3 size, Material material)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.position = position;
        box.transform.localScale = size;
        box.GetComponent<MeshRenderer>().sharedMaterial = material;
        return box;
    }

    static Material Palette(string name, Color color)
    {
        string path = $"{Folder}/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Smoothness", .15f);
        EditorUtility.SetDirty(material);
        return material;
    }
}
#endif
