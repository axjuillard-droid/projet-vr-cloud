#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Décor original et reproductible, géométrie Blender + détails fusionnés.</summary>
public static class DataCenterArtBuilder
{
    const string ModelPath = "Assets/CloudVR/Models/ServerRack_Detailed.fbx";
    const string PrefabPath = "Assets/CloudVR/Prefabs/ServerRack.prefab";
    const string MeshFolder = "Assets/CloudVR/Models";

    public static GameObject BuildRack()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null) throw new InvalidOperationException("Générer ServerRack_Detailed.fbx avec tools/art/generate_server_rack.py.");
        var rack = new GameObject("ServerRack");
        try
        {
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "Modele_Original";
            visual.transform.SetParent(rack.transform, false);
            // Import FBX observé : façades vers +Z ; visiteurs arrivent depuis -Z.
            visual.transform.localRotation = Quaternion.Euler(0,180,0);
            var palette = new Dictionary<string, Material> {
                {"Rack_Shell", Material("Rack_Shell", new Color(.075f,.09f,.11f), .25f, .3f)},
                {"Rack_Equipment", Material("Rack_Equipment", new Color(.37f,.40f,.44f), .25f, .35f)},
                {"Rack_Metal", Material("Rack_Metal", new Color(.57f,.62f,.67f), .5f, .4f)},
                {"Rack_Recess", Material("Rack_Recess", new Color(.015f,.021f,.03f), 0, .1f)},
                {"Rack_Cyan", Material("Rack_Cyan", new Color(.04f,.57f,.68f), .15f, .3f, .25f)},
                {"Rack_Green", Material("Rack_Green", new Color(.1f,.85f,.4f), 0, .25f, .8f)},
                {"Rack_Amber", Material("Rack_Amber", new Color(.98f,.54f,.06f), 0, .25f, .5f)}
            };
            foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>())
            {
                if (!palette.TryGetValue(renderer.name, out var material))
                    throw new InvalidOperationException("Mesh inattendu : " + renderer.name);
                renderer.sharedMaterial = material;
            }
            var collider = rack.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 1.1f, 0);
            collider.size = new Vector3(1.1f, 2.2f, 1); // Même enveloppe de navigation que T07.
            return PrefabUtility.SaveAsPrefabAsset(rack, PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(rack); }
    }

    public static void Decorate(Transform room)
    {
        var decor = new GameObject("Architecture_Originale").transform;
        decor.SetParent(room, false);
        var metal = Material("DC_Architecture_Metal", new Color(.28f,.33f,.39f), .25f, .3f);
        var joint = Material("DC_Architecture_Joints", new Color(.07f,.10f,.14f), 0, .1f);
        Material("DC_Floor", new Color(.27f,.30f,.34f), .05f, .25f);
        var tile = Material("DC_Architecture_Dalles", new Color(.285f,.315f,.355f), .05f, .22f);
        var light = Material("DC_Architecture_Luminaire", new Color(.76f,.86f,.9f), 0, .1f, .8f);
        var blue = Material("DC_Architecture_Accent", new Color(.07f,.48f,.63f), 0, .2f, .1f);

        // Dalles alternées et joints en surface ; le collider du sol reste continu.
        for (int x = 0; x < 10; x++)
            for (int z = 0; z < 12; z++)
                if ((x + z) % 2 == 0)
                    Box(decor, "Dalle", new Vector3(x - 4.5f, .001f, z + .5f), new Vector3(.992f,.002f,.992f), tile);
        for (int x = -5; x <= 5; x++)
            Box(decor, "Joint_sol", new Vector3(x,.002f,6), new Vector3(.006f,.003f,12), joint);
        for (int z = 0; z <= 12; z++)
            Box(decor, "Joint_sol", new Vector3(0,.002f,z), new Vector3(10,.003f,.006f), joint);

        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 4.985f;
            Box(decor, "Plinthe", new Vector3(x,.12f,6), new Vector3(.025f,.24f,12), joint);
            Box(decor, "Bande_murale", new Vector3(x,1.0f,6), new Vector3(.025f,.025f,12), blue);
            for (int z = 1; z < 12; z += 2)
                Box(decor, "Joint_mural", new Vector3(x,1.8f,z), new Vector3(.016f,2.65f,.012f), metal);
        }

        // Chemins de câbles, montants, luminaires géométriques (pas de lumière par LED).
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 2.25f;
            for (int rail = -1; rail <= 1; rail += 2)
                Box(decor, "Rail_chemin_cables", new Vector3(x + rail * .28f,2.86f,7), new Vector3(.035f,.12f,8), metal);
            for (int z = 3; z <= 11; z++)
            {
                Box(decor, "Traverse_cables", new Vector3(x,2.82f,z), new Vector3(.56f,.025f,.035f), metal);
                if (z % 2 == 1)
                    Box(decor, "Suspente", new Vector3(x,3.035f,z), new Vector3(.02f,.33f,.02f), metal);
            }
            for (int cable = -1; cable <= 1; cable++)
                Box(decor, "Cable", new Vector3(x + cable * .13f,2.88f,7), new Vector3(.023f,.023f,7.8f), joint);
        }
        for (int x = -4; x <= 4; x += 2)
            Box(decor, "Joint_plafond", new Vector3(x,3.19f,6), new Vector3(.012f,.01f,12), joint);
        for (int z = 0; z <= 12; z += 2)
            Box(decor, "Joint_plafond", new Vector3(0,3.19f,z), new Vector3(10,.01f,.012f), joint);
        for (int z = 2; z <= 10; z += 4)
            for (int side = -1; side <= 1; side += 2)
            {
                var center = new Vector3(side * .95f, 3.14f, z);
                Box(decor, "Boitier_luminaire", center, new Vector3(.38f,.08f,1.6f), metal);
                Box(decor, "Diffuseur_luminaire", center + Vector3.down * .047f, new Vector3(.30f,.012f,1.46f), light);
            }
        // Deux armoires génériques au fond ; leur volume n'empiète pas sur la zone VR.
        for (int side = -1; side <= 1; side += 2)
        {
            var center = new Vector3(side * 4.2f,1.05f,10.95f);
            Box(decor, "Armoire_technique", center, new Vector3(.72f,2.1f,.6f), metal);
            Box(decor, "Porte_armoire", center + Vector3.back * .31f, new Vector3(.64f,1.95f,.02f), tile);
            Box(decor, "Poignee_armoire", center + new Vector3(.23f,0,-.335f), new Vector3(.018f,.18f,.018f), joint);
            var solid = new GameObject("Collision_Armoire").AddComponent<BoxCollider>();
            solid.transform.SetParent(room,false);
            solid.transform.localPosition = center;
            solid.size = new Vector3(.72f,2.1f,.6f);
        }
        Box(decor, "Pied_guide", new Vector3(-2.1f,.76f,4.23f), new Vector3(.045f,1.5f,.045f), metal);
        Box(decor, "Socle_guide", new Vector3(-2.1f,.018f,4.23f), new Vector3(.65f,.036f,.40f), metal);
        MergeByMaterial(decor);
    }

    static Material Material(string name, Color color, float metallic, float smoothness, float emission = 0)
    {
        string path = "Assets/CloudVR/Materials/" + name + ".mat";
        var result = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (result == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit absent.");
            result = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(result,path);
        }
        result.color = color;
        result.SetFloat("_Metallic",metallic);
        result.SetFloat("_Smoothness",smoothness);
        if (emission > 0) { result.EnableKeyword("_EMISSION"); result.SetColor("_EmissionColor",color * emission); }
        else { result.DisableKeyword("_EMISSION"); result.SetColor("_EmissionColor",Color.black); }
        EditorUtility.SetDirty(result);
        return result;
    }

    static void Box(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent,false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
    }

    static void MergeByMaterial(Transform parent)
    {
        var renderers = parent.GetComponentsInChildren<MeshRenderer>();
        foreach (var group in renderers.GroupBy(x => x.sharedMaterial))
        {
            var mesh = new Mesh { name = group.Key.name, indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(group.Select(x => new CombineInstance {
                mesh=x.GetComponent<MeshFilter>().sharedMesh,
                transform=parent.worldToLocalMatrix * x.transform.localToWorldMatrix
            }).ToArray());
            string path=MeshFolder + "/" + group.Key.name + ".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) { EditorUtility.CopySerialized(mesh,existing); UnityEngine.Object.DestroyImmediate(mesh); mesh=existing; }
            else AssetDatabase.CreateAsset(mesh,path);
            var combined=new GameObject(group.Key.name,typeof(MeshFilter),typeof(MeshRenderer));
            combined.transform.SetParent(parent,false);
            combined.GetComponent<MeshFilter>().sharedMesh=mesh;
            combined.GetComponent<MeshRenderer>().sharedMaterial=group.Key;
        }
        foreach (var renderer in renderers) UnityEngine.Object.DestroyImmediate(renderer.gameObject);
    }
}
#endif
