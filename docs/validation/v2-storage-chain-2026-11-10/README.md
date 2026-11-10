# Baie école et stockage V2 — 10 novembre 2026

Le scénario Windows D3D11 avance avec le CharacterController et sélectionne les cibles par rayon. Il vérifie que le signal atteint l’entrée du serveur, que la porte et le tiroir s’ouvrent depuis l’allée, que l’aperçu est calculé, puis que la miniature rejoint le répartiteur via le port de sortie du serveur. La banque occupée est refusée. La banque libre reçoit l’écriture, puis l’écran client relit la même texture que celle envoyée depuis la chambre.

- [CPU, RAM et NVMe depuis l’allée](tiroir.png)
- [Photo au répartiteur et banques de stockage](repartiteur.png)
- [Écriture terminée](stockage.png)
- [Photo relue sur l’écran client](relecture.png)
- [Rapport de validation](result.json)

Les captures sont extraites de la caméra du player. Elles n’incluent pas le HUD plein écran. La simulation conserve la photo en mémoire pour la durée du parcours. Le test ne mesure ni la cadence ni la lisibilité dans un casque.
