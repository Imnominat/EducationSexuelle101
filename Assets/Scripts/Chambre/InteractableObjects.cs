using UnityEngine;

/// <summary>
/// À placer sur chaque objet interactable de la scène.
/// Définit si l'objet est un bloqueur et le message associé.
/// L'objet doit aussi avoir un XRGrabInteractable + Rigidbody pour être saisissable.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Configuration du bloqueur")]
    [Tooltip("Cocher si cet objet est un bloqueur à identifier.")]
    public bool isBlocker = false;

    [Tooltip("Nom affiché pour cet objet.")]
    public string objectLabel = "Objet";

    [Tooltip("Texte d'explication affiché si le joueur fait une erreur.")]
    [TextArea(3, 6)]
    public string explanationText = "Cet objet ne constitue pas un frein à la relation. Il n'y a pas de problème ici.";

    private bool _isProcessed = false;

    /// <summary>Vrai si l'objet a déjà été traité par une poubelle.</summary>
    public bool IsProcessed => _isProcessed;

    public void MarkAsProcessed() => _isProcessed = true;

    public void ResetProcessed() => _isProcessed = false;
}
