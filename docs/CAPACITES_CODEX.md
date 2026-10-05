# Capacités et mode d'emploi de Codex dans ce projet

État vérifié le 5 octobre 2026. Ce guide couvre les moyens utiles au projet ; les outils accessibles peuvent varier entre les sessions. `STATE.md` fait foi pour les validations, `TOOLING.md` pour l'inventaire installé. Un outil disponible ne signifie pas qu'un test a réussi.

## Ce que Codex peut prendre en charge

| Domaine | Ce que je peux faire et comment | Niveau constaté / conditions |
|---|---|---|
| Fichiers et C# | Lire et rechercher avec `rg`, modifier les scripts et notes dans le dossier du projet, produire des scripts reproductibles. Après modification C#, laisser Unity importer/recompiler puis lire la console. | Lecture et écriture vérifiées. Scripts CloudVR compilés dans l'éditeur ; comportement à vérifier séparément. |
| Unity : état | Lire les instances, projet, scène, hiérarchie, composants, références et console par MCP. Comparer l'état avant et après une action. | Vérifié dans ConnectionTest. Reconfirmer l'instance à chaque session. |
| Unity : scène | Créer/configurer des objets, composants, matériaux, prefabs et scènes par les outils MCP ; utiliser un setup éditeur réexécutable quand il faut reproduire une configuration. Relire puis sauvegarder explicitement. | Création et relecture vérifiées ; chargement et sauvegarde de scène possibles. Préserver une scène modifiée par l'utilisateur. Tous les types de composants ne sont pas validés. |
| Unity : Play | Appeler `manage_editor` avec `play`, `pause` ou `stop`, inspecter les états pendant l'exécution et lire les erreurs. | Entrée/sortie Play vérifiées. Un passage en Play n'est pas une validation de gameplay. |
| Unity : code ponctuel | `execute_code` exécute un corps de méthode C# dans l'éditeur ; utile pour inspecter des références, raccorder un événement ou appeler un setup déjà compilé. Vérifier les API et limiter chaque opération à un objectif. | Vérifié, compilateur CodeDom dans cette session. Préférer du code compatible avec ce compilateur ; ne pas supposer que la syntaxe C# récente est acceptée. |
| Unity : API | `unity_reflect` vérifie types, signatures et membres réellement chargés ; `unity_docs` et les documents locaux des packages complètent l'information. | Réflexion vérifiée. Consulter avant d'utiliser une API incertaine. |
| Packages et samples | Inspecter manifest/lock et importer les samples des packages déjà installés, via Package Manager/MCP ou `Sample.Import()`. Vérifier licence, dépendances et compilation. | Starter Assets XRI 3.0.11 importés. Aucun upgrade automatique de Unity/XRI décidé. |
| Tests automatisés | Créer des tests EditMode/PlayMode ciblés, lancer `run_tests`, récupérer le `job_id`, suivre `get_test_job`, enregistrer assertions et résultats. Tester notamment événements, états, durée et absence de doubles envois. | Outils annoncés par le serveur ; cette suite n'a pas encore été exécutée dans le projet. Un appel direct à `PhotoSender.Send()` ne teste pas le clic. |
| Entrées PC simulées | Avec Input System, injecter des événements clavier/souris dans le PlayerLoop puis observer raycast, déclenchement et confirmation. Vérifier caméra active, focus, verrouillage du curseur et périphériques. | Souris synthétique vérifiée le 05/10 : PhotoInteractionSmoke, 9 contrôles réussis dans l'éditeur. Preuves dans validation/photo-2026-10-05/. Clavier, déplacement complet et exécutable non revalidés. Distinguer injection, appel direct et clic humain. |
| XR et Quest | Configurer XR Origin, contrôleurs, interactions et téléportation à partir des Starter Assets ; inspecter bindings, colliders et composants. Une simulation XR peut contrôler une partie du comportement sans casque. | 05/10 : rig et zone intégrés/compilés ; 12 vérifications logiques XR par API sans casque réussies, démarrage VR/retour PC contrôlés et régression souris réussie. Voir validation/xr-2026-10-05/. Bindings manettes, rayon → zone → requête, tracking, rotation et Quest non testés. |
| Builds | Utiliser `manage_build` : lire plateforme/scènes/settings, construire PC ou Android, suivre `status`, lire le rapport et vérifier les fichiers produits. Alternative : Unity batchmode avec méthode éditeur, sur un dossier distinct si le projet de travail reste ouvert. | 05/10 : Windows64 réussi et testé ; APK Android 53,85 Mo compilé dans une copie ASCII, 15 contrôles statiques passés et manifeste corrigé. Build-AndroidCopy.ps1/Test-AndroidApk.ps1 reproductibles ; aucun lancement Quest. Guide BUILDS.md. |
| Exécutable PC | Lancer un build avec les permissions disponibles, capturer les logs, effectuer des tests prévus dans le programme et mesurer le comportement observé. | 05/10 : player Windows lancé masqué par tools/Test-PCBuild.ps1, 9 checks photo réussis en headless puis Direct3D11, captures inspectées et exitCode=0. Entrée synthétique dans le programme ; contrôle natif de fenêtres Windows désactivé, essai utilisateur/mouvement/performance non validés. |
| Appareil Android | Avec un Quest connecté et autorisé en mode développeur, utiliser `adb` pour vérifier sa présence, installer un APK, lire `logcat` et recueillir des mesures. | `adb devices -l` exécuté le 05/10 : aucun appareil détecté. Aucune installation ou mesure casque ; autorisation USB initiale et port du casque demandent l'utilisateur. |
| Performances | Utiliser le profiler Unity et/ou logs du build pour relever temps de frame, mémoire et erreurs, avec plateforme et conditions de mesure. Optimiser après identification du coût réel. | Outil profiler annoncé ; aucune mesure de performance PC/Quest consignée. Une mesure éditeur ne représente pas la performance Quest. |
| Blender | Lancer Blender en arrière-plan avec Python `bpy` pour inspecter/générer/adapter des modèles et exporter. Rechercher un asset adapté avant de créer, puis vérifier échelle, matériaux et complexité. | Blender 4.5.2 LTS lancé et lu par Python. Export/import Unity et modèle produit non validés ; MCP Blender non installé. |
| Assets et pédagogie | Chercher des ressources gratuites, vérifier droits et provenance, adapter le scénario, sourcer les mécanismes techniques, préparer narration provisoire, sous-titres et protocole de compréhension. | Recherche et conception déjà documentées. Voix finale, textes définitifs et compréhension utilisateur non validés. |
| Images et documents | Selon les outils/skills disponibles : générer des illustrations, lire/créer des PDF, documents Word, présentations ou tableaux ; créer un document LaTeX dans l'éditeur intégré puis compiler. | Capacités disponibles selon la session ; pas une preuve de production ou d'import dans Unity. Appliquer le skill du format et vérifier visuellement les livrables. Préserver `Le cloud.docx`. |
| Git et collaboration | Inspecter les différences, préparer commits/branches et descriptions de PR ; pousser ou créer une PR dans le cadre demandé et avec l'accès au remote. | Dépôt/LFS et remote préparés. Accès `.git` restreint dans cette session : même `git status` a rencontré une écriture LFS refusée. Demander l'escalade requise, ne pas contourner les permissions. |
| Suivi | Mettre à jour STATE/BACKLOG après une avancée, consigner preuves et inconnues, recommander le réglage du prochain prompt. | Procédure utilisée. `AGENTS.md` porte désormais la recommandation systématique modèle/effort. |

