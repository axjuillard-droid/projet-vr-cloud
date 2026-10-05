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
    public bool IsTriggered => _triggered;
    public bool IsLoading { get; private set; }

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
            Debug.Log("[StepTransition] Envoi terminé — prochaine étape du parcours à construire.");
    }

    private void LoadNextScene()
    {
        if (IsLoading) return;
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("[StepTransition] Scène absente des scènes de build : " + nextSceneName);
            ResetTrigger();
            return;
        }
        IsLoading = true;
        if (InputModeManager.Instance != null) InputModeManager.Instance.PrepareNextScene();
        SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
    }

    public void ResetTrigger()
    {
        // Un chargement déjà commencé ne peut pas être annulé par un reset photo.
        if (IsLoading) return;
        CancelInvoke(nameof(LoadNextScene));
        _triggered = false;
        if (continuePrompt != null) continuePrompt.SetActive(false);
    }
}
