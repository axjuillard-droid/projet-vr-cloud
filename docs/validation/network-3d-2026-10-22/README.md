# T17 — Voyage réseau 3D, 22 octobre 2026

Le clic/envoi de la photo déclenche désormais une séquence 3D de **7,50 s** : approche de la box, fibre agrandie et signaux mobiles, puis campus avant l'apparition dans la cour. Les six étapes illustrent réseau local, accès opérateur, collecte, routage, réseau du site et arrivée. Le tunnel, le campus et le son sont réutilisés ; aucun package ajouté. **Aucune mesure réseau, vitesse physique ou certification Tier n'est représentée.** Le stockage n'est plus annoncé avant la visite.

![Fenêtre réelle du player, narration visible à l'étape 3](native-network-hud.jpg)

Cette capture native inclut l'interface réellement affichée. La souris de l'automatisation est visible ; ce n'est pas un essai humain. Les images suivantes sont des captures contrôlées hors écran de la caméra et omettent les Canvas en Overlay : elles documentent la 3D, pas la composition finale de l'interface.

| Tunnel, caméra déplacée | Campus en fin de trajet |
|---|---|
| ![Fibre et signaux, étape 3](pc/demo-network-stage-3.png) | ![Campus, étape 6](pc/demo-network-stage-6.png) |

## Compilation et player PC

[Build final](build-result.json) : Unity 6000.0.58f2, 22/10 **11 h 55 min 47 s Paris**, Succeeded, **28,43 s**, **147 022 098 octets** pour l'ensemble du build, **0 erreur / 0 avertissement BuildPipeline**. `Build-DemoCopy.ps1 -Resume -Wait` puis `-Complete` exécutés séparément. Contrôles pré-build : [14 P0](p0-layout-audit.json), [20 P1](p1-learning-audit.json), [11 P2](p2-network-audit.json). Le contrôle statique du calque de narration ne prouve pas à lui seul que le texte est dessiné ; la capture native complète cette preuve.

**[68/68 contrôles réussis](pc/demo-smoke-result.json)**, [exitCode=0/passed=true](pc/launch-result.json), fin à **11 h 57 min 44 s Paris**. Direct3D11, entrées synthétiques ; visées par poses contrôlées. La caméra parcourt plus de 50 unités fictives, les signaux changent de position, le double déclenchement ne relance ni ne saute la séquence. Navigation cour/sas, marche au fond Z=39,0008 m en 24,75 s, accès électrique, inspection serveur et diagnostic thermique P1, six bornes et retour/reset conservés. Les frames logiques du batch ne sont pas des FPS de rendu interactif.

Le [journal final](pc/Player.log) conserve la réduction de résolution de six shadow maps dans l'atlas 2048. Aucun benchmark ni essai humain de confort. [Manifeste des empreintes et dates du dossier Windows](delivery-manifest.json) : garder le dossier Windows complet ; la date ancienne du lanceur de 672 256 octets ne date pas les données et les DLL.

## Branche confort VR sans casque

`tools/Test-NetworkComfort.ps1` : **[8/8 contrôles de logique](comfort-api/network-comfort-result.json)**, [exitCode=0](comfort-api/launch-result.json), fin à **12 h 00 min 12 s Paris**. Choix VR forcé par API, aucun appareil actif. Texture de film raccordée, transform de caméra inchangé, orientation simulée respectée, écran ancré dans le monde, transition complète/mode conservé et objets transitoires détruits après chargement.

Le film est présenté sur un écran 2D dans le monde ; la caméra du casque n'est pas conduite dans le tunnel. ScreenCapture n'a pas produit d'image dans cet essai sans casque. Les libellés des contrôles portant sur le film vérifient le raccordement de la texture, pas son rendu optique. **Ni tracking matériel, ni stéréoscopie, ni lisibilité, ni confort, ni coût GPU Quest ne sont validés. Aucun APK reconstruit.**

## Première revue et corrections

Le premier build avait déjà passé 68 contrôles PC et 8 contrôles API. [Rapport initial](first-visual-review/build-result.json) et [résultat PC initial](first-visual-review/demo-smoke-result.json) conservés. Les captures de caméra omettaient la narration : cela a été attribué à tort à un problème d'interface, avant de lire `DemoSmoke.Capture` et de constater son usage de Camera.Render/RenderTexture. Le calque de narration est maintenant inclus explicitement dans le masque du film, mais la validation décisive est l'observation native ci-dessus. Ne pas décrire les premières images comme une preuve d'un HUD réellement invisible à l'écran.

L'arrivée montrait une façade nue : fenêtres et bandeau du FBX se trouvaient derrière sa peau opaque. Décalage uniquement dans le décor de film et cadrage en trois quarts ajoutés. Aucun nouveau défaut de mur de la salle blanche déduit de ces vues de campus.

## Outils, préservation et limites

MCP HTTP/SSE sur localhost réellement utilisé après activation utilisateur ; instance ConnectionTest vérifiée, schémas relus. Chambre restituée propre, film inactif au repos, textes raccordés ; zéro entrée d'erreur retournée par la console finale. Computer-use utilisé pour observer le player ; les contrôles fonctionnels restent synthétiques.

Vingt matériaux déjà modifiés au départ sont conservés à l'identique et exclus du commit T17. Seuls les quatre matériaux P2 nouveaux et leurs GUID entrent avec ce jalon. Scènes issues de la copie compilée livrées, ProjectSettings et sujet universitaire préservés. Une commande Complete lancée depuis Python a échoué sur l'import PowerShell de Get-FileHash ; la commande directe a réussi avant livraison, sans contourner les gardes de scènes.

Les [sources et choix de réutilisation](../../RESOURCES.md) indiquent notamment pourquoi Cinemachine n'a pas été ajouté. [RFC 9293](https://www.rfc-editor.org/rfc/rfc9293.html) documente le transport TCP ; ce trajet ne représente pas une transaction réellement observée, et une arrivée/acquittement réseau ne démontre pas une persistance applicative. Voix finale et rythme de lecture restent à valider. Les quatre autres activités conservent leurs mécaniques de prototype ; prochaine tâche T18.

**Publication confirmée : commit `a6ce9f6` poussé sur origin/main**, SHA distant vérifié ; 30 objets LFS / 11 Mo. Diff, douze JSON, liens, anonymisation et Git LFS fsck contrôlés. [Confirmation](git-publication.json). Vingt matériaux préexistants restent non commités à l’identique. Un commit documentaire suivant consigne ce résultat sans modifier le build testé.
