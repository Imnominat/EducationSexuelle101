using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// À placer sur la poubelle Bad. Détecte quand un objet grabbable y est déposé
/// et délègue la logique à ChambreGameManager.
/// </summary>
public class TrashBin : MonoBehaviour
{
    private readonly Dictionary<GameObject, XRGrabInteractable> _trackedObjects = new();

    private void OnTriggerEnter(Collider other)
    {
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = other.GetComponentInParent<XRGrabInteractable>();
        if (grab == null) return;

        InteractableObject obj = grab.GetComponent<InteractableObject>();
        if (obj == null) obj = grab.GetComponentInParent<InteractableObject>();
        if (obj == null || obj.IsProcessed) return;

        if (grab.isSelected)
        {
            if (_trackedObjects.ContainsKey(grab.gameObject)) return;
            _trackedObjects[grab.gameObject] = grab;
            grab.selectExited.AddListener(OnObjectReleased);
        }
        else
        {
            ProcessPlacement(obj);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = other.GetComponentInParent<XRGrabInteractable>();
        if (grab == null || !_trackedObjects.ContainsKey(grab.gameObject)) return;

        grab.selectExited.RemoveListener(OnObjectReleased);
        _trackedObjects.Remove(grab.gameObject);
    }

    private void OnObjectReleased(SelectExitEventArgs args)
    {
        XRGrabInteractable grab = args.interactableObject as XRGrabInteractable;
        if (grab == null) return;

        grab.selectExited.RemoveListener(OnObjectReleased);
        if (!_trackedObjects.Remove(grab.gameObject)) return;

        InteractableObject obj = grab.GetComponent<InteractableObject>();
        if (obj == null) obj = grab.GetComponentInParent<InteractableObject>();
        if (obj == null || obj.IsProcessed) return;

        ProcessPlacement(obj);
    }

    private void ProcessPlacement(InteractableObject obj)
    {
        ChambreGameManager.Instance.OnObjectPlacedInBin(obj);
    }
}
