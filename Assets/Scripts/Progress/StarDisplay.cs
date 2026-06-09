using UnityEngine;

/// <summary>
/// Affiche 0 à 3 étoiles pour une salle donnée.
/// Fonctionne en UI Canvas (Image) ou en 3D (SpriteRenderer / MeshRenderer via matière).
/// Assignez 3 GameObjects dans _starObjects : ils sont activés/désactivés selon le score.
/// </summary>
public class StarDisplay : MonoBehaviour
{
    [SerializeField] RoomId _room;

    [Tooltip("Trois GameObjects représentant les étoiles (index 0 = 1re étoile).")]
    [SerializeField] GameObject[] _starObjects = new GameObject[3];

    void OnEnable()
    {
        ProgressManager.OnProgressChanged += OnProgressChanged;
        Refresh();
    }

    void OnDisable() => ProgressManager.OnProgressChanged -= OnProgressChanged;

    void OnProgressChanged(RoomId room, int _)
    {
        if (room == _room) Refresh();
    }

    public void Refresh()
    {
        int stars = ProgressManager.Instance != null ? ProgressManager.Instance.GetStars(_room) : 0;
        for (int i = 0; i < _starObjects.Length; i++)
            if (_starObjects[i] != null)
                _starObjects[i].SetActive(i < stars);
    }
}
