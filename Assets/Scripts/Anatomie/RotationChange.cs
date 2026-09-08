using UnityEngine;

// A placer sur l'objet Assembly (ou un manager dédié).
// Brancher chaque cerceau (XR Knob > On Value Change) sur la méthode correspondante.
public class RotationChange : MonoBehaviour
{
    [Header("Objet a faire pivoter (Assembly)")]
    [SerializeField] Transform target;

    [Header("Handle de chaque cerceau (champ 'Handle' du XR Knob correspondant)")]
    [SerializeField] Transform handleX;
    [SerializeField] Transform handleY;
    [SerializeField] Transform handleZ;

    Vector3 m_CurrentAngles;

    void Awake()
    {
        if (target == null)
            target = transform;

        // Initialise avec l'angle courant de chaque cerceau pour éviter un saut au premier grab
        if (handleX != null) m_CurrentAngles.x = handleX.localEulerAngles.y;
        if (handleY != null) m_CurrentAngles.y = handleY.localEulerAngles.y;
        if (handleZ != null) m_CurrentAngles.z = handleZ.localEulerAngles.y;

        Apply();
    }

    // A brancher sur le XR Knob du cerceau X
    public void OnKnobXChanged(float value)
    {
        m_CurrentAngles.x = handleX.localEulerAngles.y;
        Apply();
    }

    // A brancher sur le XR Knob du cerceau Y
    public void OnKnobYChanged(float value)
    {
        m_CurrentAngles.y = handleY.localEulerAngles.y;
        Apply();
    }

    // A brancher sur le XR Knob du cerceau Z
    public void OnKnobZChanged(float value)
    {
        m_CurrentAngles.z = handleZ.localEulerAngles.y;
        Apply();
    }

    void Apply()
    {
        target.localRotation = Quaternion.Euler(m_CurrentAngles);
    }
}
