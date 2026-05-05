using UnityEngine;

/// <summary>
/// À placer sur chaque objet interactable de la scène.
/// Définit si l'objet est un bloqueur et le message associé.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Configuration du bloqueur")]
    [Tooltip("Cocher si cet objet est un bloqueur à identifier.")]
    public bool isBlocker = false;

    [Tooltip("Nom affiché au-dessus de l'objet quand ciblé.")]
    public string objectLabel = "Objet";

    [Tooltip("Texte d'explication affiché si le joueur signale un non-bloqueur à tort.")]
    [TextArea(3, 6)]
    public string explanationText = "Cet objet ne constitue pas un frein à la relation. Il n'y a pas de problème ici.";

    [Header("Feedback visuel (optionnel)")]
    [Tooltip("Outline ou highlight activé quand l'objet est ciblé. Laisser vide pour ignorer.")]
    public Renderer highlightRenderer;

    private Material _originalMaterial;
    private bool _isReported = false;

    public bool IsReported => _isReported;

    private void Start()
    {
        if (highlightRenderer != null)
            _originalMaterial = highlightRenderer.material;
    }

    /// <summary>Appelé par GazeTargeting quand l'objet est ciblé.</summary>
    public void OnFocus()
    {
        if (_isReported) return;
        // Activer l'outline/highlight ici si souhaité
        // ex: highlightRenderer.material = highlightMaterial;
    }

    /// <summary>Appelé par GazeTargeting quand l'objet n'est plus ciblé.</summary>
    public void OnLoseFocus()
    {
        if (highlightRenderer != null && _originalMaterial != null)
            highlightRenderer.material = _originalMaterial;
    }

    /// <summary>
    /// Appelé par GazeTargeting quand le joueur appuie sur le bouton de signalement.
    /// </summary>
    public void Report()
    {
        if (_isReported) return;

        _isReported = true;
        OnLoseFocus();
        ChambreGameManager.Instance.OnObjectReported(this);
    }
}