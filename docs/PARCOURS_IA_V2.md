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
2. Suivre les six repères du trajet illustratif. Rejoindre le sas, passer le badge, prendre le cordon et le brancher au port A.
3. Ouvrir la baie, extraire le tiroir et lancer la démonstration d’inférence. La réponse du modèle reste en mémoire de session.
4. Monter au belvédère, déclencher la demande de quatre rangées et alimenter les deux rangées supplémentaires. Traverser le poste électrique et le point de vue sur la centrale, puis ouvrir la vanne et observer le retour refroidi.
5. Régler l’appoint de la tour par paliers de 25 L en s’aidant de la consigne. Une fois l’appoint équilibré, sélectionner « Retour au PC ». Le contexte fixe de chaque site remplace la mission météo.
6. Lire sur le terminal du PC la réponse générée et les sources consultées. Si le service est indisponible, la visite et le bilan restent utilisables et le terminal permet de retenter la question.

## Calcul et limites

Le modèle répond à la question à partir du corpus CloudVR. Les calculs du jeu sont en C# et n’appellent pas le RAG ni la route Python de bilan. Le jeu affiche des sous-totaux fictifs sur une heure pour le PC et la box d’un côté et le bâtiment, les serveurs, le refroidissement et l’appoint de l’autre. Les facteurs tarifaires et carbone du site sont des hypothèses fixes. Le comparatif final reprend les trois sites à charge informatique égale et avec l’appoint équilibré dans chaque site.

Le trajet s’appuie sur la cinématique et le brassage existants. Les repères réseau nomment les étapes, tandis que l’inférence est illustrée au tiroir école par le statut de traitement. La carte géographique détaillée, une coupe GPU animée et les actions de répartition entre locataires restent des travaux de représentation. Le site choisi est illustré par le même décor. Temps réseau, coût et émissions ne sont pas mesurés par requête. Le service Qwen tourne sur le PC. L’essai sur Quest reste à faire.

[Conception, corpus et sources](https://github.com/axjuillard-droid/projet-vr-cloud/blob/bc271c775f597d28e96e0588e8a97d9c8e174251/docs/PARCOURS_IA_RAG.md)


## Validation Windows du 16 novembre

Le build Unity 6000.0.58f2 annonce zéro erreur et zéro avertissement BuildPipeline. En Direct3D 11, les 30 contrôles IA ont passé une vraie requête au modèle local. Le jeu a gardé la réponse en mémoire pendant la visite, bloqué le retour sans appoint et en cas d’excès, puis affiché la réponse au retour au PC. Le même exécutable a ensuite réussi les 10 étapes du contrôle Photo et le parcours IA jusqu’au terminal lorsque le service était arrêté. Ces contrôles automatisés utilisent les points d’interaction et le CharacterController. La rédaction libre et la lisibilité du terminal restent à vérifier en essai humain. [Résultats](validation/ia-v2-2026-11-16.json)
