# Points blancs en Play — 5 octobre 2026

Unity 6000.0.58f2, Chambre, cible Android, Intel Iris Xe/Direct3D11. Au repos qualité PC ; en Play qualité Mobile, Mobile_RPAsset (HDR actif, renderScale 0,8, ombres douces initialement désactivées). Capture native caméra 1080 × 688 ; les aperçus MCP sont redimensionnés. Captures inspectées visuellement.

| Capture | Réglage | Résultat observé |
|---|---|---|
| baseline.png | Mobile original | Points blancs et taches colorées |
| hdr-off.png | Clone, HDR désactivé seulement | Points encore présents, moins lumineux |
| soft-shadows-on.png | Clone, HDR rétabli, ombres douces activées | Points absents |
| soft-shadows-off-again.png | Même clone, ombres douces désactivées | Points réapparaissent |
| saved-fix-front.png | Original corrigé, nouveau Play | Points absents |
| saved-fix-turned.png | Rotation caméra injectée (10°, 165°, 0°) | Points absents |
| saved-fix-other-angle.png | Rotation caméra injectée (18°, 195°, 0°) | Points absents |

Correctif unique : `Assets/Settings/Mobile_RPAsset.asset`, `m_SoftShadowsSupported: 1` au lieu de 0. Aucun nouveau script, changement de package ou d'anticrénelage. Clone de pipeline temporaire supprimé ; contrôleur PC désactivé seulement pendant les rotations de contrôle et restauré par arrêt de Play. Scène non modifiée/sauvegardée.

Reproduction : cible Android active, ouvrir Chambre, Play, vérifier Mobile_RPAsset et déplacer la vue. Pour une comparaison future, noter le réglage initial puis tester les ombres douces séparément et restaurer le correctif après comparaison. Les exports existants ne suivent pas automatiquement ce réglage.

Référence primaire : [Unity UUM-121981](https://issuetracker.unity.com/issues/6091/flickering-bright-white-dots-in-the-scene-when-the-android-platform-is-selected-and-dx11-graphics-api-is-used-with-irisr-xe-graphics-gpu), configuration et contournement concordants avec les observations. Pas de conclusion de panne GPU.

Validation : captures dans l'éditeur et reprise de Play ; pas de vidéo continue ni essai utilisateur en mouvement après correctif. L'utilisateur a séparément testé CloudVR-PC.exe et le juge « très bien » : retour qualitatif positif, sans mesure chiffrée. Aucun nouvel exe/APK compilé. Coût des ombres douces et rendu à mesurer sur le Quest réel ; l'APK livré précédemment utilise l'ancien réglage.

État final à 14 h 07 Paris : hors Play/compilation, Chambre propre, une caméra active, qualité PC, aucun clone de diagnostic. Console erreurs vide. Le diff Git des assets LFS n'a pas été exécuté avec succès (accès refusé au cache .git/lfs/tmp) ; contrôle textuel du réglage effectué. Aucun commit/push.
