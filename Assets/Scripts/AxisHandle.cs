using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AxisHandle : MonoBehaviour
{
    [Header("Cible � faire tourner")]
    public Transform targetObject;

    [Header("Axe de cette poign�e (espace LOCAL de l'objet cible)")]
    public Vector3 localAxis = Vector3.right;

    [Header("Sensibilit�s")]
    public float slideSensitivity  = 150f;  // degrés/mètre de déplacement
    public float twistSensitivity  = 1.2f;  // multiplicateur du twist

    [Header("Contrainte : seuil de détection du twist (degrés)")]
    public float twistDeadzone = 8f;

    [Header("Inversion de la direction")]
    public bool reverseRotation = false;  // inverser les directions de rotation

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable _interactable;
    private Transform _controller;
    private bool _grabbed;

    // état au moment du grab
    private Vector3    _ctrlPosOnGrab;
    private Quaternion _ctrlRotOnGrab;
    private Quaternion _objRotOnGrab;

    // ---- axe en world space calculé une fois au grab ----
    private Vector3 _worldAxis;
    // ---- axes perpendiculaires (pour convertir slide - rotation) ----
    private Vector3 _perpA;
    private Vector3 _perpB;

    void Awake()
    {
        _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        _interactable.selectEntered.AddListener(OnGrab);
        _interactable.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        _interactable.selectEntered.RemoveListener(OnGrab);
        _interactable.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        _controller      = args.interactorObject.transform;
        _grabbed         = true;
        _ctrlPosOnGrab   = _controller.position;
        _ctrlRotOnGrab   = _controller.rotation;
        _objRotOnGrab    = targetObject.rotation;

        // Axe en world space figé au moment du grab
        _worldAxis = targetObject.TransformDirection(localAxis).normalized;

        // Construction d'une base orthonormée autour de l'axe
        // perpA et perpB définissent le "plan de glissement"
        Vector3 arbitrary = Mathf.Abs(Vector3.Dot(_worldAxis, Vector3.up)) < 0.95f
                            ? Vector3.up : Vector3.forward;
        _perpA = Vector3.Cross(_worldAxis, arbitrary).normalized;
        _perpB = Vector3.Cross(_worldAxis, _perpA).normalized;
        var constraint = GetComponent<SingleAxisConstraint>();
        constraint?.OnGrabBegin(_objRotOnGrab);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        _grabbed    = false;
        _controller = null;
    }


    void Update()
    {
        if (!_grabbed || _controller == null) return;

        // Axe en world space (recalculé depuis la rotation BASE, pas la rotation courante)
        // On utilise _objRotOnGrab * localAxis pour avoir l'axe tel qu'il était au grab
        Vector3 worldAxisAtGrab = (_objRotOnGrab * localAxis).normalized;

        // === SLIDE : déplacement du controller ===
        Vector3 delta = _controller.position - _ctrlPosOnGrab;

        // On projette le déplacement sur les axes perpendiculaires à l'axe de la poignée
        // perpA et perpB sont fixes (calculés au grab), donc pas de dérive
        float slideA = Vector3.Dot(delta, _perpA);
        float slideB = Vector3.Dot(delta, _perpB);

        // Le déplacement perpendiculaire à l'axe génère une rotation AUTOUR de l'axe
        // La magnitude du déplacement projeté dans le plan perp = amplitude de rotation
        // Direction : produit vectoriel du déplacement avec l'axe
        Vector3 projectedDelta = delta - Vector3.Dot(delta, worldAxisAtGrab) * worldAxisAtGrab;
        float slideMagnitude = projectedDelta.magnitude;

        // Signe : déterminé par le produit vectoriel entre le déplacement et l'axe
        Vector3 cross = Vector3.Cross(projectedDelta.normalized, worldAxisAtGrab);
        float sign = Mathf.Sign(Vector3.Dot(cross, worldAxisAtGrab));

        float slideAngle = sign * slideMagnitude * slideSensitivity;

        // === TWIST : rotation du poignet ===
        Quaternion rotDelta = _controller.rotation * Quaternion.Inverse(_ctrlRotOnGrab);
        rotDelta.ToAngleAxis(out float twistAngleRaw, out Vector3 twistAxis);

        float twistAngle = 0f;
        if (twistAngleRaw > twistDeadzone && twistAngleRaw < 180f)
        {
            twistAngle = twistAngleRaw * Vector3.Dot(twistAxis.normalized, worldAxisAtGrab);
        }

        // === ROTATION FINALE ===
        // On applique UNE SEULE rotation autour de l'axe world figé au grab
        int rotationDirection = reverseRotation ? -1 : 1;
        float totalAngle = rotationDirection * (slideAngle + twistAngle * twistSensitivity);
        Quaternion deltaRot = Quaternion.AngleAxis(totalAngle, worldAxisAtGrab);
        targetObject.rotation = deltaRot * _objRotOnGrab;
    }
}