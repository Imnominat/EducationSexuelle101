using UnityEngine;

/// Désactive le GameObject du simulateur XR si SimulatorSettings.simulatorEnabled est false.
/// À attacher sur le GameObject racine du simulateur dans chaque scène.
public class XRSimulatorController : MonoBehaviour
{
    void Awake()
    {
        var settings = SimulatorSettings.Instance;
        if (settings != null && !settings.simulatorEnabled)
            gameObject.SetActive(false);
    }
}
