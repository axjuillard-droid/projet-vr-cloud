#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Implantation P0 : 31 baies de décor + Baie_01, confinement et accès technique.</summary>
public static class P0SpatialBuilder
{
    public static void Glass(Material mat, float alpha)
    {
        var c = mat.color; c.a = alpha; mat.color = c;
        mat.SetFloat("_Surface", 1); mat.SetFloat("_ZWrite", 0);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType", "Transparent"); mat.renderQueue = 3000;
        EditorUtility.SetDirty(mat);
    }

    public static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Material mat, bool solid = true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent, false);
        go.transform.localPosition = position; go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!solid) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    public static void Door(Transform parent, string name, Vector3 position, float yaw, float width, float height, Material glass, Material frame)
    {
        var root = new GameObject(name).transform; root.SetParent(parent, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        var door = root.gameObject.AddComponent<ProximitySlidingDoor>(); door.halfTravel = width * .52f;
        door.left = Box(root, "Vantail_G", new Vector3(-width/4, height/2, 0), new Vector3(width/2-.01f,height,.035f),glass).transform;
        door.right = Box(root, "Vantail_D", new Vector3(width/4, height/2, 0), new Vector3(width/2-.01f,height,.035f),glass).transform;
        Box(root,"Traverse",new Vector3(0,height+.04f,0),new Vector3(width+.12f,.08f,.10f),frame);
    }

    public static void RackRows(GameObject hall, Transform architecture)
    {
        var renderers = hall.GetComponentsInChildren<MeshRenderer>();
        float[] z = {9.8f,10.4f,11,11.6f,12.2f,12.8f,15.2f,15.8f,16.4f,17,17.6f,21.2f,21.8f,22.4f,23,23.6f};
        var groups = new Dictionary<Material,List<CombineInstance>>();
        for (int i=0;i<32;i++)
        {
            string prefix="Rack_"+(i/4)+"_"+(i%4)+"_";
            var body=Array.Find(renderers,r=>r.name==prefix+"Body");
            var front=Array.Find(renderers,r=>r.name==prefix+"Door");
            if(body==null||front==null) throw new InvalidOperationException("Baie manquante : "+prefix);
            int bank=i%2, slot=i/2;
            var center=body.bounds.center;
            bool turn=(front.bounds.center.x>center.x)!=(bank==0);
            float extent=0;
            foreach(var part in renderers) if(part.name.StartsWith(prefix))
                extent=Mathf.Max(extent,Mathf.Abs(part.bounds.center.x-center.x)+part.bounds.extents.x);
            var target=new Vector3((bank==0?-1:1)*(1.1f+extent),center.y,z[slot]);
            var matrix=Matrix4x4.Translate(target)*Matrix4x4.Rotate(Quaternion.Euler(0,turn?180:0,0))*Matrix4x4.Translate(-center);
            bool reserved=bank==1&&slot==6; // emplacement remplacé par la véritable Baie_01 à Z=14
            foreach(var part in renderers) if(part.name.StartsWith(prefix))
            {
                if(!reserved)
                {
                    var mat=part.sharedMaterial;
                    if(!groups.ContainsKey(mat)) groups[mat]=new List<CombineInstance>();
                    groups[mat].Add(new CombineInstance {mesh=part.GetComponent<MeshFilter>().sharedMesh,
                        transform=architecture.worldToLocalMatrix*matrix*part.transform.localToWorldMatrix});
                }
                part.gameObject.SetActive(false);
            }
            if(!reserved)
            {
                var collision=Box(architecture,"Baie_Fixe_"+i,target,body.bounds.size,body.sharedMaterial);
                collision.GetComponent<Renderer>().enabled=false;
            }
        }
        foreach(var pair in groups)
        {
            string path="Assets/CloudVR/Models/P0_Racks_"+pair.Key.name+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=new Mesh{name="Rangées_"+pair.Key.name};AssetDatabase.CreateAsset(mesh,path);}
            mesh.Clear(); mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(pair.Value.ToArray(),true,true); EditorUtility.SetDirty(mesh);
            var go=new GameObject("Rangées_"+pair.Key.name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(architecture,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial=pair.Key;
        }
        // Les anciennes descentes sont remplacées par des alimentations hors du chemin public.
        foreach(var part in renderers) if(part.name.StartsWith("PDU_Drop_"))part.gameObject.SetActive(false);
    }

    public static void Containment(Transform root, Material glass, Material frame)
    {
        // Façades à ±1,1 m ; toit à 2,2 m, imposte sur les baies de décor de 2 m.
        Box(root,"CAC_Toit",new Vector3(0,2.23f,16.7f),new Vector3(2.4f,.06f,14.4f),glass,false);
        for(int side=-1;side<=1;side+=2)
        {
            Box(root,"CAC_Rail",new Vector3(side*1.15f,2.24f,16.7f),new Vector3(.06f,.08f,14.4f),frame,false);
            Box(root,"CAC_Imposte",new Vector3(side*1.1f,2.1f,16.7f),new Vector3(.04f,.2f,14.4f),glass,false);
            foreach(float zs in new[]{14f,19.4f})
            {
                float length=zs==14?1.8f:3.0f;
                // Niches de consultation fermées sur l'arrière, avec retours et toit raccordés.
                Box(root,"CAC_Niche_Vitre",new Vector3(side*2.25f,1.1f,zs),new Vector3(.04f,2.2f,length),glass);
                Box(root,"CAC_Niche_Toit",new Vector3(side*1.65f,2.23f,zs),new Vector3(1.2f,.06f,length),glass,false);
                foreach(int end in new[]{-1,1})
                    Box(root,"CAC_Niche_Retour",new Vector3(side*1.65f,1.1f,zs+end*length/2),new Vector3(1.2f,2.2f,.04f),glass);
            }
        }
        // La baie interactive occupe la niche droite de Z=14 : éviter une vitre devant sa porte.
        foreach(var t in root.GetComponentsInChildren<Transform>())
            if(t.name=="CAC_Niche_Vitre"&&t.localPosition.x>0&&Mathf.Abs(t.localPosition.z-14)<.1f)t.gameObject.SetActive(false);
        Box(root,"CAC_Raccord_Baie01_A",new Vector3(1.1f,1.1f,13.27f),new Vector3(.04f,2.2f,.34f),glass);
        Box(root,"CAC_Raccord_Baie01_B",new Vector3(1.1f,1.1f,15.03f),new Vector3(.04f,2.2f,.94f),glass);
        foreach(float edge in new[]{13.445f,14.555f})
            Box(root,"CAC_Retour_Baie01",new Vector3(1.14f,1.1f,edge),new Vector3(.12f,2.2f,.02f),glass);
        Door(root,"CAC_Entree",new Vector3(0,0,9.5f),0,2.2f,2.2f,glass,frame);
        Door(root,"CAC_Sortie",new Vector3(0,0,23.9f),0,2.2f,2.2f,glass,frame);
    }

    public static void MantrapVestibule(Transform root,Material wall)
    {
        // Raccorder le sas compact aux anciennes ouvertures de 4,4 m, sans passage latéral.
        foreach(int side in new[]{-1,1})
        {
            foreach(float z in new[]{-4f,0f})
                Box(root,"Sas_Raccord_Facade",new Vector3(side*1.4f,1.6f,z),new Vector3(1.6f,3.2f,.16f),wall);
            foreach(float z in new[]{-3.46f,-.54f})
                Box(root,"Sas_Joue_Acces",new Vector3(side*.59f,1.3f,z),new Vector3(.08f,2.6f,1.05f),wall);
        }
    }

    public static void Electrical(Transform root, GameObject room, Material wall, Material glass, Material metal)
    {
        room.transform.localPosition=new Vector3(-19,0,30);
        PrefabUtility.UnpackPrefabInstance(room,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var equipment=room.GetComponentsInChildren<MeshRenderer>();
        // Le FBX avait les afficheurs tournés vers les murs : les orienter vers la circulation.
        foreach(string side in new[]{"A","B"})
        {
            for(int i=0;i<6;i++)TurnEquipment(equipment,"TGBT_"+side+"_Cabinet_"+i,"TGBT_"+side+"_",i);
            for(int i=0;i<4;i++)TurnEquipment(equipment,"UPS_"+side+"_"+i,"UPS_"+side+"_",i);
        }
        for(int i=0;i<8;i++)TurnEquipment(equipment,"Batt_Rack_"+i,"Batt_",i);
        foreach(var r in equipment)
        {
            var source=r.sharedMaterial;
            var mat=ChambreArtBuilder.GetOrCreateMaterial(source!=null?source.name:"Elec_Panel",source!=null?source.color:new Color(.2f,.24f,.28f),.2f,.3f);
            r.sharedMaterial=mat;
            if(r.name=="Elec_Floor"||r.name.Contains("Cabinet_")||r.name.StartsWith("Batt_Rack_")||
                (r.name.StartsWith("UPS_")&&!r.name.Contains("Front")&&!r.name.Contains("Screen")&&!r.name.Contains("LED")))
                if(r.GetComponent<Collider>()==null)r.gameObject.AddComponent<BoxCollider>();
        }
        // Remplacer le mur ouest par une cloison avec porte à Z=30 et baie vitrée à Z=33.
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>())
            if(r.name.StartsWith("WS_Wall_")&&r.bounds.center.x < -8)r.gameObject.SetActive(false);
        Box(root,"Paroi_Ouest_A",new Vector3(-9.1f,2.1f,14.5f),new Vector3(.2f,4.2f,29),wall);
        Box(root,"Paroi_Ouest_B",new Vector3(-9.1f,2.1f,37.5f),new Vector3(.2f,4.2f,5),wall);
        Box(root,"Porte_TGBT_Linteau",new Vector3(-9.1f,3.35f,30),new Vector3(.2f,1.7f,2),wall);
        Box(root,"Observation_TGBT_Vitre",new Vector3(-9.1f,1.65f,33),new Vector3(.05f,1.8f,4),glass);
        Box(root,"Observation_TGBT_Allege",new Vector3(-9.1f,.375f,33),new Vector3(.2f,.75f,4),wall);
        Box(root,"Observation_TGBT_Haut",new Vector3(-9.1f,3.375f,33),new Vector3(.2f,1.65f,4),wall);
        Door(root,"Acces_TGBT",new Vector3(-9.1f,0,30),90,2,2.5f,glass,metal);
        Box(root,"TGBT_Paroi_Fond",new Vector3(-29,1.6f,30),new Vector3(.15f,3.2f,12),wall);
        foreach(int side in new[]{-1,1})Box(root,"TGBT_Paroi_Laterale",new Vector3(-19,1.6f,30+side*6),new Vector3(20,3.2f,.15f),wall);
        Box(root,"TGBT_Plafond",new Vector3(-19,3.25f,30),new Vector3(20,.1f,12),wall);
        for(int x=-26;x<-10;x+=5)
        {
            var light=new GameObject("Eclairage_TGBT").AddComponent<Light>();light.transform.SetParent(root,false);
            light.transform.localPosition=new Vector3(x,2.8f,30);light.type=LightType.Point;light.range=7;light.intensity=1.4f;
        }
        Sign(root,"Signal_TGBT",new Vector3(-8.94f,2.8f,30),-90,"LOCAL ÉLECTRIQUE\nPorte automatique • observation accompagnée",1.7f);
    }

    static void TurnEquipment(MeshRenderer[] parts,string bodyName,string prefix,int index)
    {
        var body=Array.Find(parts,r=>r.name==bodyName);if(body==null)return;
        var center=body.bounds.center;
        foreach(var r in parts)if(r.name.StartsWith(prefix)&&r.name.EndsWith("_"+index))
            r.transform.RotateAround(center,Vector3.up,180);
    }

    public static void Sign(Transform parent,string name,Vector3 position,float yaw,string text,float width)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Canvas));go.transform.SetParent(parent,false);
        go.transform.localPosition=position;go.transform.localRotation=Quaternion.Euler(0,yaw,0);
        go.transform.localScale=Vector3.one*(width/800);go.GetComponent<RectTransform>().sizeDelta=new Vector2(800,220);
        go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
        var label=DemoTour.Label(go.transform,"Information",38,Vector2.zero,Vector2.one,Color.cyan);label.text=text;label.alignment=TextAnchor.MiddleCenter;
    }
}
#endif
