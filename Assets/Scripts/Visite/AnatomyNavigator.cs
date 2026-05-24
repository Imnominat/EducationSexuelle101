using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // si tu n'as pas TextMeshPro, remplace par UnityEngine.UI et Text

[System.Serializable]
public class AnatomyZone
{
    public string zoneName;
    public Transform waypoint;
    [TextArea] public string descriptionText;
    public AudioClip narration;
    public GameObject highlightEffect;
}

public class AnatomyNavigator : MonoBehaviour
{
    public List<AnatomyZone> zones;

    [Header("UI")]
    public TMP_Text labelText;
    public TMP_Text descriptionUI;

    [Header("Audio")]
    public AudioSource audioSource;

    private int currentZoneIndex = -1;

    public void ProgressToNextZone()
    {
        currentZoneIndex++;
        if (currentZoneIndex < zones.Count)
            ActivateZone(zones[currentZoneIndex]);
    }

    private void ActivateZone(AnatomyZone zone)
    {
        if (labelText != null)     labelText.text    = zone.zoneName;
        if (descriptionUI != null) descriptionUI.text = zone.descriptionText;

        if (audioSource != null && zone.narration != null)
            audioSource.PlayOneShot(zone.narration);

        zone.highlightEffect?.SetActive(true);
        if (currentZoneIndex > 0)
            zones[currentZoneIndex - 1].highlightEffect?.SetActive(false);
    }
}