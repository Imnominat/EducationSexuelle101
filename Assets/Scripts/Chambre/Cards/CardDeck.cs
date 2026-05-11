using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ScriptableObject contenant toutes les cartes du jeu.
/// Gère le tirage d'une main de 5 cartes sans doublon de catégorie.
/// Créer via clic droit > Create > CardGame > Card Deck
/// </summary>
[CreateAssetMenu(fileName = "CardDeck", menuName = "CardGame/Card Deck")]
public class CardDeck : ScriptableObject
{
    [Tooltip("Toutes les cartes disponibles dans le deck.")]
    public List<CardData> allCards = new List<CardData>();

    [Tooltip("Nombre de cartes dans une main.")]
    public int handSize = 5;

    /// <summary>
    /// Tire une main de <handSize> cartes sans doublon de catégorie.
    /// Garantit au minimum la présence de cartes de niveaux variés si possible.
    /// </summary>
    public List<CardData> DrawHand()
    {
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("[CardDeck] Le deck est vide !");
            return new List<CardData>();
        }

        List<CardData> hand = new List<CardData>();
        List<CardData> pool = new List<CardData>(allCards);
        HashSet<string> usedCategories = new HashSet<string>();

        // Mélanger le pool
        Shuffle(pool);

        // Piocher en respectant l'unicité des catégories
        foreach (CardData card in pool)
        {
            if (hand.Count >= handSize) break;

            if (!usedCategories.Contains(card.category))
            {
                hand.Add(card);
                usedCategories.Add(card.category);
            }
        }

        // Si on n'a pas assez de cartes (catégories trop restrictives),
        // compléter sans contrainte de catégorie
        if (hand.Count < handSize)
        {
            Debug.LogWarning("[CardDeck] Pas assez de catégories différentes pour remplir la main. " +
                             "Complétion sans contrainte de catégorie.");

            foreach (CardData card in pool)
            {
                if (hand.Count >= handSize) break;
                if (!hand.Contains(card))
                    hand.Add(card);
            }
        }

        return hand;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

#if UNITY_EDITOR
    /// <summary>Valide le deck dans l'éditeur (menu contextuel).</summary>
    [ContextMenu("Valider le deck")]
    private void ValidateDeck()
    {
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("Deck vide !");
            return;
        }

        var categories = allCards.GroupBy(c => c.category);
        Debug.Log($"[CardDeck] {allCards.Count} cartes, {categories.Count()} catégories.");

        foreach (var group in categories)
            Debug.Log($"  Catégorie '{group.Key}' : {group.Count()} carte(s)");

        int favorable  = allCards.Count(c => c.level == CardData.CardLevel.Favorable);
        int defavorable = allCards.Count(c => c.level == CardData.CardLevel.Defavorable);
        int bloquant   = allCards.Count(c => c.level == CardData.CardLevel.Bloquant);
        Debug.Log($"  Niveaux — Favorable: {favorable} | Défavorable: {defavorable} | Bloquant: {bloquant}");
    }
#endif
}