using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Maquette hors ligne : chemins réseau, disponibilité et restauration distinctes.</summary>
public class DataServiceLearning : MonoBehaviour
{
    public enum NetworkState { Idle, Primary, Converging, Alternate, Restored }
    public enum StorageJob { None, Replication, RecoveryPoint, Restore }
    public DemoTour tour;
    public DataServiceTarget[] targets;
    public Transform networkPanel,storagePanel;
    public Text networkStatus,networkFeedback,storageStatus,storageFeedback;
    public LineRenderer[] networkPaths,storagePaths;
    public Transform[] networkSignals,storageSignals;
    public Renderer[] networkNodes,storageNodes;
    public Material dark,ready,warning,signal;
    public NetworkState NetworkPhase { get; private set; }
    public StorageJob Job { get; private set; }
    public bool NetworkStarted => NetworkPhase!=NetworkState.Idle;
    public bool ResponseRead { get; private set; }
    public bool NetworkCompleted { get; private set; }
    public bool StorageStarted { get; private set; }
    public bool ReplicaReady { get; private set; }
    public bool RecoveryReady { get; private set; }
    public bool PrimaryOnline { get; private set; }=true;
    public bool ReplicaHasPhoto { get; private set; }
    public bool ReadFromReplica { get; private set; }
    public bool Deleted { get; private set; }
    public bool PhotoRestored { get; private set; }
    public bool StorageCompleted { get; private set; }
    public int RefusedChoices { get; private set; }
    public float Progress => Job==StorageJob.None?0:Mathf.Clamp01(_jobTime/2);
    float _networkTime,_jobTime,_motion,_nextAction;
    bool Paused
    {get{var pc=Camera.main!=null?Camera.main.GetComponentInParent<PCPlayerController>():null;return pc!=null&&pc.InputPaused;}}
    void Start()
    {
        networkFeedback.text="Borne 01 : observer deux chemins possibles vers le service.\nMaquette de principe, aucun trafic réel mesuré.";
        storageFeedback.text="Borne 03 : copies disponibles et point de restauration ont des rôles distincts.\nAucun fichier personnel n'est enregistré.";
        Draw();
    }
    void Update()
    {
        if(Paused)return;
        var camera=Camera.main;var pc=camera!=null?camera.GetComponentInParent<PCPlayerController>():null;
        bool vr=InputModeManager.Instance!=null&&InputModeManager.Instance.IsVR;
        if(!vr&&pc!=null&&((Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)||(Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame))&&
            P1LearningModule.TryPick(camera.ViewportPointToRay(new Vector3(.5f,.5f,0)),pc.transform,out var hit))
            hit.collider.GetComponentInParent<DataServiceTarget>()?.Activate();
        _motion+=Time.deltaTime;
        if(NetworkPhase==NetworkState.Converging&&(_networkTime+=Time.deltaTime)>=2)
        {NetworkPhase=NetworkState.Alternate;networkFeedback.text="CHEMIN B DISPONIBLE\nLe routage s'est adapté dans cette maquette. Lisez la réponse au nœud SERVICE. Aucun fichier n'est encore écrit.";}
        if(Job!=StorageJob.None)
        {
            _jobTime+=Time.deltaTime;
            if(_jobTime>=2)
            {
                if(Job==StorageJob.Replication){ReplicaReady=true;ReplicaHasPhoto=true;storageFeedback.text="RÉPLIQUE B PRÊTE\nUne copie asynchrone peut avoir du retard. Ici la copie v1 est terminée ; créez maintenant le point de restauration C.";}
                else if(Job==StorageJob.RecoveryPoint){RecoveryReady=true;storageFeedback.text="POINT C / PHOTO v1 CONSERVÉ\nCopie distincte dans ce scénario ; droits, rétention et tests de restauration comptent aussi. Isolez maintenant A sur la maquette.";}
                else {ReplicaHasPhoto=true;PhotoRestored=true;storageFeedback.text="RESTAURATION TERMINÉE SUR B\nA reste indisponible. Relisez la photo depuis B pour vérifier le service restauré ; le point C demeure conservé.";}
                Job=StorageJob.None;
            }
        }
        AnimateSignals();Draw();
    }
    public void BeginNetwork()
    {
        if(NetworkStarted)return;NetworkPhase=NetworkState.Primary;
        networkFeedback.text="SIGNAL SUR A\nSélectionnez le nœud LIEN A pour illustrer son indisponibilité. Le chemin B reste possible ; basculement automatique illustré.";Draw();
    }
    public void BeginStorage()
    {
        if(StorageStarted)return;StorageStarted=true;
        storageFeedback.text="PHOTO v1 SUR A\nSélectionnez B pour répliquer, puis C pour conserver un point de restauration distinct. Les 2 s de copie sont fictives.";Draw();
    }
    void Refuse(string text,bool network)
    {RefusedChoices++;if(network)networkFeedback.text=text;else storageFeedback.text=text;}
    public void Activate(DataServiceTarget.Action action)
    {
        if(Paused||Time.unscaledTime<_nextAction)return;_nextAction=Time.unscaledTime+.25f;
        bool network=(int)action<=2;
        if(network)
        {
            if(!NetworkStarted||NetworkCompleted)return;
            if(action==DataServiceTarget.Action.IsolateLink&&NetworkPhase==NetworkState.Primary)
            {NetworkPhase=NetworkState.Converging;_networkTime=0;networkFeedback.text="LIEN A INDISPONIBLE\nAdaptation du routage : 2 s fictives, service momentanément en attente. Les marqueurs ne sont pas des paquets capturés.";}
            else if(action==DataServiceTarget.Action.ReadResponse)
            {
                if(NetworkPhase!=NetworkState.Alternate){Refuse("Observez d'abord la perte du lien A et attendez le chemin B avant de lire la réponse.",true);return;}
                ResponseRead=true;networkFeedback.text="RÉPONSE DU SERVICE ILLUSTRÉE SUR B\nJoindre le service ne prouve pas l'écriture d'une photo. Rétablissez A avant de poursuivre vers le traitement.";
            }
            else if(action==DataServiceTarget.Action.RestoreLink)
            {
                if(!ResponseRead){Refuse("Lisez d'abord la réponse au nœud SERVICE lorsque B est disponible.",true);return;}
                NetworkPhase=NetworkState.Restored;NetworkCompleted=true;
                networkFeedback.text="DEUX CHEMINS DISPONIBLES\nLa redondance limite certaines interruptions ; réseau, service et stockage restent trois niveaux distincts.";
                tour.CompleteDataStep(0,"Chemin B observé, réponse lue et lien A rétabli. Le service doit encore traiter puis conserver la photo.");
            }
        }
        else
        {
            if(!StorageStarted||StorageCompleted)return;
            if(Job!=StorageJob.None){Refuse("Copie en cours : attendez sa fin. Une copie lancée n'est pas une copie déjà disponible.",false);return;}
            if(action==DataServiceTarget.Action.Replicate)
            {
                if(ReplicaReady||!PrimaryOnline||Deleted){Refuse("La réplique initiale est déjà préparée ; poursuivez avec le point C, la panne puis la restauration.",false);return;}
                Job=StorageJob.Replication;_jobTime=0;storageFeedback.text="RÉPLICATION A → B EN COURS\nCopie v1 asynchrone ; la réplique reste indisponible jusqu'à la fin.";
            }
            else if(action==DataServiceTarget.Action.RecoveryPoint)
            {
                if(!ReplicaReady||RecoveryReady||!PrimaryOnline||Deleted){Refuse("Préparez d'abord B ; le point C doit être conservé avant l'incident.",false);return;}
                Job=StorageJob.RecoveryPoint;_jobTime=0;storageFeedback.text="POINT DE RESTAURATION A → C\nPhoto v1 ; copie distincte, sans effacement automatique par les actions suivantes du scénario.";
            }
            else if(action==DataServiceTarget.Action.FailPrimary)
            {
                if(!ReplicaReady||!RecoveryReady){Refuse("Préparez B et C avant d'illustrer la panne. Une protection doit exister avant l'incident.",false);return;}
                PrimaryOnline=false;storageFeedback.text="A INDISPONIBLE • B DISPONIBLE\nLe service utilise la réplique v1. Sélectionnez LIRE PHOTO pour l'observer. Le point C n'a pas été nécessaire pour cette panne.";
            }
            else if(action==DataServiceTarget.Action.ReadPhoto)
            {
                if(PrimaryOnline){Refuse("A répond encore. Isolez A après préparation de B et C pour observer le rôle de la réplique.",false);return;}
                if(!ReplicaHasPhoto){Refuse("PHOTO ABSENTE DU SERVICE\nLa suppression logique a atteint B. Une réplique à jour ne fournit plus la version supprimée ; restaurez le point C.",false);return;}
                ReadFromReplica=true;
                if(PhotoRestored)
                {
                    StorageCompleted=true;storageFeedback.text="PHOTO v1 RELUE SUR B\nDisponibilité après panne et récupération après suppression ont été observées. A reste en panne ; sa réparation n'est pas simulée.";
                    tour.CompleteDataStep(2,"Photo v1 retrouvée après restauration du point C sur B. La réplication seule ne remplace pas une stratégie de sauvegarde.");
                }
                else storageFeedback.text="PHOTO v1 LUE SUR B\nLa panne de A n'empêche pas cette lecture. Observez maintenant SUPPRESSION PROPAGÉE pour distinguer disponibilité et sauvegarde.";
            }
            else if(action==DataServiceTarget.Action.DeleteCurrent)
            {
                if(!ReadFromReplica||Deleted){Refuse("Lisez d'abord la photo depuis B après la panne de A.",false);return;}
                Deleted=true;ReplicaHasPhoto=false;
                storageFeedback.text="SUPPRESSION LOGIQUE PROPAGÉE À B\nOption de ce scénario, pas une règle universelle. A reste hors ligne ; C conserve v1. Lisez le résultat, puis restaurez C → B.";
            }
            else if(action==DataServiceTarget.Action.RestorePhoto)
            {
                if(!RecoveryReady||!Deleted||PhotoRestored){Refuse("Le point C s'utilise ici après la suppression propagée. Il doit avoir été préparé avant elle.",false);return;}
                Job=StorageJob.Restore;_jobTime=0;storageFeedback.text="RESTAURATION C → B EN COURS\nTemps fictif ; la photo ne redevient accessible qu'à la fin, puis après vérification de lecture.";
            }
        }
        Draw();
    }
    void AnimateSignals()
    {
        bool network=NetworkPhase==NetworkState.Primary||NetworkPhase==NetworkState.Alternate||NetworkPhase==NetworkState.Restored;
        for(int i=0;i<networkSignals.Length;i++)
        {
            networkSignals[i].gameObject.SetActive(network);
            if(network)Move(networkSignals[i],networkPaths[NetworkPhase==NetworkState.Alternate?1:0],Mathf.Repeat(_motion*.4f+i/(float)networkSignals.Length,1));
        }
        for(int i=0;i<storageSignals.Length;i++)
        {
            storageSignals[i].gameObject.SetActive(Job!=StorageJob.None);
            if(Job!=StorageJob.None)Move(storageSignals[i],storagePaths[(int)Job-1],Mathf.Repeat(_jobTime*.5f+i/(float)storageSignals.Length,1));
        }
    }
    static void Move(Transform dot,LineRenderer path,float t)
    {
        float segment=t*(path.positionCount-1);int index=Mathf.Min((int)segment,path.positionCount-2);
        dot.position=path.transform.TransformPoint(Vector3.Lerp(path.GetPosition(index),path.GetPosition(index+1),segment-index));
    }
    void Draw()
    {
        networkStatus.text="RÉSEAU • MAQUETTE DE PRINCIPE\n"+(NetworkPhase==NetworkState.Idle?"En attente de la borne 01":NetworkPhase==NetworkState.Converging?"A indisponible • adaptation en cours":NetworkPhase==NetworkState.Alternate?"A indisponible • signal sur B":"A et B disponibles • signal sur A");
        networkPaths[0].sharedMaterial=NetworkPhase==NetworkState.Converging||NetworkPhase==NetworkState.Alternate?warning:signal;
        networkPaths[1].sharedMaterial=NetworkPhase==NetworkState.Alternate?ready:dark;
        networkNodes[0].sharedMaterial=NetworkPhase==NetworkState.Converging||NetworkPhase==NetworkState.Alternate?warning:signal;
        networkNodes[1].sharedMaterial=NetworkPhase==NetworkState.Alternate?ready:dark;
        networkNodes[2].sharedMaterial=ResponseRead?ready:signal;
        storageStatus.text="STOCKAGE • PHOTO v1 / SCÉNARIO\nA : "+(PrimaryOnline?"disponible":"hors ligne")+" • B : "+(ReplicaHasPhoto?"v1 disponible":ReplicaReady?"photo absente":"non préparée")+" • C : "+(RecoveryReady?"v1 conservée":"absent")+(Job!=StorageJob.None?"\nCopie : "+(Progress*100).ToString("F0")+" % • temps fictif":"");
        storageNodes[0].sharedMaterial=PrimaryOnline?signal:warning;storageNodes[1].sharedMaterial=ReplicaHasPhoto?ready:dark;storageNodes[2].sharedMaterial=RecoveryReady?ready:dark;
    }
}
