#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class PhotoV2BlockoutBuilder
{
    static void InstallGridRoute(PhotoV2Walkthrough route)
    {
        var grid = route.gameObject.AddComponent<PhotoV2GridRoute>();
        route.grid = grid;
        var graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Graphite.mat");
        var aluminum = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Aluminium.mat");
        var cyan = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Cyan.mat");
        var amber = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Ambre.mat");
        var terrain = Palette("V2 Terrain du site",new(.17f,.24f,.20f));

        Box("Prairie du réseau",new(0,-.31f,109),new(166,.60f,106),terrain);
        Box("Passerelle de visite",new(0,3.25f,73.8f),new(3.2f,.30f,25.9f),_neutral);
        foreach(int side in new[]{-1,1})
        {
            Box("Garde-corps sortie belvédère",new(side*2.49f,3.98f,60.9f),new(1.92f,1.15f,.20f),graphite);
            Box("Lisse de passerelle",new(side*1.57f,4.42f,73.92f),new(.08f,.09f,25.7f),aluminum);
            Box("Protection basse",new(side*1.57f,3.67f,73.92f),new(.08f,.53f,25.7f),graphite);
            for(int j=0;j<8;j++)
                Box("Montant de passerelle",new(side*1.57f,3.94f,61.25f+j*3.45f),new(.075f,1.10f,.075f),graphite);
        }
        grid.exitGate = Box("Porte du parcours extérieur",new(0,4.44f,60.91f),new(3.08f,2.08f,.15f),graphite);
        grid.departure = GridStop("VERS LE RÉSEAU",new(-1.04f,4.61f,60.65f),graphite,aluminum);
        grid.substation = GridStop("POSTE ÉLECTRIQUE",new(1.11f,4.62f,69.9f),graphite,aluminum);
        grid.viewpoint = GridStop("CENTRALE · POINT DE VUE",new(1.11f,4.62f,84.9f),graphite,aluminum);
        grid.guides = new[]{ GridGuide(61.7f,69.2f,cyan),GridGuide(70.5f,84.2f,cyan) };

        Box("Dalle du poste",new(8.0f,.025f,71.5f),new(9.5f,.09f,12.4f),_neutral);
        for (int i=0; i<3; i++)
        {
            float z = 68.2f + i*3.05f;
            Box("Transformateur de démonstration",new(7.5f,1.15f,z),new(2.8f,2.25f,1.65f),graphite);
            for (int j=0; j<8; j++)
                Box("Ailette",new(6.01f,1.17f,z-.66f+.18f*j),new(.27f,1.67f,.055f),aluminum);
            for(int j=0;j<3;j++)
            {
                float x = 6.65f+j*.82f;
                GridCylinder("Isolateur",new(x,2.64f,z),new(.15f,.41f,.15f),_neutral);
                Box("Tête de borne",new(x,3.06f,z),new(.23f,.07f,.23f),aluminum);
            }
        }
        Sign("POSTE DE RACCORDEMENT",new(6.6f,3.65f,66.4f),.16f,Color.white);
        foreach (float z in new[]{79f,93f,107f})
            Pylon(new Vector3(8.6f,0,z),aluminum);

        for(int phase=0;phase<3;phase++)
        {
            float x = 6.25f+phase*2.35f;
            var wire = PatchLine("Conducteur HT",graphite,.065f,19);
            var points = new Vector3[19];
            for(int j=0;j<points.Length;j++)
            {
                float z = 79 + 28*j/(float)(points.Length-1);
                float t = Mathf.Repeat((z-79)/14,1);
                points[j] = new Vector3(x,12.1f - 2.25f*4*t*(1-t),z);
            }
            wire.SetPositions(points);
        }
        grid.routeLine = PatchLine("Tracé pédagogique du réseau",cyan,.098f,7);
        grid.routeLine.SetPositions(new[]{new Vector3(0,4.43f,61),new Vector3(6.1f,3.15f,66.6f),
            new Vector3(7.5f,3.19f,74.3f),new Vector3(8.6f,12.24f,79),new Vector3(8.6f,12.24f,93),
            new Vector3(8.6f,12.24f,107),new Vector3(8.6f,5.20f,118.1f)});
        grid.routeLine.enabled = false;
        grid.flowMarkers = new Transform[4];
        for(int i=0;i<grid.flowMarkers.Length;i++)
        {
            var marker = Box("Repère d’alimentation",Vector3.zero,new(.28f,.28f,.28f),amber);
            marker.GetComponent<Collider>().enabled = false;
            marker.SetActive(false);
            grid.flowMarkers[i] = marker.transform;
        }

        var concrete = Palette("V2 Béton des tours",new(.65f,.67f,.63f));
        Box("Halle de production · silhouette",new(9.5f,4.16f,122),new(15,8.3f,10),concrete);
        Box("Bâtiment auxiliaire",new(20.1f,2.82f,121),new(5,5.6f,11),_neutral);
        GridCylinder("Enveloppe cylindrique · silhouette",new(9.2f,5.1f,130.4f),new(4.65f,5.1f,4.65f),concrete);
        var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "Coupole · silhouette";
        dome.transform.SetPositionAndRotation(new Vector3(9.2f,10.02f,130.4f),Quaternion.identity);
        dome.transform.localScale = new Vector3(9.25f,6.1f,9.25f);
        dome.GetComponent<Renderer>().sharedMaterial = concrete;
        dome.GetComponent<Collider>().enabled = false;
        var plumeMaterial = PlumeMaterial();
        grid.plumes = new ParticleSystem[2];
        for(int i=0;i<2;i++)
        {
            Vector3 pos = i == 0 ? new Vector3(-33f,0,132f) : new Vector3(-.3f,0,128.2f);
            var shell = new GameObject("Tour aéroréfrigérante · silhouette");
            shell.transform.position = pos;
            shell.AddComponent<MeshFilter>().sharedMesh = TowerMesh();
            shell.AddComponent<MeshRenderer>().sharedMaterial = concrete;
            grid.plumes[i] = Plume(pos+Vector3.up*18,plumeMaterial,920+i*21);
        }
    }

    static Collider GridStop(string caption,Vector3 p,Material graphite,Material aluminum)
    {
        var button = Box(caption,p,new(.56f,.26f,.09f),aluminum);
        Box("Support de visite",new(p.x,3.94f,p.z+.095f),new(.11f,1.08f,.115f),graphite);
        Sign(caption,p+Vector3.back*.048f,.043f,Color.white);
        return button.GetComponent<Collider>();
    }

    static GameObject GridGuide(float first,float last,Material material)
    {
        var root = new GameObject("Repères de passerelle");
        for(float z=first;z<last;z+=.82f)
        {
            var mark=Box("Repère au sol",new(0,3.423f,z),new(.13f,.012f,.39f),material);
            mark.transform.SetParent(root.transform,true);
            mark.GetComponent<Collider>().enabled = false;
        }
        root.SetActive(false);
        return root;
    }

    static void Pylon(Vector3 center,Material material)
    {
        for(int side=-1;side<=1;side+=2)
        {
            Beam(center+new Vector3(side*1.2f,0,0),center+new Vector3(side*.31f,13.8f,0),.18f,material);
            Beam(center+new Vector3(side*.93f,3.8f,0),center+new Vector3(-side*.68f,8.6f,0),.13f,material);
            Beam(center+new Vector3(side*.68f,8.6f,0),center+new Vector3(-side*.39f,12.1f,0),.13f,material);
        }
        Beam(center+new Vector3(-3.2f,12.2f,0),center+new Vector3(3.2f,12.2f,0),.19f,material);
        Beam(center+new Vector3(-.32f,13.75f,0),center+new Vector3(.32f,13.75f,0),.15f,material);
    }

    static void Beam(Vector3 a,Vector3 b,float width,Material material)
    {
        var beam = Box("Profilé de pylône",(a+b)*.5f,new(width,Vector3.Distance(a,b),width),material);
        beam.transform.rotation = Quaternion.FromToRotation(Vector3.up,b-a);
        beam.GetComponent<Collider>().enabled = false;
    }

    static void GridCylinder(string name,Vector3 p,Vector3 size,Material material)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        part.name = name;
        part.transform.position = p;
        part.transform.localScale = size;
        part.GetComponent<Renderer>().sharedMaterial = material;
        part.GetComponent<Collider>().enabled = false;
    }

    static Mesh TowerMesh()
    {
        const string path = Folder + "/V2 Tour de refroidissement.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing) return existing;
        const int rings=17, sides=64;
        var vertices=new Vector3[rings*sides];
        var triangles=new int[(rings-1)*sides*6];
        for(int h=0;h<rings;h++)
        {
            float t=h/(float)(rings-1);
            float radius=3.75f+4.1f*(t-.68f)*(t-.68f)+1.05f*Mathf.Pow(1-t,5);
            for(int j=0;j<sides;j++)
            {
                float angle=j*(2*Mathf.PI/sides);
                vertices[h*sides+j]=new Vector3(radius*Mathf.Cos(angle),18*t,radius*Mathf.Sin(angle));
                if(h==rings-1) continue;
                int at=(h*sides+j)*6, a=h*sides+j, b=h*sides+(j+1)%sides;
                triangles[at]=a; triangles[at+1]=a+sides; triangles[at+2]=b;
                triangles[at+3]=b; triangles[at+4]=a+sides; triangles[at+5]=b+sides;
            }
        }
        var mesh=new Mesh{name="V2 Tour de refroidissement",vertices=vertices,triangles=triangles};
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }

    static Material PlumeMaterial()
    {
        const string texturePath = Folder + "/V2 Masque panache.asset";
        const string materialPath = Folder + "/V2 Panache.mat";
        var existing=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(existing) return existing;
        var texture = new Texture2D(64,64,TextureFormat.RGBA32,false) { wrapMode=TextureWrapMode.Clamp };
        for(int y=0;y<64;y++)
            for(int x=0;x<64;x++)
            {
                float d=new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).magnitude;
                texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d*d),2)));
            }
        texture.Apply();
        AssetDatabase.CreateAsset(texture,texturePath);
        var material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        material.SetTexture("_BaseMap",texture);
        material.SetColor("_BaseColor",new Color(.96f,.97f,1,1));
        material.SetFloat("_Surface",1);
        material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite",0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetOverrideTag("RenderType","Transparent");
        material.renderQueue=(int)RenderQueue.Transparent;
        AssetDatabase.CreateAsset(material,materialPath);
        return material;
    }

    static ParticleSystem Plume(Vector3 p,Material material,int seed)
    {
        var system = new GameObject("Panache blanc · chaleur rejetée").AddComponent<ParticleSystem>();
        system.transform.position=p;
        system.transform.rotation=Quaternion.Euler(-83,0,0);
        system.useAutoRandomSeed=false;
        system.randomSeed=(uint)seed;
        var main=system.main;
        main.startLifetime=new ParticleSystem.MinMaxCurve(8,11);
        main.startSpeed=new ParticleSystem.MinMaxCurve(1.6f,2.3f);
        main.startSize=new ParticleSystem.MinMaxCurve(4.1f,7.2f);
        main.startColor=new Color(.97f,.98f,1,.17f);
        main.simulationSpace=ParticleSystemSimulationSpace.World;
        main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
        main.maxParticles=100;
        var emission=system.emission;
        emission.rateOverTime=8;
        var shape=system.shape;
        shape.shapeType=ParticleSystemShapeType.Cone;
        shape.angle=11;
        shape.radius=1.7f;
        var color=system.colorOverLifetime;
        color.enabled=true;
        var fade=new Gradient();
        fade.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
            new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(.6f,.62f),new GradientAlphaKey(0,1)});
        color.color=fade;
        system.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;
        return system;
    }
}
#endif
