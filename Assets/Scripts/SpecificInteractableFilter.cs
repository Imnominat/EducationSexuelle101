using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SpecificInteractableFilter : MonoBehaviour, IXRSelectFilter
{
	[SerializeField] private XRGrabInteractable allowed;

	public bool canProcess => enabled;

	public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		return interactable == (IXRSelectInteractable)allowed;
	}
}