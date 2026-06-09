using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Tablette vidéo flottante : apparaît après la séquence des plaquettes.
/// Structure attendue dans la scène :
///   TabletteVideo (ce script + VideoPlayer + AudioSource)
///   ├── Ecran          (MeshRenderer — reçoit la RenderTexture)
///   └── Canvas         (World Space, TrackedDeviceGraphicRaycaster)
///       ├── BtnPlayPause   (Button)
///       ├── BtnReculer     (Button  → Reculer10s)
///       ├── BtnAvancer     (Button  → Avancer10s)
///       ├── Slider         (Slider  → onValueChanged vide ; EventTrigger PointerDown→DebutDrag, PointerUp→FinDrag)
///       └── TexteTemps     (TextMeshProUGUI)
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class TabletteVideo : MonoBehaviour
{
    [Header("Vidéo")]
    [SerializeField] VideoClip _clip;

    [Header("Écran")]
    [SerializeField] Renderer _ecranRenderer;
    [SerializeField] int _rtLargeur = 1920;
    [SerializeField] int _rtHauteur = 1080;

    [Header("Contrôles UI")]
    [SerializeField] Slider _sliderProgression;
    [SerializeField] TextMeshProUGUI _texteTemps;
    [SerializeField] Image _iconePlayPause;
    [SerializeField] Sprite _spritePlay;
    [SerializeField] Sprite _spritePause;

    [Header("Apparition")]
    [SerializeField] float _dureeApparition = 0.5f;

    [Header("Événements")]
    public UnityEvent OnVideoCompleted;

    VideoPlayer _videoPlayer;
    RenderTexture _renderTexture;
    bool _enLecture;
    bool _dragActif;

    void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.clip = _clip;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.isLooping = false;
        _videoPlayer.loopPointReached += _ => OnVideoCompleted?.Invoke();
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        var audio = GetComponent<AudioSource>();
        if (audio != null)
            _videoPlayer.SetTargetAudioSource(0, audio);

        _renderTexture = new RenderTexture(_rtLargeur, _rtHauteur, 0);
        _renderTexture.Create();
        _videoPlayer.targetTexture = _renderTexture;

        if (_ecranRenderer != null)
        {
            var mat = new Material(Shader.Find("Unlit/Texture"));
            mat.mainTexture = _renderTexture;
            // Le VideoPlayer écrit avec l'origine en bas-gauche ;
            // on inverse l'axe V pour remettre l'image à l'endroit.
            mat.mainTextureScale  = new Vector2(1f, -1f);
            mat.mainTextureOffset = new Vector2(0f,  1f);
            _ecranRenderer.material = mat;
        }
    }

    void Update()
    {
        if (_sliderProgression == null) return;

        if (!_dragActif && _videoPlayer.frameCount > 0)
        {
            float progression = (float)_videoPlayer.frame / _videoPlayer.frameCount;
            _sliderProgression.SetValueWithoutNotify(progression);
        }

        if (_texteTemps != null)
        {
            var actuel = System.TimeSpan.FromSeconds(_videoPlayer.time);
            var total  = System.TimeSpan.FromSeconds(_videoPlayer.length);
            _texteTemps.text = $"{actuel.Minutes:D2}:{actuel.Seconds:D2} / {total.Minutes:D2}:{total.Seconds:D2}";
        }
    }

    // ── Contrôles (wirés dans l'Inspector) ───────────────────────────────

    public void BasculerLecture()
    {
        if (_enLecture) Pause();
        else            Lire();
    }

    public void Lire()
    {
        _enLecture = true;
        _videoPlayer.Play();
        if (_iconePlayPause != null && _spritePause != null)
            _iconePlayPause.sprite = _spritePause;
    }

    public void Pause()
    {
        _enLecture = false;
        _videoPlayer.Pause();
        if (_iconePlayPause != null && _spritePlay != null)
            _iconePlayPause.sprite = _spritePlay;
    }

    public void Reculer10s() => SauterDansVideo(-10f);
    public void Avancer10s() => SauterDansVideo(10f);

    void SauterDansVideo(float secondes)
    {
        _videoPlayer.time = System.Math.Max(0,
            System.Math.Min(_videoPlayer.length, _videoPlayer.time + secondes));
    }

    // ── Slider drag (wirés via EventTrigger sur le Slider) ───────────────

    public void DebutDrag()
    {
        _dragActif = true;
        _videoPlayer.Pause();
    }

    public void FinDrag()
    {
        _dragActif = false;
        if (_sliderProgression != null)
            _videoPlayer.frame = (long)(_sliderProgression.value * _videoPlayer.frameCount);
        if (_enLecture) _videoPlayer.Play();
    }

    // ── Apparition ───────────────────────────────────────────────────────

    public void Apparaitre()
    {
        gameObject.SetActive(true);
        StartCoroutine(AnimerApparition());
    }

    IEnumerator AnimerApparition()
    {
        transform.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < _dureeApparition)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / _dureeApparition);
            transform.localScale = Vector3.one * t;
            yield return null;
        }
        transform.localScale = Vector3.one;

        // Charger le premier frame sans lancer la lecture
        _videoPlayer.Play();
        yield return null;
        _videoPlayer.Pause();
        _enLecture = false;
        if (_iconePlayPause != null && _spritePlay != null)
            _iconePlayPause.sprite = _spritePlay;
    }

    void OnDestroy()
    {
        if (_renderTexture != null)
            _renderTexture.Release();
    }
}
