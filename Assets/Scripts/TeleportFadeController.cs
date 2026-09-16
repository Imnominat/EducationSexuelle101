using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// Déclenche le fondu (ScreenFader) pendant les téléportations XR, qu'elles viennent d'une
// Teleportation Area ou d'un Teleport Interactor : les deux passent par le(s) même(s)
// Teleportation Provider du XR Origin.
// À placer sur le XR Origin, avec une référence vers le(s) Teleportation Provider du rig.
//
// Astuce : Teleportation Provider a un champ "Delay Time" (délai avant d'appliquer le
// déplacement). On le force à au moins fadeOutDuration pour que le saut réel n'ait lieu
// qu'une fois l'écran noir.
public class TeleportFadeController : MonoBehaviour
{
    [Tooltip("Composants Teleportation Provider du XR Origin à surveiller.")]
    [SerializeField] private LocomotionProvider[] teleportationProviders;

    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private float fadeInDuration = 0.25f;

    private void OnEnable()
    {
        foreach (var provider in teleportationProviders)
        {
            if (provider == null) continue;
            provider.locomotionStarted += OnLocomotionStarted;
            provider.locomotionEnded += OnLocomotionEnded;

            if (provider is TeleportationProvider teleportationProvider)
                teleportationProvider.delayTime = Mathf.Max(teleportationProvider.delayTime, fadeOutDuration);
        }
    }

    private void OnDisable()
    {
        foreach (var provider in teleportationProviders)
        {
            if (provider == null) continue;
            provider.locomotionStarted -= OnLocomotionStarted;
            provider.locomotionEnded -= OnLocomotionEnded;
        }
    }

    private void OnLocomotionStarted(LocomotionProvider provider) => ScreenFader.Instance?.FadeOut(fadeOutDuration);

    private void OnLocomotionEnded(LocomotionProvider provider) => ScreenFader.Instance?.FadeIn(fadeInDuration);
}
