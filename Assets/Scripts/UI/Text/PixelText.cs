using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame
{
    public enum TextVerticalAlignment
    {
        Top = 1,
        Middle = 0,
        Bottom = -1,
    }

    public enum TextHoriztonalAlignment
    {
        Left = -1,
        Middle = 0,
        Right = 1,
    }

    [ExecuteAlways]
    public class PixelText : MonoBehaviour
    {
        [TextArea(3, 12)]
        [SerializeField] private string text;

        [SerializeField] private PixelFont font;
        [SerializeField] private int fontScale = 1;
        [SerializeField] private TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top;
        [SerializeField] private TextHoriztonalAlignment horiztonalAlignment = TextHoriztonalAlignment.Left;

        [SerializeField] private CanvasRenderer rend;

        private RectTransform rect, rendRect;
        private Mesh mesh;

        private List<Vector3> verts = new();
        private List<Vector2> uvs = new();

        private Rect prevRect;
        private Vector2 prevPos;
        private Vector2 builtBounds;


        private struct Line
        {
            public Word[] words;
            public int widthInPixels;
        }
        private struct Word
        {
            public char[] characters;
            public int widthInPixels;

            public Word(List<char> chars)
            {
                characters = chars.ToArray();
                widthInPixels = 0;
            }
        }


        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            rendRect = rend.GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (mesh == null)
            {
                mesh = new();
            }
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
                mesh = null;
            }
        }

        private void Update()
        {
            if (rect.rect != prevRect || prevPos != rect.anchoredPosition)
            {
                prevRect = rect.rect;
                prevPos = rect.anchoredPosition;
                OnValidate();
            }

            rendRect.anchoredPosition = GetTextPosition();
            rendRect.sizeDelta = rect.sizeDelta;
        }

        private void OnValidate()
        {
            Awake();
            Start();

            if (font == null) return;

            if (string.IsNullOrWhiteSpace(text))
            {
                ClearMesh();
            }
            else
            {
                RebuildMesh(text);
            }

            rend.SetMesh(mesh);
            rend.SetMaterial(font.material, null);
        }

        private void ClearMesh()
        {
            mesh.Clear();
        }
        private void RebuildMesh(string message)
        {
            verts.Clear();
            uvs.Clear();


            //
            // Preprocess
            //
            List<Line> lines = Preprocess(message);


            //
            // Mesh building
            //
            BuildText(lines);


            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            AppendTriangles(verts.Count / 4);


            RecalculateBounds();
        }

        private List<Line> Preprocess(string message)
        {
            List<Line> lines = new();
            List<Word> words = new();
            List<char> currentWord = new();
            for (int i = 0; i < message.Length; i++)
            {
                char character = message[i];

                if (character == ' ')
                {
                    Word word = new Word(currentWord);
                    word.widthInPixels = GetStringWidth(word.characters);

                    words.Add(word);
                    currentWord.Clear();
                    // FIX: Double spaces
                }
                else if (character == '\n')
                {
                    if (currentWord.Count > 0)
                    {
                        Word word = new(currentWord);
                        word.widthInPixels = GetStringWidth(word.characters);

                        words.Add(word);
                        currentWord.Clear();
                    }

                    Line line = new()
                    {
                        words = words.ToArray()
                    };
                    line.widthInPixels = words.Sum(w => w.widthInPixels);

                    lines.Add(line);
                    words.Clear();
                }
                else
                {
                    currentWord.Add(character);
                }
            }

            if (currentWord.Count > 0)
            {
                Word word = new Word()
                {
                    characters = currentWord.ToArray()
                };
                word.widthInPixels = GetStringWidth(word.characters);

                words.Add(word);
            }

            if (words.Count > 0)
            {
                Line line = new()
                {
                    words = words.ToArray()
                };
                line.widthInPixels = words.Sum(w => w.widthInPixels);

                lines.Add(line);
            }

            return lines;
        }

        private void BuildText(List<Line> lines)
        {
            // Per-textblock section
            Vector3 offset = Vector3.zero;
            if (verticalAlignment == TextVerticalAlignment.Middle)
            {
                offset.y = (lines.Count * font.characterSize.y) / 2f;
            }
            else if (verticalAlignment == TextVerticalAlignment.Bottom)
            {
                offset.y = lines.Count * font.characterSize.y;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                // Per-line section
                Line line = lines[i];

                int lineWidthInPixels = line.widthInPixels;
                lineWidthInPixels += Mathf.Max(0, (line.words.Length - 1) * font.spaceSize);
                lineWidthInPixels += line.words.Sum(w => Mathf.Max(0, (w.characters.Length - 1) * font.characterSpacing));

                offset.x = 0;
                if (horiztonalAlignment == TextHoriztonalAlignment.Middle)
                {
                    offset.x -= lineWidthInPixels / 2f;
                }
                else if (horiztonalAlignment == TextHoriztonalAlignment.Right)
                {
                    offset.x -= lineWidthInPixels;
                }

                for (int j = 0; j < line.words.Length; j++)
                {
                    // Per-word section
                    Word word = line.words[j];

                    for (int k = 0; k < word.characters.Length; k++)
                    {
                        // Per-character section
                        char character = word.characters[k];
                        CharacterRect characterRect = font.GetCharacterRect(character);

                        AppendCharacter(verts, uvs, character, offset - new Vector3(characterRect.offsetXInPixels, 0, 0));
                        offset.x += characterRect.widthInPixels;

                        // Do not add character spacing for last character
                        if (k < word.characters.Length - 1)
                        {
                            offset.x += font.characterSpacing;
                        }
                    }

                    offset.x += font.spaceSize;
                }

                offset.y -= font.characterSize.y;
            }
        }


        private int GetStringWidth(char[] message)
        {
            int width = 0;

            for (var i = 0; i < message.Length; i++)
            {
                width += font.GetCharacterRect(message[i]).widthInPixels;
            }

            return width;
        }

        private void AppendTriangles(int charactersBuilt)
        {
            int[] tris = new int[6 * charactersBuilt];
            for (int i = 0; i < charactersBuilt; i++)
            {
                int vertIndex = i * 4;
                int trisIndex = i * 6;

                tris[trisIndex + 0] = 0 + vertIndex;
                tris[trisIndex + 1] = 1 + vertIndex;
                tris[trisIndex + 2] = 2 + vertIndex;
                tris[trisIndex + 3] = 2 + vertIndex;
                tris[trisIndex + 4] = 3 + vertIndex;
                tris[trisIndex + 5] = 0 + vertIndex;
            }

            mesh.SetTriangles(tris, 0);
        }

        private void RecalculateBounds()
        {
            builtBounds = Vector2.zero;
            foreach (Vector3 vert in verts)
            {
                builtBounds.x = Mathf.Max(builtBounds.x, vert.x);
                builtBounds.y = Mathf.Max(builtBounds.y, vert.y);
            }
        }

        private void AppendCharacter(List<Vector3> verts, List<Vector2> uvs, char character, Vector3 offset)
        {
            Vector2 meshSize = font.characterSize * fontScale;

            float magicOffset = 0.01f;

            meshSize.y += magicOffset * 2;
            offset.y -= magicOffset;

            verts.Add(offset + new Vector3(0, 0, 0));
            verts.Add(offset + new Vector3(0, meshSize.y, 0));
            verts.Add(offset + new Vector3(meshSize.x, meshSize.y, 0));
            verts.Add(offset + new Vector3(meshSize.x, 0, 0));


            Vector2Int textureSize = new(font.material.mainTexture.width, font.material.mainTexture.height);
            Vector2 uv_characterSize = new(font.characterSize.x / (float)textureSize.x, font.characterSize.y / (float)textureSize.y);


            Vector2Int characterIndex = font.GetCharacterIndex(character);

            Vector2 uv_min = new Vector2(characterIndex.x * uv_characterSize.x, 1 - (characterIndex.y + 1) * uv_characterSize.y);
            Vector2 uv_max = new Vector2((characterIndex.x + 1) * uv_characterSize.x, 1 - characterIndex.y * uv_characterSize.y);

            uvs.Add(new Vector2(uv_min.x, uv_min.y));
            uvs.Add(new Vector2(uv_min.x, uv_max.y));
            uvs.Add(new Vector2(uv_max.x, uv_max.y));
            uvs.Add(new Vector2(uv_max.x, uv_min.y));
        }

        private Vector2 GetTextPosition()
        {
            Vector2 center = rect.rect.center;
            Vector2 halfsize = rect.rect.size / 2f;

            Vector3 startPos = Vector3.zero;

            startPos.x = center.x + halfsize.x * (int)horiztonalAlignment;
            startPos.y = center.y + halfsize.y * (int)verticalAlignment;

            // if (verticalAlignment == TextVerticalAlignment.Top) startPos.y -= font.characterSize.y;
            startPos.y -= font.characterSize.y;

            return startPos;
        }
    }
}