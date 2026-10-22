using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Voyage pédagogique, échelles et durées fictives ; aucun transfert réel.</summary>
public class NetworkTransitionV2 : MonoBehaviour
{
    public GameObject hudPanel;
    public Text hudTitle, hudSubtitle, hudProgress;
    public Image progressBar, fade;
    public StepTransition stepTransition;
    public Transform tourPC, boxInternet;
    public NetworkRideVisuals visuals;
    public float stepDuration=1.25f;
    public bool IsRunning { get; private set; }
    public int CurrentStep { get; private set; }
    public static int CompletedStages { get; private set; }
    public static float CompletedDuration { get; private set; }
    public static float TravelDistance { get; private set; }
    public static int Starts { get; private set; }
    public static bool UsedComfortScreen { get; private set; }
    PCPlayerController _controller;
    ChairSeatInteraction _seat;
    bool _controllerWasEnabled,_seatWasEnabled,_saved;
    Camera _camera,_filmCamera;
    Vector3 _position;
    Quaternion _rotation;
    CameraClearFlags _clear;
    Color _background;
    int _mask;
    float _far;
    RenderTexture _film;
    GameObject _screen;
    Material _screenMaterial;
    AudioClip _clip;
    readonly List<GameObject> _hidden=new List<GameObject>();
    static readonly string[] Titles={"De votre photo à la box","Accès de l'opérateur","Réseau de collecte","Transit et routage","Réseau du site","Arrivée au data center"};
    static readonly string[] Subtitles={
        "Départ du réseau local. Stockage encore à découvrir.",
        "Signaux lumineux • fibre représentée à une échelle fictive.",
        "Les échanges des utilisateurs rejoignent le réseau de collecte.",
        "Les routeurs orientent les paquets vers le service.",
        "La demande rejoint le réseau du data center.",
        "Arrivée réseau ≠ fichier enregistré. Explorez calcul et stockage."};
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEvidence(){CompletedStages=0;CompletedDuration=0;TravelDistance=0;Starts=0;UsedComfortScreen=false;}
    public void StartNetworkRide()
    {
        if(IsRunning||stepTransition==null||stepTransition.IsTriggered||visuals==null)return;
        IsRunning=true;Starts++;StartCoroutine(Play());
    }
    IEnumerator Play()
    {
        _camera=Camera.main;
        if(_camera==null){IsRunning=false;yield break;}
        _position=_camera.transform.position;_rotation=_camera.transform.rotation;
        _clear=_camera.clearFlags;_background=_camera.backgroundColor;_mask=_camera.cullingMask;_far=_camera.farClipPlane;_saved=true;
        _controller=_camera.GetComponentInParent<PCPlayerController>();_controllerWasEnabled=_controller!=null&&_controller.enabled;
        if(_controller!=null)_controller.enabled=false;
        _seat=FindFirstObjectByType<ChairSeatInteraction>();_seatWasEnabled=_seat!=null&&_seat.enabled;
        if(_seat!=null)_seat.enabled=false;
        foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if(canvas.gameObject.activeSelf&&!canvas.transform.IsChildOf(transform)){_hidden.Add(canvas.gameObject);canvas.gameObject.SetActive(false);}
        bool vr=InputModeManager.Instance!=null&&InputModeManager.Instance.IsVR;
        UsedComfortScreen=vr;
        var hud=hudPanel.GetComponent<Canvas>();hud.renderMode=RenderMode.ScreenSpaceOverlay;
        if(vr)ConfigureComfortScreen(hud);
        hudPanel.SetActive(true);CompletedStages=0;TravelDistance=0;
        float started=Time.unscaledTime;
        Vector3 previous=Vector3.zero;bool wasInWorld=false;
        var audio=gameObject.AddComponent<AudioSource>();audio.spatialBlend=0;
        _clip=ProceduralAudioService.CreateFiberTransitSound();audio.clip=_clip;audio.loop=true;audio.volume=.22f;audio.Play();
        for(int i=0;i<6;i++)
        {
            CurrentStep=i+1;hudTitle.text=Titles[i];hudSubtitle.text=Subtitles[i];
            hudProgress.text="ÉTAPE "+CurrentStep+" / 6 • échelle et durée fictives"+(vr?" • écran confort":"");
            Debug.Log("[NetworkTransitionV2] Étape "+CurrentStep+"/6 : "+Titles[i]);
            float elapsed=0;
            while(elapsed<stepDuration)
            {
                elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/stepDuration);
                float progress=(i+t)/6;progressBar.rectTransform.localScale=new Vector3(progress,1,1);
                // Coupes masquées aux changements d'échelle, aucun roulis imposé.
                float veil=i==0?Mathf.InverseLerp(.82f,1,t):i==1?1-Mathf.InverseLerp(0,.16f,t):i==4?Mathf.InverseLerp(.84f,1,t):i==5?Mathf.Max(1-Mathf.InverseLerp(0,.16f,t),Mathf.InverseLerp(.84f,1,t)):0;
                fade.color=new Color(.015f,.025f,.04f,veil);
                if(i==0&&!vr)
                {
                    Vector3 focus=boxInternet.position+Vector3.up*.12f;
                    Vector3 end=focus+(_position-focus).normalized*.9f;
                    _camera.transform.SetPositionAndRotation(Vector3.Lerp(_position,end,Mathf.SmoothStep(0,1,t)),Quaternion.Slerp(_rotation,Quaternion.LookRotation(focus-end),Mathf.SmoothStep(0,1,t)));
                }
                if(i>0)
                {
                    visuals.gameObject.SetActive(true);
                    var viewer=vr?_filmCamera:_camera;
                    viewer.clearFlags=CameraClearFlags.SolidColor;viewer.backgroundColor=new Color(.015f,.025f,.04f);viewer.cullingMask=1<<NetworkRideVisuals.Layer;viewer.farClipPlane=250;
                    float flight=Mathf.Clamp01((i-1+t)/4);
                    Vector3 p=i<5?new Vector3(.18f*Mathf.Sin(flight*Mathf.PI),3.6f,2+flight*54):new Vector3(19,10,63+t*5);
                    Quaternion rotation=i<5?Quaternion.identity:Quaternion.LookRotation(new Vector3(0,7,100)-p);
                    viewer.transform.SetPositionAndRotation(visuals.transform.TransformPoint(p),visuals.transform.rotation*rotation);
                    if(wasInWorld)TravelDistance+=Vector3.Distance(p,previous);previous=p;wasInWorld=true;
                    visuals.Animate(Time.unscaledTime-started);
                }
                yield return null;
            }
            CompletedStages++;
        }
        audio.Stop();CompletedDuration=Time.unscaledTime-started;
        hudProgress.text="Réseau rejoint • stockage à découvrir";
        Debug.Log("[NetworkTransitionV2] Six étapes 3D terminées en "+CompletedDuration.ToString("F2")+"s.");
        stepTransition.TriggerTransition();
    }
    void ConfigureComfortScreen(Canvas hud)
    {
        _film=new RenderTexture(1280,720,24);_film.Create();
        _filmCamera=new GameObject("P2_Camera_Film").AddComponent<Camera>();_filmCamera.targetTexture=_film;_filmCamera.fieldOfView=65;_filmCamera.stereoTargetEye=StereoTargetEyeMask.None;
        _filmCamera.cullingMask=1<<NetworkRideVisuals.Layer;_filmCamera.clearFlags=CameraClearFlags.SolidColor;_filmCamera.backgroundColor=new Color(.015f,.025f,.04f);
        _filmCamera.transform.SetPositionAndRotation(visuals.transform.TransformPoint(new Vector3(0,3.6f,2)),Quaternion.identity);
        _screen=GameObject.CreatePrimitive(PrimitiveType.Quad);_screen.name="P2_Ecran_Confort";Destroy(_screen.GetComponent<Collider>());
        var forward=Vector3.ProjectOnPlane(_camera.transform.forward,Vector3.up).normalized;
        if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
        _screen.transform.SetPositionAndRotation(_camera.transform.position+forward*2.4f,Quaternion.LookRotation(forward));_screen.transform.localScale=new Vector3(2.4f,1.35f,1);
        _screenMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));_screenMaterial.mainTexture=_film;_screen.GetComponent<Renderer>().sharedMaterial=_screenMaterial;
        hud.renderMode=RenderMode.WorldSpace;hud.worldCamera=_camera;hudPanel.transform.SetParent(_screen.transform,false);
        hudPanel.transform.localPosition=new Vector3(0,0,-.01f);hudPanel.transform.localScale=new Vector3(1f/1280,1f/720,1);
        hudPanel.GetComponent<RectTransform>().sizeDelta=new Vector2(1280,720);
        // Aucun transform du rig/casque modifié. Écran ancré au monde, pas à la tête.
    }
    void OnDisable()
    {
        StopAllCoroutines();
        if(_saved&&_camera!=null)
        {
            if(!UsedComfortScreen)_camera.transform.SetPositionAndRotation(_position,_rotation);
            _camera.clearFlags=_clear;_camera.backgroundColor=_background;_camera.cullingMask=_mask;_camera.farClipPlane=_far;
        }
        if(_controller!=null)_controller.enabled=_controllerWasEnabled;
        if(_seat!=null)_seat.enabled=_seatWasEnabled;
        foreach(var go in _hidden)if(go!=null)go.SetActive(true);_hidden.Clear();
        if(visuals!=null)visuals.gameObject.SetActive(false);
        if(_filmCamera!=null)Destroy(_filmCamera.gameObject);
        if(_film!=null){_film.Release();Destroy(_film);}
        if(_screen!=null){if(hudPanel!=null)hudPanel.transform.SetParent(transform,false);Destroy(_screen);}
        if(_screenMaterial!=null)Destroy(_screenMaterial);
        if(_clip!=null)Destroy(_clip);
        if(hudPanel!=null)hudPanel.SetActive(false);
        IsRunning=false;_saved=false;
    }
}
