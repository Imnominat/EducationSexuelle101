using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class XRSimulatorMenu
{
    [MenuItem("XR/Désactiver Simulateur (toutes les scènes)")]
    static void DisableAll() => ApplyToAllScenes(false);

    [MenuItem("XR/Activer Simulateur (toutes les scènes)")]
    static void EnableAll() => ApplyToAllScenes(true);

    static void ApplyToAllScenes(bool enable)
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        string originalPath = activeScene.path;

        if (activeScene.isDirty)
        {
            if (!EditorUtility.DisplayDialog("Modifications non sauvegardées",
                "La scène courante a des modifications non sauvegardées. Sauvegarder avant de continuer ?",
                "Sauvegarder", "Annuler"))
                return;
            EditorSceneManager.SaveOpenScenes();
        }

        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int simulatorsFound = 0;
        int scenesModified = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool sceneModified = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var comp in root.GetComponentsInChildren<Component>(true))
                {
                    if (comp == null) continue;
                    if (comp.GetType().Name != "XRInteractionSimulator") continue;

                    if (comp.gameObject.activeSelf != enable)
                    {
                        comp.gameObject.SetActive(enable);
                        sceneModified = true;
                    }
                    simulatorsFound++;
                }
            }

            if (sceneModified)
            {
                EditorSceneManager.SaveScene(scene);
                scenesModified++;
            }
        }

        if (!string.IsNullOrEmpty(originalPath))
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);

        string action = enable ? "activé" : "désactivé";
        if (simulatorsFound == 0)
            EditorUtility.DisplayDialog("XR Simulator",
                "Aucun XRInteractionSimulator trouvé dans les scènes de Assets/Scenes.\n" +
                "Vérifiez que le composant s'appelle bien 'XRInteractionSimulator'.", "OK");
        else
            EditorUtility.DisplayDialog("XR Simulator",
                $"Simulateur {action} sur {simulatorsFound} objet(s) dans {scenesModified} scène(s) modifiée(s).", "OK");
    }
}
