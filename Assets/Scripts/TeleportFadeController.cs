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
            provider.locomotionStateChanged += OnLocomotionStateChanged;

            if (provider is TeleportationProvider teleportationProvider)
                teleportationProvider.delayTime = Mathf.Max(teleportationProvider.delayTime, fadeOutDuration);
        }
    }

    private void OnDisable()
    {
        foreach (var provider in teleportationProviders)
        {
            if (provider == null) continue;
            provider.locomotionStateChanged -= OnLocomotionStateChanged;
        }
    }

    // locomotionStarted/locomotionEnded ne se déclenchent qu'à l'entrée dans l'état Moving,
    // c'est-à-dire une fois le délai (Delay Time) déjà écoulé : le fondu et le saut arrivaient
    // donc quasi simultanément et l'écran ne noircissait jamais avant le déplacement. On écoute
    // plutôt locomotionStateChanged pour déclencher le fondu dès l'état Preparing (début du
    // délai), et le fondu inverse à l'état Ended (juste après le déplacement réel).
    private void OnLocomotionStateChanged(LocomotionProvider provider, LocomotionState state)
    {
        switch (state)
        {
            case LocomotionState.Preparing:
                ScreenFader.Instance?.FadeOut(fadeOutDuration);
                break;
            case LocomotionState.Ended:
                ScreenFader.Instance?.FadeIn(fadeInDuration);
                break;
        }
    }
}