## Reprendre le pilotage Unity

1. Lire `STATE.md`, puis la tâche prête du `BACKLOG.md` ; charger seulement les autres documents nécessaires.
2. Unity doit ouvrir `ConnectionTest` avec la version prévue. Si Unity est fermé, vérifier le chemin de l'éditeur et le projet avant de proposer/lancer l'ouverture avec les permissions requises. L'ouverture automatique n'est pas encore validée ici.
3. Dans **Window > MCP for Unity**, sélectionner **HTTP Local**, adresse `http://127.0.0.1:8080`, démarrer le serveur et connecter l'éditeur. Le voyant doit indiquer **Session Active (ConnectionTest)**. « Configured » côté Codex ne suffit pas.
4. Vérifier les outils Unity natifs exposés au chat. S'ils sont absents, l'accès HTTP MCP direct via PowerShell a été vérifié. Endpoint MCP : `http://127.0.0.1:8080/mcp`.
5. Initialiser une nouvelle session JSON-RPC ; conserver son en-tête `Mcp-Session-Id` pour cette session uniquement. Accepter `application/json, text/event-stream` et analyser les lignes SSE `data:` ; ne pas traiter la réponse brute comme un JSON unique.
6. Lire `mcpforunity://custom-tools`, `mcpforunity://instances`, `mcpforunity://project/info` et `mcpforunity://editor/state`. Vérifier le chemin du projet, la scène, les modifications non sauvegardées, le mode Play et la compilation. Si plusieurs instances existent, sélectionner explicitement la bonne.
7. Lire `tools/list` : utiliser les schémas réels des arguments. Les noms et paramètres peuvent évoluer ; ne pas extrapoler depuis une version différente.
8. Effectuer l'action ciblée, relire son résultat et la console. Après modification de script, attendre la fin de compilation avant de manipuler son composant. Après une modification de scène, sauvegarder seulement si le résultat contrôlé est celui attendu.

Le dossier ignoré `tmp/unity/` contient les aides de la session du 05/10 : session HTTP temporaire, schémas et `Invoke-Mcp.ps1`. Ce dossier n'est pas une dépendance du projet et peut manquer sur un autre PC. Réinitialiser la session avant réutilisation ; ne jamais versionner son identifiant.

Exemples pour l'aide locale déjà présente, depuis la racine du projet et après initialisation de session :

```powershell
& ./tmp/unity/Invoke-Mcp.ps1 -Method resources/read -Uri 'mcpforunity://editor/state'
& ./tmp/unity/Invoke-Mcp.ps1 -Name read_console -Arguments '{"action":"get","types":["error","warning"],"count":"10"}'
& ./tmp/unity/Invoke-Mcp.ps1 -Name manage_editor -Arguments '{"action":"play"}'
& ./tmp/unity/Invoke-Mcp.ps1 -Name manage_editor -Arguments '{"action":"stop"}'
```

L'appel Play est une action, pas une étape à exécuter si les préconditions échouent. Le succès du transport HTTP ne suffit pas : examiner `success`, `isError`, le résultat de compilation et les erreurs retournées dans le contenu.

## Comment tester une fonctionnalité

Pour l'envoi photo, définir une preuve attendue avant de lancer le test : interaction d'entrée reçue par l'écran, état Ready → Sending → Completed, indicateur visible approprié, événement de fin reçu une fois, puis réinitialisation possible. Inclure un clic hors écran et un second clic pendant l'envoi pour vérifier les cas négatifs.

Procéder par niveaux :

1. **Implémenté** : code et références présents, pas nécessairement exécutés.
2. **Compilé dans l'éditeur** : Unity a terminé la compilation sans erreur.
3. **Testé dans l'éditeur PC** : test identifié et résultat observé ; indiquer si entrée synthétique, méthode appelée directement ou action humaine.
4. **Build PC réussi** : exécutable généré ; **testé sur PC** seulement après son lancement et les essais prévus.
5. **Build Android réussi** : APK généré ; ce résultat ne valide pas Quest.
6. **Testé dans le casque** : modèle, date, scénario, résultat et éventuelles mesures enregistrés.

Les tests avec casque portent notamment sur tracking, contrôleurs, téléportation, hauteur, lisibilité et confort. Les essais pédagogiques demandent de vrais participants ; Codex peut rédiger le protocole et analyser les observations, sans inventer les réponses.

## Quand l'utilisateur intervient

- Choix pédagogiques/artistiques importants et validation des propositions.
- Casque : accès matériel, mode développeur, autorisation USB, port du casque et appréciation du confort.
- Autorisations demandées par l'environnement : lancement hors sandbox, écriture hors périmètre, réseau restreint, accès Git/LFS ou installation nécessaire.
- Revue d'un blocage après deux tentatives infructueuses : preuves consignées, pas de boucle d'essais. Une reprise explicitement demandée permet une nouvelle phase de diagnostic ; cela ne supprime pas la règle pour les nouveaux essais.
- Changement du modèle/effort dans l'application. Une recommandation ne modifie pas la configuration du chat.

Je peux avancer sur les actions déjà autorisées sans demander de validation pour chaque manipulation. Je dois signaler précisément ce qui reste impossible ou non vérifié dans la session réelle. Connexion d'un service, achat, publication ou envoi d'un message à un tiers nécessitent le cadre demandé par l'utilisateur et les permissions appropriées.

## Choisir le modèle pour le prochain prompt

Conseils de travail datés du 05/10/2026, adaptés au projet et aux modèles annoncés par l'environnement ; vérifier les choix réellement affichés dans l'application. Ce tableau n'est pas un benchmark mesuré du projet ni une garantie de budget.

| Prochaine tâche | Réglage conseillé |
|---|---|
| Notes, petites corrections locales, synthèse courte | GPT-6 Luna — Medium, si disponible ; sinon GPT-6.1 Sol — Medium |
| Scripts C# et configuration courante d'une scène | GPT-6.1 Sol — Medium |
| Diagnostic d'interaction, XR/Quest, compilation et builds complexes | GPT-6.1 Sol — High |
| Blocage persistant ou architecture à plusieurs contraintes | GPT-6 Astra — High, si disponible |

Monter le niveau seulement quand la tâche le justifie ; Xhigh/Max ne constituent pas un réglage quotidien nécessaire. L'effort porte sur le raisonnement du modèle, pas sur la puissance de Unity ou du PC. Les [conseils officiels de sélection](https://developers.openai.com/api/docs/guides/model-selection) et la [description de l'effort](https://developers.openai.com/api/docs/guides/reasoning) ont été consultés pendant la session ; les recommandations par tâche ci-dessus sont notre choix de travail.

À chaque réponse finale, rappeler le réglage du prochain prompt et la tâche visée. Donner un réglage par option si plusieurs suites sont proposées. Si la suite est inconnue, conseiller le réglage courant et préciser quand passer à High. Ne pas différer un travail autorisé uniquement pour demander un changement de modèle.
