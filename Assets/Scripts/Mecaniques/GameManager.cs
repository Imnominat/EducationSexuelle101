using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton : accessible depuis n'importe quel script
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    public int nombrePlaquettesPositives = 5; // à ajuster selon ton contenu

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
        // Le bargraph sera géré ici à l'étape 5
    }

    // Appelé quand toutes les plaquettes positives sont placées
    void OnSequenceComplete()
    {
        Debug.Log("🎉 Séquence complète ! Déclenchement de l'animation finale.");
        // La séquence finale sera branchée ici à l'étape 6
    }
}