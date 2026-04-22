using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogs
{
    /// <summary>
    /// Raycasts from the player controller toward interactable objects.
    /// Compatible with Unity's new Input System. Swap the input check if you use XR Toolkit or legacy input.
    /// </summary>
    public class InteractionRaycaster : MonoBehaviour
    {
        [Tooltip("Max interaction distance in meters.")]
        [SerializeField] private float m_MaxDistance = 5f;

        [Tooltip("Layer mask for interactable objects.")]
        [SerializeField] private LayerMask m_InteractableLayer = ~0;

        [Header("Input — swap for your VR SDK if needed")]
        [Tooltip("Input action that triggers interaction (primary button, trigger, etc.)")]
        [SerializeField] private InputActionReference m_InteractAction;

        [Header("Optional feedback")]
        [Tooltip("Highlight shown on the currently aimed interactable (can be null).")]
        [SerializeField] private GameObject m_ReticleHighlight;

        private DialogInteractable _currentTarget;

        private void OnEnable()  => m_InteractAction?.action.Enable();
        private void OnDisable() => m_InteractAction?.action.Disable();

        void Update()
        {
            // 1. Raycast
            DialogInteractable hit = Raycast();

            // 2. Highlight feedback
            UpdateReticle(hit);

            // 3. On interact press
            if (m_InteractAction != null && m_InteractAction.action.WasPressedThisFrame())
            {
                if (hit != null)
                    hit.Interact();
            }

            _currentTarget = hit;
        }

        private DialogInteractable Raycast()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, m_MaxDistance, m_InteractableLayer))
                return hitInfo.collider.GetComponentInParent<DialogInteractable>();
            return null;
        }

        private void UpdateReticle(DialogInteractable target)
        {
            if (m_ReticleHighlight == null) return;
            m_ReticleHighlight.SetActive(target != null);
        }
    }
}