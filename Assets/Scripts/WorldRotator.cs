using UnityEngine;

public class WorldRotator : MonoBehaviour
{
    [Header("Références")]
    public Transform worldContainer;
    public Transform playerHead;
    public Transform xrOrigin;
    public Transform[] splinePoints;

    [Header("Paramètres")]
    public float smoothSpeed = 3f;

    [Header("Activation")]
    public float activationDistance = 1.5f;
    public bool isActive = false;

    [Header("Correction")]
    public float pitchOffset = 0f;

    [Header("Debug")]
    public float currentT = 0f;
    public float currentAngle = 0f;

    private float smoothedT = 0f;

    void Update()
    {
        if (splinePoints == null || splinePoints.Length < 2) return;

        // 1. Trouver T avec deux passes
        float rawT = GetClosestT(playerHead.position, 80);
        rawT = RefineT(playerHead.position, rawT, 20);

        // 2. Lisser T pour éviter les vibrations
        smoothedT = Mathf.Lerp(smoothedT, rawT, Time.deltaTime * smoothSpeed);
        currentT = smoothedT;

        // 3. Vérifier si le joueur est assez proche de la spline
        Vector3 closestPoint = GetSplinePoint(currentT);
        float distToSpline = Vector3.Distance(playerHead.position, closestPoint);
        isActive = distToSpline < activationDistance;

        if (!isActive)
        {
            // Remettre doucement le monde à plat
            worldContainer.rotation = Quaternion.Slerp(
                worldContainer.rotation,
                Quaternion.identity,
                Time.deltaTime * smoothSpeed
            );
            return;
        }

        // 4. Calculer la tangente sur le T lissé
        Vector3 tangent = GetSplineTangent(currentT);

        // 5. Calculer la rotation cible
        Quaternion targetRotation = Quaternion.FromToRotation(tangent, Vector3.forward);

        // 6. Isoler uniquement le pitch (X), ignorer yaw et roll
        Vector3 euler = targetRotation.eulerAngles;
        targetRotation = Quaternion.Euler(euler.x + pitchOffset, 0f, 0f);
        currentAngle = euler.x + pitchOffset;

        // 7. Appliquer avec lissage
        worldContainer.rotation = Quaternion.Slerp(
            worldContainer.rotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    // --- Spline Catmull-Rom ---

    Vector3 GetSplinePoint(float t)
    {
        int n = splinePoints.Length - 1;
        int i = Mathf.Min(Mathf.FloorToInt(t * n), n - 1);
        float u = t * n - i;

        Vector3 p0 = splinePoints[Mathf.Max(i - 1, 0)].position;
        Vector3 p1 = splinePoints[i].position;
        Vector3 p2 = splinePoints[Mathf.Min(i + 1, n)].position;
        Vector3 p3 = splinePoints[Mathf.Min(i + 2, n)].position;

        return 0.5f * (
            (-p0 + 3*p1 - 3*p2 + p3) * (u*u*u) +
            (2*p0 - 5*p1 + 4*p2 - p3) * (u*u) +
            (-p0 + p2) * u +
            2*p1
        );
    }

    Vector3 GetSplineTangent(float t)
    {
        float d = 0.005f;
        return (GetSplinePoint(Mathf.Clamp01(t + d)) -
                GetSplinePoint(Mathf.Clamp01(t - d))).normalized;
    }

    float GetClosestT(Vector3 pos, int samples)
    {
        float bestT = 0f;
        float bestDist = float.MaxValue;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float d = Vector3.Distance(pos, GetSplinePoint(t));
            if (d < bestDist) { bestDist = d; bestT = t; }
        }
        return bestT;
    }

    float RefineT(Vector3 pos, float roughT, int samples)
    {
        float range = 1f / (splinePoints.Length - 1);
        float bestT = roughT;
        float bestDist = float.MaxValue;
        for (int i = 0; i <= samples; i++)
        {
            float t = Mathf.Clamp01(roughT - range/2f + range * i / samples);
            float d = Vector3.Distance(pos, GetSplinePoint(t));
            if (d < bestDist) { bestDist = d; bestT = t; }
        }
        return bestT;
    }

    void OnDrawGizmos()
    {
        if (splinePoints == null || splinePoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < 60; i++)
        {
            Gizmos.DrawLine(
                GetSplinePoint(i / 60f),
                GetSplinePoint((i + 1) / 60f)
            );
        }

        Gizmos.color = Color.red;
        foreach (var p in splinePoints)
            if (p != null) Gizmos.DrawSphere(p.position, 0.05f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetSplinePoint(currentT), 0.08f);
    }
}