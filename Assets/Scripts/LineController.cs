using System;
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
            FitRows(byteRowsParent);
            FitRows(charRowsParent);
        }

        private void FitRows(Transform parent)
        {
            for (int i = 0; i < 16; i++)
            {
                var child = parent.GetChild(i).GetComponent<RectTransform>();
                child.anchoredPosition = new Vector2(11 * i + (i / 4 * 6), 0);
            }
        }

        public void Refresh_Data(Line line, byte[] bytes)
        {
            rows.SetActive(true);
            empty.SetActive(false);

            lineIndexText.text = LineIndexToHex(line.index);

            for (int i = 0; i < bytes.Length; i++)
            {
                byteRows[i].Refresh(bytes[i]);
                charRows[i].Refresh(bytes[i]);
            }
        }

        public void Refresh_Empty(Line line)
        {
            rows.SetActive(false);
            empty.SetActive(true);

            lineIndexText.text = LineIndexToHex(line.index) + " - " + LineIndexToHex(line.zerosEndIndex);
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

        private string LineIndexToHex(int index)
        {
            return "0x" + (index * 16).ToString("x8");
        }
    }
}