# Tâches et jalons

Statuts : à faire, en cours, bloqué, terminé. Une tâche n’est prête que si ses dépendances sont connues.

| ID | Tâche | Statut | Critère de fin |
|---|---|---|---|
| T00 | Organiser la mémoire du projet | terminé | Vision, décisions, état, backlog et méthode de recherche présents |
| T00a | Documenter le pilotage autonome et le conseil modèle | terminé | 05/10/2026 : CAPACITES_CODEX.md décrit capacités/méthodes/limites, TOOLING et CONTEXT reliés ; AGENTS.md impose modèle et effort recommandés à chaque réponse finale pour le prochain prompt. Aucun test matériel déduit de cette documentation. |
| T00b | Actualiser le README et publier les jalons GitHub | terminé | 05/10 : README complet avec trois captures réelles et méthode persistante dans AGENTS ; commits b62124c et b10a0ae poussés sur origin/main avec 143 objets LFS. Liens, 39 JSON, scripts et diff vérifiés ; README distant identique et trois images HTTP 200. Aucun nouveau test Unity/casque pour cette documentation. |
| T01 | Inventorier les outils locaux en lecture seule | terminé | Voir TOOLING.md ; versions et dossiers de modules constatés, limites signalées, aucune installation |
| T02 | Confirmer le matériel et les attentes universitaires | terminé | Inventaire des inconnues consigné dans DECISIONS.md ; utilisateur ne connaît pas modèles Quest, date exacte ni barème au 25/09/2026. Ce statut ne signifie pas matériel validé |
| T01a | Installer uv et connecter Unity en lecture | terminé | Exécutables et PATH vérifiés ; instance, projet et état lus par MCP HTTP |
| T01b | Tester une modification Unity par MCP | terminé | 25/09/2026 : CodexConnectionTest créé dans SampleScene, propriétés relues, console sans erreur ni avertissement ; autres objets inchangés dans la hiérarchie relue. Scène non sauvegardée ; voir TOOLING.md |
| T03 | Rechercher des bases gratuites pour le prototype | terminé | Sélection documentaire sourcée dans RESOURCES.md : XRI/OpenXR, Kenney, baie candidate et alternatives ; licences et limites de compatibilité explicites. Aucun import testé |
| T04 | Décrire le parcours photo minimal | en cours | Structure, niveaux pédagogiques et acteurs validés ; préférence pour des entreprises connues consignée en D11. Reste à sélectionner les entreprises et confirmer l'installation, détailler actions/transitions, sourcer les mécanismes et fixer les critères PC/Quest et le premier morceau jouable |
| T05 | Préparer le dépôt de développement | terminé | 28/09/2026 : Git + LFS configurés, remote GitHub public https://github.com/axjuillard-droid/projet-vr-cloud, premier commit poussé. ConnectionTest/ nettoyé (TutorialInfo et Readme supprimés, SampleScene renommée Chambre). Invitation tjoliot-30 à faire manuellement (Settings → Collaborators) |
| T06 | Construire une première interaction PC et Quest | en cours | 05/10 : interaction photo et rig XRI intégrés ; 12 vérifications XR sans casque, build Windows et 9 vérifications du player/captures réussis. APK Android de développement compilé à 13 h 42 (53,85 Mo, 0 erreur/5 avertissements), 15 contrôles statiques réussis, suivi oculaire inutilisé retiré du manifeste. Preuves : docs/validation/android-build-2026-10-05/, pc-build-2026-10-05/ et xr-2026-10-05/. Reste : lancement sur Quest identifié, essais utilisateurs PC/manettes/casque, rayon → zone → requête, rotation, lisibilité/confort/performance. T06 entier non terminé. |
| T06a | Corriger les points blancs en Play dans Unity | en cours | 05/10 : défaut reproduit Mobile/Android/Intel Iris Xe/DX11, correspondant à Unity UUM-121981. Ombres douces activées : disparition en capture ; désactivées : retour du défaut. Réglage Mobile enregistré, nouveau Play et trois angles sans points ; preuves render-2026-10-05/. Utilisateur a testé CloudVR-PC.exe et le juge très bien. Reste : confirmation du nouveau Play en mouvement naturel et coût/rendu Quest. Aucun rebuild effectué ; signalement initial confirmé dans Unity, pas dans l'exe Windows. |
| T07 | Relier la photo à une première zone de data center | terminé | 05/10 : scène/prefab/transition créés, builder rejoué ; 13 contrôles parcours PC, 13 contrôles VR injectés et 9 contrôles photo réussis dans Unity. Build final Windows deux scènes réussi à 14 h 41 (34,9 s, 0 erreur/0 avertissement), 13 contrôles parcours et 9 contrôles photo réussis dans le player DX11, exitCode=0. Captures et preuves journey-2026-10-05/. Essais utilisateur/confort/casque et narration finale restent distincts ; aucun nouvel APK. |

