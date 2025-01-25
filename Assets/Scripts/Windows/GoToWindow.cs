using TMPro;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class GoToWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField field;

        [Require] private Window window;

        [Inject] private ViewController view;

        private void Start()
        {
            window.onShowed.Subscribe(() =>
            {
                field.ActivateInputField();
                field.caretPosition = field.text.Length;
            });
        }

        public void OnGoClicked()
        {
            // int address = Convert.ToInt32(new DataTable().Compute(field.text, null));
            int address;

            if (field.text.StartsWith("0x"))
            {
                address = int.Parse(field.text.Substring(2, field.text.Length - 2), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                address = int.Parse(field.text);
            }

            view.GoTo(address);
        }
    }
}