# T18 — Local électrique et supervision, 22 octobre 2026

Version Windows finale compilée à **12 h 38 min 58 s Paris**, **105/105 contrôles** Direct3D11 terminés à **12 h 42 min 29 s**, exitCode=0. Résultat BuildPipeline : 50,84 s, 147 058 323 octets au total, 0 erreur / 0 avertissement. Le lanceur peut conserver sa date ancienne ; [manifeste SHA-256 des fichiers livrés](delivery-manifest.json).

## Expérience disponible

- Après borne 05, rejoindre le local par sa porte, observer l'UPS original et suivre les étapes du secours : batterie temporaire, groupe en démarrage, état stable puis transfert différé. Source/charge/réserve et son extérieur évoluent ; la console seule ne termine plus l'activité.
- Après borne 06, lire la perte du secteur dans la chronologie et préparer un rapport pour énergie. Mauvaise équipe et lecture manquante donnent un retour explicatif ; le rapport ne produit aucune communication externe.
- Sept cibles PC/XRI, six afficheurs dans le monde et trois témoins de source. Les scènes sont générées par les builders reproductibles existants.

## Validation et limites

[Rapport build](build-result.json), [P0, 14 contrôles](p0-layout-audit.json), [P1, 20](p1-learning-audit.json), [P2, 11](p2-network-audit.json), [exploitation, 19](p3-facility-audit.json). [Rapport player, 105](pc/demo-smoke-result.json), [lancement](pc/launch-result.json), [journal anonymisé](pc/Player.log).

Le probe injecte clavier et clics dans le vrai player : photo/transition, cour à −28 m, marche continue jusqu'à Z≈39 m, sas/scan, entrée électrique, puis trajet pupitre–UPS–pupitre au clavier. Des poses de visée contrôlées sélectionnent les commandes. Il vérifie pause, refus, attente de stabilité, observation UPS, décharge puis maintien sur groupe, rapport/conclusion et retour/reset. Le contrôle d'épuisement applique **AdvancePower(200)** par API : ce n'est pas une attente réelle de 200 s. L'énergie fournie reste bornée à la réserve disponible ; rejouer relance le scénario. La logique XRI est raccordée et contrôlée statiquement, pas testée avec des manettes.

Les captures viennent de Camera.Render dans le player à 1280×720 : les Canvas de ces pupitres sont dans le monde et visibles. Les Canvas Overlay restent absents de ce type de capture ; la narration réseau avait été observée dans la fenêtre native au jalon T17. Inspection visuelle des nouvelles vues effectuée, aucune certification générale du rendu ni validation humaine de lisibilité/confort. Ombres réduites dans l'atlas 2048 encore signalées ; pas de benchmark, de test casque ou de nouvel APK.

## Simplifications sourcées

Chaîne alimentation/UPS/groupe/distribution d'après [Vertiv](https://www.vertiv.com/499436/globalassets/campaigns---2026/powering-the-ai-era/vertiv-powering-the-ai-era_e-book_emea-english.pdf) et [DOE 2024](https://www.energy.gov/sites/default/files/2024-07/best-practice-guide-data-center-design.pdf). Les secours sont normalement automatiques ; boutons et délais découpent l'observation, sans enseigner une procédure d'exploitation.

Réserve utilisable **0,50 kWh**, demande **12 kW IT + auxiliaires P1**, démarrage **4 s** et transfert **2 s** sont fictifs. Énergie = puissance × temps ; aucune simulation AC, synchronisation, rendement, recharge, autonomie industrielle, panne matérielle réelle ou redondance A/B. Perte du service couplée au refroidissement, cible thermique fictive 40 °C à temps accéléré, sans CFD. Aucune télémétrie réelle. [DOE, PUE/WUE annuels et eau](https://www.energy.gov/cmei/femp/cooling-water-efficiency-opportunities-federal-data-centers) : le tableau affiche les données absentes au lieu d'inventer PUE annuel, consommation d'eau ou CO2e. Localisation du service, droits d'accès et clés restent des questions distinctes.

UPS/TGBT FBX, audio procédural, uGUI, XRI, primitives et raypicker existants réutilisés ; pas d'asset tiers nouveau ni dépendance ajoutée. Vingt matériaux déjà modifiés avant ce lot ont été restitués à l'identique par SHA-256 et exclus de la publication ; sujet universitaire et ProjectSettings préservés. [Audit des empreintes initiales et des deux scènes](preservation-audit.json). Chambre restituée propre hors Play, console MCP retournant zéro erreur.

## Échecs et itérations conservés

[Premier pré-build](first-prebuild-failure/README.md) arrêté par la circulation P0 : tableau à Z=39,15, corrigé à Z=39,80 sans assouplir le critère, aucune livraison de cet échec. Un ancien build-error.txt de la copie est maintenant exclu par Complete de toute livraison réussie.

[Premier player](first-pc/demo-smoke-result.json) déjà réussi à 105/105 sur build de 12 h 31 ; [lancement](first-pc/launch-result.json). Son cadrage visait une colonne, produisant une vue oblique tronquée. Seule la cible du script de capture a été recentrée avant le build final ; régression complète répétée pour vérifier les images finales. Ce succès logique initial ne suffisait pas à valider la présentation du tableau.

| Soutien batterie | Groupe établi |
|---|---|
| ![Batterie temporaire et refus d'une source non prête](pc/demo-power-battery.png) | ![Source groupe et réserve conservée](pc/demo-power-generator.png) |

| UPS original observé | Supervision et rapport |
|---|---|
| ![Inspection de l'UPS original dans le local](pc/demo-ups-inspection.png) | ![Rapport préparé après lecture de la chronologie](pc/demo-supervision-report.png) |

Réseau et Stockage conservent leurs interactions de prototype ; voix finale, évaluation pédagogique et performance matérielle restent à traiter.
