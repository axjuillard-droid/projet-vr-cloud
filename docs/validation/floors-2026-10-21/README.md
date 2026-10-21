# T12 — Surfaces concurrentes et sols, 21 octobre 2026

## Résultat livré

Le builder désactive le parking du FBX qui recouvrait la cour et traversait le bâtiment. Dans la salle, les 779 anciennes dalles sont remplacées par un dallage groupé couvrant 18 × 40 m : 2 010 dalles, 8 040 sommets, 4 020 triangles et un seul MeshRenderer. Le collider continu reste à Y=0, sans renderer actif ; les joints visibles sont en retrait à Y=−0,015 m. Aucun shader de décalage de profondeur ni changement global d'anticrénelage n'a été ajouté.

Le mesh est un asset généré avec GUID conservé. `Build-DemoCopy.ps1 -Complete` vérifie sa présence et copie le mesh et son `.meta`, en plus des scènes/matériaux. Une nouvelle exécution du builder réutilise le mesh au même chemin ; pas d'accumulation de géométrie dans la scène.

![Comparaison du même angle de salle](floor-before-after.png)

[Vidéo avant/après, cour/sas/salle/angle rasant](floor-comparison.mp4) — **balayages contrôlés hors Play**, 31 orientations par zone, yaw −30° à +30°, 960×540 par vue ; lecture synthétique à 10 images/s. Les grandes découpes parasites visibles avant ne sont plus présentes aux angles comparés. Cette vidéo ne mesure pas les FPS du player et ne constitue pas un essai humain ni casque. Les joints fins et les reflets restent visibles ; l'absence de tout scintillement, à toute résolution et sur tout matériel, n'est pas revendiquée.

## Vérifications

- [Build final](build-result.json), **16 h 56 min 39 s Paris** : Unity 6000.0.58f2, Succeeded, **0 erreur / 0 avertissement**, 20,45 s de BuildPipeline, 146 874 635 octets. Commandes `powershell -ExecutionPolicy Bypass -File tools/Build-DemoCopy.ps1 -Resume -Wait`, puis `-Complete` réellement exécutées. Le temps total du batch inclut aussi import/compilation et dépasse celui du BuildReport.
- [Player graphique](player/demo-smoke-result.json), terminé à **16 h 58 min 22 s Paris** : **37/37 contrôles réussis**, Direct3D11, exitCode=0, passed=true. Marche au clavier synthétique de la cour jusqu'à Z=39,001 m en 24,28 s, passage du sas/scan et maintien au sol, six bornes/huit actions, synthèse et retour/reset Chambre. Les poses de visée des bornes sont contrôlées par le probe ; le compteur de frames logiques ne donne pas le FPS interactif.
- [Audit MCP après livraison](geometry-after.json) : **0 ancienne dalle active**, **0 parking importé visible**, **0 mesh ou matériau manquant** dans les renderers actifs inspectés. Sol de collision sans rendu, faces des dalles à Y=0, joints à −15 mm. Huit raycasts centraux ont rencontré les zones de téléportation/collisions aux positions −28/−12/−4,1/−2/0,1/7/20/39 m ; la marche réelle du probe confirme la continuité de collision.
- Chambre : un seul sol visible nommé Sol relevé ; lancement normal du nouvel exe observé via computer-use, chambre affichée, puis player fermé. Pas de parcours natif complet effectué manuellement. Capture du probe [Chambre](player/demo-room-start.png) et [salle](player/demo-hall-end.png).
- Éditeur final : Chambre propre, hors Play. Aucune erreur C# retournée. Les captures/Player.log signalent une réduction de résolution de six shadow maps dans l'atlas 2048 : avertissement de rendu à garder pour la revue d'éclairage, distinct du bilan de compilation sans avertissement. Ce correctif n'en attribue pas tous les artefacts au z-fighting.

Les sources de la transition et des activités sont inchangées. Chambre est régénérée par le constructeur maître : mêmes noms/nombres d'objets, nouveaux identifiants de sérialisation. APK et réglages Quest inchangés ; aucun casque testé.

## Reproduire les captures

Dans Unity propre, hors Play, appeler par MCP `execute_code` :

```csharp
Type.GetType("FloorRenderCapture, Assembly-CSharp-Editor")
    .GetMethod("Capture").Invoke(null, new object[] {
        System.IO.Path.GetFullPath("../tmp/unity/floor-after")
    });
return "Captures terminées";
```

L'outil restaure les scènes initiales et ne les sauvegarde pas. Les captures avant du défaut sont conservées sous tmp local, avec deux images représentatives dans ce dossier ; ne pas écraser ces images par le builder corrigé. Pour composer deux jeux de captures :

```powershell
python tools/Compare-FloorSweeps.py tmp/unity/floor-before tmp/unity/floor-after tmp/unity/floor-comparison
```

Pillow et imageio-ffmpeg locaux sont utilisés pour la composition, sans installation ni dépendance ajoutée à Unity. Le codec ffmpeg provient de l'environnement Python existant et n'est pas redistribué. Les preuves publiées sont les rendus du projet, les rapports et un journal anonymisé ; exe, caches, sessions MCP et chemins personnels exclus.
