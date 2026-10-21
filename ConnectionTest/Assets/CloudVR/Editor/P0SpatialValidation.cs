#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class P0SpatialValidation
{
    [Serializable] class Report
    {
        public bool passed;
        public int decorativeRacks, groupedRackRenderers;
        public float nominalAisleWidth=2.2f, nominalMantrapDiameter=1.8f;
        public float measuredAisleWidth;
        public int externalGenerators;
        public float minimumGeneratorHallClearance;
        public List<string> checks=new List<string>();
    }
    public static void Validate(string output)
    {
        var original=SceneManager.GetActiveScene();
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/DataCenter.unity",OpenSceneMode.Additive);
        var report=new Report();
        var changed=new List<Collider>();
        float left=float.NegativeInfinity,right=float.PositiveInfinity;
        try
        {
            foreach(var root in scene.GetRootGameObjects())
            {
                foreach(var c in root.GetComponentsInChildren<Collider>())
                {
                    if(c.name.StartsWith("Baie_Fixe_"))report.decorativeRacks++;
                    if(c.GetComponentInParent<ProximitySlidingDoor>()!=null&&c.enabled){changed.Add(c);c.enabled=false;}
                }
                foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name.StartsWith("Rangées_"))
                {
                    report.groupedRackRenderers++;
                    foreach(var v in r.GetComponent<MeshFilter>().sharedMesh.vertices)
                    {
                        var p=r.transform.TransformPoint(v);
                        if(p.x<0)left=Mathf.Max(left,p.x);else right=Mathf.Min(right,p.x);
                    }
                }
            }
            Require(report.decorativeRacks==31,"31 baies fixes + baie interactive = 32",report);
            Require(report.groupedRackRenderers<=6,"Rendu des rangées groupé par matériau",report);
            report.measuredAisleWidth=right-left;
            Require(Mathf.Abs(report.measuredAisleWidth-2.2f)<.01f,"Largeur entre façades mesurée : "+report.measuredAisleWidth.ToString("F3")+" m",report);
            Physics.SyncTransforms();
            RaycastHit hit;
            Require(!Physics.CapsuleCast(new Vector3(0,.4f,3),new Vector3(0,1.5f,3),.25f,Vector3.forward,out hit,36,~(1<<2),QueryTriggerInteraction.Ignore),
                "Passage central sans obstacle fixe : "+(hit.collider!=null?hit.collider.name:"libre"),report);
            Require(!Physics.CapsuleCast(new Vector3(0,.4f,30),new Vector3(0,1.5f,30),.25f,Vector3.left,out hit,13,~(1<<2),QueryTriggerInteraction.Ignore),
                "Accès local électrique sans obstacle fixe : "+(hit.collider!=null?hit.collider.name:"libre"),report);
            Require(Physics.CapsuleCast(new Vector3(1.3f,.4f,-5),new Vector3(1.3f,1.5f,-5),.25f,Vector3.forward,out hit,5,~(1<<2),QueryTriggerInteraction.Ignore),
                "Raccord de façade : contournement latéral droit bloqué",report);
            Require(Physics.CapsuleCast(new Vector3(-1.3f,.4f,-5),new Vector3(-1.3f,1.5f,-5),.25f,Vector3.forward,out hit,5,~(1<<2),QueryTriggerInteraction.Ignore),
                "Raccord de façade : contournement latéral gauche bloqué",report);
            var sas=Array.Find(UnityEngine.Object.FindObjectsByType<MantrapController>(FindObjectsSortMode.None),s=>s.gameObject.scene==scene);
            Require(sas!=null&&Mathf.Abs(sas.transform.localScale.x-.9f)<.001f,"Sas nominal 1,8 m",report);
            Require(sas.scanLabel!=null&&sas.doorExit.GetComponent<Renderer>().sharedMaterial.renderQueue==3000,"Scan lisible et porte transparente",report);
            Transform zone=null;
            foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="Zone_Teleportation")zone=t;
            Require(zone!=null&&zone.localScale.x<=1.4f,"Zone de téléportation centrale contenue dans l'allée",report);
            report.minimumGeneratorHallClearance=float.PositiveInfinity;
            foreach(var root in scene.GetRootGameObjects())foreach(var r in root.GetComponentsInChildren<MeshRenderer>())
                if(r.enabled&&r.name.StartsWith("Genset_")&&r.name.EndsWith("_Container"))
                {
                    report.externalGenerators++;
                    report.minimumGeneratorHallClearance=Mathf.Min(report.minimumGeneratorHallClearance,-9f-r.bounds.max.x);
                    Require(r.bounds.max.x < -14.2f,"Groupe hors du bâtiment et du mur de salle : "+r.name,report);
                }
            Require(report.externalGenerators==3&&report.minimumGeneratorHallClearance>5.5f,
                "Trois groupes électrogènes séparés du mur intérieur d'au moins 5,5 m",report);
            report.passed=true;
        }
        finally
        {
            foreach(var c in changed)c.enabled=true;
            Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));
            EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);
        }
    }
    static void Require(bool value,string description,Report report)
    {
        if(!value)throw new InvalidOperationException(description);
        report.checks.Add(description);
    }
}
#endif
