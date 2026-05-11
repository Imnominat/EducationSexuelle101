using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScannerController : MonoBehaviour
{
    public Material materialBasic;   // bleu
    public Material materialTrue;    // vert
    public Material materialFalse;   // rouge

    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = materialBasic; // at the begining, set to blue
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HumanPositive"))
        {
            meshRenderer.material = materialFalse;
        }
        else if (other.CompareTag("HumanNegative"))
        {
            meshRenderer.material = materialTrue;
        }
    }

    void OnTriggerExit(Collider other)
    {
        meshRenderer.material = materialBasic; // back to blue when exiting
    }
}