#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Réutilise le serveur 2U et la porte existants ; construit leurs accès et le diagnostic d'air.</summary>
public static class P1LearningBuilder
{
    static Material Material(string name) { return AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/"+name+".mat"); }
    static GameObject Box(Transform root,string name,Vector3 p,Vector3 s,Material mat,bool solid=false)
    { return P0SpatialBuilder.Box(root,name,p,s,mat,solid); }
    public static void Build(DemoTour tour)
    {
        var module=new GameObject("P1_Maintenance_Et_Air").AddComponent<P1LearningModule>();
        module.transform.SetParent(tour.transform,false);tour.learning=module;module.tour=tour;
        module.cold=tour.coldMaterial;module.warm=tour.warmMaterial;
        foreach(var old in tour.thermal)old.gameObject.SetActive(false);
        // Témoins sur les façades, à la place des grands aplats flottants hérités de la démo.
        module.temperatureMarkers=new Renderer[2];
        for(int i=0;i<2;i++)module.temperatureMarkers[i]=Box(module.transform,"P1_Témoin_Entrée_Air",new Vector3(i==0?-1.065f:1.065f,1.5f,22.4f),new Vector3(.02f,.22f,.06f),tour.coldMaterial).GetComponent<Renderer>();
        var rack=GameObject.Find("Baie_01").GetComponent<ServerBladeInteraction>();
        if(rack==null||rack.serverBlade==null)throw new InvalidOperationException("Serveur détaillé requis pour P1.");
        module.rack=rack;rack.hardwareLabels=null; // Les repères et explications sont portés par le module.
        foreach(var item in new[]{rack.rackDoor.Find("P1_Poignee_Quart_Tour"),rack.serverBlade.Find("P1_Prise_Tiroir"),rack.serverBlade.Find("P1_Repères_Composants")})
            if(item!=null)UnityEngine.Object.DestroyImmediate(item.gameObject);
        var oldLabels=rack.serverBlade.Find("Hardware_Labels");if(oldLabels!=null)oldLabels.gameObject.SetActive(false);
        foreach(string name in new[]{"P1_Cadre_Baie","P1_Afficheur_Baie"})
        {var old=rack.transform.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);}
        // Le décor initial fusionne des façades pleines : le remplacer dans cette seule baie par un cadre ouvert.
        var original=rack.transform.Find("Modele_Original");if(original!=null)original.gameObject.SetActive(false);
        var body=rack.GetComponent<BoxCollider>();body.center=new Vector3(0,1.1f,.42f);body.size=new Vector3(1.1f,2.2f,.12f);
        var frame=new GameObject("P1_Cadre_Baie").transform;frame.SetParent(rack.transform,false);
        var shell=Material("Rack_Shell");var steel=Material("Rack_Metal");var dark=Material("Demo_Dark");
        foreach(int side in new[]{-1,1})Box(frame,"Montant",new Vector3(side*.51f,1.1f,0),new Vector3(.08f,2.2f,1),shell,true);
        Box(frame,"Toit",new Vector3(0,2.16f,0),new Vector3(1.1f,.08f,1),shell,true);
        Box(frame,"Socle",new Vector3(0,.05f,0),new Vector3(1.1f,.1f,1),shell,true);
        foreach(float y in new[]{.3f,.52f,.74f,.96f,1.62f,1.84f})
            Box(frame,"Panneau_Obturateur",new Vector3(0,y,-.45f),new Vector3(.92f,.18f,.03f),shell);
        foreach(int side in new[]{-1,1})
            Box(frame,"Rail_Fixe_2U",new Vector3(side*.23f,1.28f,-.05f),new Vector3(.012f,.05f,.85f),steel);
        var doorMesh=Array.Find(rack.rackDoor.GetComponentsInChildren<MeshRenderer>(),r=>r.name.Contains("Door_Mesh"));
        P0SpatialBuilder.Glass(doorMesh.sharedMaterial,.18f);
        // L'ancien groupe Handle fusionne poignée et charnières ; éviter une seconde poignée immobile.
        var oldHandle=Array.Find(rack.rackDoor.GetComponentsInChildren<MeshRenderer>(),r=>r.name.Contains("Door_Handle"));oldHandle.enabled=false;
        foreach(float y in new[]{.25f,1f,1.75f})Box(rack.rackDoor,"Axe_Charniere",new Vector3(0,y,-.03f),new Vector3(.04f,.07f,.04f),steel);
        var handlePivot=new GameObject("P1_Poignee_Quart_Tour").transform;handlePivot.SetParent(rack.rackDoor,false);
        handlePivot.localPosition=new Vector3(.87f,1.02f,-.075f);rack.handle=handlePivot;
        Box(handlePivot,"Levier",Vector3.zero,new Vector3(.025f,.18f,.045f),steel);
        var list=new List<P1ActionTarget>();
        list.Add(Target(module,handlePivot,P1ActionTarget.Action.Door,Vector3.zero,new Vector3(.18f,.26f,.12f)));
        var drawer=new GameObject("P1_Prise_Tiroir").transform;drawer.SetParent(rack.serverBlade,false);
        drawer.localPosition=new Vector3(0,.08f,.78f);
        list.Add(Target(module,drawer,P1ActionTarget.Action.Drawer,Vector3.zero,new Vector3(.38f,.12f,.08f)));
        var tray=rack.serverBlade.gameObject.GetComponent<BoxCollider>();if(tray==null)tray=rack.serverBlade.gameObject.AddComponent<BoxCollider>();
        tray.center=new Vector3(0,.045f,.37f);tray.size=new Vector3(.48f,.09f,.74f);
        module.inspectionTargets=new GameObject("P1_Repères_Composants");module.inspectionTargets.transform.SetParent(rack.serverBlade,false);
        var points=new[]{new Vector3(0,.20f,.37f),new Vector3(-.16f,.16f,.38f),new Vector3(0,.14f,.74f)};
        var labels=new[]{"CPU","RAM","NVMe"};
        module.inspectionMarkers=new Renderer[3];
        for(int i=0;i<3;i++)
        {
            var marker=Box(module.inspectionTargets.transform,"Repère_"+labels[i],points[i],new Vector3(.105f,.025f,.08f),tour.packetMaterial).transform;
            module.inspectionMarkers[i]=marker.GetComponent<Renderer>();
            // Éviter l'échelle du cube pour le collider et le texte.
            var target=new GameObject("P1_Inspection_"+labels[i]).transform;target.SetParent(module.inspectionTargets.transform,false);target.localPosition=points[i];
            list.Add(Target(module,target,(P1ActionTarget.Action)(i+2),Vector3.zero,new Vector3(.12f,.055f,.10f)));
            var label=Display(target,"Nom",new Vector3(0,.055f,0),new Vector2(.18f,.07f),Quaternion.Euler(55,180,0),120,30);
            label.text=labels[i];label.color=Color.cyan;
        }
        module.inspectionTargets.SetActive(false);
        var rackPanel=Panel(rack.transform,"P1_Afficheur_Baie",new Vector3(0,2.65f,-.55f),Quaternion.identity,new Vector2(1.45f,.76f),dark);
        Box(frame,"Support_Afficheur",new Vector3(0,2.25f,-.5f),new Vector3(.05f,.20f,.05f),steel);
        module.rackDisplay=Display(rackPanel,"Instructions",new Vector3(0,0,-.065f),new Vector2(1.35f,.68f),Quaternion.identity,680,27);

        var exit=GameObject.Find("CAC_Sortie").GetComponent<ProximitySlidingDoor>();module.containment=exit;
        var oldLatch=exit.right.Find("P1_Fermer_Confinement");if(oldLatch!=null)UnityEngine.Object.DestroyImmediate(oldLatch.gameObject);
        var control=Panel(module.transform,"P1_Diagnostic_Air",new Vector3(1.9f,1.55f,24.25f),Quaternion.Euler(0,180,0),new Vector2(1.45f,1.05f),dark);
        module.thermalDisplay=Display(control,"Mesures",new Vector3(0,.18f,-.065f),new Vector2(1.35f,.64f),Quaternion.identity,600,30);
        module.feedback=Display(control,"Diagnostic",new Vector3(0,-.33f,-.065f),new Vector2(1.35f,.30f),Quaternion.identity,600,24);
        module.feedback.text="Borne 04 : diagnostic d'une fuite de confinement.\nLes valeurs affichées sont fictives.";
        Box(control,"Pied",new Vector3(0,-1f,0),new Vector3(.06f,1f,.06f),steel);
        Box(control,"Socle",new Vector3(0,-1.5f,0),new Vector3(.55f,.08f,.4f),steel);
        foreach(int i in new[]{0,1})
        {
            var key=Box(control,i==0?"P1_Acquitter":"P1_Ventilation_Max",new Vector3(i==0?-.36f:.36f,-.68f,-.03f),new Vector3(.64f,.20f,.10f),i==0?steel:tour.warmMaterial).transform;
            // Racine sans échelle pour XRI et affichage.
            var target=new GameObject(i==0?"Action_Acquitter":"Action_Boost").transform;target.SetParent(control,false);target.localPosition=new Vector3(i==0?-.36f:.36f,-.68f,-.10f);
            list.Add(Target(module,target,i==0?P1ActionTarget.Action.ResetAlarm:P1ActionTarget.Action.BoostFans,Vector3.zero,new Vector3(.64f,.20f,.10f)));
            Display(target,"Libellé",new Vector3(0,0,-.06f),new Vector2(.6f,.12f),Quaternion.identity,300,30).text=i==0?"ACQUITTER":"VENTILER +";
        }
        var latch=new GameObject("P1_Fermer_Confinement").transform;
        latch.position=exit.right.position+new Vector3(0,.1f,.10f);latch.rotation=Quaternion.Euler(0,180,0);latch.SetParent(exit.right,true);
        Box(latch,"Poignée",Vector3.zero,new Vector3(.06f,.23f,.04f),tour.coldMaterial);
        list.Add(Target(module,latch,P1ActionTarget.Action.SealContainment,Vector3.zero,new Vector3(.40f,.34f,.12f)));
        Display(latch,"Fermer",new Vector3(0,.28f,-.07f),new Vector2(.60f,.20f),Quaternion.identity,300,27).text="FERMER\nCONFINEMENT";
        module.targets=list.ToArray();
        module.thermalVisuals=new GameObject("P1_Circulation_Air");module.thermalVisuals.transform.SetParent(module.transform,false);
        var markers=new List<Transform>();
        MakeFlow(module.thermalVisuals.transform,"Air_Froid",new[]{new Vector3(.55f,.15f,22.7f),new Vector3(.55f,1.1f,22.7f),new Vector3(1.1f,1.1f,22.7f)},tour.coldMaterial,markers);
        module.leakPath=MakeFlow(module.thermalVisuals.transform,"Fuite_Air",new[]{new Vector3(2.5f,2.5f,24.7f),new Vector3(0,1.5f,23.9f),new Vector3(0,1.2f,22.7f)},tour.warmMaterial,markers);
        module.airMarkers=markers.ToArray();module.thermalVisuals.SetActive(false);
        module.fan=new GameObject("P1_Ventilation_Témoin").transform;module.fan.SetParent(control,false);module.fan.localPosition=new Vector3(.86f,.3f,-.06f);
        for(int i=0;i<3;i++)
        {var blade=Box(module.fan,"Pale",new Vector3(0,.08f,0),new Vector3(.04f,.17f,.025f),steel);blade.transform.RotateAround(module.fan.position,module.fan.forward,i*120);}
        EditorUtility.SetDirty(rack);
    }
    static P1ActionTarget Target(P1LearningModule module,Transform parent,P1ActionTarget.Action action,Vector3 position,Vector3 size)
    {
        var go=new GameObject("Cible_"+action);go.transform.SetParent(parent,false);go.transform.localPosition=position;
        var c=go.AddComponent<BoxCollider>();c.size=size;c.isTrigger=true;
        var t=go.AddComponent<P1ActionTarget>();t.module=module;t.action=action;
        var xr=go.AddComponent<XRSimpleInteractable>();xr.interactionManager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();xr.interactionLayers=1;xr.colliders.Clear();xr.colliders.Add(c);return t;
    }
    static Transform Panel(Transform parent,string name,Vector3 position,Quaternion rotation,Vector2 size,Material mat)
    {
        var root=new GameObject(name).transform;root.SetParent(parent,false);root.localPosition=position;root.localRotation=rotation;
        Box(root,"Support",Vector3.zero,new Vector3(size.x,size.y,.1f),mat);return root;
    }
    static Text Display(Transform parent,string name,Vector3 position,Vector2 size,Quaternion rotation,int width,int fontSize)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Canvas));go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localRotation=rotation;
        go.transform.localScale=Vector3.one*(size.x/width);go.GetComponent<RectTransform>().sizeDelta=new Vector2(width,width*size.y/size.x);go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
        return DemoTour.Label(go.transform,"Texte",fontSize,new Vector2(.02f,.02f),new Vector2(.98f,.98f),Color.white);
    }
    static LineRenderer MakeFlow(Transform parent,string name,Vector3[] points,Material mat,List<Transform> markers)
    {
        var root=new GameObject(name).transform;root.SetParent(parent,false);var line=root.gameObject.AddComponent<LineRenderer>();line.sharedMaterial=mat;line.positionCount=points.Length;line.SetPositions(points);line.startWidth=line.endWidth=.025f;
        for(int i=0;i<3;i++)markers.Add(Box(root,"Sens_Air",points[0],new Vector3(.08f,.08f,.12f),mat).transform);return line;
    }
}
#endif
