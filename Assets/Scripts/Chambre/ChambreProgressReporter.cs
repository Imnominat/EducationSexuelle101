using UnityEngine;

/// <summary>
/// Rapporte la progression de la salle Chambre au ProgressManager.
///
/// Critères :
///   1 étoile  — Au moins 1 bloqueur supprimé
///   2 étoiles — Plus de 50 % des bloqueurs supprimés
///   3 étoiles — Tous les bloqueurs supprimés
///
/// Nécessite ChambreGameManager sur la scène.
/// Abonne-toi à ChambreGameManager.OnBlockerDestroyed dans Start().
/// </summary>
public class ChambreProgressReporter : MonoBehaviour
{
    void Start()
    {
        EnsureProgressManager();
        ChambreGameManager.OnBlockerDestroyed += OnBlockerDestroyed;
    }

    void OnDestroy() => ChambreGameManager.OnBlockerDestroyed -= OnBlockerDestroyed;

    void OnBlockerDestroyed(int remaining, int total)
    {
        if (total <= 0) return;
        int removed = total - remaining;

        int stars;
        if (remaining <= 0)
            stars = 3;
        else if (removed > total / 2)
            stars = 2;
        else
            stars = 1;

        ProgressManager.Instance.SetStars(RoomId.Chambre, stars);
    }

    static void EnsureProgressManager()
    {
        if (ProgressManager.Instance == null)
            new GameObject("ProgressManager").AddComponent<ProgressManager>();
    }
}
