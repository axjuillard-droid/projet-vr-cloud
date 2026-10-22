#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class NetworkRideValidation
{
    [Serializable] class Report { public bool passed;public List<string> checks=new List<string>(); }
    public static void Validate(string output)
    {
        var report=new Report();var ride=UnityEngine.Object.FindFirstObjectByType<NetworkTransitionV2>();
        try
        {
            Check(ride!=null&&ride.visuals!=null&&ride.fade!=null,"Transition reliée au décor 3D et au fondu",report);
            Check(ride.hudPanel.layer==NetworkRideVisuals.Layer,"Narration visible dans le masque de rendu du voyage",report);
            Check(Mathf.Abs(ride.stepDuration*6-7.5f)<.01f,"Six étapes totalisant 7,5 secondes narratives",report);
            Check(!ride.visuals.gameObject.activeSelf,"Décor de film inactif au repos",report);
            Check(ride.visuals.packets.Length==16,"Seize signaux animés référencés",report);
            Check(ride.visuals.transform.childCount>=20,"Tunnel réutilisé et campus présents",report);
            var bound=ride.visuals.transform.Find("P2_Fibre_0").GetComponentsInChildren<MeshRenderer>(true)[0].bounds;
            Check(bound.size.z>19&&bound.size.x<8,"Axe importé de la fibre corrigé vers Z",report);
            foreach(var r in ride.visuals.GetComponentsInChildren<MeshRenderer>(true))
                foreach(var mat in r.sharedMaterials)if(mat==null||mat.shader==null||!mat.shader.isSupported)throw new InvalidOperationException("Matériau de voyage absent ou non supporté : "+r.name);
            Check(true,"Tous les matériaux du film présents et supportés",report);
            var sender=UnityEngine.Object.FindFirstObjectByType<PhotoSender>();int routes=0;
            for(int i=0;i<sender.onSendCompleted.GetPersistentEventCount();i++)if(sender.onSendCompleted.GetPersistentTarget(i) is NetworkTransitionV2)routes++;
            Check(routes==1,"Une seule route persistante PhotoSender vers la transition",report);
            Check(UnityEngine.Object.FindObjectsByType<NetworkDataRide>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==0,"Ancien trajet sans bypass smoke retiré de la scène",report);
            Check(ride.stepTransition.nextSceneName=="DataCenter"&&ride.stepTransition.delayBeforeTransition==0,"Chargement DataCenter centralisé après la cinématique",report);
            report.passed=true;
        }
        finally{Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));}
    }
    static void Check(bool value,string text,Report report){if(!value)throw new InvalidOperationException(text);report.checks.Add(text);}
}
#endif
