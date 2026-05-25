using UnityEngine;

[CreateAssetMenu(fileName = "SimulatorSettings", menuName = "XR/Simulator Settings")]
public class SimulatorSettings : ScriptableObject
{
    public bool simulatorEnabled = true;

    private static SimulatorSettings _instance;

    public static SimulatorSettings Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<SimulatorSettings>("SimulatorSettings");
            return _instance;
        }
    }
}
