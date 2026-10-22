#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Maquettes dans les emplacements libres, hors de l'allée de 2,20 m.</summary>
public static class DataServiceBuilder
{
    static Text Label(Transform p,string name,Vector3 pos,Vector2 size,int font,string text="",int pixels=800)
    {return FacilityOperationsBuilder.Text(p,name,pos,size,pixels,font,text);}
    static GameObject Box(Transform p,string name,Vector3 pos,Vector3 size,Material mat,bool solid=false)
    {return P0SpatialBuilder.Box(p,name,pos,size,mat,solid);}
    static DataServiceTarget Target(DataServiceLearning module,Transform parent,DataServiceTarget.Action action,Vector3 position,Vector3 size)
    {
        var go=new GameObject("Données_Cible_"+action);go.transform.SetParent(parent,false);go.transform.localPosition=position;
        var collider=go.AddComponent<BoxCollider>();collider.size=size;collider.isTrigger=true;
        var target=go.AddComponent<DataServiceTarget>();target.module=module;target.action=action;
        var xr=go.AddComponent<XRSimpleInteractable>();xr.interactionManager=Object.FindFirstObjectByType<XRInteractionManager>();xr.interactionLayers=1;xr.colliders.Clear();xr.colliders.Add(collider);return target;
    }
    static DataServiceTarget Button(DataServiceLearning m,Transform p,DataServiceTarget.Action action,float x,float y,string label,float width=.72f)
    {
        var pos=new Vector3(x,y,-.07f);Box(p,"Commande_"+action,pos,new Vector3(width,.24f,.08f),m.dark);
        Label(p,"Nom_"+action,pos+Vector3.back*.05f,new Vector2(width-.04f,.19f),32,label,400);
        return Target(m,p,action,pos+Vector3.back*.06f,new Vector3(width,.25f,.1f));
    }
    static LineRenderer Path(Transform parent,string name,Vector3[] points,Material material)
    {
        var go=new GameObject(name,typeof(LineRenderer));go.transform.SetParent(parent,false);var line=go.GetComponent<LineRenderer>();
        line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);line.startWidth=line.endWidth=.018f;line.sharedMaterial=material;return line;
    }
    static Transform[] Signals(Transform parent,int count,Material mat)
    {
        var dots=new Transform[count];for(int i=0;i<count;i++)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name="Signal_illustratif_"+i;go.transform.SetParent(parent,false);go.transform.localScale=Vector3.one*.045f;Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;go.SetActive(false);dots[i]=go.transform;}return dots;
    }
    public static void Build(DemoTour tour)
    {
        var root=new GameObject("Donnees_Reseau_Stockage").transform;root.SetParent(tour.transform,false);
        var m=root.gameObject.AddComponent<DataServiceLearning>();tour.data=m;m.tour=tour;m.ready=tour.coldMaterial;m.warning=tour.warmMaterial;m.signal=tour.packetMaterial;
        m.dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/Demo_Metal.mat");var panelMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/Demo_Dark.mat");
        m.networkPanel=FacilityOperationsBuilder.Panel(root,"Maquette_Reseau",new Vector3(2.1f,1.3f,8),90,new Vector2(2.4f,1.7f),panelMat);
        m.storagePanel=FacilityOperationsBuilder.Panel(root,"Maquette_Stockage",new Vector3(2.08f,1.3f,19.4f),90,new Vector2(2.4f,1.7f),panelMat);
        foreach(var panel in new[]{m.networkPanel,m.storagePanel})Box(panel,"Pied",new Vector3(0,-1f,0),new Vector3(.08f,.6f,.08f),m.dark,true);
        m.networkStatus=Label(m.networkPanel,"État_Réseau",new Vector3(0,.62f,-.065f),new Vector2(2.3f,.36f),27);
        m.networkFeedback=Label(m.networkPanel,"Explication_Réseau",new Vector3(0,-.54f,-.065f),new Vector2(2.3f,.50f),24);
        m.storageStatus=Label(m.storagePanel,"État_Stockage",new Vector3(0,.62f,-.065f),new Vector2(2.3f,.36f),25);
        m.storageFeedback=Label(m.storagePanel,"Explication_Stockage",new Vector3(0,-.54f,-.065f),new Vector2(2.3f,.50f),24);
        var targets=new List<DataServiceTarget>();m.networkNodes=new Renderer[3];
        m.networkNodes[0]=Box(m.networkPanel,"Lien_A",new Vector3(0,.28f,-.13f),new Vector3(.50f,.20f,.10f),m.signal).GetComponent<Renderer>();
        m.networkNodes[1]=Box(m.networkPanel,"Lien_B",new Vector3(0,-.08f,-.13f),new Vector3(.50f,.20f,.10f),m.dark).GetComponent<Renderer>();
        m.networkNodes[2]=Box(m.networkPanel,"Service",new Vector3(.94f,.1f,-.13f),new Vector3(.35f,.30f,.1f),m.signal).GetComponent<Renderer>();
        Box(m.networkPanel,"Entrée",new Vector3(-.94f,.1f,-.13f),new Vector3(.35f,.3f,.1f),m.dark);
        foreach(var item in new[]{new Vector3(-.94f,.1f,-.20f),new Vector3(0,.28f,-.20f),new Vector3(0,-.08f,-.20f),new Vector3(.94f,.1f,-.20f)})
            Label(m.networkPanel,"Nom_Nœud",item,new Vector2(item.x==0?.48f:.34f,.18f),32,item.x<0?"ENTRÉE":item.x>0?"SERVICE":item.y>0?"LIEN A":"LIEN B",240);
        targets.Add(Target(m,m.networkPanel,DataServiceTarget.Action.IsolateLink,new Vector3(0,.28f,-.23f),new Vector3(.50f,.20f,.1f)));
        targets.Add(Target(m,m.networkPanel,DataServiceTarget.Action.ReadResponse,new Vector3(.94f,.1f,-.23f),new Vector3(.35f,.30f,.1f)));
        targets.Add(Button(m,m.networkPanel,DataServiceTarget.Action.RestoreLink,0,-1.03f,"RÉTABLIR LIEN A",1.1f));
        m.networkPaths=new LineRenderer[2];
        for(int i=0;i<2;i++){float y=i==0?.28f:-.08f;m.networkPaths[i]=Path(m.networkPanel,"Chemin_"+i,new[]{new Vector3(-.94f,.1f,-.16f),new Vector3(-.35f,y,-.16f),new Vector3(.35f,y,-.16f),new Vector3(.94f,.1f,-.16f)},m.signal);}
        m.storageNodes=new Renderer[3];
        for(int i=0;i<3;i++)
        {
            float x=(i-1)*.80f;m.storageNodes[i]=Box(m.storagePanel,"Volume_"+i,new Vector3(x,.1f,-.13f),new Vector3(.56f,.42f,.1f),m.dark).GetComponent<Renderer>();
            Label(m.storagePanel,"Rôle_"+i,new Vector3(x,.1f,-.20f),new Vector2(.54f,.38f),34,new[]{"A\nPRIMAIRE","B\nRÉPLIQUE","C\nPOINT v1"}[i],300);
            targets.Add(Target(m,m.storagePanel,new[]{DataServiceTarget.Action.FailPrimary,DataServiceTarget.Action.Replicate,DataServiceTarget.Action.RecoveryPoint}[i],new Vector3(x,.1f,-.23f),new Vector3(.56f,.42f,.1f)));
        }
        targets.Add(Button(m,m.storagePanel,DataServiceTarget.Action.ReadPhoto,-.80f,-1.03f,"LIRE PHOTO"));
        targets.Add(Button(m,m.storagePanel,DataServiceTarget.Action.DeleteCurrent,0,-1.03f,"SUPPRESSION\nPROPAGÉE"));
        targets.Add(Button(m,m.storagePanel,DataServiceTarget.Action.RestorePhoto,.80f,-1.03f,"RESTAURER C → B"));
        m.storagePaths=new[]{Path(m.storagePanel,"Copie_A_B",new[]{new Vector3(-.8f,.1f,-.24f),new Vector3(0,.1f,-.24f)},m.signal),
            Path(m.storagePanel,"Point_A_C",new[]{new Vector3(-.8f,.1f,-.24f),new Vector3(-.8f,.38f,-.24f),new Vector3(.8f,.38f,-.24f),new Vector3(.8f,.1f,-.24f)},m.ready),
            Path(m.storagePanel,"Restauration_C_B",new[]{new Vector3(.8f,.1f,-.24f),new Vector3(0,.1f,-.24f)},m.ready)};
        m.networkSignals=Signals(root,5,m.signal);m.storageSignals=Signals(root,4,m.ready);m.targets=targets.ToArray();
    }
}
#endif
