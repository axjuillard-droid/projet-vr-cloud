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
4. Monter au belvédère, déclencher la demande de quatre rangées et alimenter les deux rangées supplémentaires. Traverser le poste électrique et le point de vue sur la centrale, puis ouvrir la vanne et observer le retour refroidi.
5. Régler l’appoint de la tour par paliers de 25 L en s’aidant de la consigne. Une fois l’appoint équilibré, sélectionner « Retour au PC ». Le contexte fixe de chaque site remplace la mission météo.
6. Lire sur le terminal du PC la réponse générée et les sources consultées. Si le service est indisponible, la visite et le bilan restent utilisables et le terminal permet de retenter la question.

## Calcul et limites

Le modèle répond à la question à partir du corpus CloudVR. Les calculs du jeu sont en C# et n’appellent pas le RAG ni la route Python de bilan. Le jeu affiche des sous-totaux fictifs sur une heure pour le PC et la box d’un côté et le bâtiment, les serveurs, le refroidissement et l’appoint de l’autre. Les facteurs tarifaires et carbone du site sont des hypothèses fixes. Le comparatif final reprend les trois sites à charge informatique égale et avec l’appoint équilibré dans chaque site.

Le trajet reprend la cinématique et le brassage existants. Le fond géographique vient de Natural Earth. Le PC fictif part de France et les trois tracés illustrent une liaison terrestre, un trajet transatlantique et des interconnexions vers l’Asie. La carte suit le même tracé en sens inverse au retour. Les chemins et délais sont des scénarios pédagogiques. La coupe à la baie est une représentation pédagogique. Le découpage d’exemple et les cinq blocs sont illustratifs et le texte final provient du RAG local. Les actions de répartition entre locataires restent à représenter. Le site choisi est illustré par le même décor. Temps réseau, coût et émissions ne sont pas mesurés par requête. Le service Qwen tourne sur le PC. L’essai sur Quest reste à faire.

[Conception, corpus et sources](https://github.com/axjuillard-droid/projet-vr-cloud/blob/bc271c775f597d28e96e0588e8a97d9c8e174251/docs/PARCOURS_IA_RAG.md)


## Validation de l’inférence

Le player Windows passe 36 contrôles IA en Direct3D 11, dont le refus d’un lancement prématuré, le contexte, la génération progressive, le filtrage de la capsule et la réponse réelle au retour au PC. Le mode Photo passe 10/10. La capture hors écran permet d’examiner la coupe et les repères dans le décor. Le panneau d’explication et la compréhension demandent un essai humain. [Capture GPU](validation/ia-inference-2026-11-16/gpu-en-generation.png) · [Résultats](validation/ia-inference-2026-11-16/result.json)

## Validation de la carte

Le build Windows en Direct3D 11 charge la carte, passe les six étapes sur le tracé États-Unis et révèle la réponse locale après le retour. Le contrôle IA passe 31 vérifications. Le contrôle Photo passe 10/10 actions. La capture en fenêtre masquée est noire et ne permet pas de juger la lisibilité. [Rapport du routage](validation/ia-routage-2026-11-16.json)

## Validation Windows du 16 novembre · intégration initiale

Le build Unity 6000.0.58f2 annonce zéro erreur et zéro avertissement BuildPipeline. En Direct3D 11, les 30 contrôles IA ont passé une vraie requête au modèle local. Le jeu a gardé la réponse en mémoire pendant la visite, bloqué le retour sans appoint et en cas d’excès, puis affiché la réponse au retour au PC. Le même exécutable a ensuite réussi les 10 étapes du contrôle Photo et le parcours IA jusqu’au terminal lorsque le service était arrêté. Ces contrôles automatisés utilisent les points d’interaction et le CharacterController. La rédaction libre et la lisibilité du terminal restent à vérifier en essai humain. [Résultats](validation/ia-v2-2026-11-16.json)
