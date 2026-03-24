using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Rotation de l'organe relative à la caméra, axes X et Y uniquement.
/// Le mouvement en profondeur (Z) est volontairement ignoré pour éviter
/// les rotations parasites qui désorienteraient l'objet.
/// </summary>
public class OrbitRotatable : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Sensibilité de la rotation.")]
    public float sensitivity = 180f;

    [Tooltip("Inertie après relâchement (0 = arrêt immédiat, 0.95 = glisse longtemps).")]
    [Range(0f, 0.99f)]
    public float inertiaDamping = 0.90f;

    [Header("Grab Zone")]
    [Tooltip("Rayon du SphereCollider de grab (mètres).")]
    public float grabRadius = 0.18f;

    // ── Références ────────────────────────────────────────────────────────────
    private XRSimpleInteractable _interactable;
    private IXRSelectInteractor  _currentInteractor;
    private Transform            _cameraTransform;

    // ── État du drag ──────────────────────────────────────────────────────────
    private bool    _isGrabbed;
    private Vector3 _prevHandPosition;

    // ── Inertie ───────────────────────────────────────────────────────────────
    private Vector3 _inertiaAxis;
    private float   _inertiaAngle;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        SetupCollider();
        SetupInteractable();
    }

    private void Start()
    {
        _cameraTransform = Camera.main != null ? Camera.main.transform : transform;
    }

    private void Update()
    {
        if (_isGrabbed)
            HandleRotation();
        else
            ApplyInertia();
    }

    // ────────────────────────────────────────────────────────────────────────
    #region Setup

    private void SetupCollider()
    {
        var col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = false;
        col.radius    = grabRadius;
    }

    private void SetupInteractable()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        if (_interactable == null)
            _interactable = gameObject.AddComponent<XRSimpleInteractable>();

        _interactable.selectMode = InteractableSelectMode.Single;
        _interactable.selectEntered.AddListener(OnGrabbed);
        _interactable.selectExited.AddListener(OnReleased);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────
    #region Grab Callbacks

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _currentInteractor = args.interactorObject;
        _isGrabbed         = true;
        _inertiaAngle      = 0f;
        _prevHandPosition  = GetHandPosition();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _isGrabbed         = false;
        _currentInteractor = null;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────
    #region Rotation

    private void HandleRotation()
    {
        if (_currentInteractor == null) return;

        Vector3 currentPos = GetHandPosition();
        Vector3 worldDelta = currentPos - _prevHandPosition;
        _prevHandPosition  = currentPos;

        if (worldDelta.sqrMagnitude < 0.000001f) return;

        // Projection sur les axes caméra droite et haut UNIQUEMENT
        // Le dot sur camForward (profondeur) est volontairement ignoré
        float horizontal = Vector3.Dot(worldDelta, _cameraTransform.right);
        float vertical   = Vector3.Dot(worldDelta, _cameraTransform.up);

        // horizontal → rotation autour de l'axe "haut caméra"
        // vertical   → rotation autour de l'axe "droite caméra"
        Vector3 rotAxis  = (_cameraTransform.up    * (-horizontal)
                          + _cameraTransform.right *   vertical);
        float   rotAngle = rotAxis.magnitude * sensitivity;

        if (rotAngle < 0.0001f) return;

        rotAxis.Normalize();
        transform.Rotate(rotAxis, rotAngle, Space.World);

        // Mémorisation pour l'inertie
        _inertiaAxis  = rotAxis;
        _inertiaAngle = rotAngle;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────
    #region Inertia

    private void ApplyInertia()
    {
        if (_inertiaAngle < 0.01f) return;

        transform.Rotate(_inertiaAxis, _inertiaAngle, Space.World);
        _inertiaAngle *= inertiaDamping;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────

    private Vector3 GetHandPosition()
    {
        return ((MonoBehaviour)_currentInteractor).transform.position;
    }
}