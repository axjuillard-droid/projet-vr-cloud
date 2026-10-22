#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System.Collections;
using System.IO;
using UnityEngine;

public partial class DemoSmoke
{
    Component _cueExpected;
    IEnumerator InspectCue(Component expected)
    {
        yield return new WaitForEndOfFrame();
        var cue=FindFirstObjectByType<PCCrosshair>();var pc=Camera.main.GetComponentInParent<PCPlayerController>();
        if(pc.InputPaused)Check(cue!=null&&cue.CueVisible&&!cue.HasTarget&&cue.ActionHint.Contains("PAUSE")&&Time.timeScale==0&&AudioListener.pause,"Pause visible, sans cible active, temps et audio suspendus");
        else Check(cue!=null&&cue.CueVisible&&cue.HasTarget&&cue.FocusedTarget==expected&&cue.ActionHint.Length>0,"Aide contextuelle sur l'objet réellement visé : "+(expected!=null?expected.name:"aucun"));
    }
    IEnumerator CaptureInterface(string name)
    {
        // Opt-in seulement : deux essais de backbuffer non probants, revue humaine requise.
        if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--cloudvr-capture-interface")<0)yield break;
        yield return new WaitForEndOfFrame();
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
        var texture=ScreenCapture.CaptureScreenshotAsTexture();
        Check(texture!=null&&texture.width==Screen.width&&texture.height==Screen.height,"Capture complète de l'interface : "+name);
        try{File.WriteAllBytes(Path.Combine(_folder,name),texture.EncodeToPNG());}finally{Destroy(texture);}
    }
    IEnumerator ExerciseRoomPause(PhotoSender sender)
    {
        yield return InspectCue(sender.GetComponent<ScreenClickHandler>());yield return CaptureInterface("ux-room-focus.png");
        yield return DataPause();yield return InspectCue(null);
        Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);sender.Send();
        Check(sender.State==PhotoSender.SendState.Ready&&sender.photoObject.activeSelf,"Pause chambre : clic et appel Send ignorés, photo conservée");
        yield return CaptureInterface("ux-pause-room.png");yield return DataPause();
        Check(!Camera.main.GetComponentInParent<PCPlayerController>().InputPaused&&Time.timeScale==1&&!AudioListener.pause,"Reprise rétablit le temps et l'audio avant l'envoi");
        var cleanup=new GameObject("PauseCleanup_TEMP",typeof(CharacterController),typeof(PCPlayerController));cleanup.transform.position=Vector3.one*1000;
        var temporary=cleanup.GetComponent<PCPlayerController>();temporary.TogglePause();cleanup.SetActive(false);
        Check(!temporary.InputPaused&&Time.timeScale==1&&!AudioListener.pause,"Contrôle API : désactiver le rig libère la pause globale");Destroy(cleanup);
    }
    IEnumerator ExerciseAnimationPause(ServerBladeInteraction rack)
    {
        yield return DataPause();var rotation=rack.rackDoor.localRotation;yield return new WaitForSecondsRealtime(.3f);
        Check(rack.IsAnimating&&Quaternion.Angle(rotation,rack.rackDoor.localRotation)<.01f,"Pause pendant ouverture : porte sur charnière immobilisée");yield return DataPause();
    }
    IEnumerator ExerciseThermalPause(P1LearningModule module)
    {
        yield return DataPause();float temperature=module.InletTemperature;var fan=module.fan.localRotation;yield return new WaitForSecondsRealtime(.35f);
        Check(Mathf.Abs(temperature-module.InletTemperature)<.0001f&&Quaternion.Angle(fan,module.fan.localRotation)<.01f,"Pause thermique : température et ventilateur immobilisés");yield return DataPause();
    }
}
#endif
