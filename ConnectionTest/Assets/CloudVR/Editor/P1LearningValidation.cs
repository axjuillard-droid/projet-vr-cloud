#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class P1LearningValidation
{
    [Serializable] class Report { public bool passed;public float closedTrayFront,doorFront;public List<string> checks=new List<string>(); }
    public static void Validate(string output)
    {
        var original=SceneManager.GetActiveScene();
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/DataCenter.unity",OpenSceneMode.Additive);
        var report=new Report();
        var module=Array.Find(UnityEngine.Object.FindObjectsByType<P1LearningModule>(FindObjectsSortMode.None),m=>m.gameObject.scene==scene);
        try
        {
            Require(module!=null&&module.rack!=null&&module.containment!=null,"Module physique relié à la visite et au confinement",report);
            Require(module.targets.Length==8,"Huit cibles distinctes : porte, tiroir, trois composants, deux choix et fermeture",report);
            foreach(var t in module.targets)Require(t.module==module&&t.GetComponent<XRSimpleInteractable>()!=null,"Entrée PC / XRI : "+t.action,report);
            Require(!module.rack.transform.Find("Modele_Original").gameObject.activeSelf,"Anciennes façades pleines retirées de la seule Baie_01",report);
            var chassis=Array.Find(module.rack.serverBlade.GetComponentsInChildren<MeshFilter>(),f=>f.name.Contains("Chassis"));
            report.closedTrayFront=float.PositiveInfinity;
            foreach(var v in chassis.sharedMesh.vertices)report.closedTrayFront=Mathf.Min(report.closedTrayFront,module.rack.transform.InverseTransformPoint(chassis.transform.TransformPoint(v)).z);
            report.doorFront=module.rack.rackDoor.localPosition.z-.02f;
            Require(report.closedTrayFront>report.doorFront+.04f,"Tiroir fermé contenu derrière la porte",report);
            CheckRay(module,P1ActionTarget.Action.Door,new Vector3(0,1.68f,14),report);
            var closed=module.rack.rackDoor.localRotation;var inserted=module.rack.serverBlade.localPosition;
            try
            {
                module.rack.rackDoor.localRotation=closed*Quaternion.Euler(0,module.rack.doorOpenAngle,0);
                CheckRay(module,P1ActionTarget.Action.Drawer,new Vector3(0,1.68f,14),report);
                module.rack.serverBlade.localPosition=inserted+Vector3.back*module.rack.serverSlideDistance;
                module.inspectionTargets.SetActive(true);
                foreach(var a in new[]{P1ActionTarget.Action.Cpu,P1ActionTarget.Action.Ram,P1ActionTarget.Action.Storage})CheckRay(module,a,new Vector3(0,1.68f,14),report);
            }
            finally {module.rack.rackDoor.localRotation=closed;module.rack.serverBlade.localPosition=inserted;module.inspectionTargets.SetActive(false);}
            foreach(var a in new[]{P1ActionTarget.Action.ResetAlarm,P1ActionTarget.Action.BoostFans,P1ActionTarget.Action.SealContainment})CheckRay(module,a,new Vector3(0,1.68f,26),report);
            report.passed=true;
        }
        finally
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));
            EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);
        }
    }
    static void CheckRay(P1LearningModule module,P1ActionTarget.Action action,Vector3 eye,Report report)
    {
        var target=Array.Find(module.targets,t=>t.action==action);Physics.SyncTransforms();
        RaycastHit hit;bool reached=Physics.Raycast(eye,(target.transform.position-eye).normalized,out hit,3.5f,~0,QueryTriggerInteraction.Collide)&&hit.collider.GetComponentInParent<P1ActionTarget>()==target;
        Require(reached,"Visée libre "+action+" : "+(hit.collider!=null?hit.collider.name:"aucune cible"),report);
    }
    static void Require(bool value,string text,Report report){if(!value)throw new InvalidOperationException(text);report.checks.Add(text);}
}
#endif
