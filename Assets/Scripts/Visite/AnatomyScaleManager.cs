using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnatomyScaleManager : MonoBehaviour
{
    [Header("Références")]
    public Transform xrRig;
    public Camera xrCamera;

    [Header("Échelle")]
    public float normalScale = 1f;
    public float microScale = 1f / 300f;
    public float transitionDuration = 3f;

    [Header("UI à préserver du rétrécissement")]
    [Tooltip("Canvas enfant de la caméra (menu de zones, etc.). Étant sous xrRig, il rétrécit " +
             "avec le reste du corps du joueur : la distance main-caméra s'effondre proportionnellement " +
             "au rétrécissement, ce qui rend le rayon du contrôleur impossible à viser précisément " +
             "(le point visé ne dépend quasiment plus de l'orientation de la main). On compense donc " +
             "sa position/échelle locale pour qu'il garde une taille et une distance constantes face " +
             "au joueur, quelle que soit l'échelle du rig.")]
    public Transform[] scaleCompensatedUI;

    private Vector3[] compensatedUINormalLocalPos;
    private Vector3[] compensatedUINormalLocalScale;

    [Header("Debug clavier — désactiver en prod")]
    public bool keyboardTrigger = false;

    public bool IsTransitioning => isTransitioning;
    public bool IsMicro => isMicro;

    private bool isMicro = false;
    private bool isTransitioning = false;
    private float normalNearClip;

    void Start()
    {
        // Sauvegarder le nearClipPlane d'origine
        normalNearClip = xrCamera != null ? xrCamera.nearClipPlane : 0.01f;

        if (scaleCompensatedUI != null)
        {
            compensatedUINormalLocalPos = new Vector3[scaleCompensatedUI.Length];
            compensatedUINormalLocalScale = new Vector3[scaleCompensatedUI.Length];
            for (int i = 0; i < scaleCompensatedUI.Length; i++)
            {
                if (scaleCompensatedUI[i] == null) continue;
                compensatedUINormalLocalPos[i] = scaleCompensatedUI[i].localPosition;
                compensatedUINormalLocalScale[i] = scaleCompensatedUI[i].localScale;
            }
        }
    }

    void Update()
    {
        if (!keyboardTrigger || isTransitioning || Keyboard.current == null) return;
        if (Keyboard.current.eKey.wasPressedThisFrame && !isMicro) EnterMicroMode();
        if (Keyboard.current.qKey.wasPressedThisFrame && isMicro)  ExitMicroMode();
    }

    public void EnterMicroMode()
    {
        if (!isMicro && !isTransitioning)
            StartCoroutine(Transition(normalScale, microScale));
    }

    // Passage instantané en mode micro (utilisé lors d'une entrée directe depuis AnatomieF).
    public void EnterMicroModeInstant()
    {
        StopAllCoroutines();
        SetScale(microScale);
        isMicro = true;
        isTransitioning = false;
    }

    public void ExitMicroMode()
    {
        if (isMicro && !isTransitioning)
            StartCoroutine(Transition(microScale, normalScale));
    }

    private IEnumerator Transition(float from, float to)
    {
        isTransitioning = true;
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            SetScale(Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / transitionDuration)));
            yield return null;
        }
        SetScale(to);
        isMicro = to < from;
        isTransitioning = false;
    }

    public void SetScale(float scale)
    {
        Vector3 camBefore = xrCamera.transform.position;
        xrRig.localScale = Vector3.one * scale;
        xrRig.position += camBefore - xrCamera.transform.position;

        // Adapter le nearClipPlane pour éviter le clipping à l'intérieur du modèle.
        // Le joueur est minuscule : les parois sont très proches en world-space.
        if (xrCamera != null)
            xrCamera.nearClipPlane = normalNearClip * scale / normalScale;

        // Compenser l'échelle du rig sur l'UI enfant de la caméra (ex: menu de zones) pour
        // qu'elle garde une taille et une distance constantes face au joueur, quelle que soit
        // l'échelle courante — sinon elle rétrécit avec le reste du corps du joueur et la
        // distance main-caméra s'effondre, rendant le rayon du contrôleur impossible à viser.
        if (scaleCompensatedUI != null)
        {
            float factor = normalScale / scale;
            for (int i = 0; i < scaleCompensatedUI.Length; i++)
            {
                if (scaleCompensatedUI[i] == null) continue;
                scaleCompensatedUI[i].localPosition = compensatedUINormalLocalPos[i] * factor;
                scaleCompensatedUI[i].localScale = compensatedUINormalLocalScale[i] * factor;
            }
        }
    }
}
