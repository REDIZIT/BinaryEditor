using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class StatusBarController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;

        [Inject] private SelectionController selection;
        [Inject] private ViewController view;
        [Inject] private Canvas canvas;

        private List<string> words = new();

        private void Update()
        {
            words.Clear();

            Vector2Int cellPos = selection.ScreenPosToCell(Input.mousePosition / canvas.scaleFactor);
            if (cellPos != -Vector2Int.one)
            {
                int realLine = view.VirtualToAbsLine(view.Scroll + cellPos.y);
                words.Add("Line: 0x" + realLine.ToString("x2"));

                int realAddress = realLine * 16 + cellPos.x;
                words.Add("Address: 0x" + realAddress.ToString("x"));
            }

            statusText.text = string.Join(", ", words);
        }
    }
}