# T22 — Correctifs PC et limites de validation, 22 octobre 2026

Build final **18 h 06 min 59 s Paris**, Succeeded en 17,62 s, 147 111 310 octets au total, 0 erreur / 0 avertissement BuildPipeline. **193/193 contrôles fonctionnels Direct3D11** terminés à 18 h 10 min 34 s, exitCode=0. [Build](build-result.json), [rapport player](pc/demo-smoke-result.json), [lancement](pc/launch-result.json), [journal anonymisé](pc/Player.log), [manifeste SHA-256](delivery-manifest.json).

## Fonctionnalités et contrôles

Viseur PC coloré et libellé de l'objet réellement atteint, prérequis/distance et premier obstacle conservés. Portées inchangées : commandes 3,5 m, bornes 4,5 m, écran selon son composant. Une sélection possible ne garantit pas un choix pédagogique valide. Aide masquée pendant envoi/film ; pas de HUD massif ni aide PC dans le casque. Petits textes Données agrandis de 50 % en taille physique, Électricité/Supervision de 25 %, hauteur adaptée aux deux lignes ; captures du monde inspectées.

Échap hors envoi/film suspend temps et audio, affiche une indication de reprise et bloque envoi/retour/rejeu. Valeurs précédentes restituées à la reprise ou désactivation/destruction du rig ; désactivation vérifiée par API sur rig temporaire sans casque. Porte, température, ventilateur et copie immobilisés pendant la pause. L'envoi/film reste une séquence courte continue, non interrompue par Échap.

**88 pré-build** : [P0, 14](p0-layout-audit.json), [P1, 20](p1-learning-audit.json), [P2, 11](p2-network-audit.json), [exploitation, 19](p3-facility-audit.json), [données, 24](data-service-audit.json). [P0 éditeur](t22-p0-editor.json) et [données éditeur](t22-data-editor.json) réussis. Le player conserve les six activités et ajoute aides sur les vraies cibles, obstacle, pauses et gardes, nettoyage temps/audio. Entrées synthétiques et visées contrôlées ; épuisement électrique par pas API de 200 s, distinct de la marche au clavier. Aucun parcours humain complet ou benchmark déduit.

## Capture Overlay : vérification suspendue

L'API ScreenCapture.CaptureScreenshotAsTexture existe dans Unity 6000.0.58f2, vérifiée par réflexion ; cela ne validait pas son contexte. [Premier essai batch](first-pc-capture-failed/demo-smoke-result.json) : deux contrôles réussis puis texture nulle. [Second essai hors batch en fenêtre masquée](second-pc-black-frame/demo-smoke-result.json) : neuf contrôles réussis, deux PNG entièrement noirs, puis erreur du probe cherchant par défaut le viseur que le film avait désactivé. Les deux essais et journaux sont conservés. Le contrôle du viseur inclut maintenant les objets inactifs et passe dans le run final.

**La composition visuelle du nouvel Overlay n'est pas validée.** Après deux captures non probantes, cette vérification est suspendue pour revue humaine selon [AGENTS](../../../AGENTS.md). La capture est laissée en opt-in `--cloudvr-capture-interface` et n'a pas été relancée. Le run fonctionnel final n'exécute pas cette capture. Le runner -Graphics n'utilise plus -batchmode ; le headless conserve -batchmode/-nographics. L'origine du backbuffer noir en fenêtre masquée n'est pas établie.

Les captures Camera.Render du run final vérifient les panneaux et leur texte dans le monde ; elles omettent toujours l'Overlay. Chambre observée dans la fenêtre native, mais clic gêné par une alerte Windows ; aucune validation de clic humain en pause attribuée à cet essai. Images d'autres applications écartées. [Revue et défauts restants](../../REVUE_ERGONOMIE_2026-10-22.md).

## Procédure et conservation

[Premier build compilé](preliminary-build.json), non livré : probe de désactivation corrigé en revue pour désactiver un rig encore actif, puis recompilé avant player. Après les deux essais ci-dessus, recherche d'objet inactif et capture opt-in corrigées ; [premier parcours fonctionnel réussi, 193/193](first-functional-pc/demo-smoke-result.json), build de 18 h 00. Sa capture réseau montrait le dernier « A » coupé : libellé raccourci de « RÉTABLIR LIEN A » à « RÉTABLIR A », puis recompilation et régression complète ci-dessus. Le libellé final a été inspecté sur la capture du monde. [Audit](preservation-audit.json) : vingt matériaux initiaux identiques par SHA-256, exclus du commit, deux scènes identiques à la copie compilée. ProjectSettings, sujet universitaire, APK et dépendances inchangés ; aucun média/package tiers ajouté. Réutilisation uGUI/PCCrosshair/raypicker, sources Unity dans [RESOURCES](../../RESOURCES.md).

MCP bloqué par dialogue Reload de Chambre puis débloqué via computer-use sur scène propre ; scène vide propre pendant Complete, Chambre propre hors Play restituée, console retournant zéro erreur. Atlas d'ombres réduit et diagnostic de fermeture mémoire conservés sans bilan de performance. Aucun casque, voix finale, compréhension ou confort humain validés. T22 reste en cours pour la revue visuelle du libellé de sélection et de la pause.

| Maquette réseau, panneaux dans le monde | Maquette stockage, boutons agrandis |
|---|---|
| ![Réseau après basculement](pc/demo-network-alternate.png) | ![Stockage après restauration](pc/demo-storage-restored.png) |
