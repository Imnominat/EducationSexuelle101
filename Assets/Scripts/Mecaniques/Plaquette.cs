using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class Plaquette : MonoBehaviour
{
    // ─────────────────────────────────────────
    // DONNÉES
    // ─────────────────────────────────────────
    [Header("Données")]
    public PlaquetteData data;

    // ─────────────────────────────────────────
    // RÉFÉRENCES VISUELLES
    // ─────────────────────────────────────────
    [Header("Visuel")]
    public TMPro.TextMeshPro labelText;  // Le texte 3D sur la plaquette
    public Renderer cardRenderer;        // Le MeshRenderer de la carte

    // ─────────────────────────────────────────
    // COULEURS DE FEEDBACK
    // ─────────────────────────────────────────
    [Header("Couleurs")]
    public Color couleurNormale  = Color.white;
    public Color couleurErreur   = Color.red;
    public Color couleurValidee  = Color.green;

    // ─────────────────────────────────────────
    // ÉTAT INTERNE
    // ─────────────────────────────────────────
    private XRGrabInteractable _grab;
    private Rigidbody _rb;
    private bool _fixeeAuMur   = false;
    private bool _estDeposee   = false; // true une fois correctement placée

    private bool _enErreur = false;
    private bool _estGrabbee = false;

    // ─────────────────────────────────────────
    // INITIALISATION
    // ─────────────────────────────────────────
    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb   = GetComponent<Rigidbody>();

        // Abonnement aux événements XR
        _grab.selectEntered.AddListener(OnSaisie);
        _grab.selectExited.AddListener(OnRelache);
    }

    void Start()
    {
        // Affiche le label depuis le ScriptableObject
        if (data != null && labelText != null)
            labelText.text = data.label;

        // Couleur initiale
        SetCouleur(couleurNormale);
    }

    // ─────────────────────────────────────────
    // FIXATION AU MUR (appelé par MurPlaquettes)
    // ─────────────────────────────────────────

    /// <summary>
    /// Fixe la plaquette au mur au spawn :
    /// désactive la physique jusqu'à ce que le joueur la saisisse.
    /// </summary>
    public void FixerAuMur()
    {
        _fixeeAuMur      = true;
        _rb.isKinematic  = true;
        _rb.useGravity   = false;
    }

    // ─────────────────────────────────────────
    // ÉVÉNEMENTS XR
    // ─────────────────────────────────────────

    /// <summary>
    /// Déclenché quand le joueur saisit la plaquette avec un controller.
    /// La détache du mur si elle y était fixée.
    /// </summary>
    void OnSaisie(SelectEnterEventArgs args)
    {
    _estGrabbee = true;
    if (_fixeeAuMur)
    {
        _fixeeAuMur     = false;
        _rb.isKinematic = false;
        _rb.useGravity  = true;
    }
    }

    /// <summary>
    /// Déclenché quand le joueur relâche la plaquette.
    /// La physique reprend normalement (elle tombe si dans le vide).
    /// </summary>

        void OnRelache(SelectExitEventArgs args)
        {
            _estGrabbee = false;
        }
        // Rien de spécial ici pour l'instant.
        // La zone de dépôt (étape 3) gèrera la suite via OnDeposeCorrectement() ou OnErreur().
    
    public bool EstEnCoursDeGrab() => _estGrabbee;
    // ─────────────────────────────────────────
    // FEEDBACK — appelés depuis ZoneDepot.cs (étape 3)
    // ─────────────────────────────────────────

    /// <summary>
    /// Appelé par la zone de dépôt quand la plaquette est posée au bon endroit.
    /// La plaquette se verrouille en place et passe au vert.
    /// </summary>
    public void OnDeposeCorrectement()
    {
        _enErreur       = false;
        _estDeposee     = true;
        _rb.isKinematic = true;
        _rb.useGravity  = false;
        _grab.enabled   = false;

        // Désactive le collider pour ne plus bloquer les autres plaquettes
        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        SetCouleur(couleurValidee);
    }

    /// <summary>
    /// Appelé par la zone de dépôt quand une plaquette négative est posée par erreur.
    /// Passe au rouge — le joueur doit la jeter à la poubelle.
    /// </summary>
    public void OnErreur()
    {
        _enErreur = true;
        SetCouleur(couleurErreur);
    }

    /// <summary>
    /// Remet la plaquette en couleur neutre (après qu'elle a été jetée à la poubelle).
    /// </summary>
    public void ResetVisuel()
    {
        _enErreur = false; 
        SetCouleur(couleurNormale);
    }

    // ─────────────────────────────────────────
    // UTILITAIRES — accesseurs pour les autres scripts
    // ─────────────────────────────────────────

    /// <summary>True si c'est une plaquette positive (bonne réponse).</summary>
    public bool EstPositive() => data != null && data.isPositive;

    /// <summary>Retourne la zone cible attendue (Cerveau, Coeur, Poubelle).</summary>
    public ZoneCible GetZoneCible() => data != null ? data.zoneCible : ZoneCible.Poubelle;

    /// <summary>True si la plaquette a déjà été correctement déposée.</summary>
    public bool EstDeposee() => _estDeposee;

    // ─────────────────────────────────────────
    // NETTOYAGE
    // ─────────────────────────────────────────
    void OnDestroy()
    {
        // Désabonnement propre pour éviter les memory leaks
        if (_grab != null)
        {
            _grab.selectEntered.RemoveListener(OnSaisie);
            _grab.selectExited.RemoveListener(OnRelache);
        }
    }

    // ─────────────────────────────────────────
    // INTERNE
    // ─────────────────────────────────────────
    void SetCouleur(Color c)
    {
        if (cardRenderer != null)
            cardRenderer.material.color = c;
    }
}