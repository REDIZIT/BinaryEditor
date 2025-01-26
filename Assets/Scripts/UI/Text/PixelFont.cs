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

            // Debug.Log("Unknown character: " + character);
            return index;
        }

        public CharacterRect GetCharacterRect(char character)
        {
            foreach (CharacterRect rect in characterRects)
            {
                if (rect.character == character) return rect;
            }

            // Debug.LogError($"Character rect not found for '{character}'");

            return characterRects[0];
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