using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
/// <summary>
/// À placer sur la caméra VR (ou le contrôleur).
/// Gère le ciblage par raycast et affiche une interface de signalement.
/// Compatible avec XR Interaction Toolkit ou un Raycast manuel.
/// </summary>
public class GazeTargeting : MonoBehaviour
{
    [Header("Ciblage")]
    [Tooltip("Distance maximale du raycast en mètres.")]
    public float raycastDistance = 10f;

    [Tooltip("Layer mask pour les objets interactables.")]
    public LayerMask interactableLayer;

    [Header("UI de signalement")]
    [Tooltip("Canvas WorldSpace affiché quand un objet est ciblé (le bouton Signaler).")]
    public GameObject signalPanel;

    [Tooltip("Texte du label de l'objet ciblé dans le UI.")]
    public TMP_Text objectLabelText; 

    [Tooltip("Bouton Signaler dans le panel.")]
    public Button reportButton;

    [Tooltip("Croix de visée (reticle) au centre de l'écran.")]
    public GameObject reticle;

    [Header("Audio (optionnel)")]
    public AudioClip focusSound;
    public AudioClip reportSound;
    private AudioSource _audioSource;

    private InteractableObject _currentTarget;
    private InteractableObject _previousTarget;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        if (signalPanel != null) signalPanel.SetActive(false);
        if (reportButton != null) reportButton.onClick.AddListener(OnReportButtonClicked);
    }

    private void Update()
    {
        PerformRaycast();
    }

    private void PerformRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayer))
        {
            InteractableObject target = hit.collider.GetComponent<InteractableObject>();

            if (target != null && !target.IsReported)
            {
                if (target != _currentTarget)
                {
                    // Nouvel objet ciblé
                    LoseFocus();
                    _currentTarget = target;
                    _currentTarget.OnFocus();
                    ShowSignalPanel(target);
                    PlaySound(focusSound);
                }
            }
            else
            {
                LoseFocus();
            }
        }
        else
        {
            LoseFocus();
        }
    }

    private void ShowSignalPanel(InteractableObject target)
    {
        if (signalPanel == null) return;

        signalPanel.SetActive(true);

        if (objectLabelText != null)
            objectLabelText.text = target.objectLabel;

        // Positionner le panel face au joueur, légèrement au-dessus de l'objet ciblé
        PositionSignalPanel(target.transform.position);
    }

    private void PositionSignalPanel(Vector3 objectPosition)
    {
        if (signalPanel == null) return;

        Vector3 panelPos = objectPosition + Vector3.up * 0.5f;
        signalPanel.transform.position = panelPos;

        // Orienter vers la caméra
        signalPanel.transform.LookAt(transform.position);
        signalPanel.transform.Rotate(0, 180, 0);
    }

    private void LoseFocus()
    {
        if (_currentTarget != null)
        {
            _currentTarget.OnLoseFocus();
            _currentTarget = null;
        }

        if (signalPanel != null) signalPanel.SetActive(false);
    }

    private void OnReportButtonClicked()
    {
        if (_currentTarget == null) return;

        PlaySound(reportSound);
        _currentTarget.Report();

        // Cacher le panel immédiatement
        if (signalPanel != null) signalPanel.SetActive(false);
        _currentTarget = null;
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
            _audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmos()
    {
        // Visualiser le raycast dans l'éditeur
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * raycastDistance);
    }
}