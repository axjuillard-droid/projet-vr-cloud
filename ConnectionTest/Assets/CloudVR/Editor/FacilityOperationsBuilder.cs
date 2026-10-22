#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Réutilise le local TGBT, l'UPS et les interactions du parcours existant.</summary>
public static class FacilityOperationsBuilder
{
    static GameObject Box(Transform root,string name,Vector3 p,Vector3 size,Material mat,bool solid=false)
    {return P0SpatialBuilder.Box(root,name,p,size,mat,solid);}
    internal static Transform Panel(Transform root,string name,Vector3 p,float yaw,Vector2 size,Material mat)
    {
        var go=new GameObject(name).transform;go.SetParent(root,false);go.localPosition=p;go.localRotation=Quaternion.Euler(0,yaw,0);
        Box(go,"Support",Vector3.zero,new Vector3(size.x,size.y,.10f),mat,true);return go;
    }
    internal static Text Text(Transform root,string name,Vector3 p,Vector2 size,int pixels,int font,string value="")
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Canvas));go.transform.SetParent(root,false);go.transform.localPosition=p;
        go.transform.localScale=Vector3.one*(size.x/pixels);go.GetComponent<RectTransform>().sizeDelta=new Vector2(pixels,pixels*size.y/size.x);go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
        var text=DemoTour.Label(go.transform,"Texte",font,new Vector2(.02f,.02f),new Vector2(.98f,.98f),Color.white);text.text=value;return text;
    }
    static FacilityActionTarget Target(FacilityOperations module,Transform root,FacilityActionTarget.Action action,Vector3 p,Vector3 size)
    {
        var go=new GameObject("P3_Cible_"+action);go.transform.SetParent(root,false);go.transform.localPosition=p;
        var collider=go.AddComponent<BoxCollider>();collider.size=size;collider.isTrigger=true;
        var target=go.AddComponent<FacilityActionTarget>();target.module=module;target.action=action;
        var xr=go.AddComponent<XRSimpleInteractable>();xr.interactionManager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();xr.interactionLayers=1;xr.colliders.Clear();xr.colliders.Add(collider);return target;
    }
    static FacilityActionTarget Button(FacilityOperations module,Transform panel,FacilityActionTarget.Action action,Vector3 p,float width,string label,Material mat)
    {
        var key=new GameObject("P3_Commande_"+action).transform;key.SetParent(panel,false);key.localPosition=p;
        Box(key,"Bouton",Vector3.zero,new Vector3(width,.24f,.08f),mat);
        Text(key,"Légende",new Vector3(0,0,-.05f),new Vector2(width-.04f,.18f),380,24,label);
        return Target(module,key,action,new Vector3(0,0,-.06f),new Vector3(width,.25f,.10f));
    }
    public static void Build(DemoTour tour)
    {
        var root=new GameObject("P3_Exploitation_Et_Supervision").transform;root.SetParent(tour.transform,false);
        var module=root.gameObject.AddComponent<FacilityOperations>();tour.facility=module;module.tour=tour;module.cooling=tour.learning;
        var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/Demo_Dark.mat");
        var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/Demo_Metal.mat");
        module.off=metal;module.warm=tour.warmMaterial;module.cold=tour.coldMaterial;
        var panel=Panel(root,"P3_Pupitre_Electrique",new Vector3(-17.5f,1.55f,30),-90,new Vector2(2.8f,1.55f),dark);
        Box(panel,"Pied",new Vector3(0,-1f,0),new Vector3(.08f,1,.08f),metal,true);
        Box(panel,"Socle",new Vector3(0,-1.5f,0),new Vector3(.8f,.08f,.7f),metal,true);
        module.powerDisplay=Text(panel,"Source_Energie",new Vector3(0,.30f,-.065f),new Vector2(2.65f,.83f),900,29);
        module.powerFeedback=Text(panel,"Consigne",new Vector3(0,-.40f,-.065f),new Vector2(2.65f,.50f),900,23);
        var targets=new List<FacilityActionTarget>();
        targets.Add(Button(module,panel,FacilityActionTarget.Action.StartGenerator,new Vector3(-.92f,-.94f,-.02f),.86f,"OBSERVER GROUPE",tour.coldMaterial));
        targets.Add(Button(module,panel,FacilityActionTarget.Action.Transfer,new Vector3(0,-.94f,-.02f),.86f,"VOIR TRANSFERT",metal));
        targets.Add(Button(module,panel,FacilityActionTarget.Action.Replay,new Vector3(.92f,-.94f,-.02f),.86f,"REJOUER SI ÉPUISÉ",metal));
        module.sourceIndicators=new Renderer[3];
        foreach(int i in new[]{0,1,2})
        {
            var p=new Vector3((i-1)*.85f,.90f,0);
            module.sourceIndicators[i]=Box(panel,"P3_Source_"+i,p,new Vector3(.12f,.12f,.07f),metal).GetComponent<Renderer>();
            Text(panel,"Nom_Source_"+i,p+new Vector3(.25f,0,-.065f),new Vector2(.55f,.13f),240,24,new[]{"SECTEUR","BATTERIE","GROUPE"}[i]);
        }
        var screen=GameObject.Find("UPS_A_Screen_0");if(screen==null)throw new InvalidOperationException("Afficheur UPS original absent.");
        var ups=new GameObject("P3_Observer_UPS").transform;ups.SetParent(root,false);ups.position=screen.GetComponent<Renderer>().bounds.center+Vector3.back*.08f;
        targets.Add(Target(module,ups,FacilityActionTarget.Action.InspectUPS,Vector3.zero,new Vector3(.75f,.45f,.12f)));
        Text(ups,"Observer",new Vector3(0,.38f,-.08f),new Vector2(1.5f,.25f),500,28,"OBSERVER L'UPS / ONDULEUR");
        var board=Panel(root,"P3_Mur_Supervision",new Vector3(0,1.65f,39.8f),0,new Vector2(4.2f,2.15f),dark);
        foreach(int side in new[]{-1,1})Box(board,"Pied",new Vector3(side*1.5f,-1.35f,0),new Vector3(.06f,.6f,.1f),metal,true);
        Text(board,"Titre",new Vector3(0,.87f,-.065f),new Vector2(4,.27f),1200,34,"CLOUDVR / EXPLOITATION • DONNÉES FICTIVES DU PARCOURS");
        module.metricsDisplay=Text(board,"Mesures",new Vector3(-1.36f,.12f,-.065f),new Vector2(1.26f,1.20f),440,23);
        module.eventDisplay=Text(board,"Chronologie",new Vector3(0,.12f,-.065f),new Vector2(1.26f,1.20f),440,21);
        module.limitsDisplay=Text(board,"Indicateurs",new Vector3(1.36f,.12f,-.065f),new Vector2(1.26f,1.20f),440,22);
        module.supervisionFeedback=Text(board,"Rapport",new Vector3(0,-.76f,-.065f),new Vector2(4,.42f),1300,27);
        targets.Add(Button(module,board,FacilityActionTarget.Action.ReadIncident,new Vector3(-1.36f,-1.21f,-.02f),1.22f,"LIRE INCIDENT",metal));
        targets.Add(Button(module,board,FacilityActionTarget.Action.RouteNetwork,new Vector3(0,-1.21f,-.02f),1.22f,"ÉQUIPE RÉSEAU",metal));
        targets.Add(Button(module,board,FacilityActionTarget.Action.RouteEnergy,new Vector3(1.36f,-1.21f,-.02f),1.22f,"ÉQUIPE ÉNERGIE",tour.coldMaterial));
        module.targets=targets.ToArray();
        var sound=new GameObject("P3_Son_Groupe_Exterieur").transform;sound.SetParent(root,false);sound.position=new Vector3(-18,1,6.6f);
        module.generatorAudio=sound.gameObject.AddComponent<AudioSource>();module.generatorAudio.spatialBlend=1;module.generatorAudio.minDistance=4;module.generatorAudio.maxDistance=65;
    }
}
#endif
