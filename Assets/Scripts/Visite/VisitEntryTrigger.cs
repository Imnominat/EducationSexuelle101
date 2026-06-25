using UnityEngine;
using System.Collections;

// Placer ce script sur un GameObject avec un Box Collider (isTrigger = true)
// positionné à l'entrée du modèle.
[RequireComponent(typeof(Collider))]
public class VisitEntryTrigger : MonoBehaviour
{
    public AnatomyScaleManager scaleManager;
    public SplineNavigator splineNavigator;

    [Tooltip("Tag du collider joueur à détecter")]
    public string playerTag = "Player";

    [Tooltip("Progrès maximum (0-1) pour qu'une sortie physique soit acceptée. " +
             "Empêche une sortie accidentelle en début de parcours.")]
    public float exitProgressThreshold = 0.15f;

    [Tooltip("Secondes après l'activation pendant lesquelles OnTriggerExit est ignoré. " +
             "Évite la sortie parasite si le trigger bouge avec le modèle.")]
    public float exitCooldownAfterActivation = 2f;

    private bool triggered = false;
    private float activationTime = -999f;

    void Start() => GetComponent<Collider>().isTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag(playerTag)) return;
        triggered = true;

        scaleManager.EnterMicroMode();
        StartCoroutine(ActivateWhenReady());
    }

    // Le joueur sort physiquement de la zone d'entrée
    void OnTriggerExit(Collider other)
    {
        if (!triggered || !other.CompareTag(playerTag)) return;
        if (!splineNavigator.IsActive) return;

        // Ignorer les sorties dans les premières secondes après activation :
        // quand UpdatePivot() déplace anatomyPivot, le trigger bouge avec lui
        // et Unity pense que le joueur vient d'en sortir.
        if (Time.time - activationTime < exitCooldownAfterActivation) return;

        if (splineNavigator.CurrentProgress < exitProgressThreshold)
            Exit();
    }

    IEnumerator ActivateWhenReady()
    {
        yield return new WaitUntil(() => !scaleManager.IsTransitioning);
        activationTime = Time.time;
        splineNavigator.Activate();
    }

    // Appelé par SplineNavigator (sortie clavier) ou OnTriggerExit (sortie physique)
    public void Exit()
    {
        if (!splineNavigator.IsActive) return;

        splineNavigator.Deactivate();
        scaleManager.ExitMicroMode();
        StartCoroutine(ResetWhenReady());
    }

    IEnumerator ResetWhenReady()
    {
        yield return new WaitUntil(() => !scaleManager.IsTransitioning);
        triggered = false;
    }
}