## Jalons suivants sans dates imposées
T06a : contournement enregistré et testé dans Unity ; confirmation utilisateur en mouvement naturel attendue. Retour utilisateur positif sur l'exe PC. Développement prêt à reprendre sur la transition Chambre → data center selon T04 ; essais Quest dès qu'un casque est disponible. Mesurer le coût des ombres douces au prochain build/essai Android ; l'APK existant conserve les anciens réglages. Reconstruire les exports pour tester de nouvelles modifications.

Premier morceau Chambre → DataCenter désormais livré et testé dans l'exe PC (T07). Prochain morceau à définir sous T04 : interaction avec la baie pour expliquer réseau/traitement/stockage, textes provisoires et sources à vérifier avant production. Test utilisateur du nouveau parcours souhaité ; Quest à tester dès disponibilité avec un nouvel APK. Continuer par petits ajouts testés en Play puis export par étape jouable. Méthode dans BUILDS.md.

État courant, 05/10 à 14 h 45 : exe Windows Chambre/DataCenter livré, build final et 13 contrôles parcours + 9 contrôles photo réussis dans le player graphique. Nouveau parcours à essayer par l'utilisateur ; suite T04 : interaction explicative de baie. APK de 13 h 42 inchangé, Chambre seule, 15 contrôles statiques passés ; nouvel APK puis essais Quest dès disponibilité. Aucune installation ni validation matérielle.

Fin de session : rapports/manifeste/signature/SHA256 conservés dans android-build-2026-10-05/final/, 29 sources comparées, runners syntaxiquement valides, diff vérifié et processus batch terminé. Connexion MCP à rétablir avant nouvelle interaction avec l'éditeur original ; le build batch est indépendant.

### Historique du build Android
05/10 à 13 h 37 — Premier APK ASCII compilé avec succès (53,85 Mo, 0 erreur/5 avertissements), 14 contrôles statiques réussis. Relecture du manifeste : suivi oculaire imposé malgré son absence du prototype. Correctif de projet préparé, prochaine étape : rebuild sur cache puis contrôle renforcé du manifeste/signature. Aucun appareil ou lancement Quest validé.

Tâche désormais réalisée : activer Android, relire validation/SDK/NDK/JDK, construire l'APK local dans un chemin ASCII et vérifier rapport/manifeste/signature. L'essai Quest reste distinct des tests PC et du contrôle statique APK.

05/10 — Android activé, audit sur plateforme cible sans erreur et SDK/NDK/JDK/Gradle vérifiés. Premier APK échoué : chemin non ASCII (`année 5`), photo et rapport à 12 h 26 concordants. Copie ASCII préparée sans déplacer l'original ; premier lancement batch arrêté avant journal, second lancé hors sandbox puis erreur de types dans le helper corrigée. Reprise sur cette même copie à 12 h 37 avec `Build-AndroidCopy.ps1 -Resume`. Contrôle statique APK préparé, non exécuté ; ADB ne détecte aucun appareil. Preuves : docs/validation/android-build-2026-10-05/. Aucun essai casque.

Build PC terminé le 05/10 : après une première tentative annulée d'origine inconnue, seconde réussie et test du player concluant. Guide `docs/BUILDS.md`, runner `tools/Test-PCBuild.ps1`. Configuration Android préparée avec menu CloudVR et audit ; aucune compilation APK ou validation matérielle.

Analyse documentaire du 29/09/2026 terminée : les quatre pages de `CDC_INSIDE_THE_DATA_CENTER.pdf` ont été extraites et inspectées visuellement, puis comparées au sujet original et aux plans. Voir STATE.md. T04 reste en cours : intégrer la cible étudiants ESILV année 4, expliciter introduction/objectifs et synthèse, proposer des mini-défis contextuels avec feedback (ambiguïté p. 1/p. 3 à clarifier), documenter le choix photo/IA et prévoir observation des utilisateurs, validation pédagogique et mesure des performances sur PC/Quest. La cible interne 10–15 min reste compatible avec les 10–20 min estimées du PDF. Ces ajustements sont proposés, sans remplacement des décisions validées. T06 et les niveaux de validation technique restent inchangés.

1. Parcours photo complet et court, avec explication des infrastructures communes.
2. Parcours IA scénarisé et choix depuis le PC de la chambre.
3. Tests de compréhension, narration vocale, optimisation et enrichissement technique.
4. Évaluation séparée des extensions RAG réel et web.
5. Stabilisation, documentation, démonstration et présentation GitHub.

Décomposer le prochain jalon au moment utile, sans générer maintenant un catalogue de tâches hypothétiques.
