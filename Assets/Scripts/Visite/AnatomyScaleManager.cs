using UnityEngine;
using System.Collections;

public class AnatomyScaleManager : MonoBehaviour
{
    [Header("References")]
    public Transform xrRig;
    public Transform anatomyModel;
    public Camera xrCamera;

    [Header("Scale Settings")]
    public float normalScale = 1f;
    public float microScale = 1f / 300f;
    public float transitionDuration = 3f;

    [Header("Entry Point")]
    public Transform entryPoint; // positionné à la vulve

    private bool isMicro = false;

    public void EnterMicroMode()
    {
        if (!isMicro)
            StartCoroutine(ScaleTransition(normalScale, microScale, entryPoint.position));
    }

    public void ExitMicroMode()
    {
        if (isMicro)
            StartCoroutine(ScaleTransition(microScale, normalScale, entryPoint.position));
    }

    private IEnumerator ScaleTransition(float from, float to, Vector3 pivot)
    {
        float elapsed = 0f;
        Vector3 initialPos = xrRig.position;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / transitionDuration);

            float currentScale = Mathf.Lerp(from, to, t);
            xrRig.localScale = Vector3.one * currentScale;

            // Garder le pivot au bon endroit
            xrRig.position = pivot - (xrCamera.transform.position - xrRig.position);

            yield return null;
        }

        xrRig.localScale = Vector3.one * to;
        isMicro = (to == microScale);
    }
}