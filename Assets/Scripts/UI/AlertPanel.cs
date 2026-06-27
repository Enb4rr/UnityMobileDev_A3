using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class AlertPanel : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text TitleText { get; private set; }
        [field: SerializeField] public TMP_Text MessageText { get; private set; }
        [field: SerializeField] public TMP_InputField InputField { get; private set; }

        private Action onConfirmAction;

        public void ShowAlert(string title, string message, Action confirmAction)
        {
            gameObject.SetActive(true);
            
            TitleText.text = title;
            MessageText.text = message;
            InputField.text = "";
            onConfirmAction = confirmAction;
        }

        public void ConfirmAction()
        {
            onConfirmAction?.Invoke();
            gameObject.SetActive(false);
        }

        public void CancelAction()
        {
            gameObject.SetActive(false);
            InputField.text = "";
            onConfirmAction = null;
        }
    }
}
