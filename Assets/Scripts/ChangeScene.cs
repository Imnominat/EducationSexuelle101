using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneName = "Main";
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Si coché, la scène Visite démarrera directement rétrécie au début du parcours SplineAnatomy.")]
    [SerializeField] private bool directEntry = false;

    [Tooltip("Durée du fondu au noir avant le changement de scène.")]
    [SerializeField] private float fadeOutDuration = 0.3f;

    private bool hasLoaded = false;

    // Appelé par le bouton XR
    public void LoadScene()
    {
        TriggerLoad();
    }

    // Appelé automatiquement quand un objet entre dans le trigger
    private void OnTriggerEnter(Collider other)
    {
        if (hasLoaded) return;

        if (other.CompareTag(playerTag))
            TriggerLoad();
    }

    private void TriggerLoad()
    {
        if (hasLoaded) return;
        hasLoaded = true;

        if (directEntry)
            PlayerPrefs.SetInt("VisiteDirectEntry", 1);

        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeOut(fadeOutDuration, () => SceneManager.LoadScene(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }
}