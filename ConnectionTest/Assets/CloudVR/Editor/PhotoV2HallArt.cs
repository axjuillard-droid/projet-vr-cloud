#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class PhotoV2BlockoutBuilder
{
    static void DressRackRows()
    {
        var rackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CloudVR/Models/ServerRack_Detailed.fbx");
        if (!rackPrefab) throw new System.InvalidOperationException("Modèle de baie détaillé introuvable.");
        var standby = AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/Rack_Recess.mat");
        if (!standby) throw new System.InvalidOperationException("Matériau de veille introuvable.");
        for (int row=0; row<6; row++)
        {
            var line = new GameObject("Rangée équipée " + (row+1)).transform;
            float x=(row-2.5f)*2.6f;
            int bay=0;
            foreach (float z in RackPositions(row))
            {
                var cabinet=(GameObject)PrefabUtility.InstantiatePrefab(rackPrefab,line);
                cabinet.name=$"Baie {row+1:00}-{++bay:00}";
                cabinet.transform.localPosition=new Vector3(x,0,z);
                cabinet.transform.localScale=new Vector3(.72f, .955f, 1);
                // Les faces de service sont orientées vers les allées entre rangées.
                cabinet.transform.localRotation=Quaternion.Euler(0,row%2==0 ? -90 : 90,0);
                foreach (var renderer in cabinet.GetComponentsInChildren<MeshRenderer>())
                {
                    var slots=renderer.sharedMaterials;
                    for(int i=0;i<slots.Length;i++)
                    {
                        if (!slots[i]) continue;
                        string name=slots[i].name;
                        var material = name=="Rack_Green" || name=="Rack_Cyan" || name=="Rack_Amber" ?
                            standby : AssetDatabase.LoadAssetAtPath<Material>("Assets/CloudVR/Materials/"+name+".mat");
                        if (material) slots[i]=material;
                    }
                    renderer.sharedMaterials=slots;
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.BatchingStatic);
                }
                // Conserver une collision simple qui couvre aussi les poignées.
                var hit=cabinet.AddComponent<BoxCollider>();
                hit.center=new Vector3(0,1.10f,0);
                hit.size=new Vector3(1.10f,2.20f,1.10f);
            }
        }
    }

    static IEnumerable<float> RackPositions(int row)
    {
        for (int bay=0; bay<7; bay++)
            if (row!=2 || bay!=1) yield return 23+bay*1.95f;
        // Laisser l’axe central libre pour la montée. Les quatre rangées latérales continuent sous le belvédère.
        if (row!=2 && row!=3)
            for (int bay=0; bay<6; bay++) yield return 42.35f+bay*1.88f;
    }

    static void DressHallAndLookout()
    {
        var graphite=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/V2 Graphite.mat");
        var aluminum=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/V2 Aluminium.mat");
        var floor=Palette("V2 Sol technique",new(.27f,.33f,.36f));
        var white=Palette("V2 Plafonnier",new(.68f,.81f,.88f));
        white.EnableKeyword("_EMISSION");
        white.SetColor("_EmissionColor",new Color(.65f,.79f,.88f));
        EditorUtility.SetDirty(white);
        HallTrim("Revêtement salle",new(0,.008f,31),new(19.36f,.010f,17.95f),floor);
        foreach(int side in new[]{-1,1})
        {
            HallTrim("Plinthe technique",new(side*9.625f,.18f,31),new(.10f,.34f,17.8f),graphite);
            HallTrim("Bandeau paroi",new(side*9.625f,2.77f,31),new(.11f,.17f,17.8f),aluminum);
            HallTrim("Caniveau latéral",new(side*3.92f,.019f,30.95f),new(.075f,.013f,17.3f),graphite);
            HallTrim("Chemin de câbles suspendu",new(side*5.2f,4.52f,31),new(.65f,.09f,17.5f),graphite);
            HallTrim("Rail lumière",new(side*2.65f,5.29f,31),new(.10f,.045f,17.5f),white);
            for(int bay=0;bay<5;bay++)
            {
                float z=23.0f+bay*3.8f;
                HallTrim("Pilastre",new(side*9.58f,2.93f,z),new(.21f,5.75f,.37f),graphite);
                HallTrim("Suspente",new(side*5.2f,5.15f,z),new(.10f,1.18f,.12f),aluminum);
            }
        }
        for(int i=0;i<5;i++)
        {
            float z=23f+i*3.9f;
            HallTrim("Traverse haute",new(0,5.55f,z),new(19.32f,.21f,.27f),graphite);
            if(i%2==0)
            {
                var light=new GameObject("Éclairage diffus du hall").AddComponent<Light>();
                light.transform.position=new Vector3(0,5.28f,z+.25f);
                light.transform.rotation=Quaternion.Euler(90,0,0);
                light.type=LightType.Spot;
                light.spotAngle=117;
                light.innerSpotAngle=78;
                light.color=new Color(.80f,.90f,1);
                light.intensity=4.5f;
                light.range=9.1f;
            }
        }
        DressPanoramaExtension(floor,graphite,aluminum,white);
        HallTrim("Cadre supérieur du belvédère",new(0,6.37f,55.14f),new(6.86f,.25f,.36f),graphite);
        HallTrim("Plafond du belvédère",new(0,6.61f,57.98f),new(6.87f,.22f,5.68f),graphite);
        HallTrim("Plafonnier du belvédère",new(0,6.482f,57.2f),new(4.28f,.018f,.084f),white);
        HallTrim("Sol antidérapant du belvédère",new(0,3.409f,58),new(6.85f,.011f,5.8f),floor);
        foreach(int side in new[]{-1,1})
        {
            HallTrim("Montant du belvédère",new(side*3.33f,4.95f,55.18f),new(.19f,3.02f,.25f),graphite);
            HallTrim("Retour plafond du belvédère",new(side*3.33f,6.36f,58),new(.19f,.25f,5.64f),graphite);
        }
        var sign=Sign("BELVÉDÈRE   /   SALLE DES BAIES",new(0,6.38f,55.33f),.108f,Color.white);
        FaceBalcony(sign,sign.transform.parent.parent);
    }

    static void DressPanoramaExtension(Material floor,Material graphite,Material aluminum,Material white)
    {
        Box("Plafond des ailes",new(0,6.01f,47.33f),new(20,.25f,14.68f),_neutral);
        WallPair("Ailes techniques",9.8f,40,54.65f,6,_neutral);
        foreach(int side in new[]{-1,1})
        {
            Box("Sol d’aile technique",new(side*5.66f,-.153f,47.26f),new(8.05f,.30f,14.52f),floor);
            HallTrim("Chemin de câbles prolongé",new(side*5.2f,4.52f,47.32f),new(.65f,.09f,14.61f),graphite);
            HallTrim("Éclairage des ailes",new(side*3.85f,5.29f,47.32f),new(.12f,.046f,14.5f),white);
            HallTrim("Plinthe des ailes",new(side*9.64f,.18f,47.32f),new(.11f,.34f,14.54f),graphite);
            Box("Panneau de fermeture d’aile",new(side*5.82f,1.07f,54.55f),new(8.1f,2.12f,.24f),graphite);
            for(int bay=0;bay<4;bay++)
            {
                float z=41.7f+bay*3.85f;
                HallTrim("Pilastre d’aile",new(side*9.56f,2.93f,z),new(.26f,5.75f,.38f),graphite);
                HallTrim("Suspente d’aile",new(side*5.2f,5.10f,z),new(.1f,1.14f,.14f),aluminum);
            }
        }
        for(int i=0;i<4;i++)
            HallTrim("Poutre panoramique",new(0,5.56f,41.85f+i*3.85f),new(19.34f,.23f,.28f),graphite);
        HallTrim("Pont transversal de chemins de câbles",new(0,5.12f,51.24f),new(11,.13f,.47f),graphite);
        var light = new GameObject("Éclairage diffus des ailes").AddComponent<Light>();
        light.transform.position = new Vector3(0,5.32f,48);
        light.transform.rotation = Quaternion.Euler(90,0,0);
        light.type = LightType.Spot;
        light.spotAngle = 130;
        light.innerSpotAngle = 86;
        light.range = 10;
        light.intensity = 3.6f;
        light.color = new Color(.81f,.91f,1);
    }

    static void HallTrim(string name,Vector3 position,Vector3 size,Material material)
    {
        var trim=Box(name,position,size,material);
        trim.GetComponent<Collider>().enabled=false;
        GameObjectUtility.SetStaticEditorFlags(trim,StaticEditorFlags.BatchingStatic);
    }
}
#endif
