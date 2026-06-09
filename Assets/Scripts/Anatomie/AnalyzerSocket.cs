using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// À placer sur le cube analyseur (qui possède le XRSocketInteractor).
/// Gère l'affichage des données et la création/destruction du duplicata.
/// </summary>
public class AnalyzerSocket : MonoBehaviour
{
    [Header("Références — Écran")]
    [Tooltip("TextMeshPro pour le titre de l'objet analysé")]
    public TextMeshPro titleText;

    [Tooltip("TextMeshPro pour la description de l'objet analysé")]
    public TextMeshPro descriptionText;

    [Tooltip("Empty GameObject où le duplicata de l'objet sera instancié")]
    public Transform displayAnchor;

    [Header("Textes par défaut (état sans objet)")]
    public string defaultTitle = "Analyseur";
    public string defaultDescription = "Placez un objet dans le réceptacle pour obtenir des informations.";

    [Header("Paramètres du duplicata")]
    [Tooltip("Échelle appliquée au duplicata affiché")]
    public float displayScale = 0.3f;

    [Header("Événements")]
    [Tooltip("Déclenché quand un objet est analysé. Paramètre : ID/titre de l'objet.")]
    public UnityEvent<string> OnObjectAnalyzed;

    // ── Référence interne ──────────────────────────────────────────────
    private XRSocketInteractor _socket;
    private GameObject _currentDuplicate;

    // ══════════════════════════════════════════════════════════════════
    void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
        if (_socket == null)
        {
            Debug.LogError("[AnalyzerSocket] Aucun XRSocketInteractor trouvé sur cet objet !", this);
            return;
        }

        // XRI 3.x : SelectEntered / SelectExited sont toujours disponibles
        _socket.selectEntered.AddListener(OnObjectPlaced);
        _socket.selectExited.AddListener(OnObjectRemoved);
    }

    void Start()
    {
        ResetDisplay();
    }

    void OnDestroy()
    {
        if (_socket == null) return;
        _socket.selectEntered.RemoveListener(OnObjectPlaced);
        _socket.selectExited.RemoveListener(OnObjectRemoved);
    }

    // ══════════════════════════════════════════════════════════════════
    // Événements socket
    // ══════════════════════════════════════════════════════════════════

    private void OnObjectPlaced(SelectEnterEventArgs args)
    {
        GameObject placedObject = args.interactableObject.transform.gameObject;
        AnalysableObject data = placedObject.GetComponent<AnalysableObject>();

        string objectId;
        if (data == null)
        {
            ShowText("Objet non identifié", "Cet objet ne contient aucune donnée analysable.");
            objectId = placedObject.name;
        }
        else
        {
            ShowText(data.objectTitle, data.objectDescription);
            objectId = string.IsNullOrEmpty(data.objectTitle) ? placedObject.name : data.objectTitle;
        }

        OnObjectAnalyzed?.Invoke(objectId);
        SpawnDuplicate(placedObject);
    }

    private void OnObjectRemoved(SelectExitEventArgs args)
    {
        DestroyDuplicate();
        ResetDisplay();
    }

    // ══════════════════════════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════════════════════════

    private void ShowText(string title, string description)
    {
        if (titleText != null)       titleText.text       = title;
        if (descriptionText != null) descriptionText.text = description;
    }

    private void ResetDisplay()
    {
        ShowText(defaultTitle, defaultDescription);
    }

    private void SpawnDuplicate(GameObject source)
    {
        // Nettoyer un éventuel duplicata résiduel
        DestroyDuplicate();

        if (displayAnchor == null)
        {
            Debug.LogWarning("[AnalyzerSocket] displayAnchor non assigné, pas de duplicata créé.");
            return;
        }

        // Instancier le duplicata à l'ancre
        _currentDuplicate = Instantiate(source, displayAnchor.position, displayAnchor.rotation, displayAnchor);
        _currentDuplicate.name = source.name + "_Preview";
        _currentDuplicate.transform.localScale = Vector3.one * displayScale;

        // Désactiver tous les composants interactifs pour éviter les conflits XRI
        DisableInteractionComponents(_currentDuplicate);
    }

    private void DestroyDuplicate()
    {
        if (_currentDuplicate != null)
        {
            Destroy(_currentDuplicate);
            _currentDuplicate = null;
        }
    }

    /// <summary>
    /// Retire les composants XRI et physiques du duplicata pour qu'il soit
    /// purement visuel et ne perturbe pas le socket ou la physique.
    /// </summary>
    private void DisableInteractionComponents(GameObject obj)
    {
        // Rigidbody
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Tous les colliders → désactivés
        foreach (var col in obj.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        // Composants XRI (Grab, Socket, etc.) → désactivés
        foreach (var behaviour in obj.GetComponentsInChildren<MonoBehaviour>(true))
        {
            // On désactive tout ce qui vient du namespace XR Interaction Toolkit
            if (behaviour != null && behaviour.GetType().Namespace != null &&
                behaviour.GetType().Namespace.StartsWith("UnityEngine.XR"))
            {
                behaviour.enabled = false;
            }
        }

        // Désactiver le script AnalysableObject aussi (inutile sur le preview)
        var analyzable = obj.GetComponent<AnalysableObject>();
        if (analyzable != null) analyzable.enabled = false;
    }
}