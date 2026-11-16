using UnityEngine;

public sealed partial class PhotoV2Walkthrough
{
    sealed class AiNetworkRoute
    {
        public readonly Rect View;
        public readonly string Destination, Transport;
        public readonly Vector2[] Points;

        public AiNetworkRoute(Rect view, string destination, string transport, params Vector2[] points)
        {
            View = view;
            Destination = destination;
            Transport = transport;
            Points = points;
        }
    }

    const float AiAccessSeconds = 3.6f;
    const float AiTransportSeconds = 4.4f;
    const float AiArrivalSeconds = 2.1f;
    const float AiRouteSeconds = AiAccessSeconds + AiTransportSeconds + AiArrivalSeconds;
    const float AiReturnSeconds = 3.4f;
    static readonly Vector2 Paris = new(2.35f, 48.86f);
    static readonly AiNetworkRoute[] AiNetworkRoutes =
    {
        new(new Rect(-13, 41, 32, 15), "Site France", "Fibre terrestre · France",
            Paris, new Vector2(3.8f, 47.4f), new Vector2(4.84f, 45.76f)),
        new(new Rect(-93, 22, 109, 43), "Site États-Unis · côte Est", "Fibre terrestre et traversée de l’Atlantique",
            Paris, new Vector2(-4, 48), new Vector2(-24, 48), new Vector2(-52, 45),
            new Vector2(-72, 40.8f), new Vector2(-77.5f, 39.03f)),
        new(new Rect(-8, -7, 141, 66), "Site Chine · côte Est", "Interconnexions terrestres et sous-marines",
            Paris, new Vector2(5.37f, 43.3f), new Vector2(17.5f, 35), new Vector2(32.5f, 30),
            new Vector2(43, 12.6f), new Vector2(67, 11), new Vector2(95, 5),
            new Vector2(104, 1.3f), new Vector2(113, 18), new Vector2(121.47f, 31.23f))
    };
    static readonly string[] AiLegNames = { "PC", "Wi-Fi", "Fibre", "FAI", "Transport", "Site" };
    static readonly string[] AiLegDescriptions =
    {
        "Le navigateur prépare la question à envoyer.",
        "Le PC envoie les paquets par radio jusqu’à la box.",
        "La box rejoint le réseau du fournisseur par la fibre d’accès.",
        "Le fournisseur d’accès relaie la demande vers d’autres réseaux.",
        "Les réseaux de transport se relaient jusqu’à la région du site.",
        "Le signal apparaît sur la fibre d’arrivée en façade."
    };

    static readonly Color AiRouteActive = new(.27f, .87f, 1);
    static readonly Color AiRouteFuture = new(.39f, .53f, .60f);
    Texture2D _aiLand;
    GUIStyle _aiLegLabel;
    float _aiReturnProgress;

    internal bool AiTransportMapLoaded => _aiLand;
    internal float AiTransportProgress =>
        Mathf.Clamp01((_travelProgress * AiRouteSeconds - AiAccessSeconds) / AiTransportSeconds);
    internal int AiRouteLeg
    {
        get
        {
            float elapsed = _travelProgress * AiRouteSeconds;
            return elapsed < AiAccessSeconds
                ? Mathf.Clamp(Mathf.FloorToInt(4 * elapsed / AiAccessSeconds), 0, 3)
                : elapsed < AiAccessSeconds + AiTransportSeconds ? 4 : 5;
        }
    }

