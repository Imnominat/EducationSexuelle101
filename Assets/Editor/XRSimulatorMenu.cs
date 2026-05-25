using UnityEditor;
using UnityEngine;

public static class XRSimulatorMenu
{
    private const string MenuPath = "XR/Simulateur activé";
    private const string AssetPath = "Assets/Resources/SimulatorSettings.asset";

    [InitializeOnLoadMethod]
    static void Init()
    {
        EditorApplication.delayCall += RefreshCheckmark;
    }

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        var settings = GetOrCreateSettings();
        settings.simulatorEnabled = !settings.simulatorEnabled;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        RefreshCheckmark();

        string state = settings.simulatorEnabled ? "ACTIVÉ" : "DÉSACTIVÉ";
        Debug.Log($"[XR Simulator] Simulateur {state}. Rebuild nécessaire pour que la scène reflète le changement.");
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleValidate()
    {
        RefreshCheckmark();
        return true;
    }

    static void RefreshCheckmark()
    {
        var settings = Resources.Load<SimulatorSettings>("SimulatorSettings");
        Menu.SetChecked(MenuPath, settings != null && settings.simulatorEnabled);
    }

    static SimulatorSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<SimulatorSettings>(AssetPath);
        if (settings != null)
            return settings;

        settings = ScriptableObject.CreateInstance<SimulatorSettings>();
        settings.simulatorEnabled = true;
        System.IO.Directory.CreateDirectory("Assets/Resources");
        AssetDatabase.CreateAsset(settings, AssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[XR Simulator] SimulatorSettings.asset créé dans Assets/Resources/");
        return settings;
    }
}
