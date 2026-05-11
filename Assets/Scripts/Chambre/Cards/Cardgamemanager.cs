using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;
using TMPro;

public class CardGameManager : MonoBehaviour
{
    [Header("Deck")]
    public CardDeck deck;

    [Header("Cartes en scène (5 emplacements)")]
    public CardDisplay[] cardSlots = new CardDisplay[5];

    [Header("Ray Interactor (main droite)")]
    public XRRayInteractor rayInteractor;
    public InputActionReference activateAction;

    [Header("UI — Boutons Oui/Non")]
    public Button yesButton;
    public Button noButton;

    [Header("UI — Phase de correction")]
    public GameObject correctionPanel;
    public TMP_Text verdictText;
    public Transform explanationContainer;
    public GameObject explanationLinePrefab;
    public Button nextRoundButton;

    [Header("UI — HUD")]
    public TMP_Text roundCounterText;
    public GameObject endGamePanel;
    public TMP_Text endGameSummaryText;

    [Header("UI — Feedback ciblage")]
    public GameObject cardFocusPanel;
    public TMP_Text cardFocusLabel;

    [Header("Règles de validation")]
    public int maxDefavorableAllowed = 2;

    [Header("Manches")]
    public int totalRounds = 3;

    // État interne
    private int _currentRound = 0;
    private int _correctAnswers = 0;
    private List<CardData> _currentHand = new List<CardData>();

    private CardDisplay _focusedCard;
    private Coroutine _focusHideCoroutine;
    private bool _rayOnCard;
    private bool _rayOnFocusPanel;
    private bool _isInCorrectionPhase = false;

    // ─────────────────────────────────────────────
    // Init
    // ─────────────────────────────────────────────

    private void Start()
    {
        if (yesButton != null) yesButton.onClick.AddListener(() => OnPlayerAnswer(true));
        if (noButton != null)  noButton.onClick.AddListener(() => OnPlayerAnswer(false));
        if (nextRoundButton != null) nextRoundButton.onClick.AddListener(StartNextRound);

        if (correctionPanel != null) correctionPanel.SetActive(false);
        if (endGamePanel != null)    endGamePanel.SetActive(false);
        if (cardFocusPanel != null)  cardFocusPanel.SetActive(false);

        StartNextRound();
    }

    private void OnEnable()
    {
        if (activateAction != null)
        {
            activateAction.action.Enable();
            activateAction.action.performed += OnActivatePerformed;
        }
    }

    private void OnDisable()
    {
        if (activateAction != null)
            activateAction.action.performed -= OnActivatePerformed;
    }

    // ─────────────────────────────────────────────
    // Manches
    // ─────────────────────────────────────────────

    private void StartNextRound()
    {
        _currentRound++;
        _isInCorrectionPhase = false;

        if (correctionPanel != null) correctionPanel.SetActive(false);

        // Réinitialiser le bouton Suivant pour pointer vers StartNextRound
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.RemoveAllListeners();
            nextRoundButton.onClick.AddListener(StartNextRound);
        }

        UpdateRoundUI();
        DealHand();
        SetButtonsInteractable(true);
    }

    private void DealHand()
    {
        _currentHand = deck.DrawHand();

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null) continue;

            if (i < _currentHand.Count)
            {
                cardSlots[i].SetCard(_currentHand[i]);
                cardSlots[i].SetInteractable(true);
                cardSlots[i].gameObject.SetActive(true);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateRoundUI()
    {
        if (roundCounterText != null)
            roundCounterText.text = $"Manche {_currentRound} / {totalRounds}";
    }

    // ─────────────────────────────────────────────
    // Validation Oui/Non
    // ─────────────────────────────────────────────

    private void OnPlayerAnswer(bool playerSaysYes)
    {
        if (_isInCorrectionPhase) return;

        bool combinationIsCorrect = EvaluateHand(_currentHand);
        bool playerIsCorrect = (playerSaysYes == combinationIsCorrect);

        if (playerIsCorrect) _correctAnswers++;

        SetButtonsInteractable(false);
        StartCoroutine(ShowCorrection(combinationIsCorrect, playerIsCorrect));
    }

    private bool EvaluateHand(List<CardData> hand)
    {
        int bloquants    = 0;
        int defavorables = 0;

        foreach (var card in hand)
        {
            switch (card.level)
            {
                case CardData.CardLevel.Bloquant:    bloquants++;    break;
                case CardData.CardLevel.Defavorable: defavorables++; break;
            }
        }

        return bloquants == 0 && defavorables <= maxDefavorableAllowed;
    }

    // ─────────────────────────────────────────────
    // Phase de correction
    // ─────────────────────────────────────────────

    private IEnumerator ShowCorrection(bool combinationIsCorrect, bool playerIsCorrect)
    {
        _isInCorrectionPhase = true;

        // Désactiver l'interactivité des cartes
        foreach (var slot in cardSlots)
            if (slot != null) slot.SetInteractable(false);

        // Vider les explications précédentes
        if (explanationContainer != null)
            foreach (Transform child in explanationContainer)
                Destroy(child.gameObject);

        // Verdict — sans emojis pour compatibilité LiberationSans SDF
        if (verdictText != null)
        {
            if (playerIsCorrect)
                verdictText.text = combinationIsCorrect
                    ? "Bonne reponse ! Cette combinaison permet une relation saine."
                    : "Bonne reponse ! Tu as bien identifie que cette combinaison pose probleme.";
            else
                verdictText.text = combinationIsCorrect
                    ? "Pas tout a fait. Cette combinaison etait en realite acceptable."
                    : "Pas tout a fait. Cette combinaison contenait des elements problematiques.";
        }

        // Explications des cartes non-favorables
        foreach (var card in _currentHand)
        {
            if (card.level == CardData.CardLevel.Favorable) continue;

            if (explanationLinePrefab != null && explanationContainer != null)
            {
                GameObject line = Instantiate(explanationLinePrefab, explanationContainer);

                // true = chercher aussi dans les composants inactifs
                TMP_Text lineText = line.GetComponentInChildren<TMP_Text>(true);
                if (lineText != null)
                {
                    lineText.gameObject.SetActive(true); // activer si désactivé dans le prefab
                    string levelLabel = card.level == CardData.CardLevel.Bloquant
                        ? "[Bloquant]"
                        : "[Defavorable]";
                    lineText.text = $"<b>{levelLabel} {card.cardText}</b>\n{card.explanation}";
                }
                else
                {
                    Debug.LogWarning("[CardGameManager] Aucun TMP_Text trouvé dans explanationLinePrefab.");
                }
            }
        }

        if (correctionPanel != null) correctionPanel.SetActive(true);

        // Configurer le bouton Suivant / Terminer
        bool isLastRound = _currentRound >= totalRounds;
        if (nextRoundButton != null)
        {
            nextRoundButton.gameObject.SetActive(true);

            TMP_Text btnText = nextRoundButton.GetComponentInChildren<TMP_Text>(true);
            if (btnText != null)
                btnText.text = isLastRound ? "Terminer" : "Manche suivante";

            // Réassigner le listener selon la situation
            nextRoundButton.onClick.RemoveAllListeners();
            nextRoundButton.onClick.AddListener(isLastRound ? ShowEndGame : (UnityEngine.Events.UnityAction)StartNextRound);
        }

        yield return null;
    }

    private void ShowEndGame()
    {
        if (correctionPanel != null) correctionPanel.SetActive(false);
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
            if (endGameSummaryText != null)
                endGameSummaryText.text = $"Jeu termine !\nBonnes reponses : {_correctAnswers} / {totalRounds}";
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        if (yesButton != null) yesButton.interactable = state;
        if (noButton != null)  noButton.interactable  = state;
    }

    // ─────────────────────────────────────────────
    // Ciblage des cartes via Ray Interactor
    // ─────────────────────────────────────────────

    private void Update()
    {
        // Garder le ciblage actif même en correction (pour éviter le NullRef),
        // mais bloquer le signalement via _isInCorrectionPhase dans OnActivatePerformed
        if (rayInteractor != null)
            CheckCardRaycast();
    }

    private void CheckCardRaycast()
    {
        bool hitCard  = false;
        bool hitPanel = false;

        if (rayInteractor.TryGetCurrentRaycast(
                out RaycastHit? raycastHit,
                out int _,
                out UnityEngine.EventSystems.RaycastResult? uiResult,
                out int __,
                out bool isUIHit))
        {
            if (isUIHit && uiResult.HasValue && cardFocusPanel != null)
            {
                GameObject hitObj = uiResult.Value.gameObject;
                if (hitObj != null && hitObj.transform.IsChildOf(cardFocusPanel.transform))
                    hitPanel = true;
            }
            else if (raycastHit.HasValue)
            {
                Collider col = raycastHit.Value.collider;
                if (col != null)
                {
                    CardDisplay card = col.GetComponent<CardDisplay>();
                    if (card != null && card.CurrentCard != null)
                    {
                        hitCard = true;
                        OnCardFocused(card);
                    }
                }
            }
        }

        _rayOnCard       = hitCard;
        _rayOnFocusPanel = hitPanel;

        if (!hitCard && !hitPanel)
            OnCardFocusLost();
        else
            StopFocusHideCoroutine();
    }

    private void OnCardFocused(CardDisplay card)
    {
        StopFocusHideCoroutine();

        if (card == _focusedCard) return;

        if (_focusedCard != null) _focusedCard.OnLoseFocus();
        _focusedCard = card;
        _focusedCard.OnFocus();

        if (cardFocusPanel != null)
        {
            cardFocusPanel.SetActive(true);
            if (cardFocusLabel != null)
                cardFocusLabel.text = card.CurrentCard.cardText;

            cardFocusPanel.transform.position = card.transform.position + Vector3.up * 0.3f;
            Camera cam = Camera.main;
            if (cam != null)
            {
                cardFocusPanel.transform.LookAt(cam.transform.position);
                cardFocusPanel.transform.Rotate(0, 180, 0);
            }
        }
    }

    private void OnCardFocusLost()
    {
        if (_focusedCard == null) return;
        if (_focusHideCoroutine == null)
            _focusHideCoroutine = StartCoroutine(HideFocusPanelAfterDelay());
    }

    private IEnumerator HideFocusPanelAfterDelay()
    {
        float elapsed = 0f;
        while (elapsed < 3f)
        {
            if (_rayOnCard || _rayOnFocusPanel)
            {
                _focusHideCoroutine = null;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_focusedCard != null) _focusedCard.OnLoseFocus();
        _focusedCard = null;
        if (cardFocusPanel != null) cardFocusPanel.SetActive(false);
        _focusHideCoroutine = null;
    }

    private void StopFocusHideCoroutine()
    {
        if (_focusHideCoroutine != null)
        {
            StopCoroutine(_focusHideCoroutine);
            _focusHideCoroutine = null;
        }
    }

    // ─────────────────────────────────────────────
    // Signalement d'une carte (trigger manette)
    // ─────────────────────────────────────────────

    private void OnActivatePerformed(InputAction.CallbackContext ctx)
    {
        if (_isInCorrectionPhase) return;
        if (_focusedCard == null) return;
        if (!_rayOnCard && !_rayOnFocusPanel) return;

        _focusedCard.ToggleNegativeMark();
    }
}