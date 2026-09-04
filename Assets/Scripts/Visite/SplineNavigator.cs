using UnityEngine;
using UnityEngine.InputSystem;

// Les waypoints DOIVENT être enfants de anatomyPivot.
// Le pivot se déplace autour du joueur fixe.
// Déplacement par téléportation discrète entre waypoints marqués isTeleportStop,
// piloté depuis l'extérieur (WaypointTeleportAim) via GetNextStopIndex/JumpTo.
public class SplineNavigator : MonoBehaviour
{
    [Header("Waypoints — enfants de anatomyPivot")]
    public Transform[] waypoints;

    [Tooltip("Un booléen par waypoint (même index) : true = arrêt de téléportation valide, " +
             "false = point utilisé uniquement pour donner sa forme à la courbe.")]
    public bool[] isTeleportStop;

    [Header("Pivot du modèle anatomique")]
    public Transform anatomyPivot;

    [Header("Ancrage joueur")]
    [Tooltip("XR Camera ou First-Person Camera")]
    public Transform playerAnchor;

    [Header("Scale")]
    public AnatomyScaleManager scaleManager;

    [Header("Zones anatomiques")]
    public AnatomyNavigator anatomyNavigator;

    [Header("Debug clavier — désactiver en prod")]
    public bool keyboardDebug = false;

    private int seg = 0;
    private bool active = false;
    private Quaternion smoothedRot = Quaternion.identity;
    private bool rotInitialized = false;

    public bool IsActive => active;

    public int CurrentIndex => seg;

    public float CurrentProgress =>
        waypoints == null || waypoints.Length < 2 ? 0f :
        seg / (float)Mathf.Max(1, waypoints.Length - 1);

    void OnValidate()
    {
        if (waypoints == null) return;
        if (isTeleportStop == null || isTeleportStop.Length != waypoints.Length)
            System.Array.Resize(ref isTeleportStop, waypoints.Length);
    }

    public void Activate()
    {
        rotInitialized = false;
        active = true;
        JumpTo(0);
    }

    public void Deactivate() => active = false;

    void Update()
    {
        if (!keyboardDebug || !active || Keyboard.current == null) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            TryJump(1);
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            TryJump(-1);
    }

    private void TryJump(int direction)
    {
        int target = GetNextStopIndex(seg, direction);
        if (target >= 0) JumpTo(target);
    }

    // Cherche, à partir de fromIndex, le prochain waypoint marqué isTeleportStop
    // dans le sens direction (+1 avant / -1 arrière). Retourne -1 si aucun.
    public int GetNextStopIndex(int fromIndex, int direction)
    {
        if (waypoints == null || isTeleportStop == null) return -1;
        for (int i = fromIndex + direction; i >= 0 && i < waypoints.Length; i += direction)
            if (isTeleportStop[i]) return i;
        return -1;
    }

    // Repositionne/réoriente anatomyPivot instantanément sur le waypoint index.
    public void JumpTo(int index)
    {
        if (waypoints == null || waypoints.Length == 0 || anatomyPivot == null || playerAnchor == null) return;
        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        seg = index;

        Vector3 localCurrent = CatmullRomPoint(seg, 0f);
        Vector3 localDir = CatmullRomTangent(seg, 0f).normalized;
        if (localDir == Vector3.zero) localDir = Vector3.forward;

        if (!rotInitialized)
        {
            // Orientation initiale : LookRotation fixe le roulis à zéro au départ.
            smoothedRot = Quaternion.Inverse(Quaternion.LookRotation(localDir, Vector3.up));
            rotInitialized = true;
        }
        else
        {
            // Transport parallèle : même correction que l'ancien déplacement continu,
            // appliquée en un seul saut plutôt que lissée frame par frame.
            Vector3 worldTangent = smoothedRot * localDir;
            Quaternion correction = Quaternion.FromToRotation(worldTangent, Vector3.forward);
            smoothedRot = correction * smoothedRot;
        }

        anatomyPivot.SetPositionAndRotation(
            playerAnchor.position - smoothedRot * localCurrent,
            smoothedRot);
    }

    // ── Catmull-Rom standard ────────────────────────────────────────────────────

    Vector3 GetLocal(int i)
    {
        i = Mathf.Clamp(i, 0, waypoints.Length - 1);
        return waypoints[i].localPosition;
    }

    Vector3 GetLocalExt(int i)
    {
        if (i < 0)                 return 2f * GetLocal(0) - GetLocal(1);
        if (i >= waypoints.Length) return 2f * GetLocal(waypoints.Length - 1) - GetLocal(waypoints.Length - 2);
        return GetLocal(i);
    }

    Vector3 CatmullRomPoint(int s, float u)
    {
        Vector3 p0 = GetLocalExt(s - 1), p1 = GetLocal(s),
                p2 = GetLocal(s + 1),    p3 = GetLocalExt(s + 2);
        float u2 = u * u, u3 = u2 * u;
        return 0.5f * (2f * p1
            + (-p0 + p2) * u
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * u2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * u3);
    }

    Vector3 CatmullRomTangent(int s, float u)
    {
        Vector3 p0 = GetLocalExt(s - 1), p1 = GetLocal(s),
                p2 = GetLocal(s + 1),    p3 = GetLocalExt(s + 2);
        return 0.5f * (
            (-p0 + p2)
            + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * u
            + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * (u * u));
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2 || anatomyPivot == null) return;

        Gizmos.color = Color.cyan;
        int steps = 80;
        for (int i = 0; i < steps; i++)
        {
            float ta = (float)i / steps * (waypoints.Length - 1);
            float tb = (float)(i + 1) / steps * (waypoints.Length - 1);
            int sa = Mathf.Min((int)ta, waypoints.Length - 2);
            int sb = Mathf.Min((int)tb, waypoints.Length - 2);
            Gizmos.DrawLine(
                anatomyPivot.TransformPoint(CatmullRomPoint(sa, ta - sa)),
                anatomyPivot.TransformPoint(CatmullRomPoint(sb, tb - sb)));
        }

        if (isTeleportStop == null) return;
        Gizmos.color = Color.magenta;
        for (int i = 0; i < waypoints.Length; i++)
            if (i < isTeleportStop.Length && isTeleportStop[i] && waypoints[i] != null)
                Gizmos.DrawSphere(anatomyPivot.TransformPoint(waypoints[i].localPosition), 0.02f);
    }
}
