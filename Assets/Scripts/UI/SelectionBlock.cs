using UnityEngine;
using Zenject;

namespace InGame
{
    public class SelectionBlock : MonoBehaviour
    {
        [SerializeField] private GameObject left, right, top, bottom;

        private int BYTE_ROW_OFFSET => Constants.BYTE_ROW_OFFSET;
        private int ROW_WIDTH => Constants.ROW_WIDTH;
        private int ROW_SPACING => Constants.ROW_SPACING;
        private int ROW_BIG_SPACING => Constants.ROW_BIG_SPACING;
        private int LINE_HEIGHT => Constants.LINE_HEIGHT;

        private RectTransform rect;
        private int x;
        private int y;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Refresh(int x, int y, bool[,] map)
        {
            this.x = x;
            this.y = y;

            int bigSpacingsCount = x / 4;
            int spacingsCount = x;

            Vector2 pos = new Vector2(BYTE_ROW_OFFSET + x * ROW_WIDTH + spacingsCount * ROW_SPACING + bigSpacingsCount * ROW_BIG_SPACING, -y * LINE_HEIGHT);





            bool left = Has(map, -1, 0);
            bool right = Has(map, 1, 0);
            bool top = Has(map, 0, -1);
            bool bottom = Has(map, 0, 1);

            Vector2 size = new Vector2(ROW_WIDTH, LINE_HEIGHT);

            if (right)
            {
                size.x += 1;
                if (x % 4 == 3) size.x += ROW_BIG_SPACING;
            }

            rect.anchoredPosition = pos;
            rect.sizeDelta = size;




            this.left.SetActive(!left);
            this.right.SetActive(!right);
            this.top.SetActive(!top);
            this.bottom.SetActive(!bottom);
        }

        private bool Has(bool[,] map, int xOffset, int yOffset)
        {
            int askX = x + xOffset;
            int askY = y + yOffset;

            if (askX < 0 || askX >= map.GetLength(0)) return false;
            if (askY < 0 || askY >= map.GetLength(1)) return false;

            return map[askX, askY];
        }

        public class Pool : MonoMemoryPool<int, int, bool[,], SelectionBlock>
        {
            protected override void Reinitialize(int x, int y, bool[,] map, SelectionBlock inst)
            {
                inst.Refresh(x, y, map);
            }
        }
    }
}