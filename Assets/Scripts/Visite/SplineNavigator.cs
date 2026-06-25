using UnityEngine;
using UnityEngine.InputSystem;

// Les waypoints DOIVENT être enfants de anatomyPivot.
// Le pivot se déplace autour du joueur fixe.
public class SplineNavigator : MonoBehaviour
{
    [Header("Waypoints — enfants de anatomyPivot")]
    public Transform[] waypoints;
    public float moveSpeed = 0.5f;

    [Header("Pivot du modèle anatomique")]
    public Transform anatomyPivot;

    [Header("Ancrage joueur")]
    [Tooltip("XR Camera ou First-Person Camera")]
    public Transform playerAnchor;

    [Header("Scale")]
    public AnatomyScaleManager scaleManager;

    [Header("Sortie")]
    public VisitEntryTrigger entryTrigger;
    [Tooltip("Marge de recul (en t) avant la sortie par clavier")]
    public float exitMargin = 0.08f;

    [Header("Zones anatomiques")]
    public AnatomyNavigator anatomyNavigator;

    [Header("Rotation")]
    [Tooltip("Vitesse de lissage de la rotation (deg/s environ). 5-8 recommandé.")]
    public float rotationSmoothSpeed = 5f;

    private int seg = 0;
    private float t = 0f;
    private bool active = false;
    private Quaternion smoothedRot = Quaternion.identity;
    private bool rotInitialized = false;

    public bool IsActive => active;

    public float CurrentProgress =>
        waypoints == null || waypoints.Length < 2 ? 0f :
        (seg + t) / Mathf.Max(1, waypoints.Length - 1);

    public void Activate()
    {
        seg = 0;
        t = 0f;
        rotInitialized = false;
        active = true;
        UpdatePivot();
    }

    public void Deactivate() => active = false;

    void Update()
    {
        if (!active || waypoints == null || waypoints.Length < 2
            || anatomyPivot == null || playerAnchor == null) return;

        float input = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)   input =  1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input = -1f;

        t += input * moveSpeed * Time.deltaTime;

        if (seg == 0 && t < -exitMargin)
        {
            t = 0f;
            TriggerExit();
            return;
        }

        while (t >= 1f && seg < waypoints.Length - 2) { t -= 1f; seg++; }
        while (t <  0f && seg > 0)                    { t += 1f; seg--; }
        t = Mathf.Clamp01(t);

        UpdatePivot();
    }

    public void TriggerExit()
    {
        Deactivate();
        anatomyNavigator?.ResetNavigation();
        if (entryTrigger != null) entryTrigger.Exit();
        else scaleManager?.ExitMicroMode();
    }

    void UpdatePivot()
    {
        Vector3 localCurrent = CatmullRomPoint(seg, t);
        Vector3 localDir     = CatmullRomTangent(seg, t).normalized;
        if (localDir == Vector3.zero) return;

        if (!rotInitialized)
        {
            // Orientation initiale : LookRotation fixe le roulis à zéro au départ.
            smoothedRot = Quaternion.Inverse(Quaternion.LookRotation(localDir, Vector3.up));
            rotInitialized = true;
        }
        else
        {
            // Transport parallèle : correction incrémentale.
            // On cherche de combien le modèle doit encore tourner pour aligner
            // la tangente locale courante avec le forward monde.
            // La correction = arc minimal de (smoothedRot * localDir) → Vector3.forward.
            // Cet arc est borné par la courbure locale du chemin (~30° max entre
            // deux waypoints consécutifs), jamais un saut absolu de 90-180°.
            Vector3 worldTangent = smoothedRot * localDir;
            Quaternion correction = Quaternion.FromToRotation(worldTangent, Vector3.forward);
            Quaternion targetRot  = correction * smoothedRot;
            smoothedRot = Quaternion.Slerp(smoothedRot, targetRot,
                Mathf.Min(1f, Time.deltaTime * rotationSmoothSpeed));
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
    }
}
