# Parcours IA et comparaison de sites

La requête à l’IA devient le fil conducteur proposé pour la V2. L’activité « Acheminer la demande » est validée. Le joueur saisit une question dans le PC de la chambre et retrouve la réponse au même poste à la toute fin. Le scénario d’exploitation se termine par un réglage de la demande en eau. Le plan IA ci-dessous remplace la succession des météos par des conditions fixes propres aux trois sites.

Le 16 novembre, le premier parcours intégré a été publié sur la branche V2. La visite utilise son propre calcul C# et appelle ce service pour la question uniquement. Les commandes de lancement et les preuves Windows sont dans le [guide du player IA](https://github.com/axjuillard-droid/projet-vr-cloud/blob/29a602246afd30f143dde995c7cd6a0d1b245386/docs/PARCOURS_IA_V2.md).

## Premier lot disponible · RAG local

Le service récupère jusqu’à trois passages du corpus CloudVR par recherche lexicale et les transmet à Qwen via l’API locale d’Ollama. Le corpus couvre notamment le routage, les GPU, la facture électrique, le carbone et l’eau. Le modèle peut signaler qu’une question dépasse le corpus. Les sources renvoyées sont les passages proposés au modèle. L’interface doit les présenter comme « Sources consultées ».

Avec Ollama démarré et `qwen3:1.7b` disponible, depuis la racine du projet :

```powershell
python tools/cloudvr-rag/service.py
```

Dans un autre terminal :

```powershell
$question = @{ question = "Pourquoi une tour de refroidissement a besoin d'eau d'appoint ?" } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri http://127.0.0.1:8765/question -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($question))
```

