using UnityEngine;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class Plaquette : MonoBehaviour
{
    [Header("Données")]
    public PlaquetteData data;

    // Référence au label 3D (TextMeshPro)
    [Header("Visuel")]
    public TMPro.TextMeshPro labelText;
    public Renderer cardRenderer;

    // Couleurs de feedback
    [Header("Couleurs")]
    public Color couleurNormale = Color.white;
    public Color couleurErreur  = Color.red;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;
    private bool _estDeposee = false; // true quand posée sur une zone

    void Awake()
    {
        _grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Start()
    {
        // Applique le label depuis le ScriptableObject
        if (data != null && labelText != null)
            labelText.text = data.label;
    }

    // Appelé par la zone de dépôt quand la plaquette est posée correctement
    public void OnDeposeCorrectement()
    {
        _estDeposee = true;
        // Désactive le grab pour qu'on ne puisse plus la reprendre
        _grab.enabled = false;
        SetCouleur(couleurNormale);
    }

    // Appelé par la zone de dépôt ou le GameManager si erreur
    public void OnErreur()
    {
        SetCouleur(couleurErreur);
        // Le joueur doit la jeter à la poubelle
    }

    // Remet la plaquette en état neutre (après correction)
    public void ResetVisuel()
    {
        SetCouleur(couleurNormale);
    }

    void SetCouleur(Color c)
    {
        if (cardRenderer != null)
            cardRenderer.material.color = c;
    }

    // Accesseur utile pour les autres scripts
    public bool EstPositive() => data != null && data.isPositive;
    public ZoneCible GetZoneCible() => data != null ? data.zoneCible : ZoneCible.Poubelle;
}