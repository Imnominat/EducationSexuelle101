using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dialogs.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogs
{
	/// <summary>
	/// DialogUI is a MonoBehaviour responsible for managing the dialog UI elements (text, icon, audio).
	/// </summary>
	public class DialogUI : MonoBehaviour
	{
		/// <summary>
		/// If true, the first dialog in the Conversation list will be played automatically when the scene starts.
		/// </summary>
		[Tooltip("If true, the first dialog in the Conversation list will be played automatically when the scene starts.")]
		[SerializeField] private bool m_PlayOnStart = true;
		[SerializeField] private string startingDialog = "";

		/// <summary>
		/// List of DialogLogic that represents the conversation to be played by this DialogUI.
		/// Each DialogLogic contains the data and events for a single dialog entry in the conversation.
		/// </summary>
		[Tooltip("List of Dialog that represents the conversation to be played.")]
		public List<DialogLogic> Conversation = new List<DialogLogic>();

		[Header("UI References")]
		[field: SerializeField] public TMP_Text DialogLabel { get; private set; }
		[SerializeField] private Image m_IconImage;
		[SerializeField] private AudioSource m_AudioSource;

		[Header("Response UI References")]
		[SerializeField] private Transform m_ResponseContainer;
		[SerializeField] private Button m_ResponseButtonPrefab;

		/// <summary>
		/// The default sprite to use for the dialog icon when a dialog does not have a specific icon assigned.
		/// </summary>
		public Sprite DefaultIcon;

		/// <summary>
		/// List of indices corresponding to the dialogs that have been played in the current conversation.
		/// This allows us to navigate through the conversation history (ex: Previous button).
		/// </summary>
		private readonly List<int> _convHistory = new List<int>();
		/// <summary>
		/// Index of the current dialog in the conversation history
		/// </summary>
		private int _convHistoryIndex = -1;
		private List<DialogEffect> effects = new List<DialogEffect>();
		private List<Button> _currentResponseButtons = new List<Button>();
		public DialogLogic CurrentDialogLogic => IsValidHistoryIndex(_convHistoryIndex) ? Conversation[_convHistory[_convHistoryIndex]] : null;

		/// <summary>
		/// Checks if the given index is valid for the conversation history and that the corresponding dialog logic exists in the Conversation list.
		/// </summary>
		/// <param name="index">The index in the conversation history list to validate</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsValidHistoryIndex(int index) => index >= 0 && index < _convHistory.Count && _convHistory[index] >= 0 && IsValideDialogIndex(_convHistory[index]);
		/// <summary>
		/// Checks if the given index is valid for the Conversation list and that the corresponding dialog logic is not null.
		/// </summary>
		/// <param name="index"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsValideDialogIndex(int index) => index >= 0 && index < Conversation.Count && Conversation[index] != null;


		void Start()
		{
			if (DefaultIcon == null && m_IconImage != null)
			{
				DefaultIcon = m_IconImage.sprite;
			}
			if (m_PlayOnStart && Conversation.Count > 0)
			{
				_convHistoryIndex = 0;
				StartConversation(0);
			}
		}

#if UNITY_EDITOR
		void OnValidate()
		{
			if (string.IsNullOrEmpty(startingDialog) && Conversation.Count > 0)
			{
				startingDialog = Conversation[0].ID;
			}
		}
#endif

		/// <summary>
		/// Pass the validation event to the current dialog logic. (ex: progressing to the next dialog ; fast-forwarding the current dialog text ; ...)
		/// </summary>
		public void Validate()
		{
			CurrentDialogLogic.OnDialogValidate.Invoke();
		}

		/// <summary>
		/// Pass the cancel event to the current dialog logic. (ex: closing the dialog)
		/// </summary>
		public void Cancel()
		{
			CurrentDialogLogic.OnDialogCancel.Invoke();
		}

		/// <summary>
		/// Repeats the current dialog.
		/// </summary>
		/// <remarks>
		/// Note: This method does not update the conversation history since we are just repeating the current dialog.
		/// </remarks>
		public void Repeat()
		{
			CurrentDialogLogic.OnDialogRepeat.Invoke();
			PlayConversation(CurrentDialogLogic);
		}

		/// <summary>
		/// Reads the previous message in the conversation (ex: Previous dialog)
		/// </summary>
		public void Previous()
		{
			if (_convHistoryIndex <= 0)
				return;
			CurrentDialogLogic.OnDialogPrevious.Invoke();
			_convHistoryIndex--;
			PlayConversation(CurrentDialogLogic);
		}

		/// <summary>
		/// Plays the given dialog logic by updating the UI elements (text, icon, audio) accordingly.
		/// </summary>
		/// <param name="dialogLogic">The dialog logic to play</param>
		private void PlayConversation(DialogLogic dialogLogic)
		{
			// cache localy the dialog data for faster access and better readability
			DialogData dialogData = dialogLogic.Dialog;

			// Update Text
			DialogLabel.text = dialogData.DialogText;

			// Update Icon
			if (m_IconImage != null)
			{
				m_IconImage.sprite = dialogData.DialogIcon == null ? DefaultIcon : dialogData.DialogIcon;
			}

			// Update Audio
			if (m_AudioSource != null)
			{
				AudioClip audioClip = dialogData.DialogAudioClip;
				m_AudioSource.Stop();
				m_AudioSource.clip = audioClip;
				if (audioClip != null)
				{
					m_AudioSource.Play();
				}
			}

			// (Re)Start Effects
			effects.Clear();
			GetComponents(effects);
			foreach (var effect in effects)
			{
				effect.ResetEffect();
				effect.StartEffect();
			}

			// Handle Response Buttons
			if (dialogLogic.Responses != null && dialogLogic.Responses.Count > 0)
			{
				DisplayResponseButtons(dialogLogic);
			}
			else
			{
				ClearResponseButtons();
			}
		}

		/// <summary>
		/// Starts a conversation by its index in the Conversation list.
		/// This method also updates the conversation history to allow navigation through previous dialogs.
		/// </summary>
		/// <param name="index">The index in the Conversation list.</param>
		/// <exception cref="ArgumentOutOfRangeException">Happens when the index is out of range of the conversation list.</exception>
		/// <exception cref="NullReferenceException">Happens when the DialogLogic at the given index is null.</exception>
		private void StartConversation(int index)
		{
			if (index < 0 || index >= Conversation.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range for the conversation list of size {Conversation.Count}.");
			}
			DialogLogic dialogLogic = Conversation[index] ?? throw new NullReferenceException($"DialogLogic at index {index} is null.");

			PlayConversation(dialogLogic);

			// Update conversation history
			_convHistoryIndex = _convHistory.Count;
			_convHistory.Add(index);
		}

		/// <summary>
		/// Starts a conversation by its DialogLogic reference.<br/>
		/// </summary>
		/// <remarks>
		/// Note: The given DialogLogic must be part of the Conversation list, otherwise an exception will be thrown.
		/// </remarks>
		/// <param name="dialogLogic">The DialogLogic to play from the Conversation list.</param>
		public void StartConversation(DialogLogic dialogLogic)
			=> StartConversation(Conversation.IndexOf(dialogLogic));

		/// <summary>
		/// Starts a conversation by its DialogID.<br/>
		/// </summary>
		/// <remarks>
		/// Note: The given DialogID must be part of the Conversation list, otherwise an exception will be thrown.
		/// </remarks>
		/// <param name="dialogID">The DialogID of the conversation to start. (See <see cref="DialogData.DialogID"/>)</param>
		public void StartConversation(string dialogID)
			=> StartConversation(Conversation.FindIndex(d => d.ID == dialogID));

		/// <summary>
		/// Clears all the response buttons from the UI.
		/// </summary>
		private void ClearResponseButtons()
		{
			foreach (var button in _currentResponseButtons)
			{
				Destroy(button.gameObject);
			}
			_currentResponseButtons.Clear();
		}

		/// <summary>
		/// Displays response buttons for the given dialog logic.
		/// Creates buttons dynamically based on the responses in the dialog logic.
		/// </summary>
		/// <param name="dialogLogic">The dialog logic containing the responses to display</param>
		private void DisplayResponseButtons(DialogLogic dialogLogic)
		{
			// Clear previous buttons
			ClearResponseButtons();

			// Return early if no container or no prefab is set
			if (m_ResponseContainer == null || m_ResponseButtonPrefab == null)
			{
				Debug.LogWarning("ResponseContainer or ResponseButtonPrefab is not assigned in DialogUI. Response buttons cannot be displayed.");
				return;
			}

			// Create buttons for each response
			for (int i = 0; i < dialogLogic.Responses.Count; i++)
			{
				ResponseData response = dialogLogic.Responses[i];
				if (response == null) continue;

				Button button = Instantiate(m_ResponseButtonPrefab, m_ResponseContainer);
				button.gameObject.SetActive(true);

				// Set button text
				TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
				if (buttonText != null)
				{
					buttonText.text = response.ResponseText;
				}

				// Capture the response index in a closure
				int responseIndex = i;
				button.onClick.AddListener(() => OnResponseButtonClicked(responseIndex));

				_currentResponseButtons.Add(button);
			}
		}

		/// <summary>
		/// Handles the response button click event.
		/// Invokes the OnResponseSelected event and any custom logic for that response.
		/// </summary>
		/// <param name="responseIndex">The index of the selected response</param>
		private void OnResponseButtonClicked(int responseIndex)
		{
			if (CurrentDialogLogic == null || responseIndex < 0 || responseIndex >= CurrentDialogLogic.Responses.Count)
				return;

			CurrentDialogLogic.OnResponseSelected?.Invoke(responseIndex);
		}
	}
}