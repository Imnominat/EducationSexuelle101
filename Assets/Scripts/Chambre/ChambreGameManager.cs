using System;
using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Singleton gérant la logique de jeu : compteur de bloqueurs, résultat du tri dans la poubelle.
/// Placer sur un GameObject vide "GameManager" dans la scène.
/// </summary>
public class ChambreGameManager : MonoBehaviour
{
    public static ChambreGameManager Instance { get; private set; }

    /// <summary>Déclenché à chaque bloqueur supprimé. (remaining, total)</summary>
    public static event Action<int, int> OnBlockerDestroyed;

    [Header("Compteur de bloqueurs")]
    [Tooltip("Nombre total de bloqueurs dans la scène. Calculé automatiquement si autoCountBlockers est vrai.")]
    public int totalBlockers = 0;

    [Tooltip("Activer pour compter automatiquement les bloqueurs au démarrage.")]
    public bool autoCountBlockers = true;

    [Header("UI")]
    [Tooltip("Texte affichant le nombre de bloqueurs restants.")]
    public TMP_Text blockerCountText;

    [Tooltip("Panel affiché quand tous les bloqueurs sont correctement triés.")]
    public GameObject victoryPanel;

    private int _remainingBlockers;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (autoCountBlockers)
        {
            totalBlockers = 0;
            foreach (var obj in FindObjectsOfType<InteractableObject>())
                if (obj.isBlocker) totalBlockers++;
        }

        _remainingBlockers = totalBlockers;
        UpdateCounterUI();

        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    /// <summary>
    /// Appelé par TrashBin quand un objet y est déposé.
    /// </summary>
    public void OnObjectPlacedInBin(InteractableObject placedObject)
    {
        if (placedObject.IsProcessed) return;
        placedObject.MarkAsProcessed();

        Vector3 pos = placedObject.transform.position;

        if (placedObject.isBlocker)
        {
            // Bloqueur correctement identifié : bonne réponse, on le supprime
            _remainingBlockers--;
            UpdateCounterUI();
            FeedbackManager.Instance.ShowCorrectFeedback(pos);
            FeedbackManager.Instance.ShowExplanation(pos, placedObject.explanationText);
            Destroy(placedObject.gameObject);
            OnBlockerDestroyed?.Invoke(_remainingBlockers, totalBlockers);

            if (_remainingBlockers <= 0)
                StartCoroutine(TriggerVictory());
        }
        else
        {
            // Non-bloqueur : mauvaise réponse, on le retéléporte à sa position d'origine
            FeedbackManager.Instance.ShowIncorrectFeedback(pos, placedObject.explanationText);
            placedObject.ResetToOrigin();
            placedObject.ResetProcessed();
        }
    }

    private void UpdateCounterUI()
    {
        if (blockerCountText != null)
            blockerCountText.text = $"Bloqueurs restants : {_remainingBlockers}";
    }

    private IEnumerator TriggerVictory()
    {
        yield return new WaitForSeconds(1.5f);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Debug.Log("Tous les bloqueurs ont été correctement triés !");
    }
}
