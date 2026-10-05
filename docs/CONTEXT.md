# Organisation du contexte pour les assistants IA

Cette documentation constitue une mémoire de projet sur fichiers. Elle ne remplace ni les consignes de l’utilisateur ni les vérifications de l’état réel des outils.

## Ordre de reprise

1. `../AGENTS.md` : règles de travail.
2. `STATE.md` : situation vérifiée, limites et prochaine action.
3. `BACKLOG.md` : tâche prête et critère de fin.
4. Charger seulement les références utiles : `VISION.md` pour le périmètre, `DECISIONS.md` pour les choix validés, `PARCOURS.md` pour le scénario, `RESOURCES.md` pour réutilisation/licences, `TOOLING.md` pour Unity/MCP, `CAPACITES_CODEX.md` pour les procédures de pilotage et leurs limites.
5. Vérifier les préconditions réelles avant action ; en fin de session actualiser l’état et le statut sans recopier la conversation.

## Responsabilités des fichiers

Le sujet original `../Le cloud.docx` définit les exigences universitaires. `DECISIONS.md` sépare décisions utilisateur et propositions. `PARCOURS.md` détaille la conception, pas l’état de l’implémentation. `STATE.md` résume uniquement l’avancement et les preuves. En cas d’écart, ne pas inventer de validation : signaler et corriger le document périmé à partir des instructions et preuves disponibles.

## Skills et outils

- `../skills/reuse-first/SKILL.md` : seule procédure réutilisable propre au projet actuellement présente. Chargement explicite demandé dans AGENTS.md ; découverte automatique non configurée. Elle fonctionne ici par lecture du fichier, sans installation supplémentaire.
- Les skills personnels et ceux des plugins sont extérieurs au projet ; leur disponibilité dépend de la session. Ne pas les considérer comme livrés avec ce dossier.
- Graphify : consigne présente dans AGENTS.md pour `/graphify`, skill personnel disponible dans cette session. Aucun `graphify-out/graph.json` présent lors de cette vérification du 28/09. Aucun graphe généré pour cette mise à jour.
- MCP Unity : connexion à un outil, pas une mémoire ou un skill. Procédure et preuves dans TOOLING.md ; vérifier de nouveau l’instance et l’état avant mutation.

## Limites de la reprise

L’organisation a été vérifiée par lecture des fichiers et cohérence des liens ; pas encore par une nouvelle session indépendante. La mémoire n’est utile que si l’assistant reçoit le dossier, suit le point d’entrée et lit les documents. Git, les imports XR et les builds restent à préparer/valider suivant le backlog. Pas besoin de multiplier les skills pour mémoriser chaque décision : les décisions appartiennent aux documents de projet.
