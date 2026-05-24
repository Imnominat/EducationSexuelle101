using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public int zoneIndex;
    private AnatomyNavigator navigator;

    void Start() => navigator = FindAnyObjectByType<AnatomyNavigator>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("XRCamera") || other.CompareTag("Player"))
            navigator.ProgressToNextZone();
    }
}