using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// À placer sur le même GameObject que votre XRRayInteractor (main droite).
/// - Ciblage via XRRayInteractor
/// - Clic via InputAction (trigger manette) quand on vise le bouton Signaler
/// - Timer 3s ne démarre pas si le ray est sur l'objet OU sur le panel
/// </summary>
[RequireComponent(typeof(XRRayInteractor))]
public class RaycastTargeting : MonoBehaviour
{
    [Header("Référence Ray Interactor")]
    public XRRayInteractor rayInteractor;

    [Header("Signal Panel")]
    [Tooltip("Canvas WorldSpace avec TrackedDeviceGraphicRaycaster.")]
    public GameObject signalPanel;
    public TMP_Text objectLabelText;
    public Button reportButton;

    [Header("Comportement")]
    [Tooltip("Délai avant disparition du panel quand le ray n'est ni sur l'objet ni sur le panel.")]
    public float panelHideDelay = 3f;
    public float panelHeightOffset = 0.5f;

    [Header("Input Action — Trigger manette")]
    [Tooltip("Action InputSystem pour déclencher le signalement (ex: XRI RightHand/Activate).")]
    public InputActionReference activateAction;

    // ── État interne ──────────────────────────────
    private InteractableObject _currentTarget;
    private Coroutine _hideCoroutine;
    private bool _panelVisible;
    private bool _rayOnPanel;      // true quand le ray UI touche le panel
    private bool _rayOnObject;     // true quand le ray 3D touche un InteractableObject valide

    // ─────────────────────────────────────────────
    // Init
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (rayInteractor == null)
            rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void OnEnable()
    {
        if (activateAction != null)
        {
            activateAction.action.Enable();
            activateAction.action.performed += OnActivatePerformed;
        }
    }

    private void OnDisable()
    {
        if (activateAction != null)
            activateAction.action.performed -= OnActivatePerformed;

        StopHideCoroutine();
        HideSignalPanel();
    }

    private void Start()
    {
        if (signalPanel != null) signalPanel.SetActive(false);

        // onClick en secours si XR UI Input Module est bien configuré
        if (reportButton != null)
            reportButton.onClick.AddListener(OnReportButtonClicked);
    }

    // ─────────────────────────────────────────────
    // Update : détection combinée objet 3D + panel UI
    // ─────────────────────────────────────────────

    private void Update()
    {
        CheckRaycastTarget();
    }

    private void CheckRaycastTarget()
    {
        bool hitObject = false;
        bool hitPanel  = false;

        if (rayInteractor.TryGetCurrentRaycast(
                out RaycastHit? raycastHit,
                out int _,
                out UnityEngine.EventSystems.RaycastResult? uiRaycastResult,
                out int __,
                out bool isUIHit))
        {
            if (isUIHit && uiRaycastResult.HasValue)
            {
                // Le ray touche un élément UI — vérifier si c'est notre panel
                GameObject hitUIRoot = uiRaycastResult.Value.gameObject;
                if (signalPanel != null && hitUIRoot != null &&
                    hitUIRoot.transform.IsChildOf(signalPanel.transform))
                {
                    hitPanel = true;
                }
            }
            else if (raycastHit.HasValue)
            {
                // Le ray touche un objet 3D
                InteractableObject target = raycastHit.Value.collider
                    .GetComponent<InteractableObject>();

                if (target != null && !target.IsReported)
                {
                    hitObject = true;
                    OnTargetDetected(target);
                }
            }
        }

        _rayOnObject = hitObject;
        _rayOnPanel  = hitPanel;

        if (!hitObject && !hitPanel)
            OnTargetLost();
        else
            StopHideCoroutine();
    }

    // ─────────────────────────────────────────────
    // Gestion de la cible 3D
    // ─────────────────────────────────────────────

    private void OnTargetDetected(InteractableObject target)
    {
        if (target == _currentTarget)
        {
            if (_panelVisible)
                PositionSignalPanel(_currentTarget.transform.position);
            return;
        }

        if (_currentTarget != null)
            _currentTarget.OnLoseFocus();

        _currentTarget = target;
        _currentTarget.OnFocus();
        ShowSignalPanel(_currentTarget);
    }

    private void OnTargetLost()
    {
        if (_currentTarget == null) return;
        if (_hideCoroutine == null)
            _hideCoroutine = StartCoroutine(HidePanelAfterDelay());
    }

    // ─────────────────────────────────────────────
    // Panel
    // ─────────────────────────────────────────────

    private void ShowSignalPanel(InteractableObject target)
    {
        if (signalPanel == null) return;
        signalPanel.SetActive(true);
        _panelVisible = true;

        if (objectLabelText != null)
            objectLabelText.text = target.objectLabel;

        PositionSignalPanel(target.transform.position);
    }

    private void PositionSignalPanel(Vector3 objectPosition)
    {
        if (signalPanel == null) return;
        signalPanel.transform.position = objectPosition + Vector3.up * panelHeightOffset;

        Camera cam = Camera.main;
        if (cam != null)
        {
            signalPanel.transform.LookAt(cam.transform.position);
            signalPanel.transform.Rotate(0, 180, 0);
        }
    }

    private void HideSignalPanel()
    {
        if (_currentTarget != null)
        {
            _currentTarget.OnLoseFocus();
            _currentTarget = null;
        }
        if (signalPanel != null) signalPanel.SetActive(false);
        _panelVisible = false;
    }

    private IEnumerator HidePanelAfterDelay()
    {
        float elapsed = 0f;
        while (elapsed < panelHideDelay)
        {
            // Ré-évaluer à chaque frame : si le ray revient, on arrête
            if (_rayOnObject || _rayOnPanel)
            {
                _hideCoroutine = null;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        HideSignalPanel();
        _hideCoroutine = null;
    }

    private void StopHideCoroutine()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
    }

    // ─────────────────────────────────────────────
    // Signalement — deux chemins possibles
    // ─────────────────────────────────────────────

    /// <summary>
    /// Chemin 1 : InputAction (trigger manette) quand le ray est sur le panel.
    /// Plus fiable en XRI 3.x que le onClick standard.
    /// </summary>
    private void OnActivatePerformed(InputAction.CallbackContext ctx)
    {
        if (_rayOnPanel && _currentTarget != null)
            OnReportButtonClicked();
    }

    /// <summary>
    /// Chemin 2 : onClick Button Unity (fonctionne si XR UI Input Module est configuré).
    /// Les deux chemins appellent la même méthode — pas de double déclenchement possible
    /// car _currentTarget est mis à null immédiatement.
    /// </summary>
    private void OnReportButtonClicked()
    {
        if (_currentTarget == null) return;

        StopHideCoroutine();
        InteractableObject toReport = _currentTarget;
        HideSignalPanel();   // met _currentTarget à null
        toReport.Report();
    }
}