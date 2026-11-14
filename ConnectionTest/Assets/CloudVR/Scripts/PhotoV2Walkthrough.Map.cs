using UnityEngine;

public sealed partial class PhotoV2Walkthrough
{
    static readonly Color MapBackground = new(.045f,.075f,.105f,.98f);
    static readonly Color MapZone = new(.12f,.20f,.25f);
    static readonly Color MapPath = new(.34f,.49f,.55f);
    static readonly Color MapTarget = new(1f,.68f,.33f);
    GUIStyle _mapText;

    // Repères des deux zones, du bas vers le haut du plan.
    static readonly string[] BuildingStops = { "Chambre", "Sas et réseau", "Baie école", "Stockage", "Belvédère" };
    static readonly float[] BuildingZ = { 3f, 18f, 25f, 36f, 57f };
    static readonly string[] OutdoorStops = { "Belvédère", "Poste électrique", "Vue centrale", "Échangeur", "Pompe et rivière" };
    static readonly float[] OutdoorZ = { 58f, 69.9f, 84.9f, 97.5f, 110.3f };

    void DrawMovementKeys()
    {
        float width = Mathf.Min(580,Screen.width-36);
        GUI.Box(new Rect(18,Screen.height-27,width,25),GUIContent.none);
        GUI.Label(new Rect(26,Screen.height-26,width-16,24),
            "ZQSD / flèches · Maj courir · M carte · clic / E · Échap",_small);
    }

    void DrawSiteMap()
    {
        bool outdoors = player.transform.position.z >= 61f;
        string[] stops = outdoors ? OutdoorStops : BuildingStops;
        float[] positions = outdoors ? OutdoorZ : BuildingZ;
        float width = Mathf.Min(475,Screen.width-24);
        float height = Mathf.Min(520,Screen.height-24);
        var frame = new Rect((Screen.width-width)*.5f,(Screen.height-height)*.5f,width,height);
        var plan = new Rect(frame.x+18,frame.y+66,frame.width-36,frame.height-144);
        float left = outdoors ? -23f : -10f;
        float right = outdoors ? 13f : 10f;
        float near = outdoors ? 56f : 0f;
        float far = outdoors ? 113f : 61f;
        Vector2 Project(Vector3 point) => new(
            Mathf.Lerp(plan.xMin,plan.xMax,Mathf.InverseLerp(left,right,point.x)),
            Mathf.Lerp(plan.yMax,plan.yMin,Mathf.InverseLerp(near,far,point.z)));

        _mapText ??= new GUIStyle(_small) { alignment = TextAnchor.MiddleLeft };
        MapFill(frame,MapBackground);
        GUI.Label(new Rect(frame.x+18,frame.y+12,frame.width-36,26),
            outdoors ? "PLAN DU SITE EXTÉRIEUR" : "PLAN DU BÂTIMENT",_heading);
        GUI.Label(new Rect(frame.x+18,frame.y+36,frame.width-36,24),"Vue de dessus · schéma de la visite",_small);
        MapFill(plan,MapZone);

        var bottom = Project(new Vector3(0,0,near));
        var top = Project(new Vector3(0,0,far));
        MapFill(new Rect(top.x-2,top.y,4,bottom.y-top.y),MapPath);
        if (outdoors)
        {
            var bank = Project(new Vector3(-15.5f,0,102f));
            MapFill(new Rect(bank.x-19,plan.y,38,bank.y-plan.y),new Color(.10f,.37f,.49f));
            GUI.Label(new Rect(bank.x-29,plan.y+2,75,23),"Rivière",_small);
        }
        else
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var rowEnd = Project(new Vector3(side*5f,0,40));
                var rowStart = Project(new Vector3(side*5f,0,23));
                MapFill(new Rect(rowEnd.x-14,rowEnd.y,28,rowStart.y-rowEnd.y),MapPath);
            }
        }

        for (int i=0; i<stops.Length; i++)
        {
            var pin = Project(new Vector3(0,0,positions[i]));
            MapFill(new Rect(pin.x-3,pin.y-3,6,6),Color.white);
            float captionWidth = plan.xMax-pin.x-10;
            GUI.Label(new Rect(pin.x+10,Mathf.Clamp(pin.y-11,plan.y,plan.yMax-22),captionWidth,22),stops[i],_mapText);
        }

        Collider target = GuideTarget;
        if (target)
        {
            var pin = Project(target.bounds.center);
            MapFill(new Rect(pin.x-6,pin.y-6,12,12),MapTarget);
        }

        Color playerColor = _heading.normal.textColor;
        var position = Project(player.transform.position);
        MapFill(new Rect(position.x-5,position.y-5,10,10),playerColor);
        var direction = player.transform.forward;
        var tip = position + new Vector2(direction.x,-direction.z).normalized*13f;
        MapFill(new Rect(tip.x-2,tip.y-2,4,4),playerColor);
        MapFill(new Rect(frame.x+18,plan.yMax+17,8,8),playerColor);
        GUI.Label(new Rect(frame.x+31,plan.yMax+8,110,26),"Votre position",_small);
        MapFill(new Rect(frame.x+155,plan.yMax+17,8,8),MapTarget);
        GUI.Label(new Rect(frame.x+168,plan.yMax+8,170,26),"Prochaine action",_small);
        GUI.Label(new Rect(frame.x+18,plan.yMax+35,frame.width-36,26),"Relâchez M pour reprendre",_small);
    }

    static void MapFill(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect,Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
