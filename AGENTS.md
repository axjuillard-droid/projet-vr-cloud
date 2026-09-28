# Consignes du projet

## Contexte et reprise
- Lire `docs/STATE.md` en début de session ; charger ensuite seulement le contexte utile.
- `docs/VISION.md` définit le périmètre ; `docs/DECISIONS.md` distingue décisions et propositions.
- Préserver `Le cloud.docx`, source du sujet universitaire.
- Répondre et documenter en français ; garder les noms techniques usuels.

## Réutilisation avant création
- Avant de créer du code, un modèle, une animation, une simulation ou un contenu, consulter `skills/reuse-first/SKILL.md` et rechercher brièvement une solution gratuite adaptée.
- Consulter d’abord `docs/RESOURCES.md` pour ne pas refaire une recherche déjà pertinente.
- Vérifier la licence, l’attribution, la redistribution dans un dépôt public, la compatibilité et l’effort d’adaptation ; gratuit ne signifie pas librement réutilisable.
- Ne pas lancer de recherche externe pour une simple mise à jour des notes du projet ou une correction locale déjà comprise.

## Travail borné
- Choisir une tâche prioritaire prête ; définir un résultat vérifiable avant l’implémentation.
- Favoriser les scripts reproductibles et les composants réutilisables ; éviter les dépendances inutiles.
- Ne pas élargir spontanément le périmètre à la version web ou au RAG réel.
- Après deux tentatives infructueuses sur un même blocage, consigner les preuves et arrêter cette tâche pour revue humaine.
- Après chaque avancée significative et en fin de session, mettre systématiquement à jour `docs/STATE.md` et `docs/BACKLOG.md` (statuts, preuves de validation, étapes réalisées), sans attendre la fin du projet.
- Ne pas promettre un plafond de consommation garanti ; calibrer l’usage sur des sessions réelles.

## Validation
- Distinguer implémenté, compilé, testé sur PC et testé dans le casque.
- Ne jamais déclarer une validation matérielle sans essai réel ; conserver modèle du casque, date et résultat lorsqu’ils sont connus.
- Sourcer les explications techniques et signaler les simplifications pédagogiques.
- Une animation de requête représente un fonctionnement ; elle n’est pas une mesure du trajet physique réel.
- Prévoir la voix finale et les sous-titres ; éviter de figer des textes avant validation pédagogique.

## Instruction existante graphify
- Lorsque l’utilisateur écrit `/graphify`, utiliser le skill graphify disponible avant le travail demandé. Si aucun outil Skill dédié n’est disponible, lire son `SKILL.md` via les outils disponibles ; signaler une absence plutôt que prétendre l’avoir invoqué.
