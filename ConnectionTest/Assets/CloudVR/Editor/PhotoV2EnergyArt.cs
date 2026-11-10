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
        for (int bay=0; bay<7; bay++)
        {
            if (row==2 && bay==1) continue;
            float z = 23+bay*1.95f;
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
        root.SetActive(false);
    }

    static void FaceBalcony(Text label, Transform root)
    {
        var panel = label.transform.parent;
        panel.SetParent(root,true);
        panel.rotation = Quaternion.Euler(0,180,0);
    }
}
#endif
