using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Étape 1 — Chambre : le visiteur sélectionne une photo et l'envoie.
/// Ce script représente le geste familier qui déclenche le parcours.
/// Il ne simule pas un vrai transfert réseau ; l'animation illustre le concept.
/// </summary>
public class PhotoSender : MonoBehaviour
{
    [Header("Références UI")]
    [Tooltip("Objet représentant la photo sélectionnée (plane, sprite, etc.)")]
    public GameObject photoObject;

    [Tooltip("Objet représentant l'indicateur d'envoi en cours (ex. icône de chargement)")]
    public GameObject sendingIndicator;

    [Tooltip("Objet représentant la confirmation d'envoi (ex. coche verte)")]
    public GameObject confirmationIndicator;

    [Header("Paramètres d'animation")]
    [Tooltip("Durée simulée de l'envoi (en secondes) — illustratif")]
    [Range(1f, 5f)]
    public float sendDuration = 2.5f;

    [Header("Événements")]
    [Tooltip("Déclenché quand l'envoi démarre")]
    public UnityEvent onSendStarted;

    [Tooltip("Déclenché quand l'envoi est confirmé")]
    public UnityEvent onSendCompleted;

    private bool _isSending = false;

    /// <summary>
    /// Appelé par le bouton UI ou le XR interactor.
    /// </summary>
    public void Send()
    {
        if (_isSending) return;
        StartCoroutine(SendRoutine());
    }

    private IEnumerator SendRoutine()
    {
        _isSending = true;

        // Afficher l'indicateur d'envoi
        if (sendingIndicator != null) sendingIndicator.SetActive(true);
        if (confirmationIndicator != null) confirmationIndicator.SetActive(false);

        onSendStarted.Invoke();

        // Attendre la durée simulée
        yield return new WaitForSeconds(sendDuration);

        // Confirmer l'envoi
        if (sendingIndicator != null) sendingIndicator.SetActive(false);
        if (confirmationIndicator != null) confirmationIndicator.SetActive(true);

        onSendCompleted.Invoke();
        _isSending = false;
    }

    /// <summary>
    /// Réinitialise l'état (utile pour les tests répétés).
    /// </summary>
    public void Reset()
    {
        StopAllCoroutines();
        _isSending = false;
        if (sendingIndicator != null) sendingIndicator.SetActive(false);
        if (confirmationIndicator != null) confirmationIndicator.SetActive(false);
    }
}
