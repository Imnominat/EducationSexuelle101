using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// À placer sur chaque carte 3D de la scène (5 objets).
/// Reçoit les données d'une CardData et met à jour l'affichage.
/// Le ciblage et le signalement sont gérés par CardGameManager via le ray interactor.
/// </summary>
public class CardDisplay : MonoBehaviour
{
    [Header("Références UI (sur le Canvas de la carte)")]
    public TMP_Text cardTextField;
    public TMP_Text categoryTextField;
    public Image illustrationImage;
    public Image cardBackground;

    [Header("Couleurs par niveau")]
    public Color colorFavorable   = new Color(0.2f, 0.8f, 0.3f);   // vert
    public Color colorDefavorable = new Color(0.9f, 0.7f, 0.1f);   // orange
    public Color colorBloquant    = new Color(0.85f, 0.15f, 0.15f); // rouge

    [Header("Highlight ciblage")]
    [Tooltip("Renderer de la carte pour l'outline de ciblage.")]
    public Renderer cardRenderer;
    public Material defaultMaterial;
    public Material highlightMaterial;

    [Header("Indicateur de sélection négative")]
    [Tooltip("Objet affiché quand la carte est marquée comme négative par le joueur.")]
    public GameObject negativeMarker;

    // Données actuelles de la carte
    public CardData CurrentCard { get; private set; }
    public bool IsMarkedNegative { get; private set; }

    // ─────────────────────────────────────────────
    // Initialisation
    // ─────────────────────────────────────────────

    /// <summary>Appelé par CardGameManager pour assigner une carte à cet emplacement.</summary>
    public void SetCard(CardData data)
    {
        CurrentCard = data;
        IsMarkedNegative = false;

        if (negativeMarker != null)
            negativeMarker.SetActive(false);

        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (CurrentCard == null) return;

        if (cardTextField != null)
            cardTextField.text = CurrentCard.cardText;

        if (categoryTextField != null)
            categoryTextField.text = CurrentCard.category;

        if (illustrationImage != null)
        {
            illustrationImage.sprite = CurrentCard.illustration;
            illustrationImage.gameObject.SetActive(CurrentCard.illustration != null);
        }

        if (cardBackground != null)
        {
            cardBackground.color = CurrentCard.level switch
            {
                CardData.CardLevel.Favorable   => colorFavorable,
                CardData.CardLevel.Defavorable => colorDefavorable,
                CardData.CardLevel.Bloquant    => colorBloquant,
                _                              => Color.white
            };
        }
    }

    // ─────────────────────────────────────────────
    // Ciblage (appelé par CardGameManager)
    // ─────────────────────────────────────────────

    public void OnFocus()
    {
        if (cardRenderer != null && highlightMaterial != null)
            cardRenderer.material = highlightMaterial;
    }

    public void OnLoseFocus()
    {
        if (cardRenderer != null && defaultMaterial != null)
            cardRenderer.material = defaultMaterial;
    }

    // ─────────────────────────────────────────────
    // Signalement négatif (appelé par CardGameManager)
    // ─────────────────────────────────────────────

    public void ToggleNegativeMark()
    {
        IsMarkedNegative = !IsMarkedNegative;
        if (negativeMarker != null)
            negativeMarker.SetActive(IsMarkedNegative);
    }

    public void ClearNegativeMark()
    {
        IsMarkedNegative = false;
        if (negativeMarker != null)
            negativeMarker.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // Phase de correction — afficher/cacher la carte
    // ─────────────────────────────────────────────

    public void SetInteractable(bool interactable)
    {
        // Désactiver le collider pendant les phases non-interactables
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = interactable;
    }
}