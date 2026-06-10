using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rapporte la progression d'une salle Anatomie (F ou M) au ProgressManager.
///
/// Critères :
///   1 étoile  — Au moins 1 objet analysé avec l'analyseur
///   2 étoiles — Tous les objets analysés
///   3 étoiles — Tous les objets analysés ET scène de visite complétée
///
/// Wiring : assignez roomId (AnatomieF ou AnatomieM) et visitKey ("Vagina" ou "Penis").
/// Les AnalyzerSocket de la scène sont détectés automatiquement.
/// </summary>
public class AnatomieProgressReporter : MonoBehaviour
{
    [SerializeField] RoomId _roomId = RoomId.AnatomieF;

    [Tooltip("\"Vagina\" pour AnatomieF, \"Penis\" pour AnatomieM")]
    [SerializeField] string _visitKey = "Vagina";

    int _totalAnalysables;
    string _setKey;

    void Start()
    {
        EnsureProgressManager();

        _setKey = _roomId + "_Analyzed";

        // Compter tous les AnalysableObject dans la scène
        _totalAnalysables = FindObjectsByType<AnalysableObject>(FindObjectsSortMode.None).Length;

        // Écouter les sockets
        foreach (var socket in FindObjectsByType<AnalyzerSocket>(FindObjectsSortMode.None))
            socket.OnObjectAnalyzed.AddListener(OnObjectAnalyzed);

        // Rafraîchir au cas où une progression existe déjà
        Recalculate();
    }

    void OnDestroy()
    {
        foreach (var socket in FindObjectsByType<AnalyzerSocket>(FindObjectsSortMode.None))
            if (socket != null) socket.OnObjectAnalyzed.RemoveListener(OnObjectAnalyzed);
    }

    void OnObjectAnalyzed(string objectId)
    {
        ProgressManager.Instance.AddToSet(_setKey, objectId);
        Recalculate();
    }

    void Recalculate()
    {
        if (ProgressManager.Instance == null) return;

        HashSet<string> analyzed = ProgressManager.Instance.GetSet(_setKey);
        bool visitDone = ProgressManager.Instance.GetVisited(_visitKey);

        int stars;
        if (analyzed.Count >= _totalAnalysables && _totalAnalysables > 0 && visitDone)
            stars = 3;
        else if (analyzed.Count >= _totalAnalysables && _totalAnalysables > 0)
            stars = 2;
        else if (analyzed.Count > 0)
            stars = 1;
        else
            stars = 0;

        ProgressManager.Instance.SetStars(_roomId, stars);
    }

    static void EnsureProgressManager()
    {
        if (ProgressManager.Instance == null)
            new GameObject("ProgressManager").AddComponent<ProgressManager>();
    }
}
