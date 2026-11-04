# T24 — Mesures du player PC, 4 novembre 2026

Le MSAA 4x améliore les contours comparés dans T23, mais son coût est significatif sur ce PC : **54,11 ms par frame en médiane contre 46,09 ms en 1x**. Le 1x reste lent dans ce scénario ; désactiver uniquement l’anticrénelage ne suffit donc pas à assurer une visite fluide. Aucun nouveau réglage graphique du parcours normal n’a été appliqué dans T24.

## Conditions et méthode

- Player Windows **de développement**, Unity 6000.0.58f2, Direct3D11, fenêtre visible 1280×720. Intel Iris Xe Graphics, Intel Core i7-1165G7, 8 threads, Windows 11. Le nom du CPU contient sa fréquence nominale ; les fréquences instantanées et l’alimentation n’ont pas été verrouillées.
- Caméra à (0 ; 1,68 ; 8), FOV 75°, pitch −8°, yaw −12° à +12° à environ 3°/s. Scène au repos, contrôleur de déplacement désactivé ; les scripts et animations du décor continuent. Ce n’est pas un parcours humain ni une mesure de l’ensemble des activités.
- Une série **ABBA : 1x / 4x / 4x / 1x**, deux secondes d’échauffement puis huit secondes de mouvement par phase. Le focus est resté actif pendant les quatre fenêtres. vSync 0 et cadence non plafonnée demandées ; ne pas en déduire l’absence de contraintes du pilote/compositeur.
- `FrameTimingManager` Unity ; timestamps dédupliqués, cinq premiers échantillons de chaque fenêtre exclus des statistiques de timing pour écarter leur retard après l’échauffement. Intervalles entre frames relevés séparément par horloge monotone. Aucun screenshot ni encodage d’image dans les fenêtres mesurées.
- Applications de fond non contrôlées, éditeur Unity ouvert mais hors Play. Un seul PC, une seule série avec deux fenêtres par mode : pas de benchmark représentatif de tous les PC, de Release ou de Quest.

Le probe est **opt-in**, limité aux players Windows de développement et à l’éditeur pour compilation. Le parcours normal et Android ne le lancent pas. Les profils MSAA sont clonés en mémoire et restaurés ; aucun changement de `PlayerSettings.enableFrameTimingStats` nécessaire, cette collecte étant déjà active dans un Development Player. [Sources et distinction des métriques](RESOURCES.md#t24--mesures-de-frames-et-comparaison-msaa-4-novembre-2026).

## Résultats visibles

| MSAA | Frames observées | Échantillons GPU | Intervalle médian | Intervalle p95 | GPU médian | GPU p95 |
|---|---:|---:|---:|---:|---:|---:|
| 1x | 344 | 334 | 46,09 ms | 51,91 ms | 43,93 ms | 50,29 ms |
| 4x | 295 | 285 | 54,11 ms | 59,79 ms | 51,00 ms | 58,33 ms |

Les médianes des deux fenêtres 1x sont 45,86 et 46,82 ms ; celles des fenêtres 4x sont 54,28 et 54,00 ms. Les lignes regroupées sont recalculées depuis les échantillons CSV, sans moyenner les p95.

Surcoût médian observé du 4x : **+8,02 ms par frame (+17,4 %)** et **+7,07 ms de temps GPU (+16,1 %)**. Le temps GPU est proche de l’intervalle total observé et varie dans le même sens ; cela justifie de profiler le rendu en priorité. Ce n’est pas une attribution complète par passe, ni une preuve que tous les coûts CPU sont faibles : les métriques CPU peuvent inclure des attentes et du travail synchronisé.

![Médianes et p95 du player visible](validation/performance-2026-11-04/timings.png)

[Rapport original](validation/performance-2026-11-04/visible/performance-result.json), [résumé recalculé](validation/performance-2026-11-04/summary.json), CSV [phase 0](validation/performance-2026-11-04/visible/phase-0.csv), [phase 1](validation/performance-2026-11-04/visible/phase-1.csv), [phase 2](validation/performance-2026-11-04/visible/phase-2.csv), [phase 3](validation/performance-2026-11-04/visible/phase-3.csv).

Le résumé se recalcule sans dépendance Python supplémentaire : `python tools/Summarize-PCPerformance.py <dossier-du-run>`. Le script vérifie ordre ABBA, focus, rendu/GPU présents, effectifs, timestamps et statistiques des CSV ; il refuse la collecte masquée inutilisable. Les deux APNG sont vérifiés pixel par pixel contre leurs 50 PNG source.

## Essai masqué écarté

Le premier test a fini son protocole sans exception, mais avec **zéro échantillon GPU et zéro temps du thread de rendu**, alors que la boucle CPU tournait autour de 0,25 ms. Ces valeurs ne mesurent pas la cadence graphique du jeu ; **aucune comparaison de performance n’en est déduite**. [Diagnostic original conservé](validation/performance-2026-11-04/hidden-unusable/performance-result.json).

L’utilisateur a ensuite autorisé explicitement le lancement visible. La même instrumentation et le même exécutable ont fourni des temps GPU et de rendu valides dans cette seconde série. Cela appuie l’hypothèse d’un rendu non présenté en mode masqué ; le mécanisme exact côté fenêtre/pilote n’a pas été instrumenté. `passed` dans le premier rapport indique seulement l’achèvement du programme, pas la validité du benchmark. Le runner distingue désormais `gpuMeasured` et `renderingMeasured`.

## Revue en mouvement et limites

Après chronométrage, 25 angles imposés par mode ont été capturés hors écran : yaw −3° à +3° par pas de 0,25°, même position et même FOV. Deux animations APNG sans perte reproduisent ces images à **10 images/s imposées pour la revue** : cette cadence de lecture n’est pas celle du jeu mesurée ci-dessus.

[Balayage MSAA 1x](validation/performance-2026-11-04/motion-1x.png), [balayage MSAA 4x](validation/performance-2026-11-04/motion-4x.png). Consulter à la taille native pour comparer les détails. [Pixels des 50 frames vérifiés identiques aux PNG source](validation/performance-2026-11-04/motion-validation.json).

Les images aux angles comparés montrent des contours plus réguliers en 4x ; les petits textes éloignés restent peu lisibles. Une séquence de caméra imposée et des captures `Camera.Render` ne valident pas la stabilité visuelle de tous les mouvements, le confort humain ou l’Overlay. Aucun nouvel essai de capture complète de l’interface n’a été tenté.

## Livraison et suite

Build du 04/11 à **15 h 10 min 51 s Paris**, Succeeded, 0 erreur / 0 avertissement BuildPipeline. Après livraison confirmée : collecte masquée diagnostique, série visible valide terminée à **15 h 17 min 27 s**, puis **196/196 contrôles fonctionnels** terminés à **15 h 20 min 56 s**. [Preuves et limites de livraison](validation/performance-2026-11-04/README.md).

T25 est prêt : identifier les passes coûteuses (ombres, transparence, géométrie), essayer un correctif ciblé et comparer dans les mêmes conditions. Le profil produit reste 4x en attendant cette comparaison. Aucun objectif de performance matériel, résultat casque ou gain d’optimisation supplémentaire n’est revendiqué.
