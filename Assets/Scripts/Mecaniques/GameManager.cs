using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // Singleton : accessible depuis n'importe quel script
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    public int nombrePlaquettesPositives = 5;

    [Header("Événements")]
    public UnityEvent OnSequenceCompleteEvent;

    private int _plaquettesValidees = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Appelé par ZoneDepot quand une bonne plaquette est posée
    public void OnBonnePlaquette()
    {
        _plaquettesValidees++;
        Debug.Log($"✅ Plaquettes validées : {_plaquettesValidees} / {nombrePlaquettesPositives}");

        if (_plaquettesValidees >= nombrePlaquettesPositives)
            OnSequenceComplete();
    }

    // Appelé par ZoneDepot quand une mauvaise plaquette est posée
    public void OnMauvaisePlaquette(Plaquette p)
    {
        Debug.Log($"❌ Mauvaise plaquette : {p.data.label}");
    }

    void OnSequenceComplete()
    {
        Debug.Log("🎉 Séquence complète ! Apparition des tablettes vidéo.");
        OnSequenceCompleteEvent?.Invoke();
    }
}