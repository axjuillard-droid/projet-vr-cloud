# Correction monumentale — historique et reprise du 21 octobre 2026

**Reprise GO validée ensuite : [37/37 contrôles PC, corrections et captures finales](reprise/README.md).** Les deux essais ci-dessous décrivent la phase précédente.

Sources et scènes corrigées ; build Windows final de cette session réussi : Unity 6000.0.58f2, 28,20 s, 146 785 672 octets, 0 erreur / 0 avertissement.

Deux essais graphiques Direct3D11 ont échoué ; aucune réussite globale revendiquée. Les tests ont réellement lancé le player et injecté clavier/souris. La transition V2 complète (six étapes, ≥10,7 s) est vérifiée dans les deux essais. Apparition −28 m, avancée initiale et pause/reprise validées.

- Essai 1 : scan du sas observé, portes fermées pendant le scan ; la marche jusqu’à +39 m n’aboutit pas dans les 35 s. L’emplacement final n’avait pas été enregistré dans ce premier rapport. Les deux CRAC centraux de la salle ont ensuite été décalés latéralement, car leurs volumes se trouvaient sur l’axe central après rotation.
- Essai 2 : scan non atteint, position finale (0 ; 0,101 ; −11,723) après la fenêtre de marche de 35 s. Cette preuve ne suffit pas à conclure à une collision ; cadence du rendu, temps de jeu et durée effective de déplacement restent à mesurer. Pas de troisième essai, conformément à AGENTS.md.

Les bornes 8/14/20/26/32/38 m sont implémentées dans le builder et les scènes ; leurs clics et le retour Chambre n’ont pas été atteints dans ces essais. Les captures montrent la chambre, le HUD de transition, la cour et le sas. `demo-hall-end.png` de l’essai 2 est une capture au terme du délai, encore dans la cour ; son nom ne prouve pas une arrivée au fond.

Lancement sans argument smoke également effectué via computer-use : fenêtre du player observée dans Chambre, puis fermée. Aucun parcours de navigation supplémentaire. MCP Unity connecté au projet original et interrogé en lecture : hors Play/compilation, aucune erreur console remontée.

Journaux publics anonymisés (chemin utilisateur, nom du PC, adresses IP) ; originaux sous tmp ignoré. Aucun APK recompilé ni casque essayé. Revue humaine de la navigation requise avant reprise du diagnostic.
