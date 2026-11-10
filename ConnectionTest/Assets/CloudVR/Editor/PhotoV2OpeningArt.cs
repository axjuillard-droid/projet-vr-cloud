#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class PhotoV2BlockoutBuilder
{
    static void DressOpening()
    {
        var graphite = Palette("V2 Graphite", new(.045f,.066f,.092f));
        var aluminum = Palette("V2 Aluminium", new(.27f,.34f,.39f));
        var porcelain = Palette("V2 Minéral", new(.56f,.62f,.64f));
        var amber = Palette("V2 Ambre", new(.93f,.56f,.24f));
        var ink = Palette("V2 Écran", new(.025f,.055f,.09f));
        var cyan = Palette("V2 Cyan", new(.13f,.65f,.83f));
        cyan.EnableKeyword("_EMISSION");
        cyan.SetColor("_EmissionColor", new Color(.07f,.47f,.66f));
        EditorUtility.SetDirty(cyan);
        amber.EnableKeyword("_EMISSION");
        amber.SetColor("_EmissionColor", new Color(.78f,.31f,.08f));
        EditorUtility.SetDirty(amber);

        Box("Plafond chambre", new(0,3.13f,2.5f), new(8,.24f,5), _warm);
        Box("Dos de chambre", new(0,1.65f,4.94f), new(8,3.3f,.10f), _warm);
        // Le plan de chambre sort par coupe de caméra ; ce mur reste derrière le joueur à l’arrivée.
        Box("Tapis", new(1.5f,.014f,2.7f), new(3.25f,.025f,2.5f), graphite);
        PlaceRoomModel("Desk_Setup", new Vector3(1.40f,.02f,3.50f));
        Box("Socle écran", new(1.4f,.788f,3.51f), new(.36f,.035f,.24f), graphite);
        Box("Pied écran", new(1.4f,.93f,3.50f), new(.065f,.27f,.082f), graphite);
        PlaceRoomModel("Bed_Student", new Vector3(-2.54f,.02f,2.37f));
        PlaceRoomModel("Room_Decor", new Vector3(-2.20f,.02f,4.2f));
        Box("Panneau bois", new(1.65f,1.6f,4.83f), new(3.58f,2.35f,.07f), _warm);
        for (int i=0; i<16; i++)
            Box("Lame de bois", new(.04f+i*.22f,1.6f,4.78f), new(.07f,2.26f,.025f), graphite);
        Box("Lampe de bureau", new(2.55f,1.42f,4.16f), new(.24f,.13f,.27f), amber);
        Box("Pied de lampe", new(2.55f,1.13f,4.16f), new(.036f,.5f,.036f), graphite);
        Point("Lampe 2700 K", new(2.5f,1.36f,4.05f), new(1f,.63f,.34f), 2.8f, 3.2f);
        Point("Remplissage doux", new(-1.7f,2.5f,2.5f), new(1f,.78f,.58f), 1.5f, 5.7f);

        for (int i=0; i<16; i++)
        {
            float x = 2.08f+i*.48f;
            foreach (int side in new[] {-1,1})
                Box("Joint de bardage", new(x*side,4.08f,12.69f), new(.026f,7.6f,.035f), graphite);
        }
        Box("Bandeau supérieur", new(0,7.79f,12.68f), new(19.6f,.29f,.12f), aluminum);
        Box("Auvent", new(0,3.46f,12.00f), new(4.45f,.24f,2.16f), graphite);
        Box("Éclairage sous auvent", new(0,3.326f,11.99f), new(3.28f,.019f,.08f), cyan);
        for (int i=0; i<6; i++)
            Box("Joint du parvis", new(0,.012f,7.0f+i*.88f), new(3.66f,.018f,.022f), graphite);
        Box("Plaque d’entrée", new(0,4.22f,12.66f), new(3.45f,.76f,.08f), graphite);
        Sign("CLOUDVR", new(0,4.37f,12.61f), .26f, Color.white);
        Sign("01   /   ACCÈS VISITEURS", new(0,4.03f,12.61f), .095f, new Color(.52f,.87f,.96f));
        for (int side=-1; side<=1; side+=2)
        {
            Box("Bordure du parvis", new(side*2.18f,.15f,9.7f), new(.19f,.3f,5.3f), porcelain);
            Box("Protection d’accès", new(side*2.01f,.47f,11.2f), new(.13f,.94f,.13f), graphite);
        }

        Box("Plafond sas", new(0,3.0f,15.0f), new(3.46f,.16f,4), graphite);
        Box("Plafond réseau", new(0,4.0f,19.5f), new(7.75f,.16f,5), graphite);
        foreach (float z in new[] { 14.1f,15.85f,17.8f,19.5f,21.2f })
        {
            float height = z < 17 ? 2.90f : 3.90f;
            Box("Rail lumineux", new(0,height,z), new(1.96f,.035f,.095f), cyan);
        }
        Point("Sas froid", new(0,2.75f,15.0f), new(.59f,.82f,1f), 2.5f, 5f);
        Point("Salle réseau", new(0,3.35f,19.5f), new(.60f,.79f,1f), 3.5f, 6f);
        Box("Châssis de brassage", new(1.25f,1.25f,20.45f), new(1.03f,2.48f,.56f), graphite);
        for (int i=0; i<7; i++)
        {
            float y = .45f+i*.25f;
            Box("Panneau réseau " + i, new(1.25f,y,20.154f), new(.93f,.185f,.038f), aluminum);
            for(int j=0; j<8; j++)
                Box("Port", new(.86f+.11f*j,y,20.127f), new(.045f,.037f,.016f), j%3==0 ? cyan : ink);
        }
        for (int lane=0; lane<2; lane++)
        {
            var cable = new GameObject(lane==0 ? "Fibre A" : "Fibre B").AddComponent<LineRenderer>();
            cable.sharedMaterial = lane==0 ? cyan : amber;
            cable.widthMultiplier = .035f;
            cable.numCornerVertices = 3;
            cable.positionCount = 4;
            float x = 1.20f+lane*.16f;
            cable.SetPositions(new[]{new Vector3(x,1.96f,20.103f),new Vector3(x,3.24f,20.11f),
                new Vector3(x,3.24f,21.9f),new Vector3(x,3.24f,22.5f)});
        }
        Sign("01   /   ARRIVÉE RÉSEAU", new(1.25f,2.68f,20.13f), .105f, Color.white);
    }

    static void DressOpeningTargets(GameObject[] targets)
    {
        var screen = Palette("V2 Écran actif", new(.018f,.064f,.107f));
        screen.EnableKeyword("_EMISSION");
        screen.SetColor("_EmissionColor", new Color(.018f,.064f,.107f));
        EditorUtility.SetDirty(screen);
        var monitor = targets[0].transform;
        monitor.localScale = new Vector3(.81f,.47f,.09f);
        targets[0].GetComponent<MeshRenderer>().sharedMaterial = screen;
        Sign("ENVOYER LA PHOTO", monitor.position + new Vector3(0,0,-.049f), .057f, Color.white, monitor);
        Sign("OUVRIR", targets[1].transform.position + new Vector3(0,0,-.045f), .087f, Color.black, targets[1].transform);
    }

    static void InstallAccessAndPatch(PhotoV2Walkthrough route)
    {
        var graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Graphite.mat");
        var aluminum = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Aluminium.mat");
        var cyan = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Cyan.mat");
        var amber = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Ambre.mat");
        var badge = route.badgeReader.transform;
        badge.position = new Vector3(1.37f,1.24f,15.43f);
        badge.localScale = new Vector3(.24f,.32f,.09f);
        badge.GetComponent<MeshRenderer>().sharedMaterial = graphite;
        Box("Support de lecteur relié à la paroi", new(1.53f,1.24f,15.55f), new(.30f,.38f,.15f), aluminum);
        Box("Surface de lecture", new(1.37f,1.19f,15.381f), new(.13f,.15f,.009f), cyan)
            .GetComponent<Collider>().enabled = false;
        Sign("BADGE", new(1.37f,1.35f,15.377f), .039f, Color.white);
        route.badgeApproved = Box("Accès validé", new(1.37f,1.29f,15.375f), new(.11f,.026f,.012f), cyan);
        route.badgeApproved.GetComponent<Collider>().enabled = false;
        route.badgeApproved.SetActive(false);

        var patch = route.gameObject.AddComponent<PhotoV2PatchCord>();
        route.patchCord = patch;
        patch.view = route.view;
        var socket = route.targets[3].transform;
        socket.position = new Vector3(1.20f,1.34f,20.059f);
        socket.localScale = new Vector3(.15f,.11f,.11f);
        socket.GetComponent<MeshRenderer>().sharedMaterial = cyan;
        patch.portA = socket.GetComponent<Collider>();
        patch.portB = Box("Port B · réserve", new(1.43f,1.34f,20.059f), new(.15f,.11f,.11f), graphite)
            .GetComponent<Collider>();
        Sign("A", new(1.20f,1.46f,20.00f), .066f, Color.white);
        Sign("B", new(1.43f,1.46f,20.00f), .066f, Color.white);
        patch.plug = Box("Fiche du cordon", new(.92f,1.04f,19.973f), new(.075f,.063f,.13f), amber)
            .GetComponent<Collider>();
        Box("Berceau du cordon", new(.92f,.985f,19.99f), new(.19f,.055f,.19f), graphite);
        Sign("CORDON", new(.92f,.91f,19.89f), .041f, Color.white);
        patch.anchor = new GameObject("Ancrage du cordon").transform;
        patch.anchor.position = new Vector3(1.52f,.74f,20.087f);
        patch.cord = PatchLine("Cordon souple", amber, .021f, 5);
        patch.signalTrace = PatchLine("Tracé pédagogique · réseau vers calcul", cyan, .031f, 5);
        patch.signalTrace.SetPositions(new[]
        {
            new Vector3(1.20f,1.38f,19.96f), new Vector3(1.20f,3.32f,19.96f),
            new Vector3(1.20f,3.32f,24.38f), new Vector3(-1.30f,3.32f,24.38f),
            new Vector3(-1.30f,1.90f,24.38f)
        });
        patch.signalTrace.enabled = false;
        patch.signal = Box("Impulsion réseau", patch.signalTrace.GetPosition(0), new(.11f,.11f,.11f), amber).transform;
        patch.signal.GetComponent<Collider>().enabled = false;
        patch.signal.gameObject.SetActive(false);
    }

    static LineRenderer PatchLine(string name, Material material, float width, int points)
    {
        var line = new GameObject(name).AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.widthMultiplier = width;
        line.numCornerVertices = 4;
        line.numCapVertices = 3;
        line.positionCount = points;
        return line;
    }

    static void PlaceRoomModel(string name, Vector3 position)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/" + name + ".fbx");
        if (!model) throw new System.InvalidOperationException("Mobilier introuvable : " + name);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        instance.transform.position = position;
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i=0; i<materials.Length; i++)
            {
                if (!materials[i]) continue;
                var replacement = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/CloudVR/Materials/Room_" + materials[i].name.Replace("Desk_", "") + ".mat");
                if (replacement) materials[i] = replacement;
            }
            renderer.sharedMaterials = materials;
        }
    }

    static void Point(string name, Vector3 position, Color color, float intensity, float range)
    {
        var light = new GameObject(name).AddComponent<Light>();
        light.transform.position = position;
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    static void Sign(string text, Vector3 position, float height, Color color, Transform parent = null)
    {
        var panel = new GameObject(text, typeof(RectTransform), typeof(Canvas));
        panel.transform.position = position;
        if (parent) panel.transform.SetParent(parent, true);
        panel.transform.localScale = parent
            ? new Vector3(height / (90 * parent.lossyScale.x), height / (90 * parent.lossyScale.y),
                height / (90 * parent.lossyScale.z))
            : Vector3.one * (height / 90);
        panel.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(720, 100);
        var label = new GameObject("Libellé", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(panel.transform, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 72;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.text = text;
        label.color = color;
        label.raycastTarget = false;
    }
}
#endif
