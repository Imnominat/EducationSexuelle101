// Menu : Tools > Importer CSV
// Place tes fichiers CSV dans Assets/Data/CSV/ puis lance l'import.
//
// Format Plaquettes.csv  : ID, label, zoneCible
// Format Reponses.csv    : ID, texte, audioclip, nextDialogID
// Format Dialogues.csv   : ID, texte, audioclip, Reponse1ID, Reponse2ID, Reponse3ID
//
// Les audioclip sont des noms de fichier (sans extension) présents dans le projet.
// Les assets générés vont dans Assets/Data/Generated/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Dialogs;
using Responses;

public static class CsvImporter
{
    const string CSV_DIR        = "Assets/Data/CSV";
    const string OUT_PLAQUETTES = "Assets/Data/Generated/Plaquettes";
    const string OUT_REPONSES   = "Assets/Data/Generated/Reponses";
    const string OUT_DIALOGS    = "Assets/Data/Generated/DialogData";
    const string OUT_CONVS      = "Assets/Data/Generated/Conversations";

    // ─── Menus ───────────────────────────────────────────────────────────────

    [MenuItem("Tools/Importer CSV/Tout importer")]
    static void ImportAll()
    {
        ImportPlaquettes();
        var reponseMap = ImportReponses();
        ImportDialogues(reponseMap);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CsvImporter] Import terminé.");
    }

    [MenuItem("Tools/Importer CSV/Plaquettes")]
    static void ImportPlaquettesMenu()
    {
        ImportPlaquettes();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Importer CSV/Dialogues et Réponses")]
    static void ImportDialoguesMenu()
    {
        var reponseMap = ImportReponses();
        ImportDialogues(reponseMap);
        AssetDatabase.SaveAssets();
    }

    // ─── Plaquettes ──────────────────────────────────────────────────────────

    static void ImportPlaquettes()
    {
        string path = CsvPath("Plaquettes.csv");
        if (!File.Exists(path)) { Debug.LogError($"[CsvImporter] Introuvable : {path}"); return; }

        EnsureFolder(OUT_PLAQUETTES);
        var rows = ParseCsv(File.ReadAllText(path));
        if (rows.Count < 2) return;

        var h = rows[0];
        int iId    = Col(h, "ID");
        int iLabel = Col(h, "label");
        int iZone  = Col(h, "zoneCible");

        int created = 0, updated = 0;
        for (int i = 1; i < rows.Count; i++)
        {
            var r = rows[i];
            string id = Get(r, iId);
            if (string.IsNullOrWhiteSpace(id)) continue;

            if (!Enum.TryParse(Get(r, iZone), true, out ZoneCible zone))
            {
                Debug.LogWarning($"[CsvImporter] ZoneCible inconnue '{Get(r, iZone)}' ligne {i + 1}, ignorée.");
                continue;
            }

            string assetPath = $"{OUT_PLAQUETTES}/Plaquette_{id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<PlaquetteData>(assetPath);
            bool isNew = data == null;
            if (isNew) data = ScriptableObject.CreateInstance<PlaquetteData>();

            data.id        = id;
            data.label     = Get(r, iLabel);
            data.zoneCible = zone;

            if (isNew) { AssetDatabase.CreateAsset(data, assetPath); created++; }
            else       { EditorUtility.SetDirty(data); updated++; }
        }

        Debug.Log($"[CsvImporter] Plaquettes — {created} créées, {updated} mises à jour.");
    }

    // ─── Réponses ────────────────────────────────────────────────────────────

    static Dictionary<string, ResponseData> ImportReponses()
    {
        var map  = new Dictionary<string, ResponseData>();
        string path = CsvPath("Reponses.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[CsvImporter] Introuvable : {path}"); return map; }

        EnsureFolder(OUT_REPONSES);
        var rows = ParseCsv(File.ReadAllText(path));
        if (rows.Count < 2) return map;

        var h = rows[0];
        int iId    = Col(h, "ID");
        int iTexte = Col(h, "texte");
        int iAudio = Col(h, "audioclip");
        int iNext  = Col(h, "nextDialogID");

        int created = 0, updated = 0;
        for (int i = 1; i < rows.Count; i++)
        {
            var r = rows[i];
            string id = Get(r, iId);
            if (string.IsNullOrWhiteSpace(id)) continue;

            string assetPath = $"{OUT_REPONSES}/Reponse_{id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ResponseData>(assetPath);
            bool isNew = data == null;
            if (isNew) data = ScriptableObject.CreateInstance<ResponseData>();

            data.ResponseText      = Get(r, iTexte);
            data.NextDialogID      = Get(r, iNext);
            data.ResponseAudioClip = FindAudioClip(Get(r, iAudio));

            if (isNew) { AssetDatabase.CreateAsset(data, assetPath); created++; }
            else       { EditorUtility.SetDirty(data); updated++; }

            map[id] = data;
        }

        Debug.Log($"[CsvImporter] Réponses — {created} créées, {updated} mises à jour.");
        return map;
    }

    // ─── Dialogues → DialogData + ConversationData ───────────────────────────

    static void ImportDialogues(Dictionary<string, ResponseData> reponseMap)
    {
        string path = CsvPath("Dialogues.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[CsvImporter] Introuvable : {path}"); return; }

        EnsureFolder(OUT_DIALOGS);
        EnsureFolder(OUT_CONVS);

        var rows = ParseCsv(File.ReadAllText(path));
        if (rows.Count < 2) return;

        var h = rows[0];
        int iId    = Col(h, "ID");
        int iTexte = Col(h, "texte");
        int iAudio = Col(h, "audioclip");
        int iR1    = Col(h, "Reponse1ID");
        int iR2    = Col(h, "Reponse2ID");
        int iR3    = Col(h, "Reponse3ID");

        // Déterminer le nom de la conversation depuis le nom du fichier CSV si possible,
        // sinon utiliser "Pharmacie" comme défaut.
        string convAssetPath = $"{OUT_CONVS}/Conversation_Pharmacie.asset";
        var conv = AssetDatabase.LoadAssetAtPath<ConversationData>(convAssetPath);
        bool isNewConv = conv == null;
        if (isNewConv) conv = ScriptableObject.CreateInstance<ConversationData>();
        conv.Dialogs.Clear();

        int created = 0, updated = 0;
        for (int i = 1; i < rows.Count; i++)
        {
            var r = rows[i];
            string id = Get(r, iId);
            if (string.IsNullOrWhiteSpace(id)) continue;

            // DialogData asset
            string dialogAssetPath = $"{OUT_DIALOGS}/Dialog_{id}.asset";
            var dialogData = AssetDatabase.LoadAssetAtPath<DialogData>(dialogAssetPath);
            bool isNew = dialogData == null;
            if (isNew) dialogData = ScriptableObject.CreateInstance<DialogData>();

            dialogData.DialogText      = Get(r, iTexte);
            dialogData.DialogAudioClip = FindAudioClip(Get(r, iAudio));

            if (isNew) { AssetDatabase.CreateAsset(dialogData, dialogAssetPath); created++; }
            else       { EditorUtility.SetDirty(dialogData); updated++; }

            // DialogLogic entry
            var logic = new DialogLogic(id);
            logic.Dialog = dialogData;

            foreach (string rid in new[] { Get(r, iR1), Get(r, iR2), Get(r, iR3) })
            {
                if (string.IsNullOrWhiteSpace(rid)) continue;
                if (reponseMap.TryGetValue(rid, out var resp))
                    logic.Responses.Add(resp);
                else
                    Debug.LogWarning($"[CsvImporter] Réponse '{rid}' référencée dans dialogue '{id}' introuvable.");
            }

            conv.Dialogs.Add(logic);
        }

        if (isNewConv) AssetDatabase.CreateAsset(conv, convAssetPath);
        else           EditorUtility.SetDirty(conv);

        Debug.Log($"[CsvImporter] Dialogues — {created} créés, {updated} mis à jour. ConversationData : {convAssetPath}");
    }

    // ─── Utilitaires ─────────────────────────────────────────────────────────

    static string CsvPath(string filename)
        => Path.Combine(Application.dataPath, "Data", "CSV", filename);

    static string Get(List<string> row, int index)
        => index >= 0 && index < row.Count ? row[index].Trim() : string.Empty;

    static int Col(List<string> headers, string name)
    {
        int i = headers.FindIndex(h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
        if (i < 0) Debug.LogWarning($"[CsvImporter] Colonne '{name}' introuvable dans les en-têtes.");
        return i;
    }

    static AudioClip FindAudioClip(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        string[] guids = AssetDatabase.FindAssets($"{name} t:AudioClip");
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[CsvImporter] AudioClip '{name}' introuvable dans le projet.");
            return null;
        }
        if (guids.Length > 1)
            Debug.LogWarning($"[CsvImporter] Plusieurs AudioClips nommés '{name}', le premier est utilisé.");
        return AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    static void EnsureFolder(string assetPath)
    {
        string[] parts = assetPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    // ─── Parser CSV (gère les champs entre guillemets) ────────────────────────

    static List<List<string>> ParseCsv(string content)
    {
        var result = new List<List<string>>();
        var lines  = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            result.Add(ParseCsvLine(line));
        }

        return result;
    }

    static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        int i = 0;
        while (i < line.Length)
        {
            if (line[i] == '"')
            {
                i++;
                var sb = new System.Text.StringBuilder();
                while (i < line.Length)
                {
                    if (line[i] == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    { sb.Append('"'); i += 2; }
                    else if (line[i] == '"')
                    { i++; break; }
                    else
                    { sb.Append(line[i++]); }
                }
                fields.Add(sb.ToString());
                if (i < line.Length && line[i] == ',') i++;
            }
            else
            {
                int start = i;
                while (i < line.Length && line[i] != ',') i++;
                fields.Add(line.Substring(start, i - start));
                if (i < line.Length) i++;
            }
        }
        return fields;
    }
}
