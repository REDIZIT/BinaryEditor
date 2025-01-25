using TMPro;
using UnityEngine;

namespace InGame
{
    public class LineController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField lineIndexText;
        [SerializeField] private RowController[] byteRows, charRows;

        [SerializeField] private Transform byteRowsParent, charRowsParent;

        [SerializeField] private GameObject rows, empty;


        private void OnValidate()
        {
            FitRows(byteRowsParent, false);
            FitRows(charRowsParent, true);
        }

        private void FitRows(Transform parent, bool isChars)
        {
            int width = isChars ? 7 : 11;
            int bigSpacing = isChars ? 4 : 6;

            for (int i = 0; i < 16; i++)
            {
                var child = parent.GetChild(i).GetComponent<RectTransform>();
                child.anchoredPosition = new Vector2(width * i + (i / 4 * bigSpacing), 0);
            }
        }

        public void Refresh_Data(Line line, byte[] bytes)
        {
            rows.SetActive(true);
            empty.SetActive(false);

            lineIndexText.text = AddressToHex(line.index * 16);

            for (int i = 0; i < 16; i++)
            {
                if (i < bytes.Length)
                {
                    byteRows[i].Refresh(bytes[i]);
                    charRows[i].Refresh(bytes[i]);
                }
                else
                {
                    byteRows[i].Clear();
                    charRows[i].Clear();
                }
            }
        }

        public void Refresh_Empty(Line line)
        {
            rows.SetActive(false);
            empty.SetActive(true);

            int diff = line.zerosEndIndex - line.index + 1;

            lineIndexText.text = AddressToHex(line.index * 16) + " - " + AddressToHex(line.zerosEndIndex * 16 + 15) + " <color=#666>=</color> " + diff + " <color=#666>lines of</color> zeros";
        }

        public void Clear()
        {
            lineIndexText.text = "-";

            rows.SetActive(true);
            empty.SetActive(false);

            for (int i = 0; i < byteRows.Length; i++)
            {
                byteRows[i].Clear();
                charRows[i].Clear();
            }
        }

        private string AddressToHex(int address)
        {
            string hex = address.ToString("x");
            return "<color=#666>0x" + '0'.Multiply(8 - hex.Length) + "</color>" + hex;
        }
    }
}