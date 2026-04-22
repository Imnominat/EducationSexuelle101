// DialogInteractable.cs — Composant sur le personnage/objet interactable
using UnityEngine;
using UnityEngine.Events;

namespace Dialogs
{
    /// <summary>
    /// Attach this to any GameObject (NPC, object) that should open a DialogUI when interacted with.
    /// </summary>
    public class DialogInteractable : MonoBehaviour
    {
        [Tooltip("The DialogUI linked to this interactable.")]
        [SerializeField] private DialogUI m_DialogUI;

        [Tooltip("If true, close the dialog when interacting again while it is open (toggle).")]
        [SerializeField] private bool m_ToggleOnReinteract = true;

        [Space]
        public UnityEvent OnInteracted;
        public UnityEvent OnDialogClosed;

        public bool IsOpen => m_DialogUI != null && m_DialogUI.IsVisible;

        /// <summary>
        /// Call this from your VR interaction system (raycaster, hand, etc.)
        /// </summary>
        public void Interact()
        {
            if (m_DialogUI == null) return;

            if (m_ToggleOnReinteract && IsOpen)
            {
                m_DialogUI.Close();
                OnDialogClosed.Invoke();
            }
            else
            {
                m_DialogUI.Open();
                OnInteracted.Invoke();
            }
        }

        /// <summary>Force-closes the dialog from outside (ex: leaving the area).</summary>
        public void ForceClose()
        {
            if (m_DialogUI == null) return;
            m_DialogUI.Close();
            OnDialogClosed.Invoke();
        }
    }
}