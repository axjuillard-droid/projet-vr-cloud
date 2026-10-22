#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class DataServiceValidation
{
    [Serializable] class Report {public bool passed;public List<string> checks=new List<string>();}
    public static void Validate(string output)
    {
        var original=SceneManager.GetActiveScene();var scene=EditorSceneManager.OpenScene("Assets/Scenes/DataCenter.unity",OpenSceneMode.Additive);var report=new Report();
        try
        {
            var m=Array.Find(UnityEngine.Object.FindObjectsByType<DataServiceLearning>(FindObjectsSortMode.None),v=>v.gameObject.scene==scene);
            Check(m!=null&&m.tour.data==m,"Réseau et stockage reliés au parcours",report);
            Check(m.targets.Length==9&&m.networkPaths.Length==2&&m.storagePaths.Length==3,"Neuf cibles, deux chemins réseau et trois trajets de copie",report);
            Check(m.networkSignals.Length==5&&m.storageSignals.Length==4&&m.networkNodes.Length==3&&m.storageNodes.Length==3,"Marqueurs et nœuds distincts référencés",report);
            Check(m.networkStatus!=null&&m.networkFeedback!=null&&m.storageStatus!=null&&m.storageFeedback!=null,"Quatre afficheurs dans le monde raccordés",report);
            foreach(var target in m.targets)
            {
                Check(target.module==m&&target.GetComponent<XRSimpleInteractable>()!=null,"Commande PC/XRI : "+target.action,report);
                var eye=new Vector3(0,1.73f,(int)target.action<=2?8:19.4f);Physics.SyncTransforms();RaycastHit hit;
                bool reached=Physics.Raycast(eye,(target.transform.position-eye).normalized,out hit,3.5f,~0,QueryTriggerInteraction.Collide)&&hit.collider.GetComponentInParent<DataServiceTarget>()==target;
                Check(reached,"Visée données "+target.action+" : "+(hit.collider!=null?hit.collider.name:"aucun"),report);
            }
            Check(m.NetworkPhase==DataServiceLearning.NetworkState.Idle&&!m.StorageStarted&&!m.ReplicaReady&&!m.RecoveryReady,"Aucun parcours avancé ni copie préparée avant les bornes",report);
            Check(GameObject.Find("Flux_Illustratif")==null,"Ancien trajet unique à validation automatique supprimé",report);
            report.passed=true;
        }
        finally{Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);}
    }
    static void Check(bool value,string message,Report r){if(!value)throw new InvalidOperationException(message);r.checks.Add(message);}
}
#endif
