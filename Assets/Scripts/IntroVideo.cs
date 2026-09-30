using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Vidéo d'introduction : se lance au démarrage du jeu, puis le GameObject
/// est détruit une fois la vidéo terminée (ou en cas d'erreur de lecture).
/// Ne se rejoue pas quand on revient dans la scène Main pendant la même session.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class IntroVideo : MonoBehaviour
{
    static bool _dejaVue;

    VideoPlayer _videoPlayer;

    // Remise à zéro quand le rechargement de domaine est désactivé dans l'éditeur
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ReinitialiserEtat() => _dejaVue = false;

    void Awake()
    {
        if (_dejaVue)
        {
            Destroy(gameObject);
            return;
        }

        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.isLooping = false;
        _videoPlayer.loopPointReached += _ => Terminer();
        _videoPlayer.errorReceived += (_, message) =>
        {
            Debug.LogWarning($"[IntroVideo] Erreur de lecture : {message}");
            Terminer();
        };
    }

    void Start()
    {
        if (!_videoPlayer.isPlaying)
            _videoPlayer.Play();
    }

    void Terminer()
    {
        _dejaVue = true;
        Destroy(gameObject);
    }
}
