# Mesures GPU PC du 9 novembre 2026

[Protocole, résultats et revue des images](../../OPTIMISATION_GPU_2026-11-09.md)

- [Audit](pass-audit/summary.json) avec deux fenêtres de référence et six interventions à MSAA 4x.
- [Cascades](shadow-comparison/summary.json) avec quatre fenêtres ABBA et captures du même angle.
- [Occlusion ambiante](ao-comparison/summary.json) avec quatre fenêtres ABBA et captures du même angle.
- [Build Windows](build-result.json), [196 contrôles du player livré](pc/demo-smoke-result.json), [lancement D3D11](pc/launch-result.json) et [empreintes des fichiers livrés](delivery-manifest.json).
- [Chambre](pc/demo-room-start.png), [cinématique réseau](pc/demo-network-stage-3.png), [arrivée](pc/demo-arrival.png), [salle blanche](pc/demo-lighting-distance.png), [refroidissement](pc/demo-cooling-recovered.png), [supervision](pc/demo-supervision-report.png) et [bilan](pc/demo-summary.png).

Chaque série contient ses CSV de trames, le rapport du probe, le résultat du lanceur et le journal anonymisé. Depuis la racine du dépôt, le résumé se recalcule avec `python tools/Summarize-PCPerformance.py docs/validation/gpu-2026-11-09/pass-audit`. Les captures hors écran sont faites après chronométrage.

`-PassAudit` et `-AoComparison` demandent un player de référence avec le SSAO actif. Pour reproduire un essai depuis le dépôt, préparer le profil de référence dans une copie, compiler, puis passer son exécutable avec `-PlayerPath` à `tools/Test-PCPerformance.ps1 -Visible`. `-ShadowComparison` permet de rejouer l’ABBA des cascades.
