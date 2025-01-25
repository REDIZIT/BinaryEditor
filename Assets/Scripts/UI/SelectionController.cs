using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class SelectionController : MonoBehaviour
    {
        [Inject] private SelectionBlock.Pool pool;

        [Inject] private ViewController view;
        [Inject] private Canvas canvas;

        private bool[,] map;

        private List<Selection> selections = new();
        private List<SelectionBlock> blocks = new();

        private void Start()
        {
            map = new bool[16, view.LineInstsCount];
            view.onChanged.Subscribe(Refresh);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 screenPos = Input.mousePosition;
                Vector2Int cellPos = ScreenPosToCell(screenPos / canvas.scaleFactor);

                if (cellPos != -Vector2Int.one)
                {
                    int address = (view.Scroll + cellPos.y) * 16 + cellPos.x;
                    address = Mathf.Clamp(address, 0, view.File.data.Length);


                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        selections.Add(new Selection(address));
                    }
                    else if (Input.GetKey(KeyCode.LeftShift) && selections.Count > 0)
                    {
                        Selection lastSelection = selections.Last();
                        if (address > lastSelection.end) selections.Add(new(lastSelection.end, address));
                        else selections.Add(new(address, lastSelection.begin));
                    }
                    else
                    {
                        selections.Clear();
                        selections.Add(new Selection(address));
                    }

                    Refresh();
                }
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

        private Vector2Int ScreenPosToCell(Vector2 screen)
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
                    int screenAddress = view.AbsToScreenAddress(i);
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