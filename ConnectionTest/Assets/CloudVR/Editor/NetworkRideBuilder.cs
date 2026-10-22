#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class NetworkRideBuilder
{
    static Material Mat(string name,Color color)
    {
        string path="Assets/CloudVR/Materials/P2_"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(mat,path);}
        mat.SetColor("_BaseColor",color);return mat;
    }
    static void Assign(GameObject model)
    {
        foreach(var r in model.GetComponentsInChildren<MeshRenderer>())
        {
            var mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++)
            {
                var source=mats[i];if(source==null)continue;
                var existing=AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/"+source.name+".mat");if(existing!=null)mats[i]=existing;
            }
            r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.Off;
        }
    }
    public static void Build(NetworkTransitionV2 ride)
    {
        var world=new GameObject("P2_Voyage_Reseau_3D");world.transform.SetParent(ride.transform,false);world.transform.position=new Vector3(0,-120,0);
        var visuals=world.AddComponent<NetworkRideVisuals>();ride.visuals=visuals;
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/Data_Fiber_Tunnel.fbx");
        var outer=Mat("Gaine",new Color(.035f,.055f,.085f));var ring=Mat("Anneaux",new Color(.20f,.29f,.38f));
        var cyan=Mat("Lumiere_Fibre",new Color(.08f,.75f,.85f));var amber=Mat("Signal",new Color(1,.46f,.12f));
        for(int i=0;i<3;i++)
        {
            var segment=(GameObject)PrefabUtility.InstantiatePrefab(prefab,world.transform);
            segment.name="P2_Fibre_"+i;segment.transform.localPosition=new Vector3(0,3.6f,20*i);segment.transform.localRotation=Quaternion.Euler(90,0,0);segment.transform.localScale=new Vector3(3,1,3);
            foreach(var r in segment.GetComponentsInChildren<MeshRenderer>())r.sharedMaterial=r.name.Contains("Outer")?outer:r.name.Contains("Ring")?ring:r.name.Contains("Pulse")?amber:cyan;
        }
        visuals.packets=new Transform[16];
        for(int i=0;i<16;i++)
        {
            var p=GameObject.CreatePrimitive(PrimitiveType.Cube);p.name="P2_Signal_"+i;p.transform.SetParent(world.transform,false);p.transform.localScale=new Vector3(.10f,.10f,1.2f);Object.DestroyImmediate(p.GetComponent<Collider>());p.GetComponent<Renderer>().sharedMaterial=i%3==0?amber:cyan;visuals.packets[i]=p.transform;
        }
        visuals.Animate(0);
        var campusPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/DataCenter_Campus.fbx");
        var campus=(GameObject)PrefabUtility.InstantiatePrefab(campusPrefab,world.transform);campus.name="P2_Campus_Arrivee";
        campus.transform.localPosition=new Vector3(0,0,100);campus.transform.localRotation=Quaternion.Euler(0,180,0);Assign(campus);
        // Le modèle d'origine plaçait fenêtres et bandeau derrière la peau opaque de façade.
        foreach(var r in campus.GetComponentsInChildren<MeshRenderer>())
        {
            if(r.name.StartsWith("Fenetre_"))r.transform.position+=Vector3.back*.35f;
            if(r.name=="DC_Logo_Band")r.transform.position+=Vector3.back*.30f;
        }
        var lamp=new GameObject("P2_Lumiere_Campus");lamp.transform.SetParent(world.transform,false);lamp.transform.localRotation=Quaternion.Euler(35,-25,0);
        var light=lamp.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.cullingMask=1<<NetworkRideVisuals.Layer;light.shadows=LightShadows.None;
        foreach(var t in world.GetComponentsInChildren<Transform>(true))t.gameObject.layer=NetworkRideVisuals.Layer;
        world.SetActive(false);
    }
}
#endif
