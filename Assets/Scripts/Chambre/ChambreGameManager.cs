using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Singleton gérant la logique de jeu : compteur de bloqueurs, résultat du tri dans la poubelle.
/// Placer sur un GameObject vide "GameManager" dans la scène.
///
/// Déroulement en deux phases :
///   1) Le joueur jette les bloqueurs (les non-bloqueurs sont renvoyés à leur place).
///   2) Une fois tous les bloqueurs jetés, le joueur doit jeter tous les non-bloqueurs
///      (qui sont alors acceptés avec un feedback positif) pour comprendre pourquoi
///      ils n'étaient pas des bloqueurs.
/// </summary>
public class ChambreGameManager : MonoBehaviour
{
    public static ChambreGameManager Instance { get; private set; }

    /// <summary>Déclenché à chaque bloqueur supprimé. (remaining, total)</summary>
    public static event Action<int, int> OnBlockerDestroyed;

    /// <summary>Déclenché à chaque non-bloqueur jeté pendant la phase 2. (remaining, total)</summary>
    public static event Action<int, int> OnNonBlockerDestroyed;

    [Header("Compteur de bloqueurs")]
    [Tooltip("Nombre total de bloqueurs dans la scène. Calculé automatiquement si autoCountBlockers est vrai.")]
    public int totalBlockers = 0;

    [Tooltip("Activer pour compter automatiquement les bloqueurs (et non-bloqueurs) au démarrage.")]
    public bool autoCountBlockers = true;

    [Header("UI")]
    [Tooltip("Texte affichant le nombre de bloqueurs restants.")]
    public TMP_Text blockerCountText;

    [Tooltip("Panel affiché quand tous les bloqueurs sont correctement triés, puis quand tout est trié.")]
    public GameObject victoryPanel;

    [Tooltip("Texte du panel de victoire, mis à jour selon la phase atteinte.")]
    public TMP_Text victoryText;

    [Header("Messages du panel de victoire")]
    [Tooltip("Message affiché une fois tous les bloqueurs jetés.")]
    [TextArea(2, 5)]
    public string blockersDoneMessage = "Vous avez trouvé tous les éléments bloqueurs. Maintenant jetez tous les éléments non bloqueurs afin de comprendre pourquoi ils ne le sont pas.";

    [Tooltip("Message affiché une fois tous les objets (bloqueurs et non-bloqueurs) jetés.")]
    [TextArea(2, 5)]
    public string allSortedMessage = "Félicitation, vous avez jeté tous les objets pouvant être un bloqueur pour une relation ou non !";

    [Header("Poubelle")]
    [Tooltip("Renderer du cube de la poubelle dont le material change une fois tous les bloqueurs jetés.")]
    public Renderer poubelleCubeRenderer;

    [Tooltip("Material appliqué au cube de la poubelle une fois tous les bloqueurs jetés.")]
    public Material blockersDoneMaterial;

    private int _remainingBlockers;
    private int _totalNonBlockers;
    private int _remainingNonBlockers;
    private bool _blockersPhaseComplete;

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
            _totalNonBlockers = 0;
            foreach (var obj in FindObjectsByType<InteractableObject>(FindObjectsSortMode.None))
            {
                if (obj.isBlocker) totalBlockers++;
                else _totalNonBlockers++;
            }
        }

        _remainingBlockers = totalBlockers;
        _remainingNonBlockers = _totalNonBlockers;
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

        if (!_blockersPhaseComplete)
        {
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
                    StartCoroutine(TriggerBlockersComplete());
            }
            else
            {
                // Non-bloqueur : trop tôt, on le retéléporte à sa position d'origine
                FeedbackManager.Instance.ShowIncorrectFeedback(pos, placedObject.explanationText);
                placedObject.ResetToOrigin();
                placedObject.ResetProcessed();
            }
        }
        else
        {
            // Phase 2 : les non-bloqueurs sont maintenant la bonne réponse
            _remainingNonBlockers--;
            FeedbackManager.Instance.ShowCorrectFeedback(pos);
            FeedbackManager.Instance.ShowExplanation(pos, placedObject.explanationText);
            Destroy(placedObject.gameObject);
            OnNonBlockerDestroyed?.Invoke(_remainingNonBlockers, _totalNonBlockers);

            if (_remainingNonBlockers <= 0)
                StartCoroutine(TriggerAllSorted());
        }
    }

    /// <summary>
    /// Appelé par le bouton de réinitialisation. Renvoie tous les objets interactables
    /// encore présents dans la scène (non détruits) à leur position d'origine.
    /// </summary>
    public void ResetAllInteractables()
    {
        foreach (var obj in FindObjectsByType<InteractableObject>(FindObjectsSortMode.None))
        {
            XRGrabInteractable grab = obj.GetComponent<XRGrabInteractable>();
            if (grab != null && grab.isSelected)
            {
                var interactorsCopy = new List<IXRSelectInteractor>(grab.interactorsSelecting);
                foreach (var interactor in interactorsCopy)
                    grab.interactionManager.SelectExit(interactor, grab);
            }

            obj.ResetProcessed();
            obj.ResetToOrigin();
        }
    }

    private void UpdateCounterUI()
    {
        if (blockerCountText != null)
            blockerCountText.text = $"Bloqueurs restants : {_remainingBlockers}";
    }

    private IEnumerator TriggerBlockersComplete()
    {
        yield return new WaitForSeconds(1.5f);

        _blockersPhaseComplete = true;

        if (poubelleCubeRenderer != null && blockersDoneMaterial != null)
            poubelleCubeRenderer.material = blockersDoneMaterial;

        Debug.Log("Tous les bloqueurs ont été correctement triés !");

        if (_remainingNonBlockers <= 0)
        {
            // Pas de non-bloqueur à trier, on passe directement à la victoire finale
            yield return TriggerAllSorted();
            yield break;
        }

        if (victoryText != null) victoryText.text = blockersDoneMessage;
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    private IEnumerator TriggerAllSorted()
    {
        yield return new WaitForSeconds(1.5f);

        if (victoryText != null) victoryText.text = allSortedMessage;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Debug.Log("Tous les objets ont été triés !");
    }
}
