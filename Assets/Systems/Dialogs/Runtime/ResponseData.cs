using UnityEngine;

namespace Dialogs
{
	/// <summary>
	/// ResponseData represents a possible response/answer to a dialog.
	/// Can be linked to a DialogLogic to display response buttons during a conversation.
	/// </summary>
	[CreateAssetMenu(fileName = "new Response", menuName = "DialogSystem/Response", order = 2)]
	public class ResponseData : ScriptableObject
	{
		[TextArea]
		[Tooltip("The text to be displayed on the response button.")]
		public string ResponseText;

		[Tooltip("Optional icon to be displayed with the response button.")]
		public Sprite ResponseIcon;
	}
}
