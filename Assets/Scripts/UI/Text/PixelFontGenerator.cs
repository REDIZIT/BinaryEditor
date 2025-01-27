using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace InGame
{

    public static class PixelFontGenerator
    {
        public static void GenerateFromSprites(PixelFont font)
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(font.fontSheet);
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            if (data == null) return;


            Sprite[] sprites = data.Where(o => o is Sprite).Cast<Sprite>().ToArray();
            char[] chars = font.characters.Where(c => c != ' ' && c != '\n').ToArray();

            // Debug.Log(string.Concat(chars));

            if (sprites.Length != chars.Length)
            {
                throw new Exception($"Failed to generate font due to different sizes of sprites and characters.\nSprites found: {sprites.Length}, Characters inside font asset: {chars.Length}");
            }

            font.characterRects.Clear();

            for (int i = 0; i < chars.Length; i++)
            {
                char character = chars[i];
                Sprite sprite = sprites[i];

                CharacterRect rect = new()
                {
                    character = character,
                    widthInPixels = (int)sprite.rect.width
                };
                font.characterRects.Add(rect);
            }
#endif
        }

        public static void GenerateFromWidthGroups(PixelFont font)
        {
            char[] chars = font.characters.Where(c => c != ' ' && c != '\n').ToArray();
            HashSet<char> missingChars = new(chars);
            Dictionary<char, WidthGroup> groupByChar = new();

            foreach (WidthGroup group in font.widthGroups)
            {
                for (int i = 0; i < group.characters.Length; i++)
                {
                    char character = group.characters[i];
                    if (groupByChar.ContainsKey(character))
                    {
                        throw new($"Failed to generate from groups due to char duplication. Found same character '{character}' inside 2 groups. Second group: '{group.characters}'");
                    }

                    groupByChar.Add(character, group);
                    missingChars.Remove(character);
                }
            }

            if (missingChars.Count != 0)
            {
                Debug.Log($"Some characters were not found inside WidthGroups. For these chars will be used default character width ({font.characterSize.x}).\nMissing chars ({missingChars.Count}): '{string.Concat(missingChars)}'");
            }

            font.characterRects.Clear();

            for (int i = 0; i < chars.Length; i++)
            {
                char character = chars[i];

                WidthGroup group = default;
                bool isMissing = true;

                if (groupByChar.ContainsKey(character))
                {
                    group = groupByChar[character];
                    isMissing = false;
                }

                CharacterRect rect = new()
                {
                    character = character,
                    widthInPixels = isMissing ? font.characterSize.x : group.width,
                    offsetXInPixels = isMissing ? 0 : group.offsetX
                };
                font.characterRects.Add(rect);
            }
        }
    }
}