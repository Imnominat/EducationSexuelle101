using System.Collections;
using UnityEngine;

/// <summary>
/// Gère l'apparition des deux tablettes vidéo une fois la séquence de plaquettes terminée.
/// Wirer : GameManager → OnSequenceCompleteEvent → TabletteVideoManager.AfficherTablettes()
/// </summary>
public class TabletteVideoManager : MonoBehaviour
{
    [Header("Tablettes")]
    [SerializeField] TabletteVideo _tablette1;
    [SerializeField] TabletteVideo _tablette2;

    [Header("Timing")]
    [SerializeField] float _delaiAvantApparition = 1f;
    [SerializeField] float _decalageEntreTablettes = 0.35f;

    void Start()
    {
        if (_tablette1 != null) _tablette1.gameObject.SetActive(false);
        if (_tablette2 != null) _tablette2.gameObject.SetActive(false);
    }

    // Appelé par GameManager.OnSequenceCompleteEvent (wirer dans l'Inspector)
    public void AfficherTablettes()
    {
        StartCoroutine(ApparitreLesDeuxTablettes());
    }

    IEnumerator ApparitreLesDeuxTablettes()
    {
        yield return new WaitForSeconds(_delaiAvantApparition);
        _tablette1?.Apparaitre();
        yield return new WaitForSeconds(_decalageEntreTablettes);
        _tablette2?.Apparaitre();
    }
}
