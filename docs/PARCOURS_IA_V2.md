# Parcours IA · Windows

## Lancer

Démarrer Ollama et, depuis le dossier principal du projet, lancer le service local publié sur `main`. Le modèle par défaut est `qwen3:1.7b`.

```powershell
python tools/cloudvr-rag/service.py
```

Depuis la copie de travail V2 ouverte dans PowerShell, lancer le player Windows avec le mode IA.

```powershell
& .\ConnectionTest\Builds\Windows-IA\CloudVR-IA.exe --cloudvr-ai
```

Le build de développement se produit avec la méthode Unity `PhotoV2BlockoutBuilder.BuildAiWindows`. Cette méthode compile la scène V2 existante dans un dossier dédié et rétablit le réglage HTTP de l’éditeur. Le client appelle une seule adresse HTTP sur la boucle locale, `127.0.0.1:8765/question`.

## Ce que le joueur fait

1. Viser le PC dans la chambre, saisir une question et choisir France, États-Unis ou Chine.
2. Suivre les six repères du PC au site. Le Wi-Fi mène du PC à la box, puis la fibre d’accès mène au FAI. Au moment du transport, une carte montre les relais vers le site choisi. Après l’arrivée en façade, rejoindre le sas, passer le badge et brancher le cordon au port A.
3. Ouvrir la baie, extraire le tiroir, viser « Préparer la question », puis « GPU · Lancer ». La coupe met en évidence la VRAM et le GPU. Le contexte est traité dans la démonstration, puis cinq blocs apparaissent successivement pour illustrer la génération. La vraie réponse reste en mémoire de session jusqu’au retour au PC.
4. Monter au belvédère et viser « Demande de calcul ». Répartir deux nouveaux blocs entre Assistant IA, Studio vidéo et Recherche produit, puis alimenter les rangées 05 et 06. Chaque levier met en service le bloc du client attribué et ajoute sa consommation au compteur. Au poste, comparer les scénarios nucléaire et charbon pour couvrir la hausse de charge et valider un scénario. Rejoindre le point de vue, puis ouvrir la vanne et observer le retour refroidi.
5. Régler l’appoint de la tour par paliers de 25 L en s’aidant de la consigne. Une fois l’appoint équilibré, sélectionner « Retour au PC ». Le contexte fixe de chaque site remplace la mission météo.
6. Lire sur le terminal du PC la réponse générée, les sources, la répartition des blocs, le scénario choisi au poste et la comparaison des sites. Si le service est indisponible, le terminal permet de retenter la question après la visite.

## Calcul et limites

Le poste compare deux scénarios de production pour **30 kW supplémentaires**, soit **30 kWh en une heure**. Le jeu utilise des prix fictifs pour distinguer coût de production, prix de gros et facture du site, qui suit son contrat simulé. Les facteurs de cycle de vie arrondis sont de 6 g CO₂e/kWh pour le scénario nucléaire et de 900 g pour le charbon, soit 0,18 et 27 kg CO₂e pour ce même supplément. Le réseau réel mobilise plusieurs sources selon ses besoins et leur disponibilité. Le scénario choisi est rappelé au PC et le bilan par site suit les hypothèses de mix et de tarif propres à chaque site. [Marché et facture](https://www.rte-france.com/bases-electricite/consommation-electricite/decryptage-prix-electricite-marche-facturation) · [Analyse du cycle de vie, UNECE](https://unece.org/sites/default/files/2021-11/LCA_final.pdf)

Le compteur signale pendant cinq secondes le nouveau bloc alimenté, l’ouverture du refroidissement et chaque variation de l’appoint. Pour le site sélectionné, le rappel affiche la variation simulée en kWh, en euros et en g CO₂e sur la même heure. Le pompage d’appoint ajoute 0,10 kWh par palier de 25 L, et réduit le total d’autant à chaque palier retiré.

Le modèle répond à la question à partir du corpus CloudVR. Les calculs du jeu sont en C# et n’appellent pas le RAG ni la route Python de bilan. Le jeu affiche des sous-totaux fictifs sur une heure pour le PC et la box d’un côté et le bâtiment, les serveurs, le refroidissement et l’appoint de l’autre. Les facteurs tarifaires et carbone du site sont des hypothèses fixes. Le comparatif final reprend les trois sites à charge informatique égale et avec l’appoint équilibré dans chaque site.

Le belvédère illustre un fournisseur cloud qui exploite des serveurs hébergés dans un data center et alloue leur capacité à des clients. Assistant IA démarre avec 1 bloc en service et en demande 3. Studio vidéo en a 1 et en demande 2. Recherche produit en demande 2. Sur les 7 blocs demandés, 4 peuvent être servis et 3 restent en attente de capacité. L’ordre de réservation affecte les rangées 05 et 06. Le joueur répartit les blocs selon les délais indiqués et garde le même total de quatre rangées pour le comparatif électrique. L’association d’un bloc à une rangée est propre au jeu. [Hébergement en colocation](https://docs.equinix.com/colocation/) · [Allocation de calcul à des clients](https://docs.nvidia.com/infra-controller/documentation/configuration-day-1/tenant-management).

Le trajet reprend la cinématique et le brassage existants. Le fond géographique vient de Natural Earth. Le PC fictif part de France et les trois tracés illustrent une liaison terrestre, un trajet transatlantique et des interconnexions vers l’Asie. La carte suit le même tracé en sens inverse au retour. Les chemins et délais sont des scénarios pédagogiques. La coupe à la baie est une représentation pédagogique. Le découpage d’exemple et les cinq blocs sont illustratifs et le texte final provient du RAG local. Le site choisi est illustré par le même décor. Temps réseau, coût et émissions ne sont pas mesurés par requête. Le service Qwen tourne sur le PC. L’essai sur Quest reste à faire.

[Conception, corpus et sources](https://github.com/axjuillard-droid/projet-vr-cloud/blob/bc271c775f597d28e96e0588e8a97d9c8e174251/docs/PARCOURS_IA_RAG.md)


## Validation du poste électrique et du compteur

Le build Windows passe 45 contrôles IA avec une réponse réelle d’Ollama et 10/10 actions Photo en Direct3D 11. Le contrôle vérifie les deux calculs d’émissions, la reprise après fermeture et validation du panneau, le crédit du bon client, le refroidissement et les écarts de pompage sur chaque palier. Les actions du panneau sont déclenchées par le test automatisé. L’essai libre et Quest restent à faire. [Résultats](validation/ia-poste-electrique-2026-11-18.json)

## Validation du belvédère

Le player Windows passe 40 contrôles IA en Direct3D 11 avec une réponse réelle d’Ollama, et 10/10 actions Photo. Le test ferme puis rouvre le brouillon, change la réservation et active la rangée 06 avant la 05 pour vérifier quel client reçoit le calcul. La capture du player masqué est noire. La lisibilité et les clics dans le panneau restent à vérifier en essai humain, ainsi que le parcours sur Quest. [Résultats](validation/ia-belvedere-2026-11-16.json)

## Validation de l’inférence

Le player Windows passe 36 contrôles IA en Direct3D 11, dont le refus d’un lancement prématuré, le contexte, la génération progressive, le filtrage de la capsule et la réponse réelle au retour au PC. Le mode Photo passe 10/10. La capture hors écran permet d’examiner la coupe et les repères dans le décor. Le panneau d’explication et la compréhension demandent un essai humain. [Capture GPU](validation/ia-inference-2026-11-16/gpu-en-generation.png) · [Résultats](validation/ia-inference-2026-11-16/result.json)

## Validation de la carte

Le build Windows en Direct3D 11 charge la carte, passe les six étapes sur le tracé États-Unis et révèle la réponse locale après le retour. Le contrôle IA passe 31 vérifications. Le contrôle Photo passe 10/10 actions. La capture en fenêtre masquée est noire et ne permet pas de juger la lisibilité. [Rapport du routage](validation/ia-routage-2026-11-16.json)

## Validation Windows du 16 novembre · intégration initiale

Le build Unity 6000.0.58f2 annonce zéro erreur et zéro avertissement BuildPipeline. En Direct3D 11, les 30 contrôles IA ont passé une vraie requête au modèle local. Le jeu a gardé la réponse en mémoire pendant la visite, bloqué le retour sans appoint et en cas d’excès, puis affiché la réponse au retour au PC. Le même exécutable a ensuite réussi les 10 étapes du contrôle Photo et le parcours IA jusqu’au terminal lorsque le service était arrêté. Ces contrôles automatisés utilisent les points d’interaction et le CharacterController. La rédaction libre et la lisibilité du terminal restent à vérifier en essai humain. [Résultats](validation/ia-v2-2026-11-16.json)
