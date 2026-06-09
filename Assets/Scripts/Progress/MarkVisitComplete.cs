using UnityEngine;

/// <summary>
/// À placer dans les scènes de visite (Vagina, Penis…).
/// Enregistre la visite dans ProgressManager dès le chargement de la scène.
/// Configurez visitKey avec "Vagina" ou "Penis" selon la scène.
/// </summary>
public class MarkVisitComplete : MonoBehaviour
{
    [Tooltip("Clé de visite : \"Vagina\" pour AnatomieF, \"Penis\" pour AnatomieM.")]
    [SerializeField] string visitKey;

    void Start()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.SetVisited(visitKey);
        else
            // Fallback si ProgressManager n'existe pas encore (entrée directe dans la scène)
            PlayerPrefs.SetInt("Visited_" + visitKey, 1);
    }
}
