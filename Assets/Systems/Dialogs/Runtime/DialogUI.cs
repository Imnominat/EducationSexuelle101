// DialogUI.cs - Version complète avec gestion des réponses
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dialogs.Effects;
using Responses;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogs
{
    /// <summary>
    /// DialogUI manages dialog UI elements (text, icon, audio) and dynamic response buttons.
    /// </summary>
    public class DialogUI : MonoBehaviour
    {
        [Tooltip("If true, the first dialog in the Conversation list will be played automatically when the scene starts.")]
        [SerializeField] private bool m_PlayOnStart = true;
        [SerializeField] private string startingDialog = "";

        [Tooltip("List of Dialog that represents the conversation to be played.")]
        public List<DialogLogic> Conversation = new List<DialogLogic>();

        [Header("UI References")]
        [field: SerializeField] public TMP_Text DialogLabel { get; private set; }
        [SerializeField] private Image m_IconImage;
        [SerializeField] private AudioSource m_AudioSource;

        [Header("Response Buttons")]
        [Tooltip("Prefab for response buttons. Must have a ResponseButton component.")]
        [SerializeField] private ResponseButton m_ResponseButtonPrefab;
        [Tooltip("Parent transform where response buttons will be instantiated.")]
        [SerializeField] private Transform m_ResponseButtonContainer;
        [Tooltip("The Validate button — hidden when responses are available.")]
        [SerializeField] private GameObject m_ValidateButton;

        public Sprite DefaultIcon;

        private readonly List<int> _convHistory = new List<int>();
        private int _convHistoryIndex = -1;
        private List<DialogEffect> effects = new List<DialogEffect>();

        // Pool of instantiated response buttons to avoid repeated Instantiate/Destroy
        private readonly List<ResponseButton> _responseButtonPool = new List<ResponseButton>();

        public DialogLogic CurrentDialogLogic =>
            IsValidHistoryIndex(_convHistoryIndex)
                ? Conversation[_convHistory[_convHistoryIndex]]
                : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidHistoryIndex(int index) =>
            index >= 0 && index < _convHistory.Count &&
            _convHistory[index] >= 0 && IsValideDialogIndex(_convHistory[index]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValideDialogIndex(int index) =>
            index >= 0 && index < Conversation.Count && Conversation[index] != null;

        void Start()
        {
            if (DefaultIcon == null && m_IconImage != null)
                DefaultIcon = m_IconImage.sprite;

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
                startingDialog = Conversation[0].ID;
        }
#endif

        public void Validate() => CurrentDialogLogic?.OnDialogValidate.Invoke();
        public void Cancel()   => CurrentDialogLogic?.OnDialogCancel.Invoke();

        public void Repeat()
        {
            CurrentDialogLogic?.OnDialogRepeat.Invoke();
            PlayConversation(CurrentDialogLogic);
        }

        public void Previous()
        {
            if (_convHistoryIndex <= 0) return;
            CurrentDialogLogic?.OnDialogPrevious.Invoke();
            _convHistoryIndex--;
            PlayConversation(CurrentDialogLogic);
        }

        private void PlayConversation(DialogLogic dialogLogic)
        {
            DialogData dialogData = dialogLogic.Dialog;

            DialogLabel.text = dialogData.DialogText;

            if (m_IconImage != null)
                m_IconImage.sprite = dialogData.DialogIcon == null ? DefaultIcon : dialogData.DialogIcon;

            if (m_AudioSource != null)
            {
                m_AudioSource.Stop();
                m_AudioSource.clip = dialogData.DialogAudioClip;
                if (dialogData.DialogAudioClip != null)
                    m_AudioSource.Play();
            }

            effects.Clear();
            GetComponents(effects);
            foreach (var effect in effects)
            {
                effect.ResetEffect();
                effect.StartEffect();
            }

            RefreshResponseButtons(dialogLogic);

            dialogLogic.OnDialogLoaded.Invoke();
        }

        /// <summary>
        /// Shows or hides response buttons depending on whether the current dialog has responses.
        /// Uses a simple pool to avoid per-frame allocations.
        /// </summary>
        private void RefreshResponseButtons(DialogLogic dialogLogic)
        {
            bool hasResponses = dialogLogic.Responses != null && dialogLogic.Responses.Count > 0;

            // Show/hide the validate button
            if (m_ValidateButton != null)
                m_ValidateButton.SetActive(!hasResponses);

            // Hide all pooled buttons first
            foreach (var btn in _responseButtonPool)
                btn.gameObject.SetActive(false);

            if (!hasResponses || m_ResponseButtonPrefab == null || m_ResponseButtonContainer == null)
                return;

            for (int i = 0; i < dialogLogic.Responses.Count; i++)
            {
                ResponseData response = dialogLogic.Responses[i];
                if (response == null) continue;

                // Grow pool if needed
                if (i >= _responseButtonPool.Count)
                {
                    ResponseButton newBtn = Instantiate(m_ResponseButtonPrefab, m_ResponseButtonContainer);
                    _responseButtonPool.Add(newBtn);
                }

                ResponseButton button = _responseButtonPool[i];
                button.gameObject.SetActive(true);
                button.Setup(response, OnResponseSelected);
            }
        }

        /// <summary>
        /// Called when the player selects a response.
        /// Plays the optional response audio then navigates to the next dialog.
        /// </summary>
        private void OnResponseSelected(ResponseData response)
        {
            if (m_AudioSource != null && response.ResponseAudioClip != null)
            {
                m_AudioSource.Stop();
                m_AudioSource.clip = response.ResponseAudioClip;
                m_AudioSource.Play();
            }

            if (!string.IsNullOrEmpty(response.NextDialogID))
                StartConversation(response.NextDialogID);
        }

        private void StartConversation(int index)
        {
            if (index < 0 || index >= Conversation.Count)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Index {index} is out of range for the conversation list of size {Conversation.Count}.");

            DialogLogic dialogLogic = Conversation[index]
                ?? throw new NullReferenceException($"DialogLogic at index {index} is null.");

            PlayConversation(dialogLogic);

            _convHistoryIndex = _convHistory.Count;
            _convHistory.Add(index);
        }

        public void StartConversation(DialogLogic dialogLogic)
            => StartConversation(Conversation.IndexOf(dialogLogic));

        public void StartConversation(string dialogID)
            => StartConversation(Conversation.FindIndex(d => d.ID == dialogID));
    }
}