# P1 — Inspection de baie et diagnostic d'air

## Comportement implémenté

- **Borne 02** : aller à Baie_01, sélectionner sa poignée (quart de tour, ouverture sur charnières), puis la prise du serveur 2U (sortie de 0,64 m sur rails). Identifier les repères CPU, RAM et NVMe : les textes distinguent exécution, mémoire volatile et stockage persistant. Les repères consultés passent au vert. Réinsérer le tiroir pour refermer la porte et terminer l'étape. Le clic console seul, la fermeture prématurée et les clics pendant une animation ne terminent pas l'inspection.
- **Borne 04** : le refroidissement fonctionne déjà. Une porte de confinement demeure entrouverte ; une température d'entrée élevée déclenche le scénario. Acquitter l'alarme ne corrige pas la cause. Ventiler davantage augmente les auxiliaires sans supprimer la fuite. Sélectionner la poignée verte pour fermer, puis attendre le retour progressif de la température sous la cible de validation. L'étape se termine après stabilisation, pas au premier clic.
- Afficheurs et commandes dans le monde, marqueurs de circulation d'air, témoin de ventilateur animé et ambiance sonore variable. Aucun HUD massif ajouté au data center. Les bornes Réseau/Stockage/Électricité/Supervision et la transition V2 conservent leurs mécaniques précédentes.

Les commandes PC utilisent Input System et respectent la pause. Les huit cibles utilisent aussi `XRSimpleInteractable.selectEntered`. Les animations sont déclenchées par clic/sélection, sur une trajectoire contrainte ; il ne s'agit pas d'un système de prise libre ou d'une simulation mécanique de maintenance. Le modèle détaillé du serveur et la porte FBX sont réutilisés ; les façades fusionnées de l'ancienne Baie_01 sont désactivées au profit d'un cadre ouvert. Les 31 autres baies restent celles de P0.

## Réalisme et simplifications

La baie est explicitement une **démonstration hors production**. Le module ne représente ni une procédure de maintenance d'un serveur en service, ni un transfert réel de photo.

Le [guide DOE 2024](https://www.energy.gov/sites/default/files/2024-07/best-practice-guide-data-center-design.pdf) et [ENERGY STAR](https://www.energystar.gov/products/data_center_equipment/16-more-ways-cut-energy-waste-data-center/containmentenclosures) décrivent l'intérêt de limiter le mélange et le contournement d'air. Ce principe inspire le scénario ; aucune modélisation CFD ni donnée d'un site réel n'est utilisée.

| Paramètre de scénario | Valeur choisie, fictive |
|---|---|
| Charge IT constante | 12 kW |
| Auxiliaires, nominal / fuite / ventilation maximale | 3 / 4 / 5 kW |
| Température d'entrée cible, nominal / fuite / ventilation maximale | 22 / 32 / 30 °C |
| Retour thermique | Relaxation exponentielle, constante de 1,5 s ; temps accéléré |
| Validation | Porte fermée, entrée <24 °C pendant une seconde |

Ces chiffres sont des choix pédagogiques et ne prescrivent pas des seuils industriels. Le ratio de puissances (IT + auxiliaires) / IT affiche 1,25 / 1,33 / 1,42 selon l'état, sous la mention **« PUE indicatif, hypothèse de régime constant »**. Le PUE de référence décrit par le DOE §8.1 porte sur les énergies annuelles du site et de l'IT : ce ratio fictif n'est ni une mesure annuelle ni une comparaison avec un opérateur. Aucun chiffre carbone/eau réel n'est déduit.

## Validation Windows finale

**Build livré le 21/10 à 23 h 15 min 47 s Paris** : [Succeeded](build-result.json), Unity 6000.0.58f2, 32,09 s, 146 965 984 octets pour le build, **0 erreur / 0 avertissement BuildPipeline**. Resume/Wait puis Complete exécutés séparément. [Vingt contrôles P1](p1-learning-audit.json) et [quatorze contrôles P0](p0-layout-audit.json) pré-build réussis.

**[64/64 contrôles player réussis](player/demo-smoke-result.json)**, Direct3D11, [exitCode=0/passed=true](player/launch-result.json), fin à 23 h 19 min 32 s Paris. Marche au clavier synthétique de la cour au fond Z=39,0004 m en 24,75 s, sas/scan, accès électrique, sept activations de console plus manipulations, conclusion et retour/reset. Dix-neuf captures conservées ; baie extraite et afficheur thermique avant/après inspectés. Les grands aplats thermiques hérités ont été remplacés par deux petits témoins sur les façades.

![Porte ouverte et serveur extrait dans le player](player/demo-server-extracted.png)

| Ventilation accrue, fuite présente | Confinement fermé, température revenue |
|---|---|
| ![Choix non réparateur, auxiliaires fictifs à 5 kW](player/demo-heat.png) | ![Retour thermique progressif, auxiliaires fictifs à 3 kW](player/demo-cooling-recovered.png) |

Le premier essai a échoué sur la visée de poignée : [rapport initial](first-failure/demo-smoke-result.json), [journal](first-failure/Player.log), [mesure MCP](self-ray-diagnostic.json). Le rayon plongeant rencontrait le corps du visiteur avant la cible. `TryPick` ignore maintenant seulement le rig observateur, et conserve le premier obstacle du décor. Le test vérifie aussi qu'une porte fermée masque le tiroir. Un premier passage 64/64 après cette correction a été suivi de la retouche visuelle et de l'essai final documenté ici.

[Journal final anonymisé](player/Player.log) : réduction de résolution de six shadow maps dans l'atlas 2048 conservée. Aucun benchmark PC/Quest, essai humain de confort, validation de prise libre ou test de manettes. Aucun nouvel APK.

[Manifeste SHA-256](delivery-manifest.json) du dossier Windows complet : données reconstruites à 23 h 15 min 33 s, DLL à 23 h 15 min 37 s. Le lanceur Unity de 672 256 octets garde sa date du 20/10 à 14 h 58 ; cela ne date pas les scènes ni les scripts.

Les deux scènes originales livrées ont les mêmes SHA-256 que la copie compilée. Après rechargement externe, module/tour présents et scène propre observés par MCP. Deux requêtes composites de diagnostic ont ensuite échoué sur NullReferenceException ; cet audit additionnel est arrêté pour revue, sans en déduire un bilan complet des matériaux. Chambre restituée hors Play, console retournant zéro erreur. [Constat éditeur](editor-final-observation.json).

La régression P1 conserve la marche cour/sas/salle et l'accès électrique, puis utilise des poses contrôlées pour viser les interactions. Elle vérifie la pause, les doubles clics, la remise en place obligatoire et les choix thermiques non réparateurs avant le retour à la normale. Un essai humain et le casque restent distincts.

## Publication et outils

Le verrou Git antérieur a été revu après GO : fichier vide ancien, ouverture exclusive possible, aucun `git.exe` actif ; seul un filtre LFS orphelin subsistait. Verrou conservé par déplacement sous tmp, aucun processus arrêté. P1 validé sur PC ; commit/push en préparation, confirmation dans STATE. L'échec historique est conservé dans les preuves P0.

L'éditeur avait une boîte de rechargement externe qui bloquait le MCP : scène rechargée par computer-use, puis C# et mesures vérifiés. Un appel du builder a ensuite expiré côté outil tout en enregistrant les scènes : reprise de build avec de nouveaux gardes, sans contourner la protection des scènes. Ne jamais conclure à l'absence de mutation à partir du seul timeout de transport.
