// ResponseButton.cs - Nouveau composant à attacher au prefab de bouton
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Responses
{
    /// <summary>
    /// Component to attach to the response button prefab.
    /// Binds a ResponseData to a UI Button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ResponseButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_label == null)
                _label = GetComponentInChildren<TMP_Text>();
        }

        /// <summary>
        /// Initializes the button with a response and a callback.
        /// </summary>
        public void Setup(ResponseData response, System.Action<ResponseData> onSelected)
        {
            if (_label != null)
                _label.text = response.ResponseText;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onSelected?.Invoke(response));
        }
    }
}