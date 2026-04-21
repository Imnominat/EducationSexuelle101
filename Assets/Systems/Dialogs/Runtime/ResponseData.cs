// ResponseData.cs - Modification : NextDialog devient un ID de dialog
using UnityEngine;
namespace Responses
{
    [CreateAssetMenu(fileName = "new Response", menuName = "DialogSystem/Response", order = 1)]
    public class ResponseData : ScriptableObject
    {
        [TextArea]
        [Tooltip("The text to be displayed in the response.")]
        public string ResponseText;
        public AudioClip ResponseAudioClip;
        [Tooltip("The ID of the next dialog to play when this response is selected.")]
        public string NextDialogID;
    }
}