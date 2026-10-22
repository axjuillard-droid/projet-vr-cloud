using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Accueil et conclusion de la boucle de démonstration.</summary>
public class DemoRoom : MonoBehaviour
{
    void Start()
    {
        var camera=Camera.main;
        if (camera==null) return;
        var go=new GameObject("Demo_Accueil",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        go.transform.SetParent(transform,false);
        var canvas=go.GetComponent<Canvas>();
        bool vr=InputModeManager.Instance!=null && InputModeManager.Instance.IsVR;
        canvas.renderMode=vr ? RenderMode.WorldSpace : RenderMode.ScreenSpaceCamera;
        canvas.worldCamera=camera; canvas.planeDistance=.65f;
        if (vr)
        {
            go.transform.SetParent(camera.transform,false); go.transform.localPosition=new Vector3(0,-.45f,2);
            go.transform.localScale=Vector3.one*.0016f; go.GetComponent<RectTransform>().sizeDelta=new Vector2(1280,720);
        }
        else
        {
            var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
        }
        var panel=DemoTour.Panel(go.transform,"Introduction",new Vector2(.03f,.76f),new Vector2(.97f,.97f));
        var title=DemoTour.Label(panel,"Titre",26,new Vector2(.025f,.56f),new Vector2(.975f,.98f),new Color(.4f,.9f,1));
        var body=DemoTour.Label(panel,"Texte",21,new Vector2(.025f,.02f),new Vector2(.975f,.55f),Color.white);
        title.text=DemoSession.Completed ? "PHOTO RETROUVÉE  /  FIN DE LA DÉMONSTRATION" : "CLOUDVR  /  OÙ VA VOTRE PHOTO ?";
        body.text=DemoSession.Completed ?
            "Le cloud dépend de machines, d'énergie et d'équipes. Matériel, énergie et travail ont un coût et un impact.\nLe lieu d'hébergement ne suffit pas à déterminer qui contrôle les données. R : recommencer." :
            "Visez l'écran du bureau et cliquez pour envoyer la photo d'exemple, puis découvrez six équipements.\nDéplacement : WASD / ZQSD / flèches • souris : regarder • Échap : pause / reprendre hors cinématique. Démo hors ligne.";
        if (DemoSession.Completed)
        {
            var screen=FindFirstObjectByType<ScreenClickHandler>(); if (screen!=null) screen.enabled=false;
            var sender=FindFirstObjectByType<PhotoSender>();
            if (sender!=null)
            {
                var xr=sender.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>(); if(xr!=null) xr.enabled=false;
                if(sender.photoObject!=null) sender.photoObject.SetActive(true);
            }
        }
    }
    void Update()
    {
        if(InputModeManager.Instance!=null && InputModeManager.Instance.IsVR) return;
        var keyboard=Keyboard.current;
        if(keyboard==null) return;
        if(keyboard.escapeKey.wasPressedThisFrame)
        {
            Camera.main?.GetComponentInParent<PCPlayerController>()?.TogglePause();
        }
        if(DemoSession.Completed && keyboard.rKey.wasPressedThisFrame) Replay();
    }
    public void Replay()
    {
        var pc=Camera.main!=null?Camera.main.GetComponentInParent<PCPlayerController>():null;if(pc!=null&&pc.InputPaused)return;
        DemoSession.Completed=false;
        InputModeManager.Instance?.PrepareNextScene();
        SceneManager.LoadSceneAsync("Chambre");
    }
}
