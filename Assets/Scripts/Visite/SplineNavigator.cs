using UnityEngine;
using UnityEngine.InputSystem;

// Prérequis : les waypoints DOIVENT être enfants de anatomyPivot.
// C'est le pivot qui bouge autour du joueur fixe (Option A).
public class SplineNavigator : MonoBehaviour
{
    [Header("Waypoints — enfants de anatomyPivot")]
    public Transform[] waypoints;
    public float moveSpeed = 0.5f;

    [Header("Pivot du modèle anatomique")]
    public Transform anatomyPivot;

    [Header("Ancrage joueur — glisser la XR Camera ici")]
    public Transform playerAnchor;

    [Header("Scale dynamique par waypoint")]
    public AnatomyScaleManager scaleManager;
    [Tooltip("Un scale par waypoint (même longueur que waypoints). " +
             "waypointScales[0] doit égaler AnatomyScaleManager.microScale (1/300).")]
    public float[] waypointScales;

    private int seg = 0;
    private float t   = 0f;
    private bool active = false;

    public void Activate()
    {
        seg = 0;
        t   = 0f;
        active = true;
        UpdatePivot(); // positionnement immédiat sans attendre le prochain Update
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
        while (t >= 1f && seg < waypoints.Length - 2) { t -= 1f; seg++; }
        while (t <  0f && seg > 0)                    { t += 1f; seg--; }
        t = Mathf.Clamp01(t);

        // Scale dynamique : uniquement quand la transition d'entrée est terminée
        if (scaleManager != null && !scaleManager.IsTransitioning
            && waypointScales != null && waypointScales.Length == waypoints.Length)
        {
            float s0 = waypointScales[Mathf.Clamp(seg,     0, waypointScales.Length - 1)];
            float s1 = waypointScales[Mathf.Clamp(seg + 1, 0, waypointScales.Length - 1)];
            scaleManager.SetScale(Mathf.Lerp(s0, s1, t));
        }

        UpdatePivot();
    }

    void UpdatePivot()
    {
        Vector3 localCurrent = CatmullRomPoint(seg, t);
        Vector3 localDir     = CatmullRomTangent(seg, t).normalized;

        if (localDir == Vector3.zero) return; // waypoints superposés — ignorer

        Quaternion rot = Quaternion.Inverse(Quaternion.LookRotation(localDir, Vector3.up));
        anatomyPivot.SetPositionAndRotation(playerAnchor.position - rot * localCurrent, rot);
    }

    // ── Catmull-Rom ────────────────────────────────────────────────────────────

    Vector3 GetLocal(int i)
    {
        i = Mathf.Clamp(i, 0, waypoints.Length - 1);
        return waypoints[i].localPosition;
    }

    // Points fantômes aux extrémités par extrapolation
    Vector3 GetLocalExt(int i)
    {
        if (i < 0)                   return 2f * GetLocal(0) - GetLocal(1);
        if (i >= waypoints.Length)   return 2f * GetLocal(waypoints.Length - 1) - GetLocal(waypoints.Length - 2);
        return GetLocal(i);
    }

    Vector3 CatmullRomPoint(int s, float u)
    {
        Vector3 p0 = GetLocalExt(s - 1);
        Vector3 p1 = GetLocal(s);
        Vector3 p2 = GetLocal(s + 1);
        Vector3 p3 = GetLocalExt(s + 2);
        float u2 = u * u, u3 = u2 * u;
        return 0.5f * (2f * p1
            + (-p0 + p2) * u
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * u2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * u3);
    }

    Vector3 CatmullRomTangent(int s, float u)
    {
        Vector3 p0 = GetLocalExt(s - 1);
        Vector3 p1 = GetLocal(s);
        Vector3 p2 = GetLocal(s + 1);
        Vector3 p3 = GetLocalExt(s + 2);
        return 0.5f * (
            (-p0 + p2)
            + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * u
            + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * (u * u));
    }
}
