using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class FindWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField field;
        [SerializeField] private TextMeshProUGUI matchesText;

        [Require] private Window window;

        [Inject] private ViewController view;
        [Inject] private SelectionController selection;

        private List<Match> matches = new();
        private int currentMatch;


        public void OnFieldChange()
        {
            byte[] pattern = field.text.ToHexBytes();

            int i_pattern = 0;
            int pattern_beginAddress = 0;

            // bool isHalf = field.text.Length % 2 != 0;

            matches.Clear();

            if (pattern.Length > 0)
            {
                for (int i = 0; i < view.File.data.Count; i++)
                {
                    byte fileByte = view.File.data[i];
                    byte patternByte = pattern[i_pattern];

                    // if (IsMatch(fileByte, patternByte, isHalf && i_pattern == pattern.Length - 1, isHalf && i_pattern == 0))
                    if (IsMatch(fileByte, patternByte))
                    {
                        if (i_pattern == 0)
                        {
                            pattern_beginAddress = i;
                        }

                        i_pattern++;
                        if (i_pattern >= pattern.Length)
                        {
                            matches.Add(new()
                            {
                                selection = new(pattern_beginAddress, i)
                            });
                            i_pattern = 0;
                        }
                    }
                    else
                    {
                        i_pattern = 0;
                    }
                }
            }

            // selection.selections.Clear();
            // selection.selections.AddRange(matches.Select(m => m.selection));
            // selection.Refresh();

            currentMatch = 0;
            OnCurrentMatchChange();
        }

        public void OnNextClicked()
        {
            currentMatch = Mathf.Clamp(currentMatch + 1, 0, matches.Count - 1);
            OnCurrentMatchChange();
        }
        public void OnPrevClicked()
        {
            currentMatch = Mathf.Clamp(currentMatch - 1, 0, matches.Count - 1);
            OnCurrentMatchChange();
        }

        private void OnCurrentMatchChange()
        {
            if (matches.Count == 0)
            {
                matchesText.text = "no matches";
                return;
            }

            selection.selections.Clear();
            selection.selections.Add(matches[currentMatch].selection);
            selection.Refresh();

            view.GoTo(matches[currentMatch].selection.begin);

            matchesText.text = (currentMatch + 1) + " of " + matches.Count;
        }

        // private bool IsMatch(byte data, byte pattern, bool isPatternLastByte, bool isPatternFirstByte)
        private bool IsMatch(byte data, byte pattern)
        {
            // if (isPatternLastByte)
            // {
            //     byte data_left = (byte) (data >> 4);
            //     return data_left == pattern;
            // }
            // if (isPatternFirstByte)
            // {
            //     byte data_right = (byte) (data >> 4);
            //     return data_right == pattern;
            // }
            return data == pattern;
        }
    }

    public class Match
    {
        public Selection selection;
    }
}