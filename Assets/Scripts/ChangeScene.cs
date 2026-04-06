using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneName = "Main";
    [SerializeField] private string playerTag = "Player";
    private bool hasLoaded = false;

    // Appelé par le bouton XR
    public void LoadScene()
    {
        if (hasLoaded) return;
        hasLoaded = true;
        SceneManager.LoadScene(sceneName);
    }

    // Appelé automatiquement quand un objet entre dans le trigger
    private void OnTriggerEnter(Collider other)
    {
        if (hasLoaded) return;

        if (other.CompareTag(playerTag))
        {
            hasLoaded = true;
            SceneManager.LoadScene(sceneName);
        }
    }
}