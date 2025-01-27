using TMPro;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class GoToWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField field;
        [SerializeField] private InputFieldValidator fieldValidator;

        [Require] private Window window;

        [Inject] private ViewController view;
        [Inject] private SelectionController selection;

        private void Start()
        {
            window.onShowed.Subscribe(() =>
            {
                field.ActivateInputField();
                field.caretPosition = field.text.Length;
            });

            field.onSubmit.AddListener((s) =>
            {
                OnGoClicked();
            });

            field.onValueChanged.AddListener((str) =>
            {
                bool isValid = TryGetAddress(str, out _);
                fieldValidator.MarkValid(isValid);
            });
        }

        public void OnGoClicked()
        {
            TryGetAddress(field.text, out int address);
            view.GoTo(address);

            int lineAddress = view.AbsToVirtualLine(address / 16);

            selection.selections.Clear();
            selection.selections.Add(new Selection(lineAddress * 16 + address % 16));
            selection.Refresh();
        }

        private bool TryGetAddress(string str, out int address)
        {
            // int address = Convert.ToInt32(new DataTable().Compute(str, null));
            if (str.StartsWith("0x"))
            {
                return int.TryParse(str.Substring(2, field.text.Length - 2), System.Globalization.NumberStyles.HexNumber, null, out address);
            }
            else if (int.TryParse(str, out address))
            {
                return true;
            }

            address = -1;
            return false;
        }
    }
}