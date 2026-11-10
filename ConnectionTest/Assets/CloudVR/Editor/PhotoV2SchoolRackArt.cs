#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static partial class PhotoV2BlockoutBuilder
{
    static void InstallSchoolRack(PhotoV2Walkthrough route)
    {
        var root = new GameObject("Baie école · serveur de formation").transform;
        root.position = new Vector3(-1.30f,0,25.05f);
        var graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Graphite.mat");
        var aluminum = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Aluminium.mat");
        var cyan = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Cyan.mat");
        foreach (int side in new[] {-1,1})
        {
            Box("Montant", root.position+new Vector3(side*.49f,1.09f,0), new(.075f,2.18f,1), graphite)
                .transform.SetParent(root,true);
            Box("Rail fixe 2U", root.position+new Vector3(side*.236f,1.29f,-.04f),
                new(.018f,.045f,.82f), aluminum).transform.SetParent(root,true);
        }
        foreach (float y in new[] {.05f,2.125f})
            Box("Traverse", root.position+new Vector3(0,y,0), new(.98f,.09f,1), graphite)
                .transform.SetParent(root,true);
        Box("Dos de baie", root.position+new Vector3(0,1.09f,.47f), new(.88f,2.09f,.06f), graphite)
            .transform.SetParent(root,true);
        foreach (float y in new[] {.28f,.50f,.72f,.94f,1.62f,1.84f})
            Box("Obturateur", root.position+new Vector3(0,y,-.44f), new(.87f,.17f,.04f), graphite)
                .transform.SetParent(root,true);
        Sign("BAIE ÉCOLE 01", root.position + new Vector3(0,2.16f,-.508f), .078f, Color.white, root);
        route.computeInput = Box("Port réseau du serveur", root.position + new Vector3(-.28f,1.96f,-.53f),
            new(.13f,.075f,.08f), cyan).transform;
        route.computeOutput = Box("Sortie du serveur vers le stockage", root.position + new Vector3(.28f,1.96f,-.53f),
            new(.13f,.075f,.08f), AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Ambre.mat")).transform;
        route.computeInput.SetParent(root,true);
        route.computeOutput.SetParent(root,true);

        var rack = root.gameObject.AddComponent<ServerBladeInteraction>();
        route.schoolRack = rack;
        rack.rackDoor = new GameObject("Charnière").transform;
        rack.rackDoor.SetParent(root,false);
        rack.rackDoor.localPosition = new Vector3(-.47f,0,-.50f);
        var door = SchoolModel("Server_Rack_Door", rack.rackDoor);
        door.localPosition = new Vector3(.47f,0,0);
        rack.serverBlade = SchoolModel("Server_Blade_Detailed", root);
        rack.serverBlade.localPosition = new Vector3(0,1.25f,.34f);
        rack.serverBlade.localRotation = Quaternion.Euler(0,180,0);
        rack.serverSlideDistance = .64f;
        rack.animDuration = .65f;

        var handleTarget = route.targets[4].transform;
        handleTarget.position = root.position + new Vector3(.42f,1.02f,-.574f);
        handleTarget.localScale = new Vector3(.072f,.20f,.05f);
        handleTarget.GetComponent<MeshRenderer>().sharedMaterial = cyan;
        handleTarget.SetParent(rack.rackDoor,true);
        var trayTarget = route.targets[5].transform;
        trayTarget.SetParent(rack.serverBlade,true);
        trayTarget.localPosition = new Vector3(0,.061f,.779f);
        trayTarget.localScale = new Vector3(.32f,.065f,.05f);
        trayTarget.GetComponent<MeshRenderer>().sharedMaterial = cyan;

        var labels = new GameObject("Repères du serveur").transform;
        labels.SetParent(rack.serverBlade,false);
        rack.hardwareLabels = labels.gameObject;
        MarkPart(labels, "CPU · traitement", new Vector3(.06f,.38f,.38f));
        MarkPart(labels, "RAM · mémoire de travail", new Vector3(-.19f,.29f,.48f));
        MarkPart(labels, "NVMe · stockage local", new Vector3(.05f,.20f,.78f));
        labels.gameObject.SetActive(false);

        var screen = route.targets[6].transform;
        screen.position = root.position + new Vector3(.69f,1.48f,-.52f);
        screen.localScale = new Vector3(.33f,.28f,.062f);
        screen.GetComponent<MeshRenderer>().sharedMaterial = graphite;
        screen.SetParent(root,true);
        Box("Potence écran aperçu", root.position+new Vector3(.53f,1.36f,-.49f), new(.15f,.035f,.055f), aluminum)
            .transform.SetParent(root,true);
        var panel = new GameObject("Écran aperçu", typeof(RectTransform), typeof(Canvas));
        var canvas = panel.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        panel.transform.position = screen.position + Vector3.back * .033f;
        panel.transform.SetParent(root,true);
        panel.transform.localScale = Vector3.one * .0008f;
        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(395,330);
        var image = new GameObject("Miniature", typeof(RectTransform), typeof(UnityEngine.UI.RawImage))
            .GetComponent<UnityEngine.UI.RawImage>();
        image.transform.SetParent(panel.transform,false);
        image.rectTransform.sizeDelta = new Vector2(350,232);
        image.raycastTarget = false;
        image.enabled = false;
        route.previewDisplay = image;
        route.previewLabel = Sign("GÉNÉRER L’APERÇU", screen.position + new Vector3(0,0,-.035f), .031f, Color.white, root);

        // Orienter la face de service vers l’allée centrale après avoir assemblé la baie.
        root.rotation = Quaternion.Euler(0,-90,0);
    }

    static Transform SchoolModel(string name, Transform parent)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/" + name + ".fbx");
        if (!model) throw new System.InvalidOperationException("Modèle baie école introuvable : " + name);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model,parent);
        foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (!materials[i]) continue;
                var mapped = AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/" + materials[i].name + ".mat");
                if (mapped) materials[i] = mapped;
            }
            renderer.sharedMaterials = materials;
        }
        return instance.transform;
    }

    static void MarkPart(Transform parent, string title, Vector3 localPosition)
    {
        var label = Sign(title, parent.TransformPoint(localPosition), .035f, Color.white);
        label.transform.parent.SetParent(parent,true);
    }
}
#endif
