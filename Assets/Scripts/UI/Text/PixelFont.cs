using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    [CreateAssetMenu(menuName = "UI/PixelFont")]
    public class PixelFont : ScriptableObject
    {
        public Material material;
        public Vector2Int characterSize;

        public List<CharacterRect> characterRects;

        [TextArea(3, 12)]
        public string characters;

        public int spaceSize = 6;
        public int characterSpacing = 1;

        [Header("Font generator")]
        public GenerateMode generationMode;
        public bool doGenerate;

        [Header("- From sprites settings")]
        public Texture2D fontSheet;

        [Header("- From width groups settings")]
        public WidthGroup[] widthGroups;

        private Dictionary<char, Vector2Int> indexByChar = new();
        private Dictionary<char, CharacterRect> rectByChar = new();

        public enum GenerateMode
        {
            FromWidthGroups,
            FromSpriteSheet,
        }

        private void OnValidate()
        {
            if (doGenerate)
            {
                doGenerate = false;

                if (generationMode == GenerateMode.FromWidthGroups) PixelFontGenerator.GenerateFromWidthGroups(this);
                if (generationMode == GenerateMode.FromSpriteSheet) PixelFontGenerator.GenerateFromSprites(this);
            }
        }

        public Vector2Int GetCharacterIndex(char character)
        {
            if (indexByChar.TryGetValue(character, out Vector2Int index))
            {
                return index;
            }

            index = Vector2Int.zero;

            foreach (char fontChar in characters)
            {
                if (fontChar == character)
                {
                    indexByChar.Add(character, index);
                    return index;
                }

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

            // Debug.Log("Unknown character: " + character);
            indexByChar.Add(character, index);
            return index;
        }

        public CharacterRect GetCharacterRect(char character)
        {
            if (rectByChar.TryGetValue(character, out CharacterRect cacheRect))
            {
                return cacheRect;
            }

            foreach (CharacterRect rect in characterRects)
            {
                if (rect.character == character)
                {
                    rectByChar.Add(character, rect);
                    return rect;
                }
            }

            // Debug.LogError($"Character rect not found for '{character}'");

            cacheRect = characterRects[0];
            rectByChar.Add(character, cacheRect);
            return cacheRect;
        }
    }

    [System.Serializable]
    public struct WidthGroup
    {
        public string characters;
        public int width, offsetX;
    }

    [System.Serializable]
    public struct CharacterRect
    {
        public char character;
        public int widthInPixels, offsetXInPixels;

        public override string ToString()
        {
            return character.ToString();
        }
    }
}