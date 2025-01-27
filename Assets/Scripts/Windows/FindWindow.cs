using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class FindWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField field;
        [SerializeField] private InputFieldValidator fieldValidator;
        [SerializeField] private TextMeshProUGUI matchesText;
        [SerializeField] private TMP_Dropdown dropdown;

        [Require] private Window window;

        [Inject] private ViewController view;
        [Inject] private SelectionController selection;

        private List<Match> matches = new();
        private int currentMatch;


        public void OnFieldChange()
        {
            matches.Clear();

            fieldValidator.MarkValid(true);
            if (field.text.Length > 0)
            {
                if (dropdown.value == 0)
                {
                    try
                    {
                        byte[] pattern = field.text.ToHexBytes();
                        FindBin(pattern);
                    }
                    catch
                    {
                        fieldValidator.MarkValid(false);
                    }
                }
                else
                {
                    FindASCII();
                }
            }

            currentMatch = 0;
            OnCurrentMatchChange();
        }

        private void FindBin(byte[] pattern)
        {
            int i_pattern = 0;
            int pattern_beginAddress = 0;

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

        private void FindASCII()
        {
            byte[] pattern = System.Text.Encoding.ASCII.GetBytes(field.text);
            FindBin(pattern);
        }

        public void OnNextPrevClicked(int direction)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (direction > 0) currentMatch = matches.Count - 1;
                else currentMatch = 0;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                currentMatch = Mathf.Clamp(currentMatch + 10 * direction, 0, matches.Count - 1);
            }
            else
            {
                currentMatch = Mathf.Clamp(currentMatch + direction, 0, matches.Count - 1);
            }

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