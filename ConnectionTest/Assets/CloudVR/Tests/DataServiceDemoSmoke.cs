#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public partial class DemoSmoke
{
    void AimData(DataServiceLearning m,DataServiceTarget.Action action)
    {
        var camera=Camera.main;var pc=camera.GetComponentInParent<PCPlayerController>();pc.enabled=false;
        var cc=pc.GetComponent<CharacterController>();cc.enabled=false;pc.transform.position=new Vector3(0,.08f,(int)action<=2?8:19.4f);cc.enabled=true;
        var target=Array.Find(m.targets,t=>t.action==action);_cueExpected=target;camera.transform.LookAt(target.transform.position);Physics.SyncTransforms();
        bool picked=P1LearningModule.TryPick(camera.ViewportPointToRay(new Vector3(.5f,.5f,0)),pc.transform,out var hit)&&hit.collider.GetComponentInParent<DataServiceTarget>()==target;
        Check(picked,"Raycast données atteint "+action+" ; contact="+(hit.collider!=null?hit.collider.name:"aucun"));
    }
    IEnumerator DataPause()
    {
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape));yield return new WaitForSecondsRealtime(.12f);
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.08f);
    }
    IEnumerator WaitDataJob(DataServiceLearning m)
    {
        float until=Time.realtimeSinceStartup+4;while(m.Job!=DataServiceLearning.StorageJob.None&&Time.realtimeSinceStartup<until)yield return null;
        Check(m.Job==DataServiceLearning.StorageJob.None,"Copie de données terminée après attente, sans validation instantanée");
    }
    void CaptureData(DataServiceLearning m,bool network,string name)
    {Camera.main.transform.LookAt((network?m.networkPanel:m.storagePanel).position);Capture(name);}
    IEnumerator ExerciseDataNetwork(DemoTour tour)
    {
        var m=tour.data;Check(m.NetworkPhase==DataServiceLearning.NetworkState.Primary&&tour.Step==0,"Borne 01 démarre une observation, sans avancer le parcours");
        tour.Activate(0);Check(tour.Step==0&&tour.Actions==1,"Répéter la console réseau ne remplace pas l'observation");
        AimData(m,DataServiceTarget.Action.ReadResponse);yield return ClickFacility();
        Check(!m.ResponseRead&&tour.Step==0,"Réponse sur chemin B refusée avant la panne observée");
        AimData(m,DataServiceTarget.Action.IsolateLink);yield return InspectCue(_cueExpected);yield return CaptureInterface("ux-network-focus.png");yield return DataPause();yield return ClickFacility();
        Check(m.NetworkPhase==DataServiceLearning.NetworkState.Primary,"Pause : isolation réseau ignorée");yield return DataPause();
        AimData(m,DataServiceTarget.Action.IsolateLink);yield return ClickFacility();
        Check(m.NetworkPhase==DataServiceLearning.NetworkState.Converging&&!m.networkSignals[0].gameObject.activeSelf,"Perte du lien A : adaptation différée, signaux interrompus");
        CaptureData(m,true,"demo-network-link-down.png");
        AimData(m,DataServiceTarget.Action.RestoreLink);yield return ClickFacility();
        Check(!m.NetworkCompleted&&tour.Step==0,"Remise en service prématurée ne saute pas la lecture sur B");
        float until=Time.realtimeSinceStartup+4;while(m.NetworkPhase!=DataServiceLearning.NetworkState.Alternate&&Time.realtimeSinceStartup<until)yield return null;
        Check(m.NetworkPhase==DataServiceLearning.NetworkState.Alternate,"Chemin B automatiquement utilisé après adaptation fictive");
        yield return null;var position=m.networkSignals[0].position;yield return new WaitForSecondsRealtime(.15f);
        Check(m.networkSignals[0].gameObject.activeSelf&&Vector3.Distance(position,m.networkSignals[0].position)>.001f,"Signal animé en mouvement sur le chemin alternatif");
        CaptureData(m,true,"demo-network-alternate.png");
        AimData(m,DataServiceTarget.Action.ReadResponse);yield return ClickFacility();
        Check(m.ResponseRead&&!m.NetworkCompleted&&tour.Step==0,"Réponse lue sur B sans annoncer de stockage ni terminer avant remise en état");
        AimData(m,DataServiceTarget.Action.RestoreLink);yield return ClickFacility();
        Check(m.NetworkCompleted&&m.NetworkPhase==DataServiceLearning.NetworkState.Restored&&tour.Step==1,"Lien A rétabli avant passage au traitement");
        CaptureData(m,true,"demo-network-restored.png");
    }
    IEnumerator ExerciseDataStorage(DemoTour tour)
    {
        var m=tour.data;Check(m.StorageStarted&&!m.ReplicaReady&&!m.RecoveryReady&&tour.Step==2,"Borne 03 : photo v1 initiale, ni réplique ni point préparés");
        tour.Activate(2);Check(tour.Step==2,"Répéter la console stockage ne conserve pas automatiquement le fichier");
        AimData(m,DataServiceTarget.Action.FailPrimary);yield return ClickFacility();
        Check(m.PrimaryOnline&&!m.StorageCompleted,"Panne pédagogique refusée avant préparation des protections");
        AimData(m,DataServiceTarget.Action.RecoveryPoint);yield return ClickFacility();
        Check(!m.RecoveryReady&&m.Job==DataServiceLearning.StorageJob.None,"Point de restauration prématuré refusé avant la réplique guidée");
        AimData(m,DataServiceTarget.Action.RestorePhoto);yield return ClickFacility();
        Check(!m.PhotoRestored&&!m.StorageCompleted,"Restauration sans point et sans incident refusée");
        AimData(m,DataServiceTarget.Action.Replicate);yield return InspectCue(_cueExpected);yield return CaptureInterface("ux-storage-focus.png");yield return ClickFacility();
        Check(m.Job==DataServiceLearning.StorageJob.Replication&&!m.ReplicaReady&&!m.ReplicaHasPhoto,"Réplication asynchrone : B pas encore disponible au lancement");
        yield return DataPause();float progress=m.Progress;yield return new WaitForSecondsRealtime(.35f);
        Check(Mathf.Abs(m.Progress-progress)<.001f,"Pause : progression de copie suspendue");yield return DataPause();
        yield return WaitDataJob(m);Check(m.ReplicaReady&&m.ReplicaHasPhoto&&!m.RecoveryReady,"Photo v1 sur B, point C encore absent");
        AimData(m,DataServiceTarget.Action.RecoveryPoint);yield return ClickFacility();
        Check(m.Job==DataServiceLearning.StorageJob.RecoveryPoint&&!m.RecoveryReady,"Point C lui aussi préparé avec délai");yield return WaitDataJob(m);
        Check(m.RecoveryReady&&m.PrimaryOnline,"Point C conservé avant tout incident");
        AimData(m,DataServiceTarget.Action.FailPrimary);yield return ClickFacility();
        Check(!m.PrimaryOnline&&m.ReplicaHasPhoto&&m.RecoveryReady&&!m.StorageCompleted,"A en panne : B disponible, C conservé, activité encore ouverte");
        AimData(m,DataServiceTarget.Action.ReadPhoto);yield return ClickFacility();
        Check(m.ReadFromReplica&&!m.PhotoRestored&&!m.StorageCompleted,"Photo lue sur la réplique sans restaurer la sauvegarde");
        CaptureData(m,false,"demo-storage-replica.png");
        AimData(m,DataServiceTarget.Action.DeleteCurrent);yield return ClickFacility();
        Check(m.Deleted&&!m.ReplicaHasPhoto&&m.RecoveryReady&&!m.PrimaryOnline,"Suppression logique propagée à B, point C préservé et A toujours hors ligne");
        int refused=m.RefusedChoices;AimData(m,DataServiceTarget.Action.ReadPhoto);yield return ClickFacility();
        Check(m.RefusedChoices>refused&&!m.StorageCompleted,"Lecture du fichier supprimé refusée malgré la présence d'une réplique");
        CaptureData(m,false,"demo-storage-deleted.png");
        AimData(m,DataServiceTarget.Action.RestorePhoto);yield return ClickFacility();
        Check(m.Job==DataServiceLearning.StorageJob.Restore&&!m.ReplicaHasPhoto&&!m.PhotoRestored,"Restauration C vers B différée, sans photo accessible prématurément");
        yield return WaitDataJob(m);Check(m.PhotoRestored&&m.ReplicaHasPhoto&&!m.PrimaryOnline&&tour.Step==2,"v1 restaurée sur B, sans réparation fictive de A ni validation avant relecture");
        AimData(m,DataServiceTarget.Action.ReadPhoto);yield return ClickFacility();
        Check(m.StorageCompleted&&tour.Step==3&&m.RecoveryReady,"Relecture de v1 restaurée requise avant le refroidissement");
        CaptureData(m,false,"demo-storage-restored.png");
    }
}
#endif