Le service écoute uniquement sur la boucle locale. Les poids et les extraits externes restent chez leurs distributeurs. Le fichier de corpus contient des reformulations originales. L’API accepte 500 caractères par question. Une question sans correspondance renvoie `hors_corpus`. Une indisponibilité renvoie HTTP 503 et permet de retenter. [API de chat Ollama](https://docs.ollama.com/api/chat) · [Modèle Qwen3](https://ollama.com/library/qwen3:1.7b)

Le même service expose `GET /bilan/envoi`, `/bilan/calcul`, `/bilan/refroidissement` et `/bilan/eau`. Chaque réponse compare les trois sites pour une heure de simulation et donne le sous-total révélé côté joueur et côté site. À l’étape eau, `/bilan/eau?appoint_l=100` indique le déficit ou l’excès d’appoint par site et recalcule l’énergie de la pompe. Les climats, tarifs, facteurs et délais du prototype sont des **paramètres illustratifs**. Ils ne constituent pas des données de contrat ni des mesures par requête.

**Vérification du 16 novembre 2026.** Ollama et Qwen3 1,7B ont produit deux réponses réelles, sur l’eau d’appoint et sur la facture électrique. Les contrôles HTTP couvrent aussi question hors corpus, saisie vide et modèle absent. Les deux générations finales ont pris environ 13 et 17 secondes sur cette machine. La deuxième distinguait les trois notions testées avec une définition incomplète du coût de production. La réponse du modèle demande une validation pédagogique plus large. Les trois sites, quatre étapes, sommes, délais et entrées invalides de l’appoint ont été vérifiés à part.

## Intégration à la visite

1. **Chambre.** Champ de saisie, puis choix du site simulé France, États-Unis ou Chine. Enregistrer ce choix avec la question et lancer le RAG en tâche de fond dès la soumission. Garder le résultat en mémoire de session.
2. **Routage.** Faire suivre la requête par le Wi-Fi entre le PC et la box, puis par la fibre d’accès jusqu’au point de présence du FAI. Indiquer le réseau de transport, l’interconnexion internationale si nécessaire, l’arrivée optique en façade puis le brassage. Le joueur déclenche le trajet et raccorde la fibre. Comparer les segments à l’aide d’une carte construite pour les trois sites fictifs. Les routes physiques, les opérateurs et les délais des chemins représentés sont illustratifs. [Routage entre réseaux, RFC 4271](https://www.rfc-editor.org/rfc/rfc4271.html)
3. **Inférence.** À la baie, le joueur lance une démonstration. Afficher la question découpée en tokens, le modèle préchargé en mémoire et le calcul de la réponse au fur et à mesure, sans dévoiler le texte final. La production de tokens représente le calcul réel de l’inférence. [Mémoire et génération](https://huggingface.co/docs/transformers/main/kv_cache)
4. **Exploitation.** Faire apparaître un lot de requêtes simultanées. Le joueur décide de la capacité à attribuer et de la puissance qu’il peut appeler, puis met en service les rangées et règle le refroidissement et l’appoint. Conserver la simulation de production électrique et la comparaison nucléaire–charbon à énergie égale comme manipulation au poste. Le bilan compare le service fourni, le coût et les émissions. [Flexibilité réseau](https://analysesetdonnees.rte-france.com/bilan-electrique-flexibilites) · [Nucléaire et charbon sur le cycle de vie](https://unece.org/sed/documents/2021/10/reports/life-cycle-assessment-electricity-generation-options)
5. **Retour.** Après la validation de l’eau, animer le retour de la réponse et ramener le joueur au PC de départ. Afficher la réponse générée et les sources consultées. Si la génération est encore en cours, afficher l’attente au PC. En cas d’échec, proposer d’y relancer la même question. Au redémarrage du parcours, invalider le résultat de la session précédente. La simulation de délais et de localisation demeure indépendante du temps réel mesuré par Ollama.

## Répartir les usages entre entreprises

L’opérateur du site fournit l’espace et la puissance au fournisseur cloud. Celui-ci exploite les serveurs et alloue du calcul à trois clients fictifs. Assistant IA passe de 1 bloc en service à 3 demandés, Studio vidéo de 1 à 2, et Recherche produit de 0 à 2. Le joueur réserve deux blocs supplémentaires au pupitre, puis les met en service depuis le tableau électrique. Le bloc de calcul est une unité du jeu et chaque nouveau bloc correspond à une rangée pour rendre la mise sous tension observable. Trois blocs demandés restent en attente. La répartition change quels clients reçoivent les blocs et la charge finale de quatre rangées est commune aux choix et aux sites. [Colocation, Equinix](https://docs.equinix.com/colocation/) · [Allocation par client, NVIDIA](https://docs.nvidia.com/infra-controller/documentation/configuration-day-1/tenant-management)

## Comparaison au poste et indicateurs

Dans la V2, le joueur valide au poste un scénario nucléaire ou charbon pour couvrir les 30 kW supplémentaires des deux rangées. Les cartes comparent les émissions de cycle de vie à service égal et montrent des exemples fictifs de coût de production et de prix de gros. La facture reste celle du profil de site simulé. Le compteur signale chaque nouveau bloc en service, l’arrivée du refroidissement et les changements d’appoint en affichant leur effet sur le total simulé sur une heure. [Marché et facture](https://www.rte-france.com/bases-electricite/consommation-electricite/decryptage-prix-electricite-marche-facturation) · [Analyse du cycle de vie](https://unece.org/sites/default/files/2021-11/LCA_final.pdf)

Garder la même question, le même modèle et le même lot de demandes pour comparer les sites. Chaque profil comporte un climat fixe de référence, une route avec un délai aller-retour simulé, une intensité carbone datée et un tarif indicatif. Séparer **transport, file d’attente et inférence** dans le délai. Comparer d’abord la localisation à configuration égale, puis les choix d’exploitation qui changent la consommation.

| Moment | Poste du joueur et accès | Site sélectionné |
| --- | --- | --- |
| Envoi | Électricité estimée du PC et de la box, en Wh. | En attente de traitement. |
| Arrivée réseau | Afficher le réseau de transport sur une ligne distincte, avec son délai et le périmètre de l’estimation. | File d’attente et trajet simulé. |
| Calcul | La facture électrique du poste dépend du lieu du joueur. | Ajouter l’électricité des serveurs et le coût qui lui correspond. |
| Refroidissement | Afficher le sous-total déjà révélé. | Ajouter la consommation des auxiliaires, dont les pompes de circulation si elles sont représentées. |
| Eau et retour | Afficher la réponse et le bilan. | Afficher prélèvement, évaporation et restitution séparément. Ajouter la pompe d’appoint si son énergie n’est pas incluse dans les auxiliaires. |

Le compteur cumule l’énergie par poste (puissance × durée), puis applique son tarif au kWh et son facteur d’émission. Les étapes révèlent les postes déjà comptabilisés ou nouvellement mis en service. La part inconnue est marquée en attente. La facture du site utilise un prix livré. Décomposer ce tarif en fourniture, réseau et taxes sert à expliquer sa formation. Le coût de production ou le prix spot affichés au poste sont des repères et ne s’ajoutent pas de nouveau à la facture. Pour l’énergie du lot, raisonner en kWh. Pour la limite instantanée du site, raisonner en kW.

Les données françaises permettent de montrer comment un prix de gros marginal peut s’écarter d’un coût de production moyen. La comparaison de facture dépend aussi du contrat, des coûts de réseau, de la fiscalité, de la période et du profil client. Le classement de prix de la France se lit séparément de la moyenne européenne. L’intensité carbone nationale moyenne peut servir de repère éducatif, tandis que le profil du site et l’heure peuvent modifier les valeurs pertinentes. [Formation des prix, RTE](https://www.rte-france.com/bases-electricite/consommation-electricite/decryptage-prix-electricite-marche-facturation) · [Comparaison industrielle 2019–2025, AIE](https://www.iea.org/data-and-statistics/charts/estimated-final-electricity-price-for-large-industrial-customers-in-energy-intensive-industries-2019-2025) · [Intensités du mix, Ember](https://ember-energy.org/data/yearly-electricity-data/)

À la rivière, prendre un débit et un objectif de refroidissement fixes pour chaque profil. Le joueur ajuste le prélèvement jusqu’à couvrir l’appoint nécessaire, avec un retour visuel pour le déficit et l’excès. Le climat du profil influe sur le refroidissement sans déclencher de mission météo. Après stabilisation, la réponse devient visible au PC.

## Validation du premier parcours intégré

Vérifier sur Windows une question libre, un site choisi, un trajet avec six points identifiables, le passage de l’inférence au pic de demandes et un réglage d’eau qui débloque le retour. À l’écran final, la question affichée doit correspondre à la réponse de la session, avec au moins une source pour un sujet couvert. Rejouer la même charge sur deux sites et vérifier que seules les hypothèses explicitement affichées changent le résultat. Couvrir l’absence d’Ollama et un rejeu après erreur.
