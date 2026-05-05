using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Singleton gérant la logique de jeu : compteur de bloqueurs, validation des signalements.
/// Placer sur un GameObject vide "GameManager" dans la scène.
/// </summary>
public class ChambreGameManager : MonoBehaviour
{
    public static ChambreGameManager Instance { get; private set; }

    [Header("Compteur de bloqueurs")]
    [Tooltip("Nombre total de bloqueurs dans la scène. Peut être calculé automatiquement.")]
    public int totalBlockers = 0;

    [Tooltip("Activer pour compter automatiquement les bloqueurs au démarrage.")]
    public bool autoCountBlockers = true;

    [Header("UI du compteur")]
    [Tooltip("Texte affichant le nombre de bloqueurs restants.")]
    public TMP_Text blockerCountText; 

    [Tooltip("Panel affiché quand tous les bloqueurs sont trouvés.")]
    public GameObject victoryPanel;

    private int _remainingBlockers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (autoCountBlockers)
        {
            InteractableObject[] allObjects = FindObjectsOfType<InteractableObject>();
            totalBlockers = 0;
            foreach (var obj in allObjects)
                if (obj.isBlocker) totalBlockers++;
        }

        _remainingBlockers = totalBlockers;
        UpdateCounterUI();

        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    /// <summary>
    /// Appelé par InteractableObject.Report() lorsque le joueur signale un objet.
    /// </summary>
    public void OnObjectReported(InteractableObject reportedObject)
    {
        if (reportedObject.isBlocker)
        {
            // Bonne réponse
            _remainingBlockers--;
            UpdateCounterUI();
            FeedbackManager.Instance.ShowCorrectFeedback(reportedObject.transform.position);

            if (_remainingBlockers <= 0)
                StartCoroutine(TriggerVictory());
        }
        else
        {
            // Mauvaise réponse
            FeedbackManager.Instance.ShowIncorrectFeedback(
                reportedObject.transform.position,
                reportedObject.explanationText
            );
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

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Debug.Log("Tous les bloqueurs ont été identifiés !");
    }
}