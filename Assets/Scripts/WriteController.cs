using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class WriteController : MonoBehaviour
    {
        [Inject] private ViewController view;
        [Inject] private SelectionController selection;

        private int addressIndex;
        private bool isEditingSecondChar;

        private void Start()
        {
            selection.onSelectionChange.Subscribe(() =>
            {
                addressIndex = 0;
                isEditingSecondChar = false;
            });
        }

        private void Update()
        {
            if (selection.selections.Count == 0) return;
            if (view.File == null) return;

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                Erase();
            }
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                Cutout();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Space();
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                Paste();
            }
            else if (string.IsNullOrEmpty(Input.inputString) == false)
            {
                EditWithExtend();
            }
        }

        private void Erase()
        {
            foreach (int address in selection.EnumerateSelectedAddresses())
            {
                if (address < view.File.data.Count)
                {
                    view.File.data[address] = 0;
                }
            }

            view.Refresh();
        }

        private void Cutout()
        {
            foreach (int address in selection.EnumerateSelectedAddresses().Reverse())
            {
                if (address < view.File.data.Count)
                {
                    view.File.data.RemoveAt(address);
                }
            }

            view.HandleLines();
            view.Refresh();
        }

        private void Space()
        {
            var s = selection.selections[0];
            int address = s.begin;

            view.File.data.Insert(address, 0);
            
            view.HandleLines();
            view.Refresh();
        }

        private void Paste()
        {
            var s = selection.selections.Last();
            int address = s.begin;

            int selectedSize = s.end - s.begin;
            int clipboardSize = Mathf.CeilToInt(GUIUtility.systemCopyBuffer.Length / 2f);

            if (clipboardSize > selectedSize)
            {
                // Extend
                for (int i = 0; i < clipboardSize - selectedSize - 1; i++)
                {
                    view.File.data.Insert(address, 0);
                }
            }
            else if (selectedSize >= clipboardSize)
            {
                // Shrink
                for (int i = 0; i < selectedSize - clipboardSize + 1; i++)
                {
                    view.File.data.RemoveAt(address);
                }
            }


            // Overwrite
            for (int i = 0; i < GUIUtility.systemCopyBuffer.Length; i += 2)
            {
                string hex = string.Concat(GUIUtility.systemCopyBuffer[i], GUIUtility.systemCopyBuffer[i + 1]);
                byte data = byte.Parse(hex, NumberStyles.HexNumber);
                view.File.data[address + i / 2] = data;
            }

            view.Refresh();
        }

        private void EditWithExtend()
        {
            if (byte.TryParse(Input.inputString[0].ToString(), NumberStyles.HexNumber, null, out byte b) == false) return;

            addressIndex++;

            int[] addresses = selection.EnumerateSelectedAddresses().ToArray();

            int address = -1;
            if (addresses.Length > 1)
            {
                if (addressIndex < addresses.Length)
                {
                    address = addresses[addressIndex];
                }
            }
            else
            {
                address = this.selection.selections[0].begin;
            }

            if (address != -1)
            {
                byte data = isEditingSecondChar ? (byte) ((view.File.data[address] << 4) + b) : b;

                if (address >= view.File.data.Count)
                {
                    view.ExtendFile(data);
                }
                else
                {
                    view.File.data[address] = data;
                }
            }


            if (isEditingSecondChar)
            {
                var s = this.selection.selections[0];
                s.begin++;
                s.end++;
                this.selection.selections[0] = s;
                selection.Refresh();

                isEditingSecondChar = false;
            }
            else
            {
                isEditingSecondChar = true;
            }

            view.HandleLines();
            view.Refresh();
        }
    }
}