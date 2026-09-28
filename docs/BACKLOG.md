# Tâches et jalons

Statuts : à faire, en cours, bloqué, terminé. Une tâche n’est prête que si ses dépendances sont connues.

| ID | Tâche | Statut | Critère de fin |
|---|---|---|---|
| T00 | Organiser la mémoire du projet | terminé | Vision, décisions, état, backlog et méthode de recherche présents |
| T01 | Inventorier les outils locaux en lecture seule | terminé | Voir TOOLING.md ; versions et dossiers de modules constatés, limites signalées, aucune installation |
| T02 | Confirmer le matériel et les attentes universitaires | terminé | Inventaire des inconnues consigné dans DECISIONS.md ; utilisateur ne connaît pas modèles Quest, date exacte ni barème au 25/09/2026. Ce statut ne signifie pas matériel validé |
| T01a | Installer uv et connecter Unity en lecture | terminé | Exécutables et PATH vérifiés ; instance, projet et état lus par MCP HTTP |
| T01b | Tester une modification Unity par MCP | terminé | 25/09/2026 : CodexConnectionTest créé dans SampleScene, propriétés relues, console sans erreur ni avertissement ; autres objets inchangés dans la hiérarchie relue. Scène non sauvegardée ; voir TOOLING.md |
| T03 | Rechercher des bases gratuites pour le prototype | terminé | Sélection documentaire sourcée dans RESOURCES.md : XRI/OpenXR, Kenney, baie candidate et alternatives ; licences et limites de compatibilité explicites. Aucun import testé |
| T04 | Décrire le parcours photo minimal | en cours | Structure, niveaux pédagogiques et acteurs validés ; préférence pour des entreprises connues consignée en D11. Reste à sélectionner les entreprises et confirmer l'installation, détailler actions/transitions, sourcer les mécanismes et fixer les critères PC/Quest et le premier morceau jouable |
| T05 | Préparer le dépôt de développement | terminé | 28/09/2026 : Git + LFS configurés, remote GitHub public https://github.com/axjuillard-droid/projet-vr-cloud, premier commit poussé. ConnectionTest/ nettoyé (TutorialInfo et Readme supprimés, SampleScene renommée Chambre). Invitation tjoliot-30 à faire manuellement (Settings → Collaborators) |
| T06 | Construire une première interaction PC et Quest | en cours | 28/09/2026 : Scène Chambre construite en primitives URP (sol, murs, bureau, chaise, tour PC, écran, fenêtre, lumière). XRI 3.0.11 + OpenXR 1.14.0 configurés. Scripts CloudVR créés : InputModeManager, PCPlayerController, PhotoSender, StepTransition, ScreenClickHandler, PCCrosshair. Contrôle PC (WASD + souris) et raycast / détection survol écran testés et validés dans l'éditeur. Reste : validation du clic/envoi, rig VR Quest + téléportation, build et essai casque |

## Jalons suivants sans dates imposées
1. Parcours photo complet et court, avec explication des infrastructures communes.
2. Parcours IA scénarisé et choix depuis le PC de la chambre.
3. Tests de compréhension, narration vocale, optimisation et enrichissement technique.
4. Évaluation séparée des extensions RAG réel et web.
5. Stabilisation, documentation, démonstration et présentation GitHub.

Décomposer le prochain jalon au moment utile, sans générer maintenant un catalogue de tâches hypothétiques.
