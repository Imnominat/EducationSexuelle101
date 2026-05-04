using UnityEngine;

namespace Dialogs
{
	[CreateAssetMenu(fileName = "new Dialog", menuName = "DialogSystem/Dialog", order = 1)]
	public class DialogData : ScriptableObject
	{
		[TextArea]
		[Tooltip("The text to be displayed in the dialog. Can be left empty if you only want to use audio or an icon.")]
		public string DialogText;
		public AudioClip DialogAudioClip;
		public Sprite DialogIcon;
	}
}