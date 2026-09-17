using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Menu UI listant les zones de AnatomyNavigator sous forme de boutons.
// Remplace le déplacement/téléportation libre (WaypointTeleportAim) : cliquer sur un bouton
// téléporte directement au waypoint de la zone correspondante. Le bouton de la zone actuelle
// est grisé (interactable = false, via le ColorBlock.disabledColor du bouton).
public class ZoneMenuUI : MonoBehaviour
{
    [Header("Références")]
    public AnatomyNavigator anatomyNavigator;
    public SplineNavigator splineNavigator;

    [Tooltip("Désactivé au démarrage pour mettre en pause l'ancien système de téléportation libre.")]
    public WaypointTeleportAim teleportAimToPause;

    [Header("UI")]
    [Tooltip("Bouton prefab avec un TMP_Text enfant. Un exemplaire est instancié par zone.")]
    public Button buttonPrefab;
    [Tooltip("Parent des boutons instanciés (idéalement avec un Layout Group).")]
    public Transform buttonsContainer;

    private readonly List<Button> zoneButtons = new();

    void Start()
    {
        if (teleportAimToPause != null)
            teleportAimToPause.enabled = false;

        BuildButtons();

        if (anatomyNavigator != null)
        {
            anatomyNavigator.OnZoneChanged += UpdateButtonsInteractable;
            UpdateButtonsInteractable(anatomyNavigator.CurrentZoneIndex);
        }
    }

    void OnDestroy()
    {
        if (anatomyNavigator != null)
            anatomyNavigator.OnZoneChanged -= UpdateButtonsInteractable;
    }

    void BuildButtons()
    {
        if (anatomyNavigator == null || buttonPrefab == null || buttonsContainer == null) return;

        for (int i = 0; i < anatomyNavigator.zones.Count; i++)
        {
            int zoneIndex = i; // capture pour la closure
            AnatomyZone zone = anatomyNavigator.zones[i];

            Button btn = Instantiate(buttonPrefab, buttonsContainer);
            btn.gameObject.SetActive(true);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = zone.zoneName;

            btn.onClick.AddListener(() => TeleportToZone(zoneIndex));
            zoneButtons.Add(btn);
        }
    }

    public void TeleportToZone(int zoneIndex)
    {
        if (anatomyNavigator == null || splineNavigator == null) return;
        if (zoneIndex < 0 || zoneIndex >= anatomyNavigator.zones.Count) return;

        AnatomyZone zone = anatomyNavigator.zones[zoneIndex];
        if (zone.waypoint == null) return;

        if (splineNavigator.JumpToWaypoint(zone.waypoint))
            anatomyNavigator.ActivateZoneByIndex(zoneIndex);
    }

    void UpdateButtonsInteractable(int currentZoneIndex)
    {
        for (int i = 0; i < zoneButtons.Count; i++)
            zoneButtons[i].interactable = i != currentZoneIndex;
    }
}
