using UnityEngine;

/// <summary>
/// Rapporte la progression de la salle Chambre au ProgressManager.
///
/// Critères (phase 1, tri des bloqueurs) :
///   1 étoile  — Au moins 1 bloqueur supprimé
///   2 étoiles — Plus de 50 % des bloqueurs supprimés
///   3 étoiles — Tous les bloqueurs supprimés
///
/// Critères (phase 2, tri des non-bloqueurs) : même logique, mais rapportée au
/// nombre total d'objets (bloqueurs + non-bloqueurs) de la salle.
///
/// Nécessite ChambreGameManager sur la scène.
/// Abonne-toi à ChambreGameManager.OnBlockerDestroyed / OnNonBlockerDestroyed dans Start().
/// </summary>
public class ChambreProgressReporter : MonoBehaviour
{
    void Start()
    {
        EnsureProgressManager();
        ChambreGameManager.OnBlockerDestroyed += OnBlockerDestroyed;
        ChambreGameManager.OnNonBlockerDestroyed += OnNonBlockerDestroyed;
    }

    void OnDestroy()
    {
        ChambreGameManager.OnBlockerDestroyed -= OnBlockerDestroyed;
        ChambreGameManager.OnNonBlockerDestroyed -= OnNonBlockerDestroyed;
    }

    void OnBlockerDestroyed(int remaining, int total)
    {
        if (total <= 0) return;
        ProgressManager.Instance.SetStars(RoomId.Chambre, ComputeStars(remaining, total));
    }

    void OnNonBlockerDestroyed(int remainingNonBlockers, int totalNonBlockers)
    {
        int totalObjects = ChambreGameManager.Instance.totalBlockers + totalNonBlockers;
        if (totalObjects <= 0) return;

        int remainingObjects = remainingNonBlockers; // les bloqueurs sont déjà tous à 0
        ProgressManager.Instance.SetStars(RoomId.Chambre, ComputeStars(remainingObjects, totalObjects));
    }

    static int ComputeStars(int remaining, int total)
    {
        int removed = total - remaining;

        if (remaining <= 0)
            return 3;
        if (removed > total / 2)
            return 2;
        return 1;
    }

    static void EnsureProgressManager()
    {
        if (ProgressManager.Instance == null)
            new GameObject("ProgressManager").AddComponent<ProgressManager>();
    }
}
