using UnityEngine;

/// <summary>
/// À placer sur chaque objet XR Grabbable que l'analyseur peut scanner.
/// Contient les métadonnées affichées sur l'écran.
/// </summary>
public class AnalysableObject : MonoBehaviour
{
    [Header("Données affichées sur l'écran de l'analyseur")]
    [Tooltip("Titre court de l'objet (ex: 'Cristal d'Énergie')")]
    public string objectTitle = "Objet inconnu";

    [TextArea(3, 8)]
    [Tooltip("Description longue affichée dans la zone de texte description")]
    public string objectDescription = "Aucune information disponible pour cet objet.";
}