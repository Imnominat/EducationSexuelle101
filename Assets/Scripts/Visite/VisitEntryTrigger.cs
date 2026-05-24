using UnityEngine;
using System.Collections;

// Placer ce script sur un GameObject avec un Box Collider (isTrigger = true)
// positionné à l'entrée de la vulve.
// Quand le joueur entre dedans : rétrécissement + activation de la navigation.
[RequireComponent(typeof(Collider))]
public class VisitEntryTrigger : MonoBehaviour
{
    public AnatomyScaleManager scaleManager;
    public SplineNavigator splineNavigator;

    [Tooltip("Tag du collider joueur à détecter (ex : 'Player' ou 'XRCamera')")]
    public string playerTag = "Player";

    private bool triggered = false;

    void Start() => GetComponent<Collider>().isTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag(playerTag)) return;
        triggered = true;

        scaleManager.EnterMicroMode();
        StartCoroutine(ActivateWhenReady());
    }

    IEnumerator ActivateWhenReady()
    {
        // Attendre que la transition de scale soit terminée
        yield return new WaitUntil(() => !scaleManager.IsTransitioning);
        splineNavigator.Activate();
    }
}
