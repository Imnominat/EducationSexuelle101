using UnityEngine;
using System.Collections.Generic;

public class AnchorManager : MonoBehaviour
{
    [Header("Pièces à ancrer (glisser les cubes ici)")]
    public List<SelectableBlock> blocks;

    void Start()
    {
        foreach (var block in blocks)
            block.InitAnchor();
    }
}