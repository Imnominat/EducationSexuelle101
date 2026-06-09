using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rapporte la progression de la salle Pharmacie au ProgressManager.
///
/// Critères :
///   1 étoile  — Au moins 1/3 des dialogues clés atteints
///   2 étoiles — Au moins 2/3 des dialogues clés atteints
///   3 étoiles — Tous les dialogues clés atteints
///
/// Wiring : renseignez _requiredDialogIds avec les IDs des dialogues importants.
/// DialogUI.OnDialogShown (event statique) est écouté automatiquement.
/// </summary>
public class PharmacieProgressReporter : MonoBehaviour
{
    [Tooltip("IDs des dialogues dont l'atteinte est requise pour valider la salle.")]
    [SerializeField] List<string> _requiredDialogIds = new List<string>();

    const string SetKey = "Pharmacie_Dialogs";

    void Start()
    {
        EnsureProgressManager();
        Dialogs.DialogUI.OnDialogShown += OnDialogShown;
        Recalculate();
    }

    void OnDestroy() => Dialogs.DialogUI.OnDialogShown -= OnDialogShown;

    void OnDialogShown(string dialogId)
    {
        if (_requiredDialogIds.Contains(dialogId))
        {
            ProgressManager.Instance.AddToSet(SetKey, dialogId);
            Recalculate();
        }
    }

    void Recalculate()
    {
        if (ProgressManager.Instance == null || _requiredDialogIds.Count == 0) return;

        HashSet<string> reached = ProgressManager.Instance.GetSet(SetKey);
        int count = 0;
        foreach (var id in _requiredDialogIds)
            if (reached.Contains(id)) count++;

        int total = _requiredDialogIds.Count;
        int stars;
        if (count >= total)
            stars = 3;
        else if (count * 3 >= total * 2) // >= 2/3
            stars = 2;
        else if (count > 0)
            stars = 1;
        else
            stars = 0;

        ProgressManager.Instance.SetStars(RoomId.Pharmacie, stars);
    }

    static void EnsureProgressManager()
    {
        if (ProgressManager.Instance == null)
            new GameObject("ProgressManager").AddComponent<ProgressManager>();
    }
}
