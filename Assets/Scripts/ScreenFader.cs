using System;
using System.Collections;
using UnityEngine;

// À placer sur le Quad noir devant la main caméra du XR Origin.
// Pilote l'alpha du matériau (ScreenFadeBlack, shader URP Unlit transparent) via un
// MaterialPropertyBlock pour ne pas dupliquer le matériau. Utilisé pour le fondu au noir
// lors des changements de scène (SceneChanger) et des téléportations XR (TeleportFadeController).
[DisallowMultipleComponent]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Tooltip("Renderer du Quad noir. Laisser vide pour utiliser le Renderer sur ce GameObject.")]
    [SerializeField] private Renderer targetRenderer;

    [SerializeField] private float defaultFadeDuration = 0.25f;

    [Tooltip("Si coché, l'écran démarre noir puis s'éclaircit automatiquement au lancement de la scène.")]
    [SerializeField] private bool fadeInOnStart = true;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private MaterialPropertyBlock block;
    private Coroutine fadeRoutine;
    private float currentAlpha;

    private void Awake()
    {
        Instance = this;
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            SetAlpha(1f);
            FadeIn(defaultFadeDuration);
        }
    }

    public void SetAlpha(float alpha)
    {
        currentAlpha = Mathf.Clamp01(alpha);
        targetRenderer.GetPropertyBlock(block);
        Color color = Color.black;
        color.a = currentAlpha;
        block.SetColor(BaseColorId, color);
        targetRenderer.SetPropertyBlock(block);
    }

    public Coroutine FadeOut(float duration = -1f, Action onComplete = null) =>
        StartFade(1f, duration < 0f ? defaultFadeDuration : duration, onComplete);

    public Coroutine FadeIn(float duration = -1f, Action onComplete = null) =>
        StartFade(0f, duration < 0f ? defaultFadeDuration : duration, onComplete);

    private Coroutine StartFade(float target, float duration, Action onComplete)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(target, duration, onComplete));
        return fadeRoutine;
    }

    private IEnumerator FadeRoutine(float target, float duration, Action onComplete)
    {
        float start = currentAlpha;
        if (duration <= 0f)
        {
            SetAlpha(target);
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(start, target, elapsed / duration));
                yield return null;
            }
            SetAlpha(target);
        }
        fadeRoutine = null;
        onComplete?.Invoke();
    }
}
