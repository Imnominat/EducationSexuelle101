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

        [Header("Responses")]
        [Tooltip("If not empty, response buttons will be shown instead of the Validate button.")]
        public List<Responses.ResponseData> Responses = new List<Responses.ResponseData>();

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
		
	}
}
