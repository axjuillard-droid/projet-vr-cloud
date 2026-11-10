#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class PhotoV2BlockoutBuilder
{
    static void InstallStorageAndBalcony(PhotoV2Walkthrough route)
    {
        var graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Graphite.mat");
        var aluminum = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Aluminium.mat");
        var cyan = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Cyan.mat");

        var storage = route.gameObject.AddComponent<PhotoV2Storage>();
        route.storage = storage;
        Box("Pied du pupitre de stockage", new(1.70f,.51f,36.52f), new(.23f,1.02f,.28f), aluminum);
        Box("Pupitre · choix du stockage", new(1.70f,1.34f,36.52f), new(1.16f,.66f,.20f), graphite);
        Sign("02   /   STOCKAGE", new(1.70f,1.78f,36.397f), .074f, Color.white);
        var choice = route.targets[7].transform;
        choice.position = new Vector3(1.40f,1.33f,36.376f);
        choice.localScale = new Vector3(.45f,.17f,.072f);
        choice.GetComponent<MeshRenderer>().sharedMaterial = aluminum;
        storage.alternative = Box("Choix · serveur de calcul", new(2.00f,1.33f,36.376f),
            new(.45f,.17f,.072f), aluminum);
        Sign("BAIE DE STOCKAGE", choice.position + Vector3.back*.038f, .039f, Color.white, choice);
        Sign("SERVEUR DE CALCUL", storage.alternative.transform.position + Vector3.back*.038f,
            .039f, Color.white, storage.alternative.transform);
        storage.explanation = Sign("Où sera conservée la photo ?", new(1.70f,1.56f,36.411f), .054f, Color.white);
        storage.explanation.fontSize = 65;
        storage.explanation.horizontalOverflow = HorizontalWrapMode.Wrap;
        storage.explanation.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(1640,300);

        Box("Armoire de stockage", new(2.85f,1.11f,37.05f), new(.98f,2.22f,.9f), graphite);
        for (int row=0; row<7; row++)
        {
            float y = .38f+row*.245f;
            Box("Support de disques", new(2.85f,y,36.592f), new(.89f,.20f,.04f), aluminum);
            for (int drive=0; drive<5; drive++)
            {
                float x = 2.51f+drive*.17f;
                Box("Tiroir de disque", new(x,y,36.562f), new(.12f,.12f,.041f), graphite);
                Box("Voyant de disque", new(x-.036f,y-.026f,36.538f), new(.022f,.014f,.009f), cyan);
            }
        }
        Sign("STOCKAGE", new(2.85f,2.14f,36.54f), .066f, Color.white);
        storage.archiveSlot = new GameObject("Emplacement pédagogique de la photo").transform;
        storage.archiveSlot.position = new Vector3(2.85f,1.61f,36.49f);
        storage.packet = Box("Impulsion vers le stockage", new(1.70f,1.32f,36.24f), new(.09f,.09f,.09f), cyan).transform;
        storage.packet.GetComponent<Collider>().enabled = false;
        storage.packet.gameObject.SetActive(false);

        var client = route.targets[8].transform;
        client.position = new Vector3(1.36f,1.41f,38.13f);
        client.localScale = new Vector3(.78f,.49f,.087f);
        client.GetComponent<MeshRenderer>().sharedMaterial = graphite;
        Box("Pied écran client", new(1.36f,.605f,38.255f), new(.19f,1.21f,.16f), aluminum);
        Box("Socle écran client", new(1.36f,.052f,38.17f), new(.58f,.10f,.44f), graphite);
        storage.clientLabel = Sign("ÉCRAN CLIENT", client.position + Vector3.back*.048f, .058f, Color.white);
        var screen = new GameObject("Relecture de la photo", typeof(RectTransform), typeof(Canvas));
        screen.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        screen.transform.position = client.position + Vector3.back*.047f;
        screen.transform.localScale = Vector3.one*.001f;
        screen.GetComponent<RectTransform>().sizeDelta = new Vector2(760,460);
        storage.clientImage = new GameObject("Photo lue", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        storage.clientImage.transform.SetParent(screen.transform,false);
        storage.clientImage.rectTransform.sizeDelta = new Vector2(636,424);
        storage.clientImage.raycastTarget = false;
        storage.clientImage.enabled = false;

        Sign("03   /   BELVÉDÈRE", new(0,2.80f,40.15f), .12f, Color.white);
        Box("Traverse signalétique", new(0,2.82f,40.21f), new(3.1f,.43f,.10f), graphite);
        var balcony = route.targets[9].transform;
        balcony.position = new Vector3(1.19f,4.49f,58.57f);
        balcony.localScale = new Vector3(.42f,.28f,.072f);
        balcony.GetComponent<MeshRenderer>().sharedMaterial = aluminum;
        Box("Montant du repère balcon", new(1.19f,3.94f,58.65f), new(.12f,1.07f,.13f), graphite);
        Sign("BELVÉDÈRE", balcony.position + Vector3.back*.038f, .066f, Color.white, balcony);
        for(int side=-1; side<=1; side+=2)
        {
            float x = side*2.47f;
            Box("Lisse du balcon", new(x,4.42f,55.08f), new(1.91f,.065f,.07f), aluminum);
            for(int post=0;post<3;post++)
                Box("Montant du balcon", new(side*(1.56f+post*.87f),3.94f,55.08f),
                    new(.075f,1.10f,.075f), graphite);
        }
    }
}
#endif
