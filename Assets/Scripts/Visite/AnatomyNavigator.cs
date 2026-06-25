using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class AnatomyZone
{
    public string zoneName;
    public Transform waypoint;
    [TextArea] public string descriptionText;
    public AudioClip narration;
    public GameObject highlightEffect;

    [Tooltip("Canvas WorldSpace positionné dans la scène près du waypoint. " +
             "Doit avoir : 1er TMP = titre, 2ème TMP = description. " +
             "Le laisser désactivé par défaut.")]
    public GameObject worldPanel;
}

public class AnatomyNavigator : MonoBehaviour
{
    public List<AnatomyZone> zones;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Joueur")]
    [Tooltip("XR Camera ou tête du joueur — pour le billboard et la détection de proximité")]
    public Transform playerHead;

    [Header("Proximité")]
    [Tooltip("Distance (unités monde) à partir de laquelle une zone s'active")]
    public float activationRadius = 0.5f;

    private int currentZoneIndex = -1;
    private readonly List<GameObject> activePanels = new();

    void Update()
    {
        if (playerHead == null) return;

        // Billboard : les panneaux actifs font face au joueur
        foreach (var panel in activePanels)
        {
            if (panel == null) continue;
            Vector3 dir = panel.transform.position - playerHead.position;
            if (dir.sqrMagnitude > 0.0001f)
                panel.transform.rotation = Quaternion.LookRotation(dir);
        }

        // Proximité : activer la zone dont le waypoint est le plus proche du joueur
        CheckZoneProximity();
    }

    void CheckZoneProximity()
    {
        float closestDist = float.MaxValue;
        int closestZone = -1;

        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].waypoint == null) continue;
            float dist = Vector3.Distance(playerHead.position, zones[i].waypoint.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestZone = i;
            }
        }

        if (closestZone >= 0 && closestDist <= activationRadius && closestZone != currentZoneIndex)
            ActivateZoneByIndex(closestZone);
    }

    // Appelé par ZoneTrigger ou CheckZoneProximity
    public void ActivateZoneByIndex(int index)
    {
        if (index < 0 || index >= zones.Count) return;
        if (index == currentZoneIndex) return;

        if (currentZoneIndex >= 0 && currentZoneIndex < zones.Count)
            DeactivateZone(zones[currentZoneIndex]);

        currentZoneIndex = index;
        AnatomyZone zone = zones[currentZoneIndex];

        // Panneau flottant
        if (zone.worldPanel != null)
        {
            zone.worldPanel.SetActive(true);
            activePanels.Add(zone.worldPanel);

            // Remplir automatiquement les textes du panneau
            TMP_Text[] texts = zone.worldPanel.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length > 0) texts[0].text = zone.zoneName;
            if (texts.Length > 1) texts[1].text = zone.descriptionText;
        }

        if (zone.highlightEffect != null)
            zone.highlightEffect.SetActive(true);

        if (audioSource != null && zone.narration != null)
            audioSource.PlayOneShot(zone.narration);
    }

    public void ResetNavigation()
    {
        foreach (var zone in zones)
            DeactivateZone(zone);

        currentZoneIndex = -1;
        activePanels.Clear();
    }

    private void DeactivateZone(AnatomyZone zone)
    {
        if (zone.worldPanel != null)
        {
            zone.worldPanel.SetActive(false);
            activePanels.Remove(zone.worldPanel);
        }
        if (zone.highlightEffect != null)
            zone.highlightEffect.SetActive(false);
    }
}
