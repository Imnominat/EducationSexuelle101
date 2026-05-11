using UnityEngine;

/// <summary>
/// ScriptableObject définissant une carte du jeu.
/// Créer via clic droit > Create > CardGame > Card Data
/// </summary>
[CreateAssetMenu(fileName = "NewCard", menuName = "CardGame/Card Data")]
public class CardData : ScriptableObject
{
    public enum CardLevel
    {
        Favorable,    // Positif — facilite la relation
        Defavorable,  // Négatif modéré — frein mais pas rédhibitoire
        Bloquant      // Rédhibitoire — invalide la combinaison à lui seul
    }

    [Header("Identité")]
    [Tooltip("Texte court affiché sur la face de la carte.")]
    public string cardText = "Nom de la situation";

    [Tooltip("Catégorie de la carte — une seule carte par catégorie dans la main.")]
    public string category = "Consentement";

    [Tooltip("Niveau de la carte.")]
    public CardLevel level = CardLevel.Favorable;

    [Header("Contenu pédagogique")]
    [Tooltip("Explication affichée lors de la phase de correction pour les cartes Défavorable/Bloquant.")]
    [TextArea(3, 6)]
    public string explanation = "Explication pédagogique de pourquoi cette carte impacte la relation.";

    [Header("Visuel (optionnel)")]
    [Tooltip("Image illustrant la carte.")]
    public Sprite illustration;

    [Tooltip("Couleur de fond selon le niveau — peut être assignée automatiquement.")]
    public Color cardColor = Color.white;
}