using System;
using UnityEngine;

/// <summary>
/// Singleton persistant entre les scènes. Stocke la progression (étoiles 0-3) de chaque salle
/// via PlayerPrefs. Ne descend jamais en dessous de l'ancien score.
/// </summary>
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    /// <summary>Déclenché chaque fois qu'une salle change de score. (salle, nouvelles étoiles)</summary>
    public static event Action<RoomId, int> OnProgressChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Étoiles ───────────────────────────────────────────────────────────

    public int GetStars(RoomId room) => PlayerPrefs.GetInt("Stars_" + room, 0);

    /// <summary>Met à jour les étoiles si le nouveau score est supérieur.</summary>
    public void SetStars(RoomId room, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        if (stars <= GetStars(room)) return;
        PlayerPrefs.SetInt("Stars_" + room, stars);
        PlayerPrefs.Save();
        OnProgressChanged?.Invoke(room, stars);
    }

    // ── Flags de visite (scènes séparées : Vagina, Penis…) ───────────────

    public bool GetVisited(string key) => PlayerPrefs.GetInt("Visited_" + key, 0) == 1;

    public void SetVisited(string key)
    {
        if (GetVisited(key)) return;
        PlayerPrefs.SetInt("Visited_" + key, 1);
        PlayerPrefs.Save();
    }

    // ── Compteurs inter-sessions (objets analysés, etc.) ─────────────────

    public int GetCount(string key) => PlayerPrefs.GetInt("Count_" + key, 0);

    public void SetCount(string key, int value)
    {
        PlayerPrefs.SetInt("Count_" + key, value);
        PlayerPrefs.Save();
    }

    /// <summary>Stocke un ensemble d'IDs sous forme de chaîne CSV.</summary>
    public System.Collections.Generic.HashSet<string> GetSet(string key)
    {
        string csv = PlayerPrefs.GetString("Set_" + key, "");
        var set = new System.Collections.Generic.HashSet<string>();
        if (!string.IsNullOrEmpty(csv))
            foreach (var s in csv.Split(','))
                if (!string.IsNullOrEmpty(s)) set.Add(s);
        return set;
    }

    public void AddToSet(string key, string value)
    {
        var set = GetSet(key);
        if (!set.Add(value)) return;
        PlayerPrefs.SetString("Set_" + key, string.Join(",", set));
        PlayerPrefs.Save();
    }

    // ── Réinitialisation ─────────────────────────────────────────────────

    public void ResetAll()
    {
        foreach (RoomId room in Enum.GetValues(typeof(RoomId)))
        {
            PlayerPrefs.DeleteKey("Stars_" + room);
        }
        string[] visitKeys = { "Vagina", "Penis" };
        foreach (var k in visitKeys) PlayerPrefs.DeleteKey("Visited_" + k);

        string[] setKeys = { "AnatomieF_Analyzed", "AnatomieM_Analyzed", "Pharmacie_Dialogs" };
        foreach (var k in setKeys) PlayerPrefs.DeleteKey("Set_" + k);

        PlayerPrefs.Save();
    }
}
