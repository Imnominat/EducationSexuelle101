using UnityEngine;
using System.Collections.Generic;

public class MurValidation : MonoBehaviour
{
    [Header("Zone associée")]
    public ZoneCible zoneAssociee;

    [Header("Disposition")]
    public float espacementX = 0.22f;
    public float espacementY = 0.28f;
    public int colonnes = 0; // 0 = auto

    [Header("Visuel")]
    public Color couleurValidee = Color.green;

    private List<GameObject> _plaquettesAffichees = new List<GameObject>();

    // ── Appelé par ZoneDepot quand une plaquette est validée ──────────
    public void AjouterPlaquette(Plaquette p)
    {
        // Détache la plaquette de son parent actuel
        p.transform.SetParent(this.transform);

        // Recalcule la grille avec la nouvelle plaquette
        _plaquettesAffichees.Add(p.gameObject);
        ReorganiserGrille();
    }

    // ── Recalcule les positions de toutes les plaquettes sur le mur ───
    void ReorganiserGrille()
    {
        int total = _plaquettesAffichees.Count;
        if (total == 0) return;

        int cols = colonnes > 0 ? colonnes : CalculerColonnesOptimales(total);
        float largeurTotale = (cols - 1) * espacementX;

        for (int i = 0; i < total; i++)
        {
            int col = i % cols;
            int row = i / cols;

            int plaquettesDansRangee = (row == Mathf.CeilToInt((float)total / cols) - 1)
                ? total - row * cols
                : cols;

            float decalageX = 0f;
            if (plaquettesDansRangee < cols)
            {
                float largeurRangee = (plaquettesDansRangee - 1) * espacementX;
                decalageX = (largeurTotale - largeurRangee) / 2f;
            }

            float offsetX = col * espacementX + decalageX - largeurTotale / 2f;
            float offsetY = -row * espacementY;

            // Anime le déplacement vers la nouvelle position
            StartCoroutine(DeplacerVers(
                _plaquettesAffichees[i],
                transform.TransformPoint(new Vector3(offsetX, offsetY, 0f))
            ));
        }
    }

    // ── Déplacement fluide vers la position cible ─────────────────────
    System.Collections.IEnumerator DeplacerVers(GameObject go, Vector3 cible)
    {
        float duree = 0.3f;
        float temps = 0f;
        Vector3 depart = go.transform.position;

        while (temps < duree)
        {
            temps += Time.deltaTime;
            go.transform.position = Vector3.Lerp(depart, cible, temps / duree);
            go.transform.rotation = this.transform.rotation; // aligne sur le mur
            yield return null;
        }

        go.transform.position = cible;
    }

    int CalculerColonnesOptimales(int total)
    {
        int cols = Mathf.RoundToInt(Mathf.Sqrt(total));
        while (cols * Mathf.CeilToInt((float)total / cols) < total)
            cols++;
        if (cols < Mathf.CeilToInt((float)total / cols))
            cols++;
        return Mathf.Clamp(cols, 1, total);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        int total = Mathf.Max(1, _plaquettesAffichees?.Count ?? 1);
        int cols  = colonnes > 0 ? colonnes : CalculerColonnesOptimales(total);
        float largeurTotale = (cols - 1) * espacementX;

        for (int i = 0; i < total; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float offsetX = col * espacementX - largeurTotale / 2f;
            float offsetY = -row * espacementY;
            Vector3 pos = transform.TransformPoint(new Vector3(offsetX, offsetY, 0f));
            Gizmos.DrawWireCube(pos, new Vector3(0.18f, 0.10f, 0.01f));
        }
    }
#endif
}