#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class FacilityOperationsValidation
{
    [Serializable] class Report{public bool passed;public List<string> checks=new List<string>();}
    public static void Validate(string output)
    {
        var original=SceneManager.GetActiveScene();var scene=EditorSceneManager.OpenScene("Assets/Scenes/DataCenter.unity",OpenSceneMode.Additive);var report=new Report();
        try
        {
            var module=Array.Find(UnityEngine.Object.FindObjectsByType<FacilityOperations>(FindObjectsSortMode.None),m=>m.gameObject.scene==scene);
            Check(module!=null&&module.tour.facility==module&&module.cooling==module.tour.learning,"Exploitation reliée au parcours et au modèle thermique",report);
            Check(module.targets.Length==7&&module.generatorAudio!=null,"Sept commandes distinctes et son extérieur référencés",report);
            foreach(var target in module.targets)
            {
                Check(target.module==module&&target.GetComponent<XRSimpleInteractable>()!=null,"Commande PC/XRI : "+target.action,report);
                Vector3 eye=(int)target.action>3?new Vector3(0,1.73f,37.3f):target.action==FacilityActionTarget.Action.InspectUPS?new Vector3(-21,1.73f,33):new Vector3(-15,1.73f,30);
                Physics.SyncTransforms();RaycastHit hit;
                bool reached=Physics.Raycast(eye,(target.transform.position-eye).normalized,out hit,3.5f,~0,QueryTriggerInteraction.Collide)&&hit.collider.GetComponentInParent<FacilityActionTarget>()==target;
                Check(reached,"Visée libre "+target.action+" : "+(hit.collider!=null?hit.collider.name:"aucun contact"),report);
            }
            Check(module.powerDisplay!=null&&module.powerFeedback!=null&&module.metricsDisplay!=null&&module.eventDisplay!=null&&module.limitsDisplay!=null&&module.supervisionFeedback!=null,"Six afficheurs du monde raccordés",report);
            Check(module.sourceIndicators.Length==3,"Indicateurs distincts secteur, batterie et groupe",report);
            Check(module.Phase==FacilityOperations.PowerPhase.Normal&&module.ServiceAvailable&&!module.PowerStarted,"État initial sans incident forcé ni coupure préalable",report);
            report.passed=true;
        }
        finally{Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);}
    }
    static void Check(bool value,string message,Report report){if(!value)throw new InvalidOperationException(message);report.checks.Add(message);}
}
#endif
