# Chambre → DataCenter — 5 octobre 2026

Premier morceau jouable : envoi photo illustratif, confirmation puis chargement asynchrone unique de DataCenter. Salle pédagogique originale, première baie repérée et deux baies secondaires, matériaux partagés et prefab ServerRack. Aucun trajet réseau réel ou site/entreprise prétendument représenté. Textes provisoires ; voix et sous-titres finaux restent à produire.

Builder reproductible : CloudVR > Build Scene > Data center et transition photo, ou DataCenterSceneBuilder.Build() hors Play avec scènes propres. Régénère DataCenter depuis les rigs de Chambre, relie StepTransition, inscrit les deux scènes et sauvegarde. Rejoué deux fois ; la plaque de la baie a été avancée pour masquer les voyants derrière le texte. Les contrôles du player final doivent valider ce dernier réglage visuel.

Validation dans Unity 6000.0.58f2/Intel Iris Xe/DX11 :

- editor-pc/ : 13 contrôles réussis à 14 h 27 Paris, clic Input System/raycast, scène remplacée, mode/spawn/unicité, clavier synthétique et collision ; deux captures inspectées. Le contrôleur est isolé dans Chambre pour orienter la caméra vers l'écran, actif dans DataCenter pour les tests de déplacement.
- photo-regression/ : 9 contrôles réussis à 14 h 30, resets et unicité de l'envoi toujours fonctionnels. Aucun transfert de scène dans ce test de reset, qui annule la transition différée avant son délai.
- editor-vr/ : 13 contrôles réussis à 14 h 31, mode VR forcé, Send appelé directement, conservation du mode lors du chargement, provider/zone liés à DataCenter, téléportation injectée. Aucun casque, tracking, manette, chemin rayon → zone ou confort validé.

Premier build Windows deux scènes réussi, job build-b265706a63 : 69,9 s, 138,49 Mo rapportés, 0 erreur/0 avertissement. Premier test GPU du player échoué au contrôle collision après 11 contrôles réussis (player-first-failed/). Les attentes en temps réel du probe étaient différentes du temps intégré par PCPlayerController ; attentes de mouvement remplacées par WaitForSeconds et positions ajoutées au rapport pour la seconde tentative. Aucun code de collision modifié, hypothèse de chronométrage encore à confirmer. Ancien player sauvegardé dans Builds/Windows-Photo-20261005/. APK existant inchangé et limité à Chambre. Helper de copie Android adapté pour les scènes enabled, compilation dans une nouvelle copie et contrôle APK encore nécessaires.

Reproduction : Play depuis Chambre puis PhotoInteractionSmoke.Run() pour la régression photo, ou JourneySmoke.Run() pour le parcours PC, ou JourneySmoke.Run(true) pour la logique VR sans matériel ; arrêter Play entre scénarios. Les probes temporaires ne sont pas sauvegardés, seul JourneySmoke survit temporairement au chargement pour observer la nouvelle scène. Runner Windows : tools/Test-PCBuild.ps1 -Graphics -Journey.

## Livrable final et contrôles Windows

Second build réussi à 14 h 41 Paris, build-70a8e60074 : 34,9 s, 138,49 Mo rapportés, 0 erreur/0 avertissement. Fichier windows-build-final-result.json. Exe au chemin habituel ConnectionTest/Builds/Windows/CloudVR-PC.exe avec tout son dossier de données.

- player-journey/ : 13 contrôles réussis à 14 h 42, WindowsPlayer/Direct3D11/Intel Iris Xe, exitCode=0. Position arrêtée par le collider (0, 0,08, 6,67). Le changement du chronométrage du probe suffit, sans modification du code de collision. Deux captures finales inspectées ; la plaque masque les voyants après l'ajustement de géométrie.
- player-photo/ : 9 contrôles réussis à 14 h 45, même player, exitCode=0. Délai runner augmenté explicitement à 90 s pour cette reprise. player-photo-timeout/ conserve le premier run : rapport passed=true mais runner timeout à 45 s, origine du délai de fermeture non établie. Ne pas le confondre avec un contrôle réussi du processus.
- player-sha256.json : empreintes de l'exe, data.unity3d et Assembly-CSharp.dll. L'exe Unity seul ne caractérise pas tout le contenu du build ; garder le dossier de données.
- editor-final.json : Chambre propre, Auto, Windows64, hors Play/build/compilation, une caméra et aucun probe. Aucun processus de test laissé actif ; diff des fichiers texte vérifié. À la fin des essais, aucun commit/push n'avait été fait ; publication traitée ensuite dans STATE.md.

Niveau atteint : implémenté, compilé Windows et parcours testé automatiquement avec GPU. Essai utilisateur du nouveau parcours, compréhension, mouvement naturel, FPS et casque restent à mesurer ; aucun nouveau APK. Aucun déplacement automatique continu de caméra, trajet de photo réel, infrastructure/entreprise réelle ou narration finale revendiqués.

Dernière lecture console MCP après les tests du player : no_unity_session. Pas de retry, de changement ou de crash déduit. La preuve d'état éditeur est celle de 14 h 42 ; aucune revalidation live plus récente. Connexion à vérifier à la prochaine session ; livrable et rapports du player déjà contrôlés.
