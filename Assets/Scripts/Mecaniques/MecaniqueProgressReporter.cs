using UnityEngine;

/// <summary>
/// Rapporte la progression de la salle Mécanique au ProgressManager.
///
/// Critères :
///   1 étoile  — Séquence de plaquettes complétée
///   2 étoiles — 1 vidéo regardée jusqu'à la fin
///   3 étoiles — 2 vidéos regardées jusqu'à la fin
///
/// Wiring : assignez les deux TabletteVideo dans l'inspector.
/// GameManager.OnSequenceCompleteEvent → MecaniqueProgressReporter.OnPlaquettesComplete
/// </summary>
public class MecaniqueProgressReporter : MonoBehaviour
{
    [SerializeField] TabletteVideo _tablette1;
    [SerializeField] TabletteVideo _tablette2;

    int _videosFinies = 0;

    void Start()
    {
        EnsureProgressManager();

        if (_tablette1 != null) _tablette1.OnVideoCompleted.AddListener(OnVideoFinie);
        if (_tablette2 != null) _tablette2.OnVideoCompleted.AddListener(OnVideoFinie);

        // Restaurer l'état persiste (si déjà joué)
        int stars = ProgressManager.Instance.GetStars(RoomId.Mecanique);
        if (stars >= 2) _videosFinies = Mathf.Max(_videosFinies, stars - 1);
    }

    void OnDestroy()
    {
        if (_tablette1 != null) _tablette1.OnVideoCompleted.RemoveListener(OnVideoFinie);
        if (_tablette2 != null) _tablette2.OnVideoCompleted.RemoveListener(OnVideoFinie);
    }

    // Appelé par GameManager.OnSequenceCompleteEvent (wirer dans l'Inspector)
    public void OnPlaquettesComplete()
    {
        ProgressManager.Instance.SetStars(RoomId.Mecanique, 1);
    }

    void OnVideoFinie()
    {
        _videosFinies++;
        int stars = _videosFinies >= 2 ? 3 : 2;
        ProgressManager.Instance.SetStars(RoomId.Mecanique, stars);
    }

    static void EnsureProgressManager()
    {
        if (ProgressManager.Instance == null)
            new GameObject("ProgressManager").AddComponent<ProgressManager>();
    }
}
