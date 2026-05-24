using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnatomyScaleManager : MonoBehaviour
{
    [Header("Références")]
    public Transform xrRig;
    public Camera xrCamera;

    [Header("Échelle")]
    public float normalScale = 1f;
    public float microScale = 1f / 300f;
    public float transitionDuration = 3f;

    [Header("Debug clavier — désactiver en prod")]
    public bool keyboardTrigger = false;

    public bool IsTransitioning => isTransitioning;
    public bool IsMicro => isMicro;

    private bool isMicro = false;
    private bool isTransitioning = false;

    void Update()
    {
        if (!keyboardTrigger || isTransitioning) return;
        if (Keyboard.current.eKey.wasPressedThisFrame && !isMicro) EnterMicroMode();
        if (Keyboard.current.qKey.wasPressedThisFrame && isMicro)  ExitMicroMode();
    }

    public void EnterMicroMode()
    {
        if (!isMicro && !isTransitioning)
            StartCoroutine(Transition(normalScale, microScale));
    }

    public void ExitMicroMode()
    {
        if (isMicro && !isTransitioning)
            StartCoroutine(Transition(microScale, normalScale));
    }

    private IEnumerator Transition(float from, float to)
    {
        isTransitioning = true;
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            SetScale(Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / transitionDuration)));
            yield return null;
        }
        SetScale(to);
        isMicro = to < from;
        isTransitioning = false;
    }

    // Public : appelé aussi par SplineNavigator pour le scale dynamique pendant la visite
    public void SetScale(float scale)
    {
        Vector3 camBefore = xrCamera.transform.position;
        xrRig.localScale = Vector3.one * scale;
        xrRig.position += camBefore - xrCamera.transform.position;
    }
}
