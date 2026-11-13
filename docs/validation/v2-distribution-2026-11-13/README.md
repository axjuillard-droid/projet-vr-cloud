# Distribution électrique et refroidissement V2

Le player Windows D3D11 parcourt les séquences Photo et Exploitation. Le test vérifie que la manette seule ne met aucune nouvelle rangée sous tension, que chaque levier allume sa propre rangée et que la sortie attend les départs demandés. À six rangées, la vanne ajoute 6 kW à la consommation simulée. La température redescend après le retour froid à charge constante. La comparaison de la rivière reste jouable avec les départs déjà autorisés.

- [Rangées demandées](demande.png)
- [Deux départs en service](tableau.png)
- [Échangeur et conduites](echangeur.png)
- [Rapport de validation](result.json)

Les captures proviennent de la caméra du player. Validation Quest et lisibilité en mouvement à évaluer. Le journal signale JobTempAlloc à la fermeture comme dans le build précédent. Les seuils, puissances et temps accélérés appartiennent au scénario. Le principe du transfert de chaleur vers un fluide puis du rejet extérieur s’appuie sur le [guide DOE/NREL](https://www.energy.gov/sites/default/files/2024-07/best-practice-guide-data-center-design.pdf).
