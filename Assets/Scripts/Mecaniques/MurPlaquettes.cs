using UnityEngine;

public class MurPlaquettes : MonoBehaviour
{
    [Header("Prefab de base")]
    public GameObject plaquettePrefab;

    [Header("Données de toutes les plaquettes")]
    public PlaquetteData[] toutesLesPlaquettes;

    [Header("Disposition sur le mur")]
    [Tooltip("Laisser à 0 pour calculer automatiquement selon le nombre de plaquettes")]
    public int colonnes = 0;
    public float espacementX = 0.22f;
    public float espacementY = 0.28f;

    [Tooltip("Centre la grille horizontalement sur le mur")]
    public bool centrerHorizontalement = true;

    [Header("Fixation au mur")]
    public bool fixeesAuMur = true;
    public float epaisseurMur = 0.02f;

    void Start()
    {
        int total = toutesLesPlaquettes.Length;
        if (total == 0) return;

        // ── Calcul automatique du nombre de colonnes ──────────────────
        // On cherche la disposition la plus "carrée" possible,
        // en favorisant légèrement plus de colonnes que de rangées.
        int cols = colonnes > 0 ? colonnes : CalculerColonnesOptimales(total);
        int rows = Mathf.CeilToInt((float)total / cols);

        // Largeur totale occupée par la grille
        float largeurTotale = (cols - 1) * espacementX;

        for (int i = 0; i < total; i++)
        {
            int col = i % cols;
            int row = i / cols;

            // Dernière rangée : combien de plaquettes restantes ?
            int plaquettesDansRangee = (row == rows - 1)
                ? total - row * cols  // rangée incomplète
                : cols;               // rangée complète

            // Décalage X pour centrer la dernière rangée si incomplète
            float decalageX = 0f;
            if (centrerHorizontalement && plaquettesDansRangee < cols)
            {
                float largeurRangee = (plaquettesDansRangee - 1) * espacementX;
                decalageX = (largeurTotale - largeurRangee) / 2f;
            }

            // Offset dans le plan du mur (XY local)
            // On part du coin haut-gauche et on descend
            float offsetX = col * espacementX + decalageX;
            float offsetY = -row * espacementY; // négatif = on descend

            // Centrage global de la grille sur le pivot du mur
            if (centrerHorizontalement)
                offsetX -= largeurTotale / 2f;

            Vector3 offset = new Vector3(offsetX, offsetY, epaisseurMur);

            GameObject go = Instantiate(
                plaquettePrefab,
                transform.TransformPoint(offset),
                transform.rotation
            );

            Plaquette p = go.GetComponent<Plaquette>();
            if (p != null)
            {
                p.data = toutesLesPlaquettes[i];
                if (fixeesAuMur)
                    p.FixerAuMur();
            }
        }
    }

    /// <summary>
    /// Calcule le nombre de colonnes optimal pour approcher
    /// une disposition carrée, en favorisant plus de colonnes que de rangées.
    /// </summary>
    int CalculerColonnesOptimales(int total)
    {
        // Racine carrée = point de départ pour une grille carrée
        int cols = Mathf.RoundToInt(Mathf.Sqrt(total));

        // On s'assure qu'on ne déborde pas et qu'on couvre tous les éléments
        while (cols * Mathf.CeilToInt((float)total / cols) < total)
            cols++;

        // Favorise légèrement plus de colonnes (grille plus large que haute)
        // ce qui est plus naturel pour un mur face au joueur
        if (cols < Mathf.CeilToInt((float)total / cols))
            cols++;

        return Mathf.Clamp(cols, 1, total);
    }

    // ── Gizmo pour visualiser la grille dans l'éditeur Unity ──────────
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (toutesLesPlaquettes == null || toutesLesPlaquettes.Length == 0) return;

        int total = toutesLesPlaquettes.Length;
        int cols  = colonnes > 0 ? colonnes : CalculerColonnesOptimales(total);
        int rows  = Mathf.CeilToInt((float)total / cols);

        float largeurTotale = (cols - 1) * espacementX;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < total; i++)
        {
            int col = i % cols;
            int row = i / cols;

            int plaquettesDansRangee = (row == rows - 1)
                ? total - row * cols
                : cols;

            float decalageX = 0f;
            if (centrerHorizontalement && plaquettesDansRangee < cols)
            {
                float largeurRangee = (plaquettesDansRangee - 1) * espacementX;
                decalageX = (largeurTotale - largeurRangee) / 2f;
            }

            float offsetX = col * espacementX + decalageX;
            float offsetY = -row * espacementY;

            if (centrerHorizontalement)
                offsetX -= largeurTotale / 2f;

            Vector3 pos = transform.TransformPoint(
                new Vector3(offsetX, offsetY, epaisseurMur)
            );

            // Dessine un petit cube cyan à chaque emplacement de plaquette
            Gizmos.DrawWireCube(pos, new Vector3(0.18f, 0.10f, 0.01f));
        }
    }
#endif
}