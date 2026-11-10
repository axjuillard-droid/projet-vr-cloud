#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static partial class PhotoV2BlockoutBuilder
{
    static void InstallWaterRoute(PhotoV2Walkthrough route)
    {
        var water = route.gameObject.AddComponent<PhotoV2Water>();
        route.water = water;
        water.energy = route.energy;
        var graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Graphite.mat");
        var aluminum = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Aluminium.mat");
        var cyan = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Cyan.mat");
        var amber = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Ambre.mat");
        var gravel = Palette("V2 Berge minérale",new(.47f,.39f,.26f));
        var waterMaterial = Palette("V2 Eau de rivière",new(.10f,.34f,.43f));
        var rippleMaterial = Palette("V2 Reflet de rivière",new(.27f,.59f,.62f));
        var fishMaterial = Palette("V2 Poissons",new(.11f,.21f,.20f));

        water.gate = Box("Accès rivière",new(0,3.98f,98.97f),new(3.22f,1.15f,.16f),graphite);
        Box("Passerelle rivière",new(0,3.25f,105.29f),new(3.2f,.30f,12.73f),_neutral);
        foreach(int side in new[]{-1,1})
        {
            Box("Lisse rivière",new(side*1.57f,4.42f,105.35f),new(.08f,.09f,12.65f),aluminum);
            Box("Protection basse rivière",new(side*1.57f,3.67f,105.35f),new(.08f,.53f,12.65f),graphite);
        }
        Box("Garde-corps final",new(0,3.98f,111.55f),new(3.22f,1.15f,.16f),graphite);
        water.guide = GridGuide(99.6f,108.3f,cyan);
        water.entry = GridStop("VERS LA RIVIÈRE",new(1.05f,4.62f,98.48f),graphite,aluminum);
        Vector3 weather = new(-1.08f,4.53f,109.55f), load = new(1.08f,4.53f,109.55f);
        water.weather = Box("Commande météo",weather,new(.65f,.28f,.11f),cyan).GetComponent<Collider>();
        water.load = Box("Commande charge",load,new(.69f,.28f,.11f),amber).GetComponent<Collider>();
        Box("Pied météo",weather+new Vector3(0,-.59f,.10f),new(.14f,1.1f,.16f),graphite);
        Box("Pied charge",load+new Vector3(0,-.59f,.10f),new(.14f,1.1f,.16f),graphite);
        water.weatherLabel = Sign("ÉTÉ SEC",weather+Vector3.back*.06f,.054f,Color.black);
        water.loadLabel = Sign("DEMANDE DE CALCUL",load+Vector3.back*.06f,.038f,Color.black);

        var river = new GameObject("Rivière · scénario fictif").transform;
        river.position=new Vector3(-15.5f,0,102);
        var bed = RiverSurface("Lit et berges",river,new Vector3(0,.037f,0),gravel);
        bed.localScale=new Vector3(1.29f,1,1);
        water.surface = RiverSurface("Surface d’eau",river,new Vector3(0,.111f,0),waterMaterial);
        for(int i=0;i<18;i++)
        {
            int side=i%2==0 ? -1:1;
            float x = side*(4.55f+Mathf.Repeat(i*.83f,3.3f));
            var stone=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name="Galets de la rive";
            stone.transform.SetParent(river,false);
            stone.transform.localPosition=new Vector3(x,.105f,1.25f+i*2.31f);
            stone.transform.localScale=new Vector3(.47f+i%3*.2f,.19f,.46f+i%2*.32f);
            stone.GetComponent<Renderer>().sharedMaterial=gravel;
            stone.GetComponent<Collider>().enabled=false;
        }
        water.fish=new Transform[7];
        for(int i=0;i<water.fish.Length;i++)
        {
            var fish=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fish.name="Poisson · présence illustrative";
            fish.transform.SetParent(river,false);
            fish.transform.localScale=new Vector3(.24f,.15f,.57f);
            fish.GetComponent<Renderer>().sharedMaterial=fishMaterial;
            fish.GetComponent<Collider>().enabled=false;
            water.fish[i]=fish.transform;
        }
        water.ripples=new Transform[24];
        for(int i=0;i<water.ripples.Length;i++)
        {
            var ripple=Box("Ride de surface",Vector3.zero,new(.42f,.006f,.14f),rippleMaterial);
            ripple.transform.SetParent(river,false);
            ripple.GetComponent<Collider>().enabled=false;
            water.ripples[i]=ripple.transform;
        }

        Box("Dalle de l’option évaporative",new(-6.2f,.02f,112.1f),new(5.9f,.10f,7.4f),_neutral);
        Box("Tour évaporative · variante pédagogique",new(-6.2f,1.47f,111.2f),new(3.14f,2.88f,2.5f),graphite);
        for(int i=0;i<11;i++)
            Box("Grille de tour",new(-4.61f,1.43f,110.04f+i*.21f),new(.09f,1.74f,.057f),aluminum);
        Sign("OPTION ÉVAPORATIVE",new(-6.2f,3.15f,109.9f),.12f,Color.white);
        GridCylinder("Prise d’eau",new(-10.1f,.28f,106.8f),new(.62f,.34f,.62f),graphite);
        GridCylinder("Retour d’eau",new(-10.1f,.24f,117.35f),new(.57f,.30f,.57f),aluminum);
        Sign("PRÉLÈVEMENT",new(-9.9f,.76f,106.7f),.09f,Color.white);
        Sign("RESTITUTION",new(-9.9f,.66f,117.22f),.09f,Color.white);

        water.workingFlows = new GameObject("Flux de l’option évaporative");
        water.intake=PatchLine("Prélèvement",cyan,.18f,4);
        water.intake.transform.SetParent(water.workingFlows.transform,true);
        water.intake.SetPositions(new[]{new Vector3(-15.2f,.24f,106.8f),new Vector3(-10.1f,.65f,106.8f),
            new Vector3(-6.4f,1.14f,106.8f),new Vector3(-6.4f,1.14f,109.95f)});
        water.discharge=PatchLine("Restitution",aluminum,.08f,4);
        water.discharge.transform.SetParent(water.workingFlows.transform,true);
        water.discharge.SetPositions(new[]{new Vector3(-6.5f,.95f,112.46f),new Vector3(-6.5f,.47f,117.35f),
            new Vector3(-10.1f,.41f,117.35f),new Vector3(-15.2f,.22f,117.35f)});
        water.evaporation=Plume(new Vector3(-6.2f,2.98f,111.2f),PlumeMaterial(),1327);
        water.evaporation.transform.SetParent(water.workingFlows.transform,true);
        var main=water.evaporation.main;
        main.startLifetime=2.4f;
        main.startSize=new ParticleSystem.MinMaxCurve(.65f,1.22f);
        main.startSpeed=new ParticleSystem.MinMaxCurve(.85f,1.3f);
        var shape=water.evaporation.shape;
        shape.radius=.51f;
        water.workingFlows.SetActive(false);
    }

    static Transform RiverSurface(string name,Transform parent,Vector3 position,Material material)
    {
        const string path = Folder + "/V2 Surface de rivière.asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (!mesh)
        {
            const int slices=24;
            var vertices=new Vector3[(slices+1)*2];
            var triangles=new int[slices*6];
            for(int i=0;i<=slices;i++)
            {
                float z=i*1.8f;
                float center=.75f*Mathf.Sin(i*.25f);
                float width=7.35f+.69f*Mathf.Sin(i*.39f);
                vertices[2*i]=new Vector3(center-width,0,z);
                vertices[2*i+1]=new Vector3(center+width,0,z);
                if(i==slices) continue;
                int a=i*2, t=i*6;
                triangles[t]=a; triangles[t+1]=a+2; triangles[t+2]=a+1;
                triangles[t+3]=a+1; triangles[t+4]=a+2; triangles[t+5]=a+3;
            }
            mesh=new Mesh{name="V2 Surface de rivière",vertices=vertices,triangles=triangles};
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,path);
        }
        var surface=new GameObject(name);
        surface.transform.SetParent(parent,false);
        surface.transform.localPosition=position;
        surface.AddComponent<MeshFilter>().sharedMesh=mesh;
        surface.AddComponent<MeshRenderer>().sharedMaterial=material;
        return surface.transform;
    }
}
#endif
