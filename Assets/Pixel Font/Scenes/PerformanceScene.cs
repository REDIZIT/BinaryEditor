using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace InGame
{
    public class PerformanceScene : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI proText;
        [SerializeField] private PixelText pixelText;

        [SerializeField] private List<string> randomStrings = new();

        [SerializeField] private int rebuildPerFrame = 1;
        [SerializeField] private bool isPixelFrame;

        private int stringIndex;

        private void Start()
        {
            Vector2Int size = new(128, 64);

            for (int s = 0; s < 3; s++)
            {
                char[] chars = new char[size.x * size.y];
                for (int i = 0; i < size.y; i++)
                {
                    for (int j = 0; j < size.x; j++)
                    {
                        chars[i * size.x + j] = (char)('a' + (s + i) % 22);
                    }

                    chars[i * size.x + size.x - 1] = '\n';
                }

                string str = new string(chars);
                randomStrings.Add(str);
            }
        }

        private void Update()
        {
            isPixelFrame = !isPixelFrame;

            if (isPixelFrame)
            {
                Stopwatch pixel = Stopwatch.StartNew();
                for (int i = 0; i < rebuildPerFrame; i++)
                {
                    pixelText.text = randomStrings[(stringIndex + i) % randomStrings.Count];
                }
                pixel.Stop();

                Debug.Log($"Pixel set text: {pixel.ElapsedMilliseconds} ms ({pixel.ElapsedTicks} ticks)");
            }
            else
            {
                Stopwatch pro = Stopwatch.StartNew();
                for (int i = 0; i < rebuildPerFrame; i++)
                {
                    proText.text = randomStrings[(stringIndex + i) % randomStrings.Count];
                }
                pro.Stop();

                Debug.Log($"TMP set text: {pro.ElapsedMilliseconds} ms ({pro.ElapsedTicks} ticks)");
            }

            stringIndex = (stringIndex + 1) % randomStrings.Count;
        }
    }
}