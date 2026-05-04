using UnityEngine;

// Ce décorateur permet de créer des assets depuis le menu Unity
[CreateAssetMenu(fileName = "NewPlaquette", menuName = "SeriousGame/Plaquette")]
public class PlaquetteData : ScriptableObject
{
    [Header("Contenu")]
    public string label;           // Ex: "Détente"
    public bool isPositive;        // true = bonne plaquette, false = erreur

    [Header("Zone cible")]
    public ZoneCible zoneCible;    // Cerveau, Coeur, ou Poubelle

    [Header("Visuel (optionnel)")]
    public Color couleurLabel = Color.white;
}

// Les zones où on peut déposer une plaquette
public enum ZoneCible
{
    Cerveau,
    Coeur,
    Poubelle
}