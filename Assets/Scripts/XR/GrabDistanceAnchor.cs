using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Par défaut, l'attach transform d'un Ray Interactor est un enfant rigide du contrôleur :
// avancer/reculer le bras avance/recule donc l'objet tenu à distance. On fige ici le point
// de pivot (mémorisé en espace local du XR Origin, pour continuer à suivre la téléportation/
// les déplacements du joueur) au moment du grab, et seule l'inclinaison du joystick fait
// varier la distance ensuite ; la rotation du poignet continue de viser librement.
//
// La direction utilisée pour positionner l'objet vient de interactor.transform.forward (visée
// réelle du contrôleur), pas de attach.forward : le Rotate Manipulation natif du XRRayInteractor
// (joystick gauche/droite) fait tourner l'attach transform sur lui-même pour faire pivoter l'objet
// tenu, et si on utilisait attach.forward ici, cette rotation ferait aussi dériver la position
// (l'objet swinguerait autour du pivot au lieu de simplement tourner sur lui-même).
[RequireComponent(typeof(XRRayInteractor))]
public class GrabDistanceAnchor : MonoBehaviour
{
    [SerializeField] InputActionReference translateInput;
    [SerializeField] float translateSpeed = 1f;
    [SerializeField] float minDistance = 0.1f;
    [SerializeField] float maxDistance = 15f;

    XRRayInteractor interactor;
    XROrigin xrOrigin;
    Vector3 pivotLocalOffset;
    float currentDistance;
    bool isHolding;

    void Awake()
    {
        interactor = GetComponent<XRRayInteractor>();
        xrOrigin = GetComponentInParent<XROrigin>();
    }

    void OnEnable()
    {
        interactor.selectEntered.AddListener(OnSelectEntered);
        interactor.selectExited.AddListener(OnSelectExited);
        if (translateInput != null)
            translateInput.action.Enable();
    }

    void OnDisable()
    {
        interactor.selectEntered.RemoveListener(OnSelectEntered);
        interactor.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (xrOrigin == null || interactor.attachTransform == null)
            return;

        var handPosition = interactor.transform.position;
        pivotLocalOffset = xrOrigin.transform.InverseTransformPoint(handPosition);
        currentDistance = Mathf.Clamp(Vector3.Distance(handPosition, interactor.attachTransform.position), minDistance, maxDistance);
        isHolding = true;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        isHolding = interactor.hasSelection;
    }

    void LateUpdate()
    {
        if (!isHolding || xrOrigin == null)
            return;

        if (translateInput != null)
        {
            var input = translateInput.action.ReadValue<Vector2>();
            currentDistance = Mathf.Clamp(currentDistance + input.y * translateSpeed * Time.deltaTime, minDistance, maxDistance);
        }

        var attach = interactor.attachTransform;
        var pivotWorld = xrOrigin.transform.TransformPoint(pivotLocalOffset);
        attach.position = pivotWorld + interactor.transform.forward * currentDistance;
    }
}
