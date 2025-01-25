using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class SelectionController : MonoBehaviour
    {
        public SmartAction onSelectionChange = new();

        [Inject] private SelectionBlock.Pool pool;

        [Inject] private ViewController view;
        [Inject] private Canvas canvas;

        private bool[,] map;

        public List<Selection> selections = new();
        private List<SelectionBlock> blocks = new();

        private int verticalRectFirstSelection;

        private void Start()
        {
            map = new bool[16, view.LineInstsCount];
            view.onChanged.Subscribe(Refresh);
        }

        private void Update()
        {
            if (!Input.GetKey(KeyCode.LeftControl) || !Input.GetKey(KeyCode.LeftShift))
            {
                verticalRectFirstSelection = -1;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 screenPos = Input.mousePosition;
                Vector2Int cellPos = ScreenPosToCell(screenPos / canvas.scaleFactor);

                if (cellPos != -Vector2Int.one)
                {
                    int address = view.VirtualToAbsLine(view.Scroll + cellPos.y) * 16 + cellPos.x;
                    address = Mathf.Clamp(address, 0, view.File.data.Count);


                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                    {
                        //
                        // Rect selection
                        //
                        if (verticalRectFirstSelection == -1)
                        {
                            // If first time rect used
                            verticalRectFirstSelection = selections.Count - 1;
                        }
                        else
                        {
                            selections.RemoveRange(verticalRectFirstSelection + 1, selections.Count - verticalRectFirstSelection - 1);
                        }

                        Selection lastSelection = selections[verticalRectFirstSelection];

                        Vector2 a = new Vector2(lastSelection.end % 16, lastSelection.end / 16);
                        Vector2 b = new Vector2(address % 16, address / 16);

                        Vector2 min = new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
                        Vector2 max = new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

                        int a_x = (int)min.x;
                        int b_x = (int)max.x;
                        int delta_x = b_x - a_x;

                        int a_y = (int)min.y;
                        int b_y = (int)max.y;
                        int delta_y = b_y - a_y;

                        for (int y = 0; y <= delta_y; y++)
                        {
                            int begin = a_y * 16 + a_x + y * 16;
                            int end = begin + delta_x;
                            selections.Add(new(begin, end));
                        }
                    }
                    else if (Input.GetKey(KeyCode.LeftControl))
                    {
                        //
                        // Single selection
                        //
                        selections.Add(new Selection(address));
                    }
                    else if (Input.GetKey(KeyCode.LeftShift) && selections.Count > 0)
                    {
                        //
                        // Continues list selection
                        //
                        Selection lastSelection = selections.Last();
                        if (address > lastSelection.end) selections.Add(new(lastSelection.end, address));
                        else selections.Add(new(address, lastSelection.begin));
                    }
                    else
                    {
                        //
                        // Drop selection and select single
                        //
                        selections.Clear();
                        selections.Add(new Selection(address));
                    }

                    onSelectionChange.Fire();
                    Refresh();
                }
            }


            if (selections.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    selections.Clear();
                    onSelectionChange.Fire();
                    Refresh();
                }
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                {
                    CopySelection();
                }


                int x = Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0;
                int y = Input.GetKeyDown(KeyCode.DownArrow) ? -1 : Input.GetKeyDown(KeyCode.UpArrow) ? 1 : 0;
                int offset = -y * 16 + x;

                if (offset != 0)
                {
                    var s = selections[selections.Count - 1];
                    s.begin += offset;
                    s.end += offset;
                    selections[selections.Count - 1] = s;

                    onSelectionChange.Fire();
                    Refresh();
                }
            }
        }

        private void CopySelection()
        {
            if (selections.Count == 0) return;

            List<string> chars = new();

            foreach (int address in EnumerateSelectedAddresses())
            {
                byte b = view.File.data[address];
                chars.Add(b.ToString("x2"));
            }

            // string str = Encoding.ASCII.GetString(byteArray); // Copy as ASCII characters
            string str = string.Concat(chars);

            GUIUtility.systemCopyBuffer = str;
        }

        public IEnumerable<int> EnumerateSelectedAddresses()
        {
            HashSet<int> selectedAddresses = new();

            foreach (Selection selection in selections)
            {
                int selectionBeginLine = selection.begin / 16;
                int selectionEndLine = selection.end / 16;

                int realBegin = view.VirtualToAbsLine(selectionBeginLine) * 16 + selection.begin % 16;

                int realEnd = view.VirtualToAbsLine(selectionEndLine) * 16 + selection.end % 16;

                for (int i = realBegin; i <= realEnd; i++)
                {
                    selectedAddresses.Add(i);
                }
            }

            foreach (int address in selectedAddresses.OrderBy(a => a))
            {
                yield return address;
            }
        }

        private void ClearBlocks()
        {
            foreach (var block in blocks)
            {
                pool.Despawn(block);
            }
            blocks.Clear();
        }

        public Vector2Int ScreenPosToCell(Vector2 screen)
        {
            RectTransform viewRect = view.GetComponent<RectTransform>();

            int y = (int) ((Screen.height / canvas.scaleFactor - screen.y) / Constants.LINE_HEIGHT);

            float baseX = viewRect.offsetMin.x + Constants.BYTE_ROW_OFFSET;

            for (int x = 0; x < 16; x++)
            {
                float cellBeginX = baseX + x * Constants.ROW_WIDTH;
                cellBeginX += Constants.ROW_SPACING * x;
                cellBeginX += Constants.ROW_BIG_SPACING * (x / 4);

                float cellEndX = cellBeginX + Constants.ROW_WIDTH + 1; // Ignore spacing (by adding 1) to better user experience (no uninterectable pixel-gaps)

                if (screen.x >= cellBeginX && screen.x < cellEndX)
                {
                    return new Vector2Int(x, y);
                }
            }

            return -Vector2Int.one;
        }

        public void Refresh()
        {
            ClearMap();

            foreach (Selection selection in selections)
            {
                for (int i = selection.begin; i <= selection.end; i++)
                {
                    int virtualAddress = view.AbsToVirtualLine(i / 16) * 16 + i % 16;
                    int screenAddress = view.VirtualToScreenAddress(virtualAddress);
                    if (screenAddress >= 0 && screenAddress < map.GetLength(0) * map.GetLength(1))
                    {
                        map[screenAddress % 16, screenAddress / 16] = true;
                    }
                }
            }

            ClearBlocks();
            InstantiateBlocks();
        }

        private void InstantiateBlocks()
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y])
                    {
                        // SelectionBlock inst = Instantiate(prefab.gameObject, container).GetComponent<SelectionBlock>();
                        // inst.Refresh(x, y, map);
                        SelectionBlock inst = pool.Spawn(x, y, map);
                        blocks.Add(inst);
                    }
                }
            }
        }

        private void ClearMap()
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = false;
                }
            }
        }
    }

    public struct Selection
    {
        public int begin;
        public int end;

        public Selection(int cell)
        {
            begin = cell;
            end = cell;
        }
        public Selection(int begin, int end)
        {
            this.begin = begin;
            this.end = end;
        }
    }
}