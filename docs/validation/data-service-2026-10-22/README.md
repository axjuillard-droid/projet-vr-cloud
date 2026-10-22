# T21 — Réseau et stockage interactifs, 22 octobre 2026

Build Windows final **17 h 20 min 02 s Paris**, Succeeded en 58,76 s, 147 095 349 octets au total, 0 erreur / 0 avertissement BuildPipeline. **152/152 contrôles player Direct3D11** terminés à 17 h 23 min 42 s, exitCode=0. [Build](build-result.json), [résultat player](pc/demo-smoke-result.json), [lancement](pc/launch-result.json), [journal anonymisé](pc/Player.log), [manifeste de livraison SHA-256](delivery-manifest.json).

## Expérience et sources

Borne 01 : maquette à droite Z=8, sélectionner LIEN A, attendre le basculement automatique illustré vers B, lire SERVICE et rétablir A. Marqueurs mobiles sur le chemin disponible ; pas de stockage annoncé à cette étape. [Cisco, fabric et chemins IP](https://www.cisco.com/c/en/us/products/collateral/switches/nexus-9000-series-switches/white-paper-c11-743245.html) documente plusieurs chemins et ECMP. Notre schéma actif/secours n'implémente ni cette topologie complète, ni TCP/BGP/ECMP. Les 2 s et signaux sont fictifs, pas des mesures ou paquets capturés.

Borne 03 : maquette dans la niche droite Z=19,4, B pour préparer la réplique, C pour conserver un point distinct, A pour illustrer sa panne. Lire la photo sur B, observer une suppression propagée, restaurer C → B puis relire v1. Chaque copie/restauration prend 2 s fictives. A reste hors ligne, son éventuelle version résiduelle n'est pas servie. Neuf nouvelles commandes PC/XRI et quatre afficheurs dans le monde ; ancien flux unique à validation après clic supprimé.

[AWS, protection des données](https://docs.aws.amazon.com/AmazonS3/latest/userguide/data-protection.html), [suppression et réplication](https://docs.aws.amazon.com/AmazonS3/latest/userguide/delete-marker-replication.html), [restauration AWS Backup](https://docs.aws.amazon.com/aws-backup/latest/devguide/restoring-s3.html) servent de références documentaires, aucun SDK/service réel utilisé. La suppression propagée est **une configuration choisie pour le scénario**, tandis que S3 ne réplique pas le marqueur par défaut dans sa configuration actuelle. Aucune règle universelle de réplication n'est enseignée. C conserve v1 dans le modèle : sécurité, permissions, rétention, domaines de panne, versionnement complet, cohérence distribuée et validité d'une sauvegarde réelle ne sont pas simulés. Aucun fichier personnel stocké, transmis ou supprimé.

## Contrôles

**88 pré-build** : [P0, 14](p0-layout-audit.json), [P1, 20](p1-learning-audit.json), [P2, 11](p2-network-audit.json), [exploitation, 19](p3-facility-audit.json), [données, 24](data-service-audit.json). [P0 éditeur](t21-p0-editor.json) et [données éditeur](t21-data-editor.json) également réussis avant build.

Le probe ajoute aux 105 contrôles T18 47 contrôles : entrées et neuf visées réelles, refus avant panne/lecture/protection, basculement différé et signaux en mouvement, pause d'une copie, réplique indisponible avant fin, point conservé avant incident, lecture sur B sans restauration, suppression logique puis photo absente, restauration différée et relecture avant validation. Régression cour/sas/marche continue, baie, thermique, électricité, supervision, conclusion et retour/reset conservée. L'épuisement de la réserve électrique reste un pas API explicite de 200 s, distinct de la marche au clavier. Les visées utilisent des poses contrôlées ; ce n'est pas une visite humaine continue ni un benchmark.

Captures player Camera.Render 1280×720 : ces maquettes utilisent des Canvas dans le monde, inspectés visuellement. Les Canvas Overlay ne sont pas inclus dans ce type de capture ; narration de transition vérifiée dans la fenêtre native au jalon T17, pas revalidée nativement ici. Atlas d'ombres 2048 réduit encore signalé ; pas d'absence universelle de scintillement revendiquée. XRI raccordé et contrôlé statiquement ; **aucun essai manettes/casque, aucun nouvel APK**. Voix, compréhension, lisibilité et confort humains restent à valider.

Le journal conserve aussi le diagnostic de fermeture `PlayerConnection::Cleanup` de type `MemoryLeaks`, champ allocatedMemory de 589 099 octets ; T18 présentait déjà 588 787 octets. Attribution et impact non mesurés : aucun bilan de fuite mémoire ni de performance n'est déduit de ce message seul.

## Conservation et incident de procédure

[Audit de préservation](preservation-audit.json) : vingt matériaux modifiés avant ce lot identiques à leurs empreintes initiales, exclus du commit ; deux scènes identiques à la copie compilée. ProjectSettings, sujet universitaire, APK et dépendances inchangés. Helpers de panneaux T18, primitives, matériaux, uGUI/XRI et raypicker P1 réutilisés ; aucun média/package tiers ajouté.

L'import initial limité aux scripts n'avait pas encore importé les nouveaux fichiers : type temporairement absent, import de tous les assets puis compilation réussie. Un client Python de refresh resté en attente après le rechargement a été arrêté ; Unity conservé. Chambre propre hors Play et console retournant zéro erreur en fin de validation.

Un [premier lancement](preliminary-cancelled/launch-result.json) a été démarré avant la confirmation de Complete, puis volontairement arrêté (exitCode −1, aucun rapport final). Il est écarté, son journal et ses images partielles sont conservés. Le helper de restitution des matériaux a été ajusté à l'énumération des tableaux JSON de Windows PowerShell ; les vingt empreintes ont ensuite été vérifiées. L'essai final a démarré après livraison confirmée, sans changement du produit entre ces deux essais.

| Adaptation après perte de A | Signal sur le chemin B |
|---|---|
| ![Lien A indisponible](pc/demo-network-link-down.png) | ![Chemin B en service](pc/demo-network-alternate.png) |

| Réplique disponible après panne de A | Photo absente après suppression propagée |
|---|---|
| ![Lecture depuis B](pc/demo-storage-replica.png) | ![Point C conserve v1](pc/demo-storage-deleted.png) |

![Photo v1 restaurée puis relue sur B](pc/demo-storage-restored.png)

Prochaine revue humaine : rejoindre les maquettes sans poses imposées, trouver les nœuds sélectionnables, lire les petits boutons depuis la position de marche et reformuler la différence réplique/sauvegarde. Relever les hésitations et le temps de lecture avant de retoucher le rythme ou de préparer l'essai Quest.