    void DrawAiTransit()
    {
        int leg = AiRouteLeg;
        if (leg == 4)
        {
            DrawAiRouteMap(AiTransportProgress, false);
        }

        float width = Mathf.Min(760, Screen.width - 32);
        var panel = new Rect((Screen.width - width) * .5f, Screen.height - 132, width, 112);
        MapFill(panel, MapBackground);
        _aiLegLabel ??= new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter };
        float cell = (panel.width - 28) / AiLegNames.Length;
        for (int i = 0; i < AiLegNames.Length; i++)
        {
            float x = panel.x + 14 + i * cell;
            bool current = i == leg;
            MapFill(new Rect(x, panel.y + 12, cell - 5, 27), current ? AiRouteActive : MapZone);
            _aiLegLabel.normal.textColor = current ? MapBackground : Color.white;
            GUI.Label(new Rect(x, panel.y + 12, cell - 5, 27), AiLegNames[i], _aiLegLabel);
        }
        GUI.Label(new Rect(panel.x + 14, panel.y + 46, panel.width - 28, 27), AiLegDescriptions[leg], _instruction);
        GUI.Label(new Rect(panel.x + 14, panel.y + 79, panel.width - 28, 21),
            SelectedAiSite.Name + " · trajet illustratif", _small);
    }

    void DrawAiRouteMap(float progress, bool returning)
    {
        var route = AiNetworkRoutes[_aiSiteIndex];
        float width = Mathf.Min(810, Screen.width - 32);
        float height = Mathf.Min(375, Screen.height - 152);
        var panel = new Rect((Screen.width - width) * .5f, (Screen.height - height - 120) * .5f, width, height);
        var bounds = route.View;
        float mapHeight = Mathf.Min(height - 100, (width - 32) * bounds.height / bounds.width);
        float mapWidth = mapHeight * bounds.width / bounds.height;
        var area = new Rect(panel.center.x - mapWidth * .5f, panel.y + 65, mapWidth, mapHeight);
        MapFill(panel, MapBackground);
        GUI.Label(new Rect(panel.x + 16, panel.y + 9, width - 32, 28),
            returning ? "RETOUR DE LA RÉPONSE" : "RÉSEAU DE TRANSPORT", _heading);
        GUI.Label(new Rect(panel.x + 16, panel.y + 36, width - 32, 25), route.Transport, _small);
        if (_aiLand)
        {
            var uv = new Rect((bounds.x + 180) / 360, (bounds.y + 90) / 180,
                bounds.width / 360, bounds.height / 180);
            GUI.DrawTextureWithTexCoords(area, _aiLand, uv, false);
        }
        else MapFill(area, MapZone);

        Vector2 Project(Vector2 lonLat) => new(
            area.x + (lonLat.x - bounds.x) * area.width / bounds.width,
            area.yMax - (lonLat.y - bounds.y) * area.height / bounds.height);

        var points = route.Points;
        int count = points.Length;
        float total = 0;
        for (int i = 1; i < count; i++)
            total += Vector2.Distance(Project(points[i - 1]), Project(points[i]));
        float remaining = Mathf.Clamp01(progress) * total;
        Vector2 signal = Project(points[returning ? count - 1 : 0]);
        for (int i = 1; i < count; i++)
        {
            var start = Project(points[returning ? count - i : i - 1]);
            var end = Project(points[returning ? count - 1 - i : i]);
            float length = Vector2.Distance(start, end);
            DrawAiRouteLine(start, end, 2, AiRouteFuture);
            if (remaining <= 0) continue;
            signal = Vector2.Lerp(start, end, remaining / length);
            DrawAiRouteLine(start, signal, 3, AiRouteActive);
            remaining -= length;
        }
        DrawAiMapPin(Project(points[0]), "PC · France", area, true);
        DrawAiMapPin(Project(points[count - 1]), route.Destination, area, false);
        MapFill(new Rect(signal.x - 4, signal.y - 4, 8, 8), AiRouteActive);
        GUI.Label(new Rect(panel.x + 16, panel.yMax - 28, width - 32, 21),
            $"Trajet illustratif · aller-retour simulé {SelectedAiSite.NetworkMs} ms · Natural Earth", _small);
    }

    void DrawAiMapPin(Vector2 point, string label, Rect area, bool origin)
    {
        const float labelWidth = 150;
        float x = Mathf.Clamp(origin ? point.x - 90 : point.x + 8, area.x + 3, area.xMax - labelWidth);
        float y = origin ? point.y - 27 : point.y + 5;
        MapFill(new Rect(point.x - 2, point.y - 2, 4, 4), Color.white);
        GUI.Label(new Rect(x, y, labelWidth, 21), label, _small);
    }

    static void DrawAiRouteLine(Vector2 start, Vector2 end, float thickness, Color color)
    {
        var displacement = end - start;
        if (displacement.sqrMagnitude < .001f) return;
        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(Mathf.Atan2(displacement.y, displacement.x) * Mathf.Rad2Deg, start);
        MapFill(new Rect(start.x, start.y - thickness * .5f, displacement.magnitude, thickness), color);
        GUI.matrix = matrix;
    }
}
