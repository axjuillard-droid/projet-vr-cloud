#if UNITY_EDITOR || (DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN)
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>Extension du probe PC : marche réelle, sélections physiques et borne du modèle par API.</summary>
public partial class DemoSmoke
{
    IEnumerator ClickFacility()
    {Button(true);yield return new WaitForSecondsRealtime(.15f);Button(false);yield return new WaitForSecondsRealtime(.3f);}
    IEnumerator WalkFacility(Key key,float target)
    {
        var pc=Camera.main.GetComponentInParent<PCPlayerController>();pc.transform.rotation=Quaternion.identity;Camera.main.transform.localRotation=Quaternion.identity;pc.enabled=true;
        bool x=key==Key.A||key==Key.D;float direction=key==Key.A||key==Key.S?-1:1;
        _movementKey=key;Forward(true);float until=Time.realtimeSinceStartup+10;
        while((target-(x?pc.transform.position.x:pc.transform.position.z))*direction>.04f&&Time.realtimeSinceStartup<until)yield return null;
        Forward(false);yield return new WaitForSecondsRealtime(.08f);pc.enabled=false;
        Check(Mathf.Abs((x?pc.transform.position.x:pc.transform.position.z)-target)<.16f,"Marche électrique "+key+" vers "+target+" atteinte : "+pc.transform.position);
    }
    void AimFacility(FacilityOperations module,FacilityActionTarget.Action action,Vector3 position)
    {
        var camera=Camera.main;var pc=camera.GetComponentInParent<PCPlayerController>();pc.enabled=false;
        var cc=pc.GetComponent<CharacterController>();cc.enabled=false;pc.transform.position=position;cc.enabled=true;
        var target=Array.Find(module.targets,t=>t.action==action);camera.transform.LookAt(target.transform.position);Physics.SyncTransforms();
        bool reached=P1LearningModule.TryPick(camera.ViewportPointToRay(new Vector3(.5f,.5f,0)),pc.transform,out var hit)&&hit.collider.GetComponentInParent<FacilityActionTarget>()==target;
        Check(reached,"Raycast exploitation atteint "+action+" ; contact="+(hit.collider!=null?hit.collider.name:"aucun"));
    }
    IEnumerator ExerciseFacilityPower(DemoTour tour)
    {
        var facility=tour.facility;var controller=Camera.main.GetComponentInParent<PCPlayerController>();
        Check(tour.BackupShown&&tour.Step==4&&facility.OnBattery&&facility.ServiceAvailable,"Perte secteur : soutien UPS automatique, étape non validée par la console");
        tour.Activate(4);Check(tour.Step==4,"Second clic sur borne 05 ne remplace pas la visite électrique");
        yield return WalkFacility(Key.S,30);yield return WalkFacility(Key.A,-15);
        Check(controller.transform.position.x < -14.8f&&Mathf.Abs(controller.transform.position.z-30)<.2f,"Pupitre électrique rejoint au clavier depuis la borne 05");
        // Borne du modèle contrôlée par API, distincte de l'écoulement réel de la marche.
        float reserve=facility.BatteryPercent*.005f,energy=facility.SiteEnergyKWh;
        facility.AdvancePower(200);yield return new WaitForSecondsRealtime(.2f);
        Check(!facility.ServiceAvailable&&!tour.learning.CoolingActive&&facility.Phase==FacilityOperations.PowerPhase.Exhausted,"Réserve épuisée par pas de temps API : service et refroidissement indisponibles");
        Check(Mathf.Abs(facility.SiteEnergyKWh-energy-reserve)<.0001f,"Énergie fournie bornée par la réserve restante, sans fourniture après épuisement");
        AimFacility(facility,FacilityActionTarget.Action.Replay,new Vector3(-15,.08f,30));yield return ClickFacility();
        Check(facility.OnBattery&&facility.ServiceAvailable&&!facility.UPSInspected&&tour.Step==4,"Rejouer rétablit la démonstration sans sauter l'étape ni bloquer le visiteur");
        AimFacility(facility,FacilityActionTarget.Action.StartGenerator,new Vector3(-15,.08f,30));
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape));yield return new WaitForSecondsRealtime(.15f);
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.1f);
        yield return ClickFacility();Check(controller.InputPaused&&facility.Phase==FacilityOperations.PowerPhase.Battery,"Pause : commande de groupe ignorée");
        InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Escape));yield return new WaitForSecondsRealtime(.15f);InputSystem.QueueStateEvent(_keyboard,new KeyboardState());
        AimFacility(facility,FacilityActionTarget.Action.Transfer,new Vector3(-15,.08f,30));yield return ClickFacility();
        Check(facility.OnBattery&&!facility.PowerCompleted&&facility.RefusedChoices>0,"Transfert vers une source non prête refusé");
        AimFacility(facility,FacilityActionTarget.Action.StartGenerator,new Vector3(-15,.08f,30));yield return ClickFacility();
        Check(facility.Phase==FacilityOperations.PowerPhase.Starting&&facility.OnBattery&&facility.ServiceAvailable,"Démarrage réel de la séquence du groupe, UPS en soutien");
        AimFacility(facility,FacilityActionTarget.Action.Transfer,new Vector3(-15,.08f,30));yield return ClickFacility();
        Check(facility.Phase==FacilityOperations.PowerPhase.Starting&&tour.Step==4,"Transfert refusé pendant stabilisation du groupe");
        Camera.main.transform.LookAt(facility.powerDisplay.transform.position);yield return new WaitForEndOfFrame();Capture("demo-power-battery.png");
        float charge=facility.BatteryPercent;
        yield return WalkFacility(Key.W,33);yield return WalkFacility(Key.A,-21);
        Check(controller.transform.position.x < -20.8f&&Mathf.Abs(controller.transform.position.z-33)<.2f,"UPS original rejoint à pied en contournant pupitre et batteries");
        AimFacility(facility,FacilityActionTarget.Action.InspectUPS,new Vector3(-21,.08f,33));yield return ClickFacility();
        Check(facility.UPSInspected&&facility.ServiceAvailable&&facility.BatteryPercent<charge,"UPS inspecté par raycast, réserve déchargée pendant le soutien");
        yield return new WaitForEndOfFrame();Capture("demo-ups-inspection.png");
        yield return WalkFacility(Key.D,-15);yield return WalkFacility(Key.S,30);
        Check(facility.GeneratorReady&&facility.OnBattery,"Retour au pupitre : groupe stabilisé, batterie toujours source de charge");
        AimFacility(facility,FacilityActionTarget.Action.Transfer,new Vector3(-15,.08f,30));yield return ClickFacility();
        Check(tour.Step==4&&facility.Phase==FacilityOperations.PowerPhase.Transferring,"Autorisation de transfert sans validation instantanée");
        float until=Time.realtimeSinceStartup+5;while(!facility.PowerCompleted&&Time.realtimeSinceStartup<until)yield return null;
        Check(facility.PowerCompleted&&facility.Phase==FacilityOperations.PowerPhase.Generator&&facility.ServiceAvailable,"Charge sur groupe et UPS observé avant validation de l'électricité");
        charge=facility.BatteryPercent;yield return new WaitForSecondsRealtime(.4f);
        Check(Mathf.Abs(charge-facility.BatteryPercent)<.001f,"La réserve cesse de se décharger après transfert");
        Camera.main.transform.LookAt(facility.powerDisplay.transform.position);yield return new WaitForEndOfFrame();Capture("demo-power-generator.png");
    }
    IEnumerator ExerciseFacilitySupervision(DemoTour tour)
    {
        var facility=tour.facility;
        Check(facility.SupervisionStarted&&!tour.Finished,"Borne 06 ouvre une observation guidée, sans conclusion automatique");
        AimFacility(facility,FacilityActionTarget.Action.RouteEnergy,new Vector3(0,.08f,37.3f));yield return ClickFacility();
        Check(!facility.ReportPrepared&&tour.Step==5,"Rapport prématuré refusé avant lecture de l'incident");
        AimFacility(facility,FacilityActionTarget.Action.RouteNetwork,new Vector3(0,.08f,37.3f));yield return ClickFacility();
        Check(!facility.ReportPrepared&&tour.Step==5,"Routage réseau non pertinent ne valide pas l'incident électrique");
        AimFacility(facility,FacilityActionTarget.Action.ReadIncident,new Vector3(0,.08f,37.3f));yield return ClickFacility();
        Check(facility.IncidentRead&&facility.metricsDisplay.text.Contains("kWh")&&facility.limitsDisplay.text.Contains("non calculé"),"Chronologie identifiée, énergie de session et indicateurs non mesurés distingués");
        Camera.main.transform.LookAt(facility.metricsDisplay.transform.parent.parent.position);yield return new WaitForEndOfFrame();Capture("demo-supervision.png");
        AimFacility(facility,FacilityActionTarget.Action.RouteEnergy,new Vector3(0,.08f,37.3f));yield return ClickFacility();
        Check(facility.ReportPrepared&&tour.AlertAcknowledged&&tour.Finished,"Rapport pédagogique préparé pour énergie après lecture, sans envoi externe");
        Camera.main.transform.LookAt(facility.metricsDisplay.transform.parent.parent.position);yield return new WaitForEndOfFrame();Capture("demo-supervision-report.png");
    }
}
#endif
