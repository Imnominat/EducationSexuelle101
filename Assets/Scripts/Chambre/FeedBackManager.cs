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

    [Tooltip("Hauteur au-dessus de l'objet pour afficher les feedbacks.")]
    public float feedbackHeightOffset = 0.8f;

    private Camera _mainCamera;

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

        // Afficher le panneau d'explication devant l'objet
        if (explanationPanelPrefab != null && !string.IsNullOrEmpty(explanation))
        {
            StartCoroutine(ShowExplanationPanel(objectPosition, explanation));
        }
    }

    private IEnumerator ShowExplanationPanel(Vector3 objectPosition, string explanation)
    {
        // Légèrement décalé vers le joueur pour être lisible
        Vector3 spawnPos = objectPosition + Vector3.up * (feedbackHeightOffset * 0.3f);
        GameObject panel = Instantiate(explanationPanelPrefab, spawnPos, Quaternion.identity);
        FaceCamera(panel);

        // Injecter le texte dans le prefab
        Text textComponent = panel.GetComponentInChildren<Text>();
        if (textComponent != null)
            textComponent.text = explanation;

        // Aussi compatible TextMeshPro
        TMPro.TMP_Text tmpComponent = panel.GetComponentInChildren<TMPro.TMP_Text>();
        if (tmpComponent != null)
            tmpComponent.text = explanation;

        if (explanationDisplayDuration > 0)
        {
            yield return new WaitForSeconds(explanationDisplayDuration);
            StartCoroutine(FadeAndDestroy(panel, 0f, 0.8f));
        }
        else
        {
            yield return null;
            // Panel permanent jusqu'à la fin de la scène
        }
    }

    // ─────────────────────────────────────────────
    // Utilitaires
    // ─────────────────────────────────────────────

    /// <summary>Oriente un GameObject face à la caméra principale.</summary>
    private void FaceCamera(GameObject go)
    {
        if (_mainCamera == null) return;
        go.transform.LookAt(_mainCamera.transform.position);
        go.transform.Rotate(0, 180, 0);
    }

    /// <summary>Attend 'displayDuration' puis fade le CanvasGroup sur 'fadeDuration', puis détruit.</summary>
    private IEnumerator FadeAndDestroy(GameObject go, float displayDuration, float fadeDuration)
    {
        if (go == null) yield break;

        // Attente avant le fade
        if (displayDuration > 0)
            yield return new WaitForSeconds(displayDuration);

        // Récupérer ou créer un CanvasGroup pour le fade
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Canvas canvas = go.GetComponentInChildren<Canvas>();
            if (canvas != null)
                cg = canvas.gameObject.AddComponent<CanvasGroup>();
            else
                cg = go.AddComponent<CanvasGroup>();
        }

        // Fade out
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