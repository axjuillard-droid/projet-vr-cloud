#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Ajout idempotent de la démo aux deux scènes ; ne régénère pas les décors.</summary>
public static class DemoTourBuilder
{
    [MenuItem("CloudVR/Build Scene/Démo guidée complète")]
    public static void Build()
    {
        if(Application.isPlaying) throw new InvalidOperationException("Arrêter Play avant le builder.");
        for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Scène non enregistrée : "+SceneManager.GetSceneAt(i).path);
        var scene=EditorSceneManager.OpenScene(DataCenterSceneBuilder.ScenePath);
        var old=GameObject.Find("Demo_Parcours"); if(old!=null) UnityEngine.Object.DestroyImmediate(old);
        var tour=new GameObject("Demo_Parcours").AddComponent<DemoTour>();
        var dark=Mat("Demo_Dark",new Color(.035f,.065f,.11f));
        var cyan=Mat("Demo_Cyan",new Color(.12f,.8f,.9f));
        var warm=Mat("Demo_Warm",new Color(1,.35f,.10f));
        var cold=Mat("Demo_Cold",new Color(.15f,.75f,.55f));
        var metal=Mat("Demo_Metal",new Color(.28f,.36f,.46f));
        tour.coldMaterial=cold; tour.warmMaterial=warm; tour.packetMaterial=cyan;
        tour.stations=new DemoStation[6];
        var manager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
        for(int i=0;i<6;i++)
        {
            var go=new GameObject("Borne_"+(i+1).ToString("00")); go.transform.SetParent(tour.transform);
            float z = 8f + i * 6f;
            go.transform.position = new Vector3(i % 2 == 0 ? -1.9f : 1.9f, 0, z);
            go.transform.rotation=Quaternion.Euler(0,i%2==0 ? -90 : 90,0);
            var station=go.AddComponent<DemoStation>(); station.index=i; tour.stations[i]=station;
            Box(go.transform,"Console",new Vector3(0,.62f,0),new Vector3(.85f,1.24f,.6f),dark);
            Box(go.transform,"Socle",new Vector3(0,.045f,0),new Vector3(1,.09f,.75f),metal);
            station.indicator=Box(go.transform,"Bande_lumineuse",new Vector3(0,1.20f,-.31f),new Vector3(.78f,.04f,.025f),cyan).GetComponent<Renderer>();
            var panel=Box(go.transform,"Écran",new Vector3(0,1.63f,-.12f),new Vector3(1.15f,.65f,.08f),dark);
            var collider=go.AddComponent<BoxCollider>(); collider.center=new Vector3(0,.96f,0); collider.size=new Vector3(1.2f,1.92f,.65f);
            var xr=go.AddComponent<XRSimpleInteractable>(); xr.interactionManager=manager; xr.interactionLayers=1; xr.colliders.Clear(); xr.colliders.Add(collider);
            var screen=new GameObject("Légende",typeof(RectTransform),typeof(Canvas)); screen.transform.SetParent(go.transform,false);
            screen.transform.localPosition=new Vector3(0,1.63f,-.165f); screen.transform.localScale=Vector3.one*.002f;
            screen.GetComponent<RectTransform>().sizeDelta=new Vector2(550,290); screen.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            station.caption=DemoTour.Label(screen.transform,"Action",34,new Vector2(.04f,.04f),new Vector2(.96f,.96f),Color.white);
            station.caption.alignment=TextAnchor.MiddleCenter; station.caption.text=(i+1).ToString("00")+"  "+DemoTour.Titles[i];
            // Icônes géométriques originales, sans collider supplémentaire.
            if(i<3) for(int row=0;row<3;row++) Box(go.transform,"Module",new Vector3((row-1)*.20f,.83f,-.315f),new Vector3(.14f,.24f,.03f),metal);
            else if(i==3) for(int row=0;row<7;row++) Box(go.transform,"Grille_air",new Vector3(0,.4f+row*.09f,-.32f),new Vector3(.6f,.025f,.03f),metal);
            else if(i==4) for(int row=0;row<2;row++) Box(go.transform,"Batterie",new Vector3((row-.5f)*.3f,.68f,-.33f),new Vector3(.22f,.38f,.06f),cold);
            else Box(go.transform,"Signal",new Vector3(0,.72f,-.33f),new Vector3(.58f,.22f,.04f),cyan);
        }
        var flow=new GameObject("Flux_Illustratif",typeof(LineRenderer)); flow.transform.SetParent(tour.transform,false);
        var line=flow.GetComponent<LineRenderer>(); line.sharedMaterial=cyan; line.positionCount=4; line.startWidth=line.endWidth=.025f;
        tour.flowRoot=flow.transform;
        var packet=GameObject.CreatePrimitive(PrimitiveType.Sphere); packet.name="Requête_illustrée"; packet.transform.SetParent(flow.transform,false); packet.transform.localScale=Vector3.one*.13f;
        UnityEngine.Object.DestroyImmediate(packet.GetComponent<Collider>()); packet.GetComponent<Renderer>().sharedMaterial=cyan; tour.packet=packet.transform; flow.SetActive(false);
        tour.thermal=new Renderer[3];
        for(int i=0;i<3;i++) tour.thermal[i]=Box(tour.transform,"Repère_thermique",new Vector3(2.8f,2.23f,14+i*6),new Vector3(1.1f,.035f,.95f),cold).GetComponent<Renderer>();
        // Ancien panneau remplacé par l'objectif et les bornes : garder l'allée lisible.
        foreach(var name in new[]{"Guide_Visite","Guide_Visite_Support"}) { var go=GameObject.Find(name); if(go!=null) go.SetActive(false); }
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        scene=EditorSceneManager.OpenScene(DataCenterSceneBuilder.ChambrePath);
        if(UnityEngine.Object.FindFirstObjectByType<DemoRoom>()==null) new GameObject("Demo_Accueil").AddComponent<DemoRoom>();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Debug.Log("[DemoTour] Six bornes et accueil enregistrés.");
    }
    static Material Mat(string name,Color color)
    {
        var path="Assets/CloudVR/Materials/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null) { var shader=Shader.Find("Universal Render Pipeline/Unlit"); if(shader==null) throw new InvalidOperationException("URP Unlit requis."); mat=new Material(shader); AssetDatabase.CreateAsset(mat,path); }
        mat.color=color; EditorUtility.SetDirty(mat); return mat;
    }
    static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material mat)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=size;
        go.GetComponent<Renderer>().sharedMaterial=mat; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
    }
}
#endif
