using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// À placer sur chaque poubelle. Détecte quand un objet grabbable y est déposé
/// et délègue la logique à ChambreGameManager.
/// </summary>
public class TrashBin : MonoBehaviour
{
    public enum BinType { Good, Bad }

    [Tooltip("Good = bonne poubelle (objets OK), Bad = mauvaise poubelle (bloqueurs).")]
    public BinType binType;

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
            // Objet tenu : on attend qu'il soit lâché à l'intérieur
            if (_trackedObjects.ContainsKey(grab.gameObject)) return;
            _trackedObjects[grab.gameObject] = grab;
            grab.selectExited.AddListener(OnObjectReleased);
        }
        else
        {
            // Objet déjà lâché qui entre dans le trigger (ex: lancé)
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
        ChambreGameManager.Instance.OnObjectPlacedInBin(obj, binType);
    }
}
