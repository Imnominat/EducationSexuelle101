using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaquette", menuName = "SeriousGame/Plaquette")]
public class PlaquetteData : ScriptableObject
{
    public string id;
    public string label;
    public ZoneCible zoneCible;
}

// Les zones où on peut déposer une plaquette
public enum ZoneCible
{
    Corps,
    Poubelle
}