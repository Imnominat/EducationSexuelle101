using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dialogs
{
	[Serializable]
	public class DialogLogic
	{
		[SerializeField] private string _id;
		public string ID => _id;
		public DialogData Dialog;

		[Space]
		[Tooltip("List of possible responses for this dialog. If empty, the dialog will be treated normally. If populated, response buttons will be displayed instead of the standard buttons.")]
		public List<ResponseData> Responses = new List<ResponseData>();


		/// <summary>
		/// Called when the button "Validate" is pressed.
		/// </summary>
		[Space]
		public UnityEvent OnDialogValidate;

		/// <summary>
		/// Called before dialog is reloaded when the "Previous" button is pressed.
		/// </summary>
		[Space]
		[Tooltip("Called before dialog is reloaded when the \"Previous\" button is pressed.")]
		public UnityEvent OnDialogPrevious;

		/// <summary>
		/// Called when the Cancel button is pressed.
		/// </summary>
		[Space]
		public UnityEvent OnDialogCancel;

		/// <summary>
		/// Called before dialog is reloaded when the "Repeat" button is pressed.
		/// </summary>
		[Space]
		[Tooltip("Called before dialog is reloaded when the \"Repeat\" button is pressed.")]
		public UnityEvent OnDialogRepeat;

		/// <summary>
		/// Called whenever a conversation finished to load (after Validate, Previous or Repeat).
		/// </summary>
		[Space]
		[Tooltip("Called whenever a dialog has finished loading (after Validate, Previous or Repeat).")]
		public UnityEvent OnDialogLoaded;

		/// <summary>
		/// Called when a response button is selected. The response index is passed as parameter.
		/// </summary>
		[Space]
		[Tooltip("Called when a response button is selected (includes response index).")]
		public UnityEvent<int> OnResponseSelected;
	}
}