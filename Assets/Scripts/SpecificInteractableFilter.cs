using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SpecificInteractableFilter : MonoBehaviour, IXRSelectFilter, IXRHoverFilter
{
    [SerializeField] private XRGrabInteractable allowed;

    bool IXRSelectFilter.canProcess => enabled;
    bool IXRHoverFilter.canProcess => enabled;

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        return interactable == (IXRSelectInteractable)allowed;
    }

    public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
    {
        return interactable == (IXRHoverInteractable)allowed;
    }
}