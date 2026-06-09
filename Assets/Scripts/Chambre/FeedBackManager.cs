using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Singleton gérant tous les feedbacks visuels : image de validation, croix rouge, message d'explication.
/// Placer sur un GameObject vide "FeedbackManager" dans la scène.
/// </summary>
public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("Images de feedback")]
    [Tooltip("Prefab contenant un Canvas WorldSpace avec une image de validation (ex: coche verte).")]
    public GameObject correctFeedbackPrefab;

    [Tooltip("Prefab contenant un Canvas WorldSpace avec une image de refus (croix rouge).")]
    public GameObject incorrectFeedbackPrefab;

    [Header("Message d'explication")]
    [Tooltip("Prefab d'un Canvas WorldSpace avec un Text pour afficher l'explication.")]
    public GameObject explanationPanelPrefab;

    [Header("Timings")]
    [Tooltip("Durée d'affichage de l'image de validation avant de commencer le fade.")]
    public float correctDisplayDuration = 1.5f;

    [Tooltip("Durée du fade out de l'image de validation.")]
    public float correctFadeDuration = 0.8f;

    [Tooltip("Durée d'affichage de la croix rouge avant de commencer le fade.")]
    public float incorrectDisplayDuration = 1.0f;

    [Tooltip("Durée du fade out de la croix rouge.")]
    public float incorrectFadeDuration = 0.8f;

    [Tooltip("Durée d'affichage du message d'explication (0 = permanent).")]
    public float explanationDisplayDuration = 6f;

    [Tooltip("Hauteur au-dessus de l'objet pour afficher les feedbacks (icône).")]
    public float feedbackHeightOffset = 0.8f;

    [Tooltip("Hauteur au-dessus de l'objet pour afficher le panneau d'explication.")]
    public float explanationHeightOffset = 1.6f;

    private Camera _mainCamera;

    // Garde anti-suraffichage : un seul panneau d'explication à la fois
    private GameObject _currentExplanationPanel;
    private Coroutine _currentExplanationCoroutine;

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
        _mainCamera = Camera.main;
    }

    // ─────────────────────────────────────────────
    // Bonne réponse : image de validation + fade
    // ─────────────────────────────────────────────
    public void ShowCorrectFeedback(Vector3 objectPosition)
    {
        if (correctFeedbackPrefab == null) return;

        Vector3 spawnPos = objectPosition + Vector3.up * feedbackHeightOffset;
        GameObject feedback = Instantiate(correctFeedbackPrefab, spawnPos, Quaternion.identity);
        FaceCamera(feedback);

        StartCoroutine(FadeAndDestroy(feedback, correctDisplayDuration, correctFadeDuration));
    }

    // ─────────────────────────────────────────────
    // Mauvaise réponse : croix rouge + explication
    // ─────────────────────────────────────────────
    public void ShowIncorrectFeedback(Vector3 objectPosition, string explanation)
    {
        if (incorrectFeedbackPrefab != null)
        {
            Vector3 spawnPos = objectPosition + Vector3.up * feedbackHeightOffset;
            GameObject feedback = Instantiate(incorrectFeedbackPrefab, spawnPos, Quaternion.identity);
            FaceCamera(feedback);
            StartCoroutine(FadeAndDestroy(feedback, incorrectDisplayDuration, incorrectFadeDuration));
        }

        ShowExplanation(objectPosition, explanation);
    }

    // ─────────────────────────────────────────────
    // Panneau d'explication (toujours affiché)
    // ─────────────────────────────────────────────
    public void ShowExplanation(Vector3 objectPosition, string explanation)
    {
        if (explanationPanelPrefab == null || string.IsNullOrEmpty(explanation)) return;

        // Détruire immédiatement le panneau précédent s'il existe
        if (_currentExplanationCoroutine != null)
        {
            StopCoroutine(_currentExplanationCoroutine);
            _currentExplanationCoroutine = null;
        }
        if (_currentExplanationPanel != null)
        {
            Destroy(_currentExplanationPanel);
            _currentExplanationPanel = null;
        }

        _currentExplanationCoroutine = StartCoroutine(ShowExplanationPanel(objectPosition, explanation));
    }

    private IEnumerator ShowExplanationPanel(Vector3 objectPosition, string explanation)
    {
        Vector3 spawnPos = objectPosition + Vector3.up * explanationHeightOffset;
        GameObject panel = Instantiate(explanationPanelPrefab, spawnPos, Quaternion.identity);
        _currentExplanationPanel = panel;
        FaceCamera(panel);

        Text textComponent = panel.GetComponentInChildren<Text>();
        if (textComponent != null)
            textComponent.text = explanation;

        TMPro.TMP_Text tmpComponent = panel.GetComponentInChildren<TMPro.TMP_Text>();
        if (tmpComponent != null)
            tmpComponent.text = explanation;

        if (explanationDisplayDuration > 0)
        {
            yield return new WaitForSeconds(explanationDisplayDuration);
            if (_currentExplanationPanel == panel)
                _currentExplanationPanel = null;
            StartCoroutine(FadeAndDestroy(panel, 0f, 0.8f));
        }
        else
        {
            yield return null;
        }
    }

    // ─────────────────────────────────────────────
    // Utilitaires
    // ─────────────────────────────────────────────

    private void FaceCamera(GameObject go)
    {
        if (_mainCamera == null) return;
        go.transform.LookAt(_mainCamera.transform.position);
        go.transform.Rotate(0, 180, 0);
    }

    private IEnumerator FadeAndDestroy(GameObject go, float displayDuration, float fadeDuration)
    {
        if (go == null) yield break;

        if (displayDuration > 0)
            yield return new WaitForSeconds(displayDuration);

        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Canvas canvas = go.GetComponentInChildren<Canvas>();
            if (canvas != null)
                cg = canvas.gameObject.AddComponent<CanvasGroup>();
            else
                cg = go.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        float startAlpha = cg.alpha;

        while (elapsed < fadeDuration)
        {
            if (go == null) yield break;
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        if (go != null)
            Destroy(go);
    }
}
