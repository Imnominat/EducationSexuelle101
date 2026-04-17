using Dialogs;
using UnityEngine;

namespace Dialogs.Examples
{
	/// <summary>
	/// Simple automatic dialog branch manager.
	/// No coding needed - just configure NextDialogAfterResponse in the inspector!
	/// </summary>
	public class DialogResponseExample : MonoBehaviour
	{
		[SerializeField] private DialogUI dialogUI;

		private void OnEnable()
		{
			if (dialogUI == null)
				dialogUI = GetComponent<DialogUI>();

			// Subscribe to response selection events for all dialogs
			foreach (var dialogLogic in dialogUI.Conversation)
			{
				dialogLogic.OnResponseSelected.AddListener(OnResponseSelected);
			}
		}

		private void OnDisable()
		{
			if (dialogUI != null)
			{
				foreach (var dialogLogic in dialogUI.Conversation)
				{
					dialogLogic.OnResponseSelected.RemoveListener(OnResponseSelected);
				}
			}
		}

		/// <summary>
		/// Automatically load the next dialog based on which response was selected.
		/// </summary>
		private void OnResponseSelected(int responseIndex)
		{
			DialogLogic currentDialog = dialogUI.CurrentDialogLogic;
			if (currentDialog == null) return;

			// Check if there's a configured next dialog for this response
			if (responseIndex >= 0 && responseIndex < currentDialog.NextDialogAfterResponse.Count)
			{
				string nextDialogID = currentDialog.NextDialogAfterResponse[responseIndex];
				
				// Only proceed if the next dialog ID is not empty
				if (!string.IsNullOrEmpty(nextDialogID))
				{
					Debug.Log($"Response {responseIndex} selected → Loading dialog: {nextDialogID}");
					dialogUI.StartConversation(nextDialogID);
				}
			}
		}
	}

	/// <summary>
	/// Advanced example: A more sophisticated dialog manager that tracks choices and triggers events.
	/// Use this if you want to do more than just load the next dialog.
	/// </summary>
	public class AdvancedDialogManager : MonoBehaviour
	{
		[SerializeField] private DialogUI dialogUI;
		private DialogChoiceTracker choiceTracker = new DialogChoiceTracker();

		private void Start()
		{
			if (dialogUI != null)
			{
				foreach (var dialogLogic in dialogUI.Conversation)
				{
					dialogLogic.OnResponseSelected.AddListener(OnResponseSelected);
				}
			}
		}

		private void OnResponseSelected(int responseIndex)
		{
			DialogLogic currentDialog = dialogUI.CurrentDialogLogic;
			if (currentDialog != null)
			{
				// Track the choice
				choiceTracker.RecordChoice(currentDialog.ID, responseIndex);
				
				// Do your custom logic
				ProcessGameLogic(currentDialog.ID, responseIndex);

				// Then auto-load the next dialog (if configured)
				if (responseIndex >= 0 && responseIndex < currentDialog.NextDialogAfterResponse.Count)
				{
					string nextDialogID = currentDialog.NextDialogAfterResponse[responseIndex];
					if (!string.IsNullOrEmpty(nextDialogID))
					{
						dialogUI.StartConversation(nextDialogID);
					}
				}
			}
		}

		private void ProcessGameLogic(string dialogID, int responseIndex)
		{
			// Your custom game logic here
			// Example: dialogue has consequence on world state
			
			if (dialogID == "MoralChoiceDialog")
			{
				if (responseIndex == 0)
				{
					// Good choice
					GivePlayerPoints(10);
				}
				else
				{
					// Bad choice
					GivePlayerPoints(-10);
				}
			}
		}

		private void GivePlayerPoints(int points)
		{
			Debug.Log($"Player gained {points} points");
			// Implement actual point system here
		}
	}

	/// <summary>
	/// Helper class to track player choices throughout the game.
	/// </summary>
	public class DialogChoiceTracker
	{
		private System.Collections.Generic.Dictionary<string, int> choices = 
			new System.Collections.Generic.Dictionary<string, int>();

		public void RecordChoice(string dialogID, int responseIndex)
		{
			if (!choices.ContainsKey(dialogID))
				choices[dialogID] = responseIndex;
			else
				choices[dialogID] = responseIndex;

			Debug.Log($"Dialog '{dialogID}' - Response {responseIndex} recorded");
		}

		public int GetPlayerChoice(string dialogID)
		{
			return choices.ContainsKey(dialogID) ? choices[dialogID] : -1;
		}

		public void Reset()
		{
			choices.Clear();
		}
	}
}
