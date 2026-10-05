# Module original de baie CloudVR

Premier module graphique produit le 5 octobre 2026 : châssis, rails de fixation, neuf façades génériques (serveurs, stockage et switch), aérations, poignées, vis et voyants. Géométrie propre au projet, sans modèle ni texture d'un pack commercial. Les équipements restent des représentations pédagogiques, pas un modèle industriel normalisé.

## Reproduire

Depuis la racine du dépôt, avec Blender 4.5.2 installé :

```powershell
& 'C:/Program Files/Blender Foundation/Blender 4.5/blender.exe' --background --factory-startup --python tools/art/generate_server_rack.py
```

Le script produit :

- `art/source/ServerRack.blend`, source modifiable ;
- `ConnectionTest/Assets/CloudVR/Models/ServerRack_Detailed.fbx`, modèle d'import Unity ;
- `art/server-rack-report.json`, compteurs de géométrie et paramètres.

Les dimensions nominales sont 1,10 × 2,20 × 1,00 m ; petits détails latéraux compris, Unity mesure environ 1,11 × 2,20 × 1,00 m. Le pivot est au sol. L'import FBX convertit les axes ; le builder oriente le modèle de 180° autour de Y pour présenter les façades aux visiteurs arrivant depuis -Z.

Le générateur fusionne les pièces par matériau : **6 228 triangles, sept meshes**. Cette réduction du nombre d'objets ne constitue pas une mesure de performance sur Quest. Aucune lumière par voyant, aucun script runtime ni texture externe dans le modèle.

## Intégrer dans Unity

Attendre l'import et la compilation. Hors Play, avec scènes propres, exécuter **CloudVR > Build Scene > Data center et transition photo**. Attention : ce builder régénère DataCenter ; garder toute personnalisation manuelle avant de le rejouer.

`DataCenterArtBuilder` réutilise le FBX, affecte sept matériaux URP, construit le prefab `ServerRack` et conserve son collider simple de navigation. Le même prefab est instancié trois fois. Dalles, joints, chemins de câbles, armoires et luminaires sont regroupés en **cinq meshes architecturaux**, réenregistrés dans Models avec leurs GUID conservés lors des reprises.

Les rigs, la zone de téléportation, le point de départ et la transition photo restent gérés par `DataCenterSceneBuilder`. Les armoires ajoutées disposent de colliders simples hors de la zone de téléportation. Trois lumières éclairent la scène ; coût du rendu à mesurer dans le casque.

## Vérifier

Les preuves sont dans [la validation graphique](../docs/validation/art-2026-10-05/README.md). Play et player de développement utilisent le scénario existant :

```powershell
./tools/Test-PCBuild.ps1 -Graphics -Journey -TimeoutSeconds 90
```

Comparer les captures pour juger le détail, les proportions et la lisibilité. Le pourcentage « 80 % de qualité » discuté est une cible subjective, pas un résultat mesuré. Chambre et interaction pédagogique de la baie sont les prochains morceaux distincts.
