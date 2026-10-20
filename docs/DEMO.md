# Première démo guidée — parcours photo

Préparation du 20 octobre 2026. Consulter STATE.md et les preuves pour connaître le niveau réellement validé ; ce guide décrit le parcours implémenté, pas une certification Quest.

## Jouer sur PC

Lancer le dossier complet `ConnectionTest/Builds/Windows/`, puis `CloudVR-PC.exe`. Les exports ne se mettent pas à jour avec les scripts : reconstruire après une modification. La démo est hors ligne, sans compte ni photo personnelle.

1. Dans Chambre, viser l'écran du bureau et cliquer sur la photo d'exemple.
2. Dans DataCenter, suivre l'objectif et les bornes numérotées. Les bornes alternent gauche/droite et laissent l'allée centrale libre.
3. Réseau : observer un échange représenté vers la baie.
4. Traitement : distinguer la machine du logiciel qu'elle exécute.
5. Stockage : découvrir pourquoi le fichier reste disponible après l'envoi.
6. Refroidissement : révéler la chaleur, puis activer à nouveau la borne pour rétablir le refroidissement représenté.
7. Électricité : illustrer le relais batterie, puis rétablir la source principale avec une seconde activation.
8. Supervision : transmettre l'alerte à l'équipe ; lire la synthèse.
9. Entrée pour retrouver la photo dans Chambre. R pour recommencer.

Commandes : WASD/ZQSD/flèches, souris pour regarder, clic ou E sur une borne à moins de 4,5 m. Échap libère/verrouille la souris et suspend le déplacement/regard PC tant qu'elle est libre. Les étapes déjà vues se relisent ; une borne future ne saute pas la progression. Les animations durent environ 2,8 s ; les explications restent affichées jusqu'à l'action suivante. Aucun quiz final imposé.

## Ce que représente cette démo

Six notions complémentaires dans une seule salle. Le flux lumineux est un schéma, sans mesure de trajet ni de débit. La couleur thermique est un repère pédagogique, sans calcul de température. La batterie n'a pas d'autonomie simulée ; le refroidissement à air est un exemple, pas l'unique technologie possible. Les secours et les responsabilités des équipes sont simplifiés. La photo retrouvée est l'image d'exemple locale ; aucun fichier n'est envoyé ou hébergé.

Les textes s'appuient sur les sources de RESOURCES.md, section T08. Voix, sous-titres synchronisés à une voix, approfondissements chiffrés, entreprises réelles, équipements industriels détaillés, interaction IA et validation pédagogique restent des étapes distinctes. Les textes affichés servent de narration provisoire lisible sans son.

L'accueil et les bornes ont un raccordement XRI ; le nouveau parcours complet reste à vérifier dans un casque identifié. Il ne faut pas déduire une validation Quest de la version PC ou de la présence de ces composants.

## Reconstruire et contrôler

`CloudVR > Build Scene > Démo guidée complète` ajoute/remplace uniquement le groupe Demo_Parcours dans DataCenter et l'accueil dans Chambre ; les autres décors sont conservés. Sauvegarder les scènes avant le builder. Le builder de décor T07, qui régénère DataCenter, doit être suivi du builder de démo si on l'utilise.

Alternative isolée, sans fermer l'éditeur :

```powershell
./tools/Build-DemoCopy.ps1
# Lire le journal et attendre l'arrêt du PID indiqué, puis :
./tools/Build-DemoCopy.ps1 -Complete
./tools/Test-PCBuild.ps1 -Graphics -Demo -TimeoutSeconds 120
```

La copie contient Assets/Packages/ProjectSettings ; seule la dépendance du connecteur MCP éditeur est omise dans cette copie pour éviter un clone Git réseau bloquant. Les packages du jeu restent aux versions du projet. Le helper régénère les bornes et construit deux scènes. Complete exige succès du BuildReport, arrêt normal du batch et empreintes inchangées des scènes/réglages originaux avant livraison ; il conserve l'ancien player dans Windows-PreDemo-20261020. Les scènes générées et matériaux Demo sont rapatriés, aucun ProjectSettings de la copie ne remplace l'original. Si l'éditeur original est resté ouvert, rouvrir Chambre depuis le disque avant de sauvegarder une ancienne scène en mémoire.

Après une correction, Resume synchronise la copie avant relance ; ne l'utiliser qu'une fois le batch terminé. Conserver le journal précédent avant reprise. Les copies et builds restent exclus de Git. Aucun nettoyage automatique.

Le test Demo utilise la souris/clavier synthétiques pour l'envoi, une avancée initiale, les huit activations, le retour et le redémarrage. Pour viser chaque borne de façon reproductible, il repositionne explicitement le rig : ces contrôles ne prouvent donc pas un trajet à pied complet ni le confort. Les captures montrent le player réel. La navigation naturelle exige un essai humain distinct.
