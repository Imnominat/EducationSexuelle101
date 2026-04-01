using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AxisHandle : MonoBehaviour
{
    [Header("Cible � faire tourner")]
    public Transform targetObject;

    [Header("Axe de cette poign�e (espace LOCAL de l'objet cible)")]
    public Vector3 localAxis = Vector3.right;

    [Header("Sensibilit�s")]
    public float slideSensitivity  = 150f;  // degr�s/m�tre de d�placement
    public float twistSensitivity  = 1.2f;  // multiplicateur du twist

    [Header("Contrainte : seuil de d�tection du twist (degr�s)")]
    public float twistDeadzone = 8f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable _interactable;
    private Transform _controller;
    private bool _grabbed;

    // �tat au moment du grab
    private Vector3    _ctrlPosOnGrab;
    private Quaternion _ctrlRotOnGrab;
    private Quaternion _objRotOnGrab;

    // ---- axe en world space calcul� une fois au grab ----
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

        // Axe en world space fig� au moment du grab
        _worldAxis = targetObject.TransformDirection(localAxis).normalized;

        // Construction d'une base orthonorm�e autour de l'axe
        // perpA et perpB d�finissent le "plan de glissement"
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

        // === TWIST (rotation du poignet autour de l'axe) ===
        Quaternion rotDelta = _controller.rotation * Quaternion.Inverse(_ctrlRotOnGrab);
        rotDelta.ToAngleAxis(out float angle, out Vector3 rotAxis);

        // Projection du delta de rotation sur notre axe
        float twistAmount = 0f;
        if (angle > twistDeadzone)
        {
            twistAmount = angle * Vector3.Dot(rotAxis.normalized, _worldAxis);
        }

        // === SLIDE (d�placement du controller - rotation sur axe perpendiculaire) ===
        Vector3 delta = _controller.position - _ctrlPosOnGrab;

        // D�composition du d�placement dans le plan perpendiculaire � l'axe
        float slideA = Vector3.Dot(delta, _perpA); // - rotation autour de perpB
        float slideB = Vector3.Dot(delta, _perpB); // - rotation autour de perpA

        float slideAngleA = -slideA * slideSensitivity;
        float slideAngleB =  slideB * slideSensitivity;

        // === COMBINAISON (depuis la rotation initiale au grab) ===
        Quaternion qTwist  = Quaternion.AngleAxis(twistAmount * twistSensitivity, _worldAxis);
        Quaternion qSlideA = Quaternion.AngleAxis(slideAngleA, _perpB);
        Quaternion qSlideB = Quaternion.AngleAxis(slideAngleB, _perpA);

        // Dans Update(), remplacer l'assignation finale :

        Quaternion rawRotation = qTwist * qSlideA * qSlideB * _objRotOnGrab;

        // Appliquer la contrainte si le composant est présent
        var constraint = GetComponent<SingleAxisConstraint>();
        if (constraint != null)
            targetObject.rotation = constraint.FilterRotation(rawRotation, _objRotOnGrab);
        else
            targetObject.rotation = rawRotation;
            }
}