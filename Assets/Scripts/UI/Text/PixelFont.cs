using UnityEngine;

namespace InGame
{
    [CreateAssetMenu(menuName = "UI/PixelFont")]
    public class PixelFont : ScriptableObject
    {
        public Material material;
        public Vector2Int characterSize;

        [TextArea(3, 12)]
        public string characters;

        public int spaceSize = 6;

        public Margin[] margins;

        public Vector2Int GetCharacterIndex(char character)
        {
            Vector2Int index = Vector2Int.zero;

            foreach (char fontChar in characters)
            {
                if (fontChar == character) return index;

                if (fontChar == '\n')
                {
                    index.y++;
                    index.x = 0;
                }
                else
                {
                    index.x++;
                }
            }

            Debug.Log("Unknown character: " + character);
            return index;
        }

        public Margin GetMargin(char character)
        {
            foreach (Margin margin in margins)
            {
                if (margin.characters.Contains(character))
                {
                    return margin;
                }
            }

            return default;
        }
    }

    [System.Serializable]
    public struct Margin
    {
        public string characters;
        public int left, right;
    }
}