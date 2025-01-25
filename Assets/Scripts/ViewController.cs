using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class ViewController : MonoBehaviour
    {
        [SerializeField] private LineController prefab;
        [SerializeField] private Transform container;

        private LineController[] lineInsts = new LineController[32];

        private List<Line> lines = new();

        private int scroll;
        private BinaryFile file;

        private void Start()
        {
            for (int i = 0; i < lineInsts.Length; i++)
            {
                var inst = Instantiate(prefab.gameObject, container).GetComponent<LineController>();
                lineInsts[i] = inst;
                inst.Clear();
            }
            OnChange();
        }

        public void Show(BinaryFile file)
        {
            this.file = file;

            HandleLines();
            OnChange();
        }

        private void HandleLinesRaw()
        {
            lines.Clear();

            int lastLineIndex = Mathf.CeilToInt(file.data.Length / 16f);

            for (int i = 0; i < lastLineIndex; i++)
            {
                lines.Add(new Line()
                {
                    index = i,
                });
            }
        }
        private void HandleLines()
        {
            lines.Clear();
            bool isCollectingZeroedLines = false;
            Line line = new();

            int linesCount = Mathf.CeilToInt(file.data.Length / 16f);

            for (int i = 0; i < linesCount; i++)
            {
                bool isThisLineZeroed = true;
                for (int j = 0; j < 16; j++)
                {
                    int byteIndex = i * 16 + j;
                    if (byteIndex >= file.data.Length)
                    {
                        isThisLineZeroed = false;
                        break;
                    }

                    if (file.data[byteIndex] != 0)
                    {
                        isThisLineZeroed = false;
                        break;
                    }
                }

                if (isThisLineZeroed)
                {
                    if (isCollectingZeroedLines == false)
                    {
                        isCollectingZeroedLines = true;
                        line.index = i;
                    }

                    line.zerosEndIndex = i;
                }
                else
                {
                    if (isCollectingZeroedLines)
                    {
                        isCollectingZeroedLines = false;
                        lines.Add(line); // Push collected line
                        line.zerosEndIndex = 0;
                    }

                    line.index = i;
                    lines.Add(line); // Push current line
                }
            }
        }

        private void Update()
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                scroll -= (int)Input.mouseScrollDelta.y;
                OnChange();
            }
        }

        private void OnChange()
        {
            // int lastLineIndex = data.Length / 16 - 1;

            for (int i = 0; i < lineInsts.Length; i++)
            {
                LineController lineInst = lineInsts[i];

                int dataLineIndex = scroll + i;


                if (dataLineIndex < 0 || dataLineIndex > lines.Count - 1)
                {
                    lineInst.Clear();
                }
                else
                {
                    Line line = lines[dataLineIndex];
                    if (line.zerosEndIndex != 0)
                    {
                        lineInst.Refresh_Empty(line);
                    }
                    else
                    {
                        int byteIndex = line.index * 16;
                        byte[] bytes;
                        if (line.index == lines.Count - 1)
                        {
                            bytes = file.data[byteIndex..];
                        }
                        else
                        {
                            bytes = file.data[byteIndex..(byteIndex + 16)];
                        }

                        lineInst.Refresh_Data(line, bytes);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct Line
    {
        public int index;
        public int zerosEndIndex;
    }
}