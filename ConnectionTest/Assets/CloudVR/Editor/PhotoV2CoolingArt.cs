#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static partial class PhotoV2BlockoutBuilder
{
    static void InstallCoolingRoute(PhotoV2Walkthrough route)
    {
        var cooling = route.gameObject.AddComponent<PhotoV2Cooling>();
        route.cooling = cooling;
        cooling.energy = route.energy;
        route.energy.cooling = cooling;
        var graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Graphite.mat");
        var aluminum = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Aluminium.mat");
        var cyan = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Cyan.mat");
        var amber = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Ambre.mat");
        var copper = Palette("V2 Retour cuivre",new(.48f,.23f,.12f));
        var steelBlue = Palette("V2 Départ acier bleui",new(.16f,.32f,.44f));

        cooling.gate = Box("Accès au rejet de chaleur",new(0,3.98f,86.69f),new(3.22f,1.15f,.16f),graphite);
        Box("Passerelle du refroidissement",new(0,3.25f,92.83f),new(3.2f,.30f,12.48f),_neutral);
        foreach(int side in new[]{-1,1})
        {
            Box("Lisse de passerelle thermique",new(side*1.57f,4.42f,92.73f),new(.08f,.09f,12.55f),aluminum);
            Box("Protection basse thermique",new(side*1.57f,3.67f,92.73f),new(.08f,.53f,12.55f),graphite);
        }
        cooling.guide = GridGuide(87.1f,97.4f,cyan);
        cooling.observation = GridStop("ÉCHANGEUR · REJET À L’AIR",new(-1.06f,4.61f,97.5f),graphite,aluminum);

        Box("Support de vanne",new(-1.12f,3.97f,85.57f),new(.17f,1.14f,.17f),aluminum);
        cooling.wheel = new GameObject("Volant de vanne").transform;
        cooling.wheel.position = new Vector3(-1.12f,4.65f,85.47f);
        var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hub.name = "Moyeu du volant";
        hub.transform.SetParent(cooling.wheel,false);
        hub.transform.localRotation = Quaternion.Euler(90,0,0);
        hub.transform.localScale = new Vector3(.14f,.044f,.14f);
        hub.GetComponent<Renderer>().sharedMaterial = amber;
        hub.GetComponent<Collider>().enabled = false;
        for(int j=0;j<8;j++)
        {
            float angle = j*45;
            var segment = Box("Jante de vanne",cooling.wheel.position + Quaternion.Euler(0,0,angle)*Vector3.up*.185f,
                new(.158f,.032f,.042f),amber);
            segment.transform.rotation = Quaternion.Euler(0,0,angle);
            segment.transform.SetParent(cooling.wheel,true);
            segment.GetComponent<Collider>().enabled = false;
        }
        var spoke = Box("Traverse du volant",cooling.wheel.position,new(.32f,.044f,.035f),amber);
        spoke.transform.SetParent(cooling.wheel,true);
        spoke.GetComponent<Collider>().enabled = false;
        var valveHit = new GameObject("Cible de la vanne").AddComponent<BoxCollider>();
        valveHit.transform.position = cooling.wheel.position + Vector3.back*.03f;
        valveHit.size = new Vector3(.42f,.42f,.14f);
        cooling.valve = valveHit;
        Sign("VANNE · RENFORT DE DÉBIT",new(-1.12f,4.94f,85.38f),.046f,Color.white);
        cooling.status = Sign("",new(-1.12f,5.12f,85.39f),.043f,Color.white);

        var hot = new[]{new Vector3(-7.1f,2.6f,34.75f),new Vector3(-3.1f,2.6f,34.75f),new Vector3(-4.0f,4.56f,36),
            new Vector3(-4.0f,4.56f,84),new Vector3(-4.0f,4.56f,95.2f),new Vector3(-6.25f,2.13f,95.2f)};
        var cold = new[]{new Vector3(-6.25f,1.55f,96.2f),new Vector3(-4.62f,4.06f,96.2f),
            new Vector3(-4.62f,4.06f,84),new Vector3(-4.62f,4.06f,36),new Vector3(-3.3f,2.6f,34.45f),new Vector3(-7.1f,2.6f,34.45f)};
        CoolTube(hot[1],new Vector3(7.1f,2.6f,34.75f),.12f,copper);
        CoolTube(cold[cold.Length-2],new Vector3(7.1f,2.6f,34.45f),.12f,steelBlue);
        for (int row=0; row<6; row++)
        {
            float rowX = (row-2.5f)*2.6f;
            CoolTube(new Vector3(rowX,2.6f,34.75f),new Vector3(rowX,2.14f,34.75f),.075f,copper);
            CoolTube(new Vector3(rowX,2.6f,34.45f),new Vector3(rowX,2.14f,34.45f),.075f,steelBlue);
        }
        cooling.hotPath = CoolPipe("Retour chaud",hot,copper,amber);
        cooling.coldPath = CoolPipe("Départ froid",cold,steelBlue,cyan);
        cooling.hotMarkers = CoolMarkers("Repères chauds",amber);
        cooling.coldMarkers = CoolMarkers("Repères froids",cyan);
        for(float z=44;z<=92;z+=8)
        {
            Box("Support de conduites",new(-4.3f,2.05f,z),new(.15f,4.12f,.16f),aluminum);
            Box("Traverse de conduites",new(-4.30f,3.85f,z),new(1.12f,.11f,.13f),aluminum);
        }

        Box("Dalle thermique",new(-7.9f,.025f,96.4f),new(7.3f,.10f,7.0f),_neutral);
        Box("Échangeur à plaques · corps",new(-6.32f,1.46f,95.68f),new(1.38f,2.82f,1.31f),graphite);
        for(int i=0;i<10;i++)
            Box("Empilement de plaques",new(-5.61f,1.46f,95.12f+i*.118f),new(.13f,2.14f,.056f),aluminum);
        Sign("ÉCHANGEUR",new(-6.32f,2.86f,95),.105f,Color.white);
        Box("Aéroréfrigérant sec",new(-9.0f,1.30f,96.9f),new(3.18f,2.5f,3.05f),graphite);
        for(int i=0;i<14;i++)
            Box("Ailette du rejet sec",new(-7.37f,1.45f,95.57f+i*.199f),new(.11f,1.64f,.055f),aluminum);
        Sign("REJET SEC À L’AIR",new(-9.0f,2.73f,95.32f),.105f,Color.white);
        CoolTube(new Vector3(-7.0f,1.91f,95.42f),new Vector3(-7.74f,1.91f,95.72f),.11f,copper);
        CoolTube(new Vector3(-7.0f,1.39f,96.06f),new Vector3(-7.74f,1.39f,96.35f),.11f,steelBlue);
        cooling.fans = new Transform[2];
        cooling.airMarkers = new Transform[4];
        for(int i=0;i<2;i++)
        {
            var fan = new GameObject("Ventilateur du rejet sec").transform;
            fan.position = new Vector3(-9,2.64f,96.15f+i*1.47f);
            cooling.fans[i] = fan;
            for(int j=0;j<5;j++)
            {
                float angle=j*72;
                var blade=Box("Pale",fan.position+Quaternion.Euler(0,angle,0)*Vector3.forward*.34f,
                    new(.16f,.043f,.45f),aluminum);
                blade.transform.rotation=Quaternion.Euler(0,angle,0);
                blade.transform.SetParent(fan,true);
                blade.GetComponent<Collider>().enabled=false;
            }
            for(int j=0;j<2;j++)
            {
                var origin = new GameObject("Origine du repère de chaleur").transform;
                origin.position=fan.position+new Vector3(j*.56f-.28f,.19f,0);
                var air = Box("Chaleur transférée à l’air",origin.position,new(.11f,.45f,.11f),amber);
                air.GetComponent<Collider>().enabled=false;
                air.transform.SetParent(origin,true);
                air.SetActive(false);
                cooling.airMarkers[i*2+j] = air.transform;
            }
        }
    }

    static Transform[] CoolMarkers(string name,Material material)
    {
        var markers = new Transform[16];
        for(int i=0;i<markers.Length;i++)
        {
            var marker = Box(name,Vector3.zero,new(.22f,.22f,.22f),material);
            marker.GetComponent<Collider>().enabled=false;
            marker.SetActive(false);
            markers[i]=marker.transform;
        }
        return markers;
    }

    static LineRenderer CoolPipe(string name,Vector3[] points,Material metal,Material signal)
    {
        var line=PatchLine("Visualisation · "+name,signal,.042f,points.Length);
        var visual=new Vector3[points.Length];
        for(int i=0;i<points.Length;i++)
        {
            visual[i]=points[i]+Vector3.up*.21f;
            if(i>0) CoolTube(points[i-1],points[i],.12f,metal);
        }
        line.SetPositions(visual);
        line.enabled=false;
        return line;
    }

    static void CoolTube(Vector3 start,Vector3 end,float radius,Material material)
    {
        var pipe=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pipe.name="Conduite de refroidissement";
        pipe.transform.position=(start+end)*.5f;
        pipe.transform.rotation=Quaternion.FromToRotation(Vector3.up,end-start);
        pipe.transform.localScale=new Vector3(radius*2,Vector3.Distance(start,end)*.5f,radius*2);
        pipe.GetComponent<Renderer>().sharedMaterial=material;
        pipe.GetComponent<Collider>().enabled=false;
    }
}
#endif
