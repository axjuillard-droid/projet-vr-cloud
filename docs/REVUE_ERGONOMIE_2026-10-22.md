# Revue du parcours CloudVR — 22 octobre 2026

Périmètre T22 : build T21 de 17 h 20, scripts des six activités et captures du player. Chambre observée dans la fenêtre native par computer-use ; une alerte Windows a gêné le clic et la souris verrouillée a changé l'orientation. Cet essai ne valide pas une visite naturelle complète. Images d'applications étrangères au jeu écartées des preuves publiques. Les captures T21 hors écran ne contiennent pas les Canvas Overlay ; leur absence ne prouve pas l'absence du viseur.

Résultat des correctifs T22 : build du 22/10 à 18 h 06 min 59 s, 88 contrôles pré-build et **193/193 contrôles fonctionnels PC** réussis à 18 h 10 min 34 s. Boutons des panneaux inspectés. Le premier parcours réussi montrait un « A » coupé sur le bouton réseau : libellé raccourci à « RÉTABLIR A », recompilé, inspecté et régression complète relancée. Capture utilisateur reçue : aide de la chambre visible sous le viseur. Pause et aides de salle blanche encore à revue humaine ; captures automatisées suspendues après texture nulle puis PNG noirs selon la règle des deux tentatives. [Preuves et limites](validation/ergonomics-2026-10-22/README.md).

## Défauts ciblés

| Réf. | Observation et preuve | Traitement T22 |
|---|---|---|
| ER-01 | La croix PC reste identique devant une poignée, un nœud ou un mur. Les cubes de maquette n'annoncent pas leur sélection. Code PCCrosshair T21 et revue des panneaux. | Indication courte sous le viseur, couleur et nom de l'objet atteint ; prérequis et portée. Premier obstacle et fonctionnement testés ; composition Overlay encore à revue humaine. |
| ER-02 | Libellés inférieurs des maquettes nettement plus petits que les textes principaux à 1280×720. Captures Réseau/Stockage/TGBT. | Agrandissement de 50 % de la taille physique du texte des boutons Données, 25 % pour Électricité/Supervision ; hauteur ajustée pour deux lignes. Panneaux du monde inspectés, aucun confort casque déduit. |
| ER-03 | Aucun retour visible d'Échap dans la chambre. Facility/Data bloquent leur progression, mais le thermique P1 et les animations de baie continuent selon leur propre temps. Code des modules et fenêtre native. | Pause PC du temps et de l'audio, libellé de reprise ; restitution des valeurs antérieures, y compris quand le rig est désactivé/détruit. |
| ER-04 | ScreenClickHandler et PhotoSender.Send ne contrôlent pas InputPaused. Un appel peut lancer l'envoi alors que les commandes sont suspendues. Lecture du code ; pas de clic natif probant après l'alerte Windows. | Protection aux deux points d'entrée ; test avec clic synthétique et appel API distingués. |
| ER-05 | Retour par Entrée et replay par R ne respectent pas la pause. Code DemoTour/DemoRoom. | Gardes ajoutées ; test du retour bloqué puis reprise. |
| ER-06 | Le viseur est affiché sans tenir compte de la séquence photo/réseau. Code T21 ; les PNG Camera.Render ne permettent pas de l'observer. | Viseur/aide masqués pendant l'envoi et le film. La séquence courte reste continue ; Échap redevient une pause après elle. |

Les aides ne choisissent ni action correcte ni progression à la place du visiteur. Un objet sélectionnable peut toujours expliquer un refus ; la couleur du viseur indique une sélection possible, pas la validité pédagogique du choix. Les quatre maquettes et panneaux gardent leurs textes dans le monde ; pas de retour au HUD massif de salle blanche.

## Limites et défauts restant à traiter

| Priorité | Constat | Vérification ou travail suivant |
|---|---|---|
| P1 | Consignes parfois longues et répétées entre borne et panneau ; supervision dense. | Observer les hésitations et la reformulation par des visiteurs, puis raccourcir avec validation pédagogique. Ne pas inventer un temps de compréhension à partir du probe. |
| P1 | Les maquettes restent des schémas guidés, pas la manipulation d'équipements réseau/stockage industriels. | Évaluer leur compréhension avant d'ajouter des modèles détaillés ; garder leurs simplifications explicites. |
| P2 | Baies sombres et répétitives, matériaux/détails peu variés ; sensation d'échelle supérieure au niveau de détail. | Revue artistique et références industrielles, sans masquer les espaces de circulation ni transformer toutes les façades en sources lumineuses. |
| P2 | Certains luminaires paraissent très brillants et des joints/lignes minces peuvent encore aliaser. | Comparaisons aux mêmes angles, puis mesures avant réglage URP/éclairage. Le correctif des grands triangles de mur ne garantit pas l'absence de tout scintillement. |
| P1 | Atlas d'ombres réduit encore signalé dans les journaux ; aucun budget de frame PC/Quest établi. | Profiler sur matériel identifié. Les durées du runner ne sont pas une mesure de performance d'affichage. |
| P1 | Voix finale absente ; confort, taille angulaire des textes et interactions manettes non validés. | Essai sur casque identifié, daté, avec lecture et sélection ; ne pas transposer les tailles PC à une validation VR. |

Sources de conception et limites dans [RESOURCES](RESOURCES.md), vision dans [VISION](VISION.md). T22 est un lot d'ergonomie PC ; il ne clôt pas la critique artistique, l'évaluation pédagogique ou la validation matérielle du projet.
