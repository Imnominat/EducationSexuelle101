using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Reproduit le geste de téléportation XRI ("lever le joystick" → courbe + cible → relâcher
// pour confirmer), mais restreint la destination aux waypoints de SplineNavigator marqués
// isTeleportStop. Ne raycast jamais sur le mesh de l'organe : la cible est toujours un
// waypoint existant, jamais un point de la géométrie.
//
// Assigner teleportModeActivate / teleportModeCancel sur les actions "Teleport Mode" /
// "Teleport Mode Cancel" de XRI Default Input Actions (même main que ce contrôleur), pour
// garder exactement le même geste que le reste du projet (Chambre.unity).
public class WaypointTeleportAim : MonoBehaviour
{
    [Header("Navigation")]
    public SplineNavigator splineNavigator;

    [Header("Input — mêmes actions que la téléportation XRI standard")]
    public InputActionReference teleportModeActivate;
    public InputActionReference teleportModeCancel;

    [Header("Visuel de visée")]
    public Transform rayOrigin;
    public LineRenderer lineRenderer;
    public Transform reticle;

    [Header("Confort (optionnel)")]
    [Tooltip("Laisser vide pour un saut instantané sans fondu.")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.15f;

    [Tooltip("Zone morte de l'axe du stick sous laquelle on garde la dernière direction visée.")]
    public float stickDeadzone = 0.2f;

    private InputAction stickAction;
    private bool aiming = false;
    private int aimDirection = 1;
    private int targetIndex = -1;
    private bool fading = false;

    void Awake()
    {
        stickAction = new InputAction("TeleportAimStick", InputActionType.Value,
            "<XRController>/{Primary2DAxis}", expectedControlType: "Vector2");
    }

    void OnEnable()
    {
        stickAction?.Enable();
        if (teleportModeActivate != null) teleportModeActivate.action.performed += OnActivate;
        if (teleportModeCancel != null) teleportModeCancel.action.performed += OnConfirm;
    }

    void OnDisable()
    {
        stickAction?.Disable();
        if (teleportModeActivate != null) teleportModeActivate.action.performed -= OnActivate;
        if (teleportModeCancel != null) teleportModeCancel.action.performed -= OnConfirm;
        StopAiming();
    }

    void OnDestroy() => stickAction?.Dispose();

    void OnActivate(InputAction.CallbackContext ctx)
    {
        if (splineNavigator == null || !splineNavigator.IsActive || fading) return;
        aiming = true;
    }

    void OnConfirm(InputAction.CallbackContext ctx)
    {
        if (!aiming) return;
        aiming = false;

        int index = targetIndex;
        HideAimVisuals();

        if (index >= 0 && splineNavigator != null && splineNavigator.IsActive)
            StartCoroutine(FadeAndJump(index));
    }

    void Update()
    {
        if (!aiming || splineNavigator == null || !splineNavigator.IsActive || fading)
        {
            if (targetIndex != -1 || (lineRenderer != null && lineRenderer.enabled))
                HideAimVisuals();
            return;
        }

        float stickY = stickAction != null ? stickAction.ReadValue<Vector2>().y : 0f;
        if (Mathf.Abs(stickY) > stickDeadzone)
        {
            float lookDot = Vector3.Dot(splineNavigator.playerAnchor.forward, Vector3.forward);
            float signedInput = stickY * (lookDot >= 0f ? 1f : -1f);
            aimDirection = signedInput >= 0f ? 1 : -1;
        }

        targetIndex = splineNavigator.GetNextStopIndex(splineNavigator.CurrentIndex, aimDirection);
        UpdateAimVisuals();
    }

    void UpdateAimVisuals()
    {
        bool valid = targetIndex >= 0;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = valid && rayOrigin != null;
            if (valid && rayOrigin != null)
            {
                Vector3 targetPos = TargetWorldPosition(targetIndex);
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, rayOrigin.position);
                lineRenderer.SetPosition(1, targetPos);
            }
        }

        if (reticle != null)
        {
            reticle.gameObject.SetActive(valid);
            if (valid) reticle.position = TargetWorldPosition(targetIndex);
        }
    }

    void HideAimVisuals()
    {
        targetIndex = -1;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (reticle != null) reticle.gameObject.SetActive(false);
    }

    void StopAiming()
    {
        aiming = false;
        HideAimVisuals();
    }

    Vector3 TargetWorldPosition(int index)
    {
        Transform wp = splineNavigator.waypoints[index];
        return splineNavigator.anatomyPivot.TransformPoint(wp.localPosition);
    }

    IEnumerator FadeAndJump(int index)
    {
        fading = true;

        if (fadeCanvasGroup != null)
        {
            yield return Fade(0f, 1f);
            splineNavigator.JumpTo(index);
            yield return Fade(1f, 0f);
        }
        else
        {
            splineNavigator.JumpTo(index);
        }

        fading = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = to;
    }
}
