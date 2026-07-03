using UnityEngine;
using UnityEngine.SceneManagement;

// Placer ce script sur un GameObject avec un Collider (isTrigger = true).
// Assigner exitCanvas dans l'Inspector — le canvas sera activé/désactivé automatiquement.
// Le bouton "Sortir" du canvas doit appeler ExitTrigger.ReloadScene() via UnityEvent.
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    [Tooltip("Canvas (ou panel) à afficher quand le joueur entre dans la zone.")]
    public GameObject exitCanvas;

    [Tooltip("Tag du collider joueur à détecter.")]
    public string playerTag = "Player";

    [Tooltip("Scène à charger quand le joueur appuie sur Sortir.")]
    public string sceneToLoad = "AnatomieF";

    // Devient vrai la première fois que le joueur quitte physiquement la zone d'entrée.
    // Empêche le canvas de s'afficher au spawn (le pivot anatomique recentre "Sortie"
    // pile sur le joueur dès l'activation de la visite, ce qui déclenchait OnTriggerEnter
    // instantanément et exposait le bouton "Sortir" aux manettes sans action du joueur).
    private bool hasLeftEntrance = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (exitCanvas != null) exitCanvas.SetActive(false);
        hasLeftEntrance = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!hasLeftEntrance) return;
        if (exitCanvas != null) exitCanvas.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        hasLeftEntrance = true;
        if (exitCanvas != null) exitCanvas.SetActive(false);
    }

    // Appeler cette méthode depuis le bouton "Sortir" du canvas.
    public void ReloadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
