using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère les transitions entre étapes du parcours.
/// L'étape 1 (Chambre) se termine quand la photo est envoyée avec succès,
/// déclenchant la transition vers l'étape 2 (Arrivée au data center).
/// </summary>
public class StepTransition : MonoBehaviour
{
    [Header("Scène suivante")]
    [Tooltip("Nom exact de la scène à charger (doit être dans Build Settings)")]
    public string nextSceneName = "";

    [Header("Délai avant transition")]
    [Range(0f, 5f)]
    public float delayBeforeTransition = 2f;

    [Header("Option : objet d'indication")]
    [Tooltip("Objet affiché pour inviter le visiteur à continuer (flèche, texte...)")]
    public GameObject continuePrompt;

    private bool _triggered = false;

    /// <summary>
    /// Déclenché par l'événement OnSendCompleted du PhotoSender.
    /// </summary>
    public void TriggerTransition()
    {
        if (_triggered) return;
        _triggered = true;

        if (continuePrompt != null) continuePrompt.SetActive(true);

        if (!string.IsNullOrEmpty(nextSceneName))
            Invoke(nameof(LoadNextScene), delayBeforeTransition);
        else
            Debug.LogWarning("[StepTransition] Aucune scène suivante configurée — étape terminale pour ce prototype.");
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void ResetTrigger()
    {
        _triggered = false;
        if (continuePrompt != null) continuePrompt.SetActive(false);
    }
}
