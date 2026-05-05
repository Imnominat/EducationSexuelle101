using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneDepot : MonoBehaviour
{
    [Header("Zone")]
    public ZoneCible typeDeLaZone;

    [Header("Feedback visuel")]
    public Renderer zoneRenderer;
    public Color couleurAttente = new Color(1f, 1f, 1f, 0.3f);
    public Color couleurValidee = new Color(0f, 1f, 0f, 0.6f);
    public Color couleurErreur  = new Color(1f, 0f, 0f, 0.6f);

    [Header("Feedback sonore")]
    public AudioSource audioSource;
    public AudioClip sonSucces;
    public AudioClip sonErreur;

    [Header("Mur de validation associé")]
    public MurValidation murValidation;

    // Plaquettes correctement déposées sur cette zone
    private List<Plaquette> _plaquettesValidees = new List<Plaquette>();

    void Start()
    {
        SetCouleur(couleurAttente);
    }

    // ── Détection par trigger ─────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        Plaquette p = other.GetComponentInParent<Plaquette>();
        if (p == null)           return;
        if (p.EstDeposee())      return; // déjà validée quelque part
        if (p.EstEnCoursDeGrab()) return; // le joueur tient encore la plaquette

        bool bonneZone = p.GetZoneCible() == typeDeLaZone;
        bool positive  = p.EstPositive();

        if (bonneZone)
        {
            _plaquettesValidees.Add(p);
            p.OnDeposeCorrectement();
            JouerSon(sonSucces);
            GameManager.Instance.OnBonnePlaquette();

            // Envoie la plaquette sur le mur de validation
            if (murValidation != null)
                murValidation.AjouterPlaquette(p);
            else
                SetCouleur(couleurValidee); // fallback si pas de mur
        }
        else
        {
            // ❌ Mauvaise plaquette ou mauvaise zone
            p.OnErreur();
            JouerSon(sonErreur);
            GameManager.Instance.OnMauvaisePlaquette(p);
            StartCoroutine(ResetPlaquetteApresDelai(p));
        }
    }

    IEnumerator ResetPlaquetteApresDelai(Plaquette p)
    {
        yield return new WaitForSeconds(0.6f);
        p.ResetVisuel();
    }

    void SetCouleur(Color c)
    {
        if (zoneRenderer != null)
            zoneRenderer.material.color = c;
    }

    void JouerSon(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}