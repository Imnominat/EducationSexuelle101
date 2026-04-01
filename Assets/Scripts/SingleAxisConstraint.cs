using UnityEngine;

[RequireComponent(typeof(AxisHandle))]
public class SingleAxisConstraint : MonoBehaviour
{
    public enum ConstrainedAxis { None, X, Y, Z }

    [Header("Seuil angulaire pour vérouiller un axe (degrés)")]
    public float lockThreshold = 10f;

    private AxisHandle _handle;
    private ConstrainedAxis _lockedAxis = ConstrainedAxis.None;
    private Quaternion _lastRotation;
    private bool _axisLocked;

    void Awake()
    {
        _handle = GetComponent<AxisHandle>();
    }

    // Appelé par AxisHandle (voir modification ci-dessous)
    public void OnGrabBegin(Quaternion objectRotation)
    {
        _lastRotation = objectRotation;
        _axisLocked   = false;
        _lockedAxis   = ConstrainedAxis.None;
    }

    // Filtre la rotation calculée par AxisHandle
    // Retourne la rotation autorisée
    public Quaternion FilterRotation(Quaternion proposed, Quaternion baseRotation)
    {
        // Delta depuis la base
        Quaternion delta = proposed * Quaternion.Inverse(baseRotation);
        delta.ToAngleAxis(out float totalAngle, out Vector3 totalAxis);

        if (totalAngle < 0.01f)
            return proposed;

        // Décomposer le delta en Euler (angles sur X, Y, Z)
        Vector3 euler = (proposed * Quaternion.Inverse(baseRotation)).eulerAngles;

        // Convertir en [-180, 180]
        float ex = NormalizeAngle(euler.x);
        float ey = NormalizeAngle(euler.y);
        float ez = NormalizeAngle(euler.z);

        float ax = Mathf.Abs(ex);
        float ay = Mathf.Abs(ey);
        float az = Mathf.Abs(ez);
        float maxAngle = Mathf.Max(ax, ay, az);

        // Détection et verrouillage de l'axe dominant
        if (!_axisLocked && maxAngle > lockThreshold)
        {
            if (ax >= ay && ax >= az)      _lockedAxis = ConstrainedAxis.X;
            else if (ay >= ax && ay >= az) _lockedAxis = ConstrainedAxis.Y;
            else                           _lockedAxis = ConstrainedAxis.Z;
            _axisLocked = true;
        }

        if (!_axisLocked)
            return proposed;

        // N'appliquer QUE l'axe verrouillé
        float constrainedAngle = _lockedAxis switch
        {
            ConstrainedAxis.X => ex,
            ConstrainedAxis.Y => ey,
            ConstrainedAxis.Z => ez,
            _                 => 0f
        };

        Vector3 constrainedAxis = _lockedAxis switch
        {
            ConstrainedAxis.X => baseRotation * Vector3.right,
            ConstrainedAxis.Y => baseRotation * Vector3.up,
            ConstrainedAxis.Z => baseRotation * Vector3.forward,
            _                 => Vector3.up
        };

        return Quaternion.AngleAxis(constrainedAngle, constrainedAxis) * baseRotation;
    }

    float NormalizeAngle(float a)
    {
        while (a >  180f) a -= 360f;
        while (a < -180f) a += 360f;
        return a;
    }

    public ConstrainedAxis CurrentLock => _lockedAxis;
}