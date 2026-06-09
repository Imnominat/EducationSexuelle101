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

    [Tooltip("Texte d'explication affiché quand l'objet est jeté dans la poubelle.")]
    [TextArea(3, 6)]
    public string explanationText = "Cet objet ne constitue pas un frein à la relation. Il n'y a pas de problème ici.";

    private bool _isProcessed = false;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    public bool IsProcessed => _isProcessed;

    private void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public void MarkAsProcessed() => _isProcessed = true;

    public void ResetProcessed() => _isProcessed = false;

    /// <summary>Retéléporte l'objet à sa position et rotation d'origine.</summary>
    public void ResetToOrigin()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }
}
