#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Branche confort VR sans casque : appels API, aucune validation de tracking matériel.</summary>
public class NetworkComfortSmoke : MonoBehaviour
{
    [Serializable] class Report {public bool passed;public string error;public List<string> checks=new List<string>();}
    readonly Report _report=new Report();string _folder;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--cloudvr-network-comfort-smoke")<0)return;
        var p=new GameObject("NetworkComfortSmoke_TEMP").AddComponent<NetworkComfortSmoke>();DontDestroyOnLoad(p);p.StartCoroutine(p.Guard());
    }
    IEnumerator Guard()
    {
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--cloudvr-smoke-output");_folder=args[index+1];
        var iterator=Exercise();
        while(true)
        {
            object step=null;bool next;
            try{next=iterator.MoveNext();if(next)step=iterator.Current;}
            catch(Exception e){End(false,e.ToString());yield break;}
            if(!next){End(true,"");yield break;}yield return step;
        }
    }
    void Check(bool value,string text){if(!value)throw new InvalidOperationException(text);_report.checks.Add(text);}
    IEnumerator Exercise()
    {
        Application.runInBackground=true;yield return new WaitForSecondsRealtime(1);
        Check(!UnityEngine.XR.XRSettings.isDeviceActive,"Aucun casque actif : essai de logique par API uniquement");
        InputModeManager.Instance.SetMode(InputModeManager.InputMode.VR);
        var camera=Camera.main;var pos=camera.transform.position;var rot=camera.transform.rotation;
        var ride=FindFirstObjectByType<NetworkTransitionV2>();var sender=FindFirstObjectByType<PhotoSender>();sender.Send();
        float until=Time.realtimeSinceStartup+8;while(!ride.IsRunning&&Time.realtimeSinceStartup<until)yield return null;
        Check(ride.IsRunning&&NetworkTransitionV2.UsedComfortScreen,"Branche VR : écran de cinéma choisi sans déplacement imposé");
        var screen=GameObject.Find("P2_Ecran_Confort");var anchor=screen.transform.position;
        Check(screen!=null&&GameObject.Find("P2_Camera_Film").GetComponent<Camera>().targetTexture!=null,"Film 3D rendu dans une texture sur écran du monde");
        while(ride.CurrentStep<3&&Time.realtimeSinceStartup<until)yield return null;
        Check(Vector3.Distance(pos,camera.transform.position)<.001f&&Quaternion.Angle(rot,camera.transform.rotation)<.1f,"Transform du casque inchangé pendant le vol virtuel");
        var simulated=rot*Quaternion.Euler(0,12,0);camera.transform.rotation=simulated;
        yield return new WaitForSecondsRealtime(.2f);
        Check(Quaternion.Angle(simulated,camera.transform.rotation)<.1f&&Vector3.Distance(anchor,screen.transform.position)<.001f,"Orientation simulée conservée et écran ancré dans le monde");
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(_folder,"network-comfort.png"));
        until=Time.realtimeSinceStartup+10;while(SceneManager.GetActiveScene().name!="DataCenter"&&Time.realtimeSinceStartup<until)yield return null;
        yield return new WaitForSecondsRealtime(.5f);
        Check(SceneManager.GetActiveScene().name=="DataCenter"&&InputModeManager.Instance.IsVR,"Arrivée DataCenter en mode VR conservé");
        Check(NetworkTransitionV2.CompletedStages==6&&NetworkTransitionV2.TravelDistance>50,"Film complet et caméra de film réellement déplacée");
        Check(GameObject.Find("P2_Camera_Film")==null&&GameObject.Find("P2_Ecran_Confort")==null,"Caméra de film et écran détruits après chargement");
    }
    void End(bool passed,string error)
    {
        _report.passed=passed;_report.error=error;File.WriteAllText(Path.Combine(_folder,"network-comfort-result.json"),JsonUtility.ToJson(_report,true));
        Debug.Log("[NetworkComfortSmoke] passed="+passed+" "+error);
        if(!Application.isEditor)Application.Quit(passed?0:1);
    }
}
#endif
