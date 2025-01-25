using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    public class ViewController : MonoBehaviour
    {
        public int LineInstsCount => lineInsts.Length;
        public int Scroll => scroll;
        public BinaryFile File => file;

        public SmartAction onChanged = new();

        [SerializeField] private LineController prefab;
        [SerializeField] private Transform container;

        [SerializeField] private Scrollbar scrollbar;

        private LineController[] lineInsts = new LineController[48];

        public List<Line> lines = new();

        private int scroll;
        private BinaryFile file;

        private void Start()
        {
            for (int i = 0; i < lineInsts.Length; i++)
            {
                var inst = Instantiate(prefab.gameObject, container).GetComponent<LineController>();
                lineInsts[i] = inst;

                inst.Clear();

                var rect = inst.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -i * 12);
            }

            ClearLines();
        }

        public void Show(BinaryFile file)
        {
            this.file = file;

            if (file == null)
            {
                ClearLines();
            }
            else
            {
                HandleLines();
                OnChange();
            }
        }

        public void GoTo(int address)
        {
            int addressLineIndex = address / 16;

            scroll = AbsToVirtualLine(addressLineIndex);

            OnChange();
        }

        public int AbsToScreenAddress(int address)
        {
            return address - scroll * 16;
        }

        public int VirtualToAbsLine(int virtualLine)
        {
            int absLine = 0;

            for (int i = 0; i < virtualLine; i++)
            {
                if (i >= lines.Count) return -1;

                Line line = lines[i];

                absLine++;

                if (line.zerosEndIndex != 0)
                {
                    absLine += line.zerosEndIndex - line.index;
                }
            }

            return absLine;
        }
        public int AbsToVirtualLine(int absLine)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];

                if (line.zerosEndIndex == 0)
                {
                    if (line.index == absLine)
                    {
                        return i;
                    }
                }
                else
                {
                    if (absLine >= line.index && absLine <= line.zerosEndIndex)
                    {
                        return i;
                    }
                }
            }

            return -1;

            // for (int i = 0; i < absLine; i++)
            // {
            //     if (i >= lines.Count) return -1;
            //
            //     Line line = lines[virtualLine];
            //     virtualLine++;
            //
            //     if (line.zerosEndIndex != 0)
            //     {
            //         i = line.zerosEndIndex;
            //     }
            // }
            //
            // return virtualLine;
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

        private void ClearLines()
        {
            foreach (LineController line in lineInsts)
            {
                line.Clear();
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


            if (lines.Count > 0)
            {
                scrollbar.size = Mathf.Clamp(lineInsts.Length / (float) lines.Count, 0.05f, 1);
                scrollbar.SetValueWithoutNotify(scroll / (float) lines.Count);
            }
            else
            {
                scrollbar.size = 1;
                scrollbar.SetValueWithoutNotify(0);
            }

            onChanged.Fire();
        }

        public void OnSliderChanged(float value)
        {
            scroll = (int) (value * lines.Count);
            OnChange();
        }
    }

    [System.Serializable]
    public struct Line
    {
        public int index;
        public int zerosEndIndex;
    }
}