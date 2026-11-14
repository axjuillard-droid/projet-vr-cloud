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

        var amber = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/V2 Ambre.mat");
        var storage = route.gameObject.AddComponent<PhotoV2Storage>();
        route.storage = storage;
        storage.view = route.view;
        storage.writtenIndicator = cyan;

        const float x = 1.48f, face = 36.13f;
        var cabinet = new GameObject("Baie de stockage").transform;
        cabinet.position = new Vector3(x,0,36.63f);
        GameObject CabinetBox(string name, Vector3 position, Vector3 size, Material material)
        {
            var item = Box(name, position, size, material);
            item.transform.SetParent(cabinet,true);
            return item;
        }
        Vector3 CabinetPoint(Vector3 point) => cabinet.TransformPoint(point - cabinet.position);

        CabinetBox("Armoire de stockage", new(x,1.16f,36.63f), new(1.12f,2.32f,.90f), graphite);
        Sign("02 / STOCKAGE", new(x,2.26f,face), .090f, Color.white, cabinet);
        storage.explanation = Sign("EN ATTENTE DE LA PHOTO", new(x,1.93f,face), .038f, Color.white, cabinet);
        CabinetBox("Répartiteur de la baie", new(x,2.09f,face), new(.20f,.08f,.12f), aluminum);
        var distributor = new Vector3(x,2.09f,face-.12f);

        var busy = CabinetBox("Banque A · occupée", new(x,1.66f,face), new(.95f,.22f,.08f), graphite);
        storage.busyBank = busy.GetComponent<Collider>();
        Sign("A / OCCUPÉE", busy.transform.position + Vector3.back*.043f, .068f, Color.white, busy.transform);
        CabinetBox("Voyant banque A", new(x-.42f,1.66f,face-.047f), new(.055f,.045f,.015f), amber)
            .GetComponent<Collider>().enabled = false;
        var available = route.targets[7].transform;
        available.SetParent(cabinet,true);
        available.name = "Banque B · disponible";
        available.position = new Vector3(x,1.34f,face);
        available.localScale = new Vector3(.95f,.22f,.08f);
        available.GetComponent<MeshRenderer>().sharedMaterial = aluminum;
        storage.bankIndicator = CabinetBox("Voyant banque B", new(x-.42f,1.34f,face-.047f), new(.055f,.045f,.015f), aluminum)
            .GetComponent<Renderer>();
        storage.bankIndicator.GetComponent<Collider>().enabled = false;
        storage.bankLabel = Sign("B / LIBRE", available.position + Vector3.back*.043f, .068f, Color.white, available);

        for (int row=0; row<3; row++)
        {
            float y = .40f+row*.245f;
            CabinetBox("Support de disques", new(x,y,face+.03f), new(.94f,.20f,.04f), aluminum);
            for (int drive=0; drive<5; drive++)
            {
                float driveX = x-.34f+drive*.17f;
                CabinetBox("Tiroir de disque", new(driveX,y,face), new(.12f,.12f,.041f), graphite);
                CabinetBox("Voyant de disque", new(driveX-.036f,y-.026f,face-.024f), new(.022f,.014f,.009f), cyan);
            }
        }

        cabinet.rotation = Quaternion.Euler(0,52,0);
        var inlet = CabinetPoint(distributor);
        var outlet = route.computeOutput.position;
        storage.serverLink = PatchLine("Liaison locale du serveur", amber, .025f, 2);
        storage.serverLink.SetPositions(new[] { outlet, outlet });
        storage.serverLink.enabled = false;
        var deliveryPath = new[]
        {
            outlet, new Vector3(outlet.x,2.83f,outlet.z), new Vector3(outlet.x,2.83f,35.38f),
            new Vector3(inlet.x,2.83f,35.38f), new Vector3(inlet.x,2.83f,inlet.z), inlet
        };
        storage.delivery = PatchLine("Câble du serveur à la baie de stockage", aluminum, .038f, deliveryPath.Length);
        storage.delivery.SetPositions(deliveryPath);
        storage.deliverySignal = amber;
        storage.branch = PatchLine("Écriture vers la banque B", amber, .024f, 4);
        storage.branch.SetPositions(new[] { inlet, CabinetPoint(new Vector3(x+.47f,2.09f,distributor.z)),
            CabinetPoint(new Vector3(x+.47f,1.34f,distributor.z)), CabinetPoint(new Vector3(x+.32f,1.34f,face-.048f)) });
        storage.branch.enabled = false;

        var packet = new GameObject("Miniature en transit", typeof(RectTransform), typeof(Canvas));
        packet.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        packet.transform.localScale = Vector3.one*.002f;
        packet.GetComponent<RectTransform>().sizeDelta = new Vector2(130,86);
        storage.packetCarrier = packet.transform;
        storage.packet = new GameObject("Photo", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        storage.packet.transform.SetParent(packet.transform, false);
        storage.packet.rectTransform.sizeDelta = new Vector2(130,86);
        // Garder la miniature devant la section du câble lorsque son support atteint le répartiteur.
        storage.packet.rectTransform.localPosition = new Vector3(0,-10,-24);
        storage.packet.raycastTarget = false;
        storage.packet.enabled = false;

        var client = route.targets[8].transform;
        client.position = new Vector3(1.04f,1.46f,38.13f);
        client.localScale = new Vector3(1.31f,.69f,.087f);
        client.GetComponent<MeshRenderer>().sharedMaterial = graphite;
        Box("Pied écran client", new(1.04f,.605f,38.255f), new(.19f,1.21f,.16f), aluminum);
        Box("Socle écran client", new(1.04f,.052f,38.17f), new(.58f,.10f,.44f), graphite);
        storage.clientLabel = Sign("ÉCRAN CLIENT", client.position + new Vector3(0,.25f,-.048f), .050f, Color.white);
        var screen = new GameObject("Relecture de la photo", typeof(RectTransform), typeof(Canvas));
        screen.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        screen.transform.position = client.position + Vector3.back*.047f;
        screen.transform.localScale = Vector3.one*.001f;
        screen.GetComponent<RectTransform>().sizeDelta = new Vector2(1290,660);
        storage.clientImage = new GameObject("Photo lue", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        storage.clientImage.transform.SetParent(screen.transform,false);
        storage.clientImage.rectTransform.sizeDelta = new Vector2(430,286);
        storage.clientImage.rectTransform.anchoredPosition = new Vector2(-310,8);
        storage.clientImage.raycastTarget = false;
        storage.clientImage.enabled = false;

        storage.clientPreview = new GameObject("Aperçu à comparer", typeof(RectTransform), typeof(RawImage))
            .GetComponent<RawImage>();
        storage.clientPreview.transform.SetParent(screen.transform,false);
        storage.clientPreview.rectTransform.sizeDelta = new Vector2(208,138);
        storage.clientPreview.rectTransform.anchoredPosition = new Vector2(310,8);
        storage.clientPreview.raycastTarget = false;
        storage.clientPreview.enabled = false;

        storage.originalChoice = ReadChoice("96 × 64",client.position+new Vector3(-.31f,-.225f,-.06f),aluminum);
        storage.previewChoice = ReadChoice("24 × 16",client.position+new Vector3(.31f,-.225f,-.06f),aluminum);
        var slot = storage.branch.GetPosition(storage.branch.positionCount-1);
        var screenPort = client.position + new Vector3(0,.34f,-.06f);
        var returnPath = new[] { slot, new Vector3(slot.x,2.63f,slot.z),
            new Vector3(screenPort.x,2.63f,slot.z), new Vector3(screenPort.x,2.63f,screenPort.z), screenPort };
        storage.readPath = PatchLine("Retour de la banque vers l’écran client",cyan,.034f,returnPath.Length);
        storage.readPath.SetPositions(returnPath);
        storage.readPath.enabled = false;

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
            float railingX = side*2.47f;
            Box("Lisse du balcon", new(railingX,4.42f,55.08f), new(1.91f,.065f,.07f), aluminum);
            for(int post=0;post<3;post++)
                Box("Montant du balcon", new(side*(1.56f+post*.87f),3.94f,55.08f),
                    new(.075f,1.10f,.075f), graphite);
        }
    }

    static Collider ReadChoice(string size, Vector3 position, Material material)
    {
        var button = Box("Vérifier le fichier "+size,position,new(.35f,.13f,.11f),material);
        Sign(size,position+Vector3.back*.058f,.062f,Color.white,button.transform);
        button.SetActive(false);
        return button.GetComponent<Collider>();
    }
}
#endif
