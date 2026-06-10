using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton persistant entre les scènes (DontDestroyOnLoad).
/// Toute la progression est en mémoire vive : fresh start à chaque lancement.
/// </summary>
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    /// <summary>Déclenché chaque fois qu'une salle change de score. (salle, nouvelles étoiles)</summary>
    public static event Action<RoomId, int> OnProgressChanged;

    // ── Stockage en mémoire ───────────────────────────────────────────────
    readonly Dictionary<RoomId, int>              _stars   = new();
    readonly Dictionary<string, bool>             _visited = new();
    readonly Dictionary<string, HashSet<string>>  _sets    = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Étoiles ───────────────────────────────────────────────────────────

    public int GetStars(RoomId room) =>
        _stars.TryGetValue(room, out int v) ? v : 0;

    /// <summary>Met à jour les étoiles seulement si le nouveau score est supérieur.</summary>
    public void SetStars(RoomId room, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        if (stars <= GetStars(room)) return;
        _stars[room] = stars;
        OnProgressChanged?.Invoke(room, stars);
    }

    // ── Flags de visite (scènes séparées : Vagina, Penis…) ───────────────

    public bool GetVisited(string key) =>
        _visited.TryGetValue(key, out bool v) && v;

    public void SetVisited(string key)
    {
        if (GetVisited(key)) return;
        _visited[key] = true;
    }

    // ── Ensembles d'IDs (objets analysés, dialogues atteints…) ───────────

    public HashSet<string> GetSet(string key)
    {
        if (!_sets.TryGetValue(key, out var set))
        {
            set = new HashSet<string>();
            _sets[key] = set;
        }
        return set;
    }

    public void AddToSet(string key, string value) => GetSet(key).Add(value);
}
