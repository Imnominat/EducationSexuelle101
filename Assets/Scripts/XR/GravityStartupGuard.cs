using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

// Sur casque réel, la position absolue rapportée par le tracking XR au tout début de la session
// peut être très différente du point de spawn placé dans la scène (espace de suivi/Guardian décalé
// par rapport à l'endroit physique du joueur), ce qui téléporte le XR Origin loin de la salle dès le
// lancement puis le fait tomber (plus de sol praticable en dessous). On bloque la gravité et on
// recentre le rig sur son point de spawn dès que la hauteur de tête rapportée redevient plausible.
[RequireComponent(typeof(GravityProvider))]
public class GravityStartupGuard : MonoBehaviour
{
    [SerializeField] float minValidHeadHeight = 0.3f;
    [SerializeField] float maxWaitSeconds = 3f;

    GravityProvider gravityProvider;
    XROrigin xrOrigin;
    Vector3 spawnPosition;

    void Awake()
    {
        gravityProvider = GetComponent<GravityProvider>();
        xrOrigin = GetComponentInParent<XROrigin>();
        if (xrOrigin != null)
            spawnPosition = xrOrigin.transform.position;
    }

    void OnEnable()
    {
        gravityProvider.useGravity = false;
        StartCoroutine(WaitForValidTracking());
    }

    IEnumerator WaitForValidTracking()
    {
        float elapsed = 0f;
        while (elapsed < maxWaitSeconds)
        {
            if (xrOrigin == null || xrOrigin.CameraInOriginSpaceHeight >= minValidHeadHeight)
                break;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (xrOrigin != null)
        {
            var desiredCameraWorldPos = new Vector3(
                spawnPosition.x,
                spawnPosition.y + xrOrigin.CameraInOriginSpaceHeight,
                spawnPosition.z);
            xrOrigin.MoveCameraToWorldLocation(desiredCameraWorldPos);
        }

        gravityProvider.useGravity = true;
    }
}
