using UnityEngine;

public class MurPlaquettes : MonoBehaviour
{
    [Header("Prefab de base")]
    public GameObject plaquettePrefab;

    [Header("Données de toutes les plaquettes")]
    public PlaquetteData[] toutesLesPlaquettes;

    [Header("Disposition sur le mur")]
    public int colonnes = 5;           // nombre de plaquettes par rangée horizontale
    public float espacementX = 0.22f;  // espace entre plaquettes (gauche/droite)
    public float espacementY = 0.28f;  // espace entre rangées (haut/bas)
    public float offsetYBase = 0f;     // hauteur du bas du mur (ajuste selon la taille du joueur)

    [Header("Fixation au mur")]
    public bool fixeesAuMur = true;    // les plaquettes sont collées jusqu'à ce qu'on les prenne
    public float epaisseurMur = 0.02f; // léger décalage vers l'avant (Z) pour qu'elles soient visibles

    void Start()
    {
        for (int i = 0; i < toutesLesPlaquettes.Length; i++)
        {
            int col = i % colonnes;
            int row = i / colonnes;

            // Position en grille XY (mur = plan vertical)
            Vector3 offset = new Vector3(
                col * espacementX,
                row * espacementY + offsetYBase,
                epaisseurMur
            );

            // La plaquette fait face au joueur (rotation du parent = rotation du mur)
            GameObject go = Instantiate(
                plaquettePrefab,
                transform.TransformPoint(offset),
                transform.rotation
            );

            Plaquette p = go.GetComponent<Plaquette>();
            if (p != null)
            {
                p.data = toutesLesPlaquettes[i];

                // Si fixées au mur : on désactive la gravité jusqu'à ce qu'on saisisse
                if (fixeesAuMur)
                    p.FixerAuMur();
            }
        }
    }
}