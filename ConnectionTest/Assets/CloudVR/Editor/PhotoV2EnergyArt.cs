#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class PhotoV2BlockoutBuilder
{
    static GameObject CreateRowLights(int row)
    {
        var root = new GameObject($"Indicateurs rangée {row + 1}");
        var cyan = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Cyan.mat");
        float x = (row - 2.5f)*2.6f;
        foreach (float z in RackPositions(row))
        {
            var strip = Box("État calcul",new(x,2.121f,z),new(.49f,.032f,.61f),cyan);
            strip.transform.SetParent(root.transform,true);
            strip.GetComponent<Collider>().enabled = false;
        }
        root.SetActive(row==2 || row==3);
        return root;
    }

    static void InstallEnergy(PhotoV2Walkthrough route, GameObject[] lights)
    {
        var graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Graphite.mat");
        var aluminum = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Aluminium.mat");
        var amber = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Ambre.mat");
        var root = new GameObject("Supervision · demande de calcul");
        var energy = root.AddComponent<PhotoV2Energy>();
        route.energy = energy;
        // Le secteur central est la charge initiale. Puis éveil du secteur droit et du secteur gauche.
        energy.rowLights = new[]{lights[2],lights[3],lights[4],lights[5],lights[1],lights[0]};
        var desk = Box("Pupitre énergie",new(.83f,4.02f,55.91f),new(.70f,1.23f,.44f),graphite);
        desk.transform.SetParent(root.transform,true);
        var plate = Box("Platine du levier",new(.85f,4.65f,56.02f),new(.30f,.052f,.19f),aluminum);
        plate.transform.SetParent(root.transform,true);
        energy.handle = new GameObject("Pivot manette").transform;
        energy.handle.SetParent(root.transform,false);
        energy.handle.position = new Vector3(.85f,4.67f,56.02f);
        var grip = Box("Poignée de demande",new(.85f,4.91f,56.02f),new(.12f,.12f,.12f),amber);
        grip.transform.SetParent(energy.handle,true);
        energy.control = grip.GetComponent<Collider>();
        var shaft = Box("Tige de manette",new(.85f,4.795f,56.02f),new(.036f,.24f,.036f),aluminum);
        shaft.transform.SetParent(energy.handle,true);
        shaft.GetComponent<Collider>().enabled = false;
        energy.handle.localRotation = Quaternion.Euler(-24,0,0);
        var plateText = Sign("DEMANDE DE CALCUL",new(.83f,4.50f,56.137f),.058f,Color.white);
        FaceBalcony(plateText,root.transform);
        energy.meter = Sign("",new(.83f,4.18f,56.137f),.050f,Color.white);
        FaceBalcony(energy.meter,root.transform);
        energy.meter.rectTransform.sizeDelta = new Vector2(1180,200);
        var canvas = energy.meter.transform.parent.GetComponent<RectTransform>();
        canvas.sizeDelta = energy.meter.rectTransform.sizeDelta;
        InstallDistribution(energy,root.transform,graphite,aluminum,amber);
        root.SetActive(false);
    }

    static void InstallDistribution(PhotoV2Energy energy, Transform root, Material graphite, Material aluminum, Material amber)
    {
        const float x = -2.04f, face = 56.14f;
        var cabinet = Box("Tableau électrique",new(x,4.53f,face-.21f),new(1.76f,2.24f,.40f),graphite);
        cabinet.transform.SetParent(root,true);
        FaceBalcony(Sign("TABLEAU ÉLECTRIQUE",new(x,5.42f,face),.077f,Color.white),root);
        energy.cabinetMeter = Sign("",new(x,5.24f,face),.046f,Color.white);
        FaceBalcony(energy.cabinetMeter,root);
        energy.breakers = new Collider[4];
        energy.breakerHandles = new Transform[4];
        energy.breakerLabels = new Text[4];
        energy.feeders = new LineRenderer[4];
        energy.live = amber;
        energy.idle = aluminum;
        int[] rowOrder = {4,5,1,0};

        for (int i=0; i<rowOrder.Length; i++)
        {
            float px = x-.63f+i*.42f;
            string row = $"RANGÉE {rowOrder[i]+1:00}";
            var plate = Box(row,new(px,4.62f,face),new(.35f,.82f,.09f),aluminum);
            plate.transform.SetParent(root,true);
            FaceBalcony(Sign(row,new(px,4.94f,face+.055f),.038f,Color.black),root);
            energy.breakerLabels[i] = Sign("",new(px,4.21f,face+.06f),.041f,Color.white);
            FaceBalcony(energy.breakerLabels[i],root);
            var lever = new GameObject(row + " · pivot").transform;
            lever.SetParent(root,false);
            lever.position = new Vector3(px,4.50f,face+.078f);
            energy.breakerHandles[i] = lever;
            var grip = Box("Levier · "+row,lever.position+new Vector3(0,.133f,.11f),new(.16f,.13f,.09f),amber);
            grip.transform.SetParent(lever,true);
            var arm = Box("Bras du levier",lever.position+new Vector3(0,.067f,.06f),new(.05f,.20f,.055f),graphite);
            arm.transform.SetParent(lever,true);
            arm.transform.rotation = Quaternion.Euler(37,0,0);
            arm.GetComponent<Collider>().enabled = false;
            energy.breakers[i] = grip.GetComponent<Collider>();
            float rowX = (rowOrder[i]-2.5f)*2.6f;
            float z = 53.15f-i*.46f;
            var points = new[] {new Vector3(px,5.65f,face-.15f),new Vector3(px,5.93f,face-.15f),
                new Vector3(px,5.93f,z),new Vector3(rowX,5.93f,z),new Vector3(rowX,4.68f,z),
                new Vector3(rowX,4.68f,24.1f),new Vector3(rowX,2.14f,24.1f)};
            energy.feeders[i] = PatchLine("Départ électrique · "+row,aluminum,.055f,points.Length);
            energy.feeders[i].SetPositions(points);
            var label = Sign(row,new(rowX,3.38f,53.1f),.105f,Color.white);
            FaceBalcony(label,root);
        }
        var incoming = new[] {new Vector3(0,4.43f,61),new Vector3(-1.72f,5.61f,61),
            new Vector3(-1.72f,5.76f,56.48f),new Vector3(x,5.76f,face-.10f),new Vector3(x,5.65f,face-.10f)};
        energy.incoming = PatchLine("Arrivée du poste au tableau",aluminum,.078f,incoming.Length);
        energy.incoming.SetPositions(incoming);
    }

    static void FaceBalcony(Text label, Transform root)
    {
        var panel = label.transform.parent;
        panel.SetParent(root,true);
        panel.rotation = Quaternion.Euler(0,180,0);
    }
}
#endif
