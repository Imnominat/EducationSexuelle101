using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class SelectableBlock : MonoBehaviour
{
    [Header("Snap")]
    public float snapRadius      = 0.12f;  // rayon de détection en mètres
    public float snapLerpSpeed   = 20f;    // vitesse du lerp (0 = instantané)

    [Header("Ghost")]
    public Material ghostMaterial;         // matériau semi-transparent à créer dans l'éditeur
    public float ghostRevealRadius = 0.18f; // rayon d'apparition du ghost (> snapRadius)

    // --- state interne ---
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _interactable;
    private Transform _anchor;
    private GameObject _ghost;
    private Renderer _ghostRenderer;

    private bool _held;
    private bool _snapping;
    private bool _snapped;

    private Transform _originalParent;

    // -------------------------------------------------------
    // Init appelé par AnchorManager
    // -------------------------------------------------------
    public void InitAnchor()
    {
        _originalParent = transform.parent;

        // Créer l'anchor (empty) aux coordonnées actuelles de la pièce
        var anchorGO = new GameObject($"Anchor_{gameObject.name}");
        anchorGO.transform.SetParent(_originalParent);
        anchorGO.transform.position = transform.position;
        anchorGO.transform.rotation = transform.rotation;
        _anchor = anchorGO.transform;

        // Créer le ghost (copie du mesh, matériau ghost)
        _ghost = CreateGhost();
        _ghost.SetActive(false);

        // Interactable
        _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        _interactable.selectEntered.AddListener(OnGrab);
        _interactable.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        if (_interactable == null) return;
        _interactable.selectEntered.RemoveListener(OnGrab);
        _interactable.selectExited.RemoveListener(OnRelease);
    }

    // -------------------------------------------------------
    // Grab / Release
    // -------------------------------------------------------
    void OnGrab(SelectEnterEventArgs args)
    {
        _held     = true;
        _snapping = false;
        _snapped  = false;

        transform.SetParent(null, worldPositionStays: true);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        _held = false;

        float dist = Vector3.Distance(transform.position, _anchor.position);

        if (dist <= snapRadius)
        {
            // Dans la zone : on snap
            _snapping = true;
        }
        else
        {
            // Hors zone : rattachement libre à l'anchor parent
            transform.SetParent(_originalParent, worldPositionStays: true);
        }

        _ghost.SetActive(false);
    }

    // -------------------------------------------------------
    // Update
    // -------------------------------------------------------
    void Update()
    {
        if (_snapping)
        {
            UpdateSnap();
            return;
        }

        if (_held)
            UpdateGhostVisibility();
    }

    void UpdateSnap()
    {
        if (snapLerpSpeed <= 0f)
        {
            SnapInstant();
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position, _anchor.position,
            snapLerpSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, _anchor.rotation,
            snapLerpSpeed * Time.deltaTime);

        // Snap terminé quand on est très proche
        if (Vector3.Distance(transform.position, _anchor.position) < 0.001f)
            SnapInstant();
    }

    void SnapInstant()
    {
        transform.position = _anchor.position;
        transform.rotation = _anchor.rotation;
        transform.SetParent(_originalParent, worldPositionStays: true);
        _snapping = false;
        _snapped  = true;
    }

    void UpdateGhostVisibility()
    {
        float dist = Vector3.Distance(transform.position, _anchor.position);

        if (dist <= ghostRevealRadius)
        {
            // Apparition progressive selon la proximité
            float t = 1f - (dist / ghostRevealRadius);
            _ghost.SetActive(true);

            if (_ghostRenderer != null)
            {
                Color c = _ghostRenderer.material.color;
                c.a = Mathf.Lerp(0f, 0.35f, t); // max 35% d'opacité
                _ghostRenderer.material.color = c;
            }
        }
        else
        {
            _ghost.SetActive(false);
        }
    }

    // -------------------------------------------------------
    // Ghost
    // -------------------------------------------------------
GameObject CreateGhost()
{
    var ghostGO = new GameObject($"Ghost_{gameObject.name}");
    ghostGO.transform.SetParent(_anchor);
    ghostGO.transform.localPosition = Vector3.zero;
    ghostGO.transform.localRotation = Quaternion.identity;
    ghostGO.transform.localScale    = Vector3.one;

    foreach (var mf in GetComponentsInChildren<MeshFilter>())
    {
        // Filtre par tag plutôt que par nom
        if (mf.gameObject.CompareTag("GhostMesh")) continue;

        var child = new GameObject(mf.gameObject.name + "_ghostmesh");
        child.tag = "GhostMesh"; // ← on tague pour éviter la récursion
        child.transform.SetParent(ghostGO.transform);
        child.transform.localPosition = mf.transform.localPosition;
        child.transform.localRotation = mf.transform.localRotation;
        child.transform.localScale    = mf.transform.localScale;

        child.AddComponent<MeshFilter>().mesh = mf.sharedMesh;
        var mr = child.AddComponent<MeshRenderer>();

        if (ghostMaterial != null)
        {
            mr.material = ghostMaterial;
            if (_ghostRenderer == null) _ghostRenderer = mr;
        }
    }

    return ghostGO;
}}