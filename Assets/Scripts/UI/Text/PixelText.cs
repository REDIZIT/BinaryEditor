using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace InGame
{
    [ExecuteAlways]
    public class PixelText : MonoBehaviour
    {
        public float FontScale
        {
            get => fontScale;
            set { fontScale = value; Dirty(); }
        }

        public string text
        {
            get => _text;
            set { _text = value; Dirty(); }
        }

        public PixelFont Font
        {
            get => font;
            set { font = value; Dirty(); }
        }

        [TextArea(3, 12)]
        [SerializeField] private string _text;

        [SerializeField] private PixelFont font;
        [SerializeField] private float fontScale = 1;
        [SerializeField] private TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top;
        [SerializeField] private TextHoriztonalAlignment horiztonalAlignment = TextHoriztonalAlignment.Left;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private bool richText;

        private GameObject rendererInst;
        private RectTransform rendRect;
        private CanvasRenderer rend;
        private PixelCatcher catcher;


        private RectTransform rect;
        private Mesh mesh;

        private List<Vector3> verts = new();
        private List<Vector2> uvs = new();
        private List<Color> colors = new();

        private List<Char> preprocessChars = new();
        private static List<int> triangles = new();
        private Vector2 fontSheetSize;

        private Rect prevRect;
        private Vector2 prevPos;
        private Vector2 builtBounds;

        private bool isDirty;


        private struct Line
        {
            public Word[] words;
            public int widthInPixels;
        }
        private struct Word : IDisposable
        {
            public NativeArray<Char> characters;
            public int widthInPixels;

            public Word(List<Char> chars)
            {
                widthInPixels = 0;

                // characters = new Char[chars.Count];
                characters = new NativeArray<Char>(chars.Count, Allocator.Temp);

                for (int i = 0; i < chars.Count; i++)
                {
                    characters[i] = chars[i];
                }
            }

            public void Dispose()
            {
                characters.Dispose();
            }
        }
        private struct Char
        {
            public char character;
            public Color color;
        }

        private struct Tag
        {
            public int firstOpenIndex;
            public int firstCloseIndex;
            public int secondOpenIndex;
            public int secondCloseIndex;
            public Color color;
        }


        private void OnEnable()
        {
            Awake();
        }

        private void OnDisable()
        {
            OnDestroy();
        }

        private void OnValidate()
        {
            OnEnable();
            Awake();
            Start();
            RebuildText();
        }

        private void Awake()
        {
            if (rendererInst == null)
            {
                PixelCatcher potentialRenderer = transform.GetComponentInChildren<PixelCatcher>();

                if (potentialRenderer == null || potentialRenderer.GetComponent<CanvasRenderer>() == null)
                {
                    rendererInst = new GameObject("Text renderer", typeof(RectTransform), typeof(CanvasRenderer), typeof(PixelCatcher));
                    rendererInst.transform.SetParent(transform);
                    rendererInst.transform.localScale = Vector3.one;

                    rendRect = rendererInst.GetComponent<RectTransform>();
                    rendRect.pivot = Vector2.zero;
                    rendRect.sizeDelta = Vector2.zero;
                }
                else
                {
                    rendererInst = potentialRenderer.gameObject;
                }
            }

            if (rend == null || catcher == null || rect == null)
            {
                rend = rendererInst.GetComponent<CanvasRenderer>();
                catcher = rendererInst.GetComponent<PixelCatcher>();
                rect = GetComponent<RectTransform>();
            }
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
                if (Application.isPlaying) Destroy(mesh);
                else DestroyImmediate(mesh);

                mesh = null;
            }
        }

        private void Update()
        {
            Awake();

            if (isDirty)
            {
                isDirty = false;
                RebuildText();
            }

            if (rect.rect != prevRect || prevPos != rect.anchoredPosition)
            {
                prevRect = rect.rect;
                prevPos = rect.anchoredPosition;
                OnValidate();
            }

            if (font != null)
            {
                catcher.localPosition = GetTextPosition();
            }
        }

        private void RebuildText()
        {
            if (string.IsNullOrWhiteSpace(_text) || font == null)
            {
                ClearMesh();
            }
            else
            {
                RebuildMesh(_text);
            }

            rend.SetMesh(mesh);

            if (font != null) rend.SetMaterial(font.material, null);
        }

        private void ClearMesh()
        {
            mesh.Clear();
        }
        private void RebuildMesh(string message)
        {
            verts.Clear();
            uvs.Clear();
            colors.Clear();

            Debug.Log("RebuildMesh for " + message.Length);

            //
            // Preprocess
            //
            List<Line> lines = Preprocess(message);


            //
            // Mesh building
            //
            fontSheetSize = new(font.material.mainTexture.width, font.material.mainTexture.height);
            BuildText(lines);


            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            AppendTriangles(verts.Count / 4);
            mesh.SetColors(colors);


            RecalculateBounds();
        }

        private List<Char> PreprocessRichText(string message)
        {
            List<Char> chars = new();

            bool isTagCollecting = false;

            List<Tag> tags = new();
            Tag currentTag = default;

            // <color=#123>my text</color>

            for (int i = 0; i < message.Length; i++)
            {
                char character = message[i];

                if (character == '<')
                {
                    if (isTagCollecting)
                    {
                        if (i < message.Length && message[i + 1] == '/')
                        {
                            // second <
                            currentTag.secondOpenIndex = i;
                        }
                        else
                        {
                            isTagCollecting = false;
                            currentTag = default;
                        }
                    }
                    else
                    {
                        // first <
                        isTagCollecting = true;
                        currentTag.firstOpenIndex = i;
                    }
                }
                else if (character == '>')
                {
                    if (isTagCollecting)
                    {
                        if (currentTag.firstCloseIndex == 0)
                        {
                            // first >
                            currentTag.firstCloseIndex = i;
                        }
                        else
                        {
                            // second >
                            currentTag.secondCloseIndex = i;
                            isTagCollecting = false;

                            int hexIndex = currentTag.firstOpenIndex + "color=".Length + 1;
                            int hexLength = currentTag.firstCloseIndex - hexIndex;
                            ColorUtility.TryParseHtmlString(message.Substring(hexIndex, hexLength), out Color color);
                            currentTag.color = color;

                            tags.Add(currentTag);
                            currentTag = default;
                        }
                    }
                }
            }


            if (tags.Count == 0)
            {
                for (int i = 0; i < message.Length; i++)
                {
                    chars.Add(new Char()
                    {
                        character = message[i],
                        color = default
                    });
                }
            }
            else
            {
                int tagIndex = 0;
                Color color = default;

                for (int i = 0; i < message.Length; i++)
                {
                    Tag nextTag = tagIndex < tags.Count ? tags[tagIndex] : default;

                    if (i == nextTag.firstOpenIndex)
                    {
                        color = nextTag.color;
                        i = nextTag.firstCloseIndex;

                    }
                    else if (i == nextTag.secondOpenIndex)
                    {
                        color = default;
                        i = nextTag.secondCloseIndex;
                        tagIndex++;
                    }
                    else
                    {
                        chars.Add(new Char()
                        {
                            character = message[i],
                            color = color
                        });
                    }
                }
            }


            return chars;
        }

        private List<Char> PreprocessChars(string message)
        {
            preprocessChars.Clear();

            for (int i = 0; i < message.Length; i++)
            {
                preprocessChars.Add(new()
                {
                    character = message[i],
                    color = default
                });
            }

            return preprocessChars;
        }

        private List<Line> Preprocess(string message)
        {
            List<Line> lines = new();
            List<Word> words = new();
            List<Char> currentWord = new();

            List<Char> chars = richText ? PreprocessRichText(message) : PreprocessChars(message);

            for (int i = 0; i < chars.Count; i++)
            {
                Char character = chars[i];

                if (character.character == ' ')
                {
                    Word word = new Word(currentWord);
                    word.widthInPixels = GetStringWidth(word.characters);

                    words.Add(word);
                    currentWord.Clear();
                    // FIX: Double spaces
                }
                else if (character.character == '\n')
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
                Word word = new Word(currentWord);
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

            foreach (var word in words)
            {
                word.Dispose();
            }

            return lines;
        }

        private void BuildText(List<Line> lines)
        {
            Vector2 charSize = (Vector2)font.characterSize * fontScale;
            float charSpacing = font.characterSpacing * fontScale;
            float spaceSize = font.spaceSize * fontScale;


            // Per-textblock section
            Vector3 offset = Vector3.zero;
            if (verticalAlignment == TextVerticalAlignment.Middle)
            {
                offset.y = (lines.Count * charSize.y) / 2f;
            }
            else if (verticalAlignment == TextVerticalAlignment.Bottom)
            {
                offset.y = lines.Count * charSize.y;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                // Per-line section
                Line line = lines[i];

                float lineWidthInPixels = line.widthInPixels * fontScale;
                lineWidthInPixels += Mathf.Max(0, (line.words.Length - 1) * spaceSize);
                lineWidthInPixels += line.words.Sum(w => Mathf.Max(0, (w.characters.Length - 1) * charSpacing));

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
                        Char character = word.characters[k];
                        CharacterRect characterRect = font.GetCharacterRect(character.character);

                        AppendCharacter(verts, uvs, colors, character, offset - new Vector3(characterRect.offsetXInPixels, 0, 0));
                        offset.x += characterRect.widthInPixels * fontScale;

                        // Do not add character spacing for last character
                        if (k < word.characters.Length - 1)
                        {
                            offset.x += charSpacing;
                        }
                    }

                    offset.x += spaceSize;
                }

                offset.y -= charSize.y;
            }
        }


        private int GetStringWidth(NativeArray<Char> message)
        {
            int width = 0;

            for (var i = 0; i < message.Length; i++)
            {
                width += font.GetCharacterRect(message[i].character).widthInPixels;
            }

            return width;
        }

        private void AppendTriangles(int charactersBuilt)
        {
            int trisIndex = triangles.Count / 6;
            int delta = charactersBuilt - trisIndex;

            for (int i = 0; i < delta; i++)
            {
                int vertIndex = (trisIndex + i) * 4;

                triangles.Add(0 + vertIndex);
                triangles.Add(1 + vertIndex);
                triangles.Add(2 + vertIndex);
                triangles.Add(2 + vertIndex);
                triangles.Add(3 + vertIndex);
                triangles.Add(0 + vertIndex);
            }

            mesh.SetTriangles(triangles, 0, charactersBuilt * 6, 0, true);
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

        private void AppendCharacter(List<Vector3> verts, List<Vector2> uvs, List<Color> colors, Char character, Vector3 offset)
        {
            Vector2 meshSize = (Vector2)font.characterSize * fontScale;

            verts.Add(offset + new Vector3(0, 0, 0));
            verts.Add(offset + new Vector3(0, meshSize.y, 0));
            verts.Add(offset + new Vector3(meshSize.x, meshSize.y, 0));
            verts.Add(offset + new Vector3(meshSize.x, 0, 0));


            Vector2 uv_characterSize = new(font.characterSize.x / fontSheetSize.x, font.characterSize.y / fontSheetSize.y);


            Vector2Int characterIndex = font.GetCharacterIndex(character.character);

            Vector2 uv_min = new Vector2(characterIndex.x * uv_characterSize.x, 1 - (characterIndex.y + 1) * uv_characterSize.y);
            Vector2 uv_max = new Vector2((characterIndex.x + 1) * uv_characterSize.x, 1 - characterIndex.y * uv_characterSize.y);

            uvs.Add(new Vector2(uv_min.x, uv_min.y));
            uvs.Add(new Vector2(uv_min.x, uv_max.y));
            uvs.Add(new Vector2(uv_max.x, uv_max.y));
            uvs.Add(new Vector2(uv_max.x, uv_min.y));


            Color meshColor = character.color == default ? color : character.color;

            colors.Add(meshColor);
            colors.Add(meshColor);
            colors.Add(meshColor);
            colors.Add(meshColor);
        }

        private Vector2 GetTextPosition()
        {
            Vector2 halfsize = rect.rect.size / 2f;

            Vector3 startPos = Vector3.zero;

            startPos.x = halfsize.x * (int)horiztonalAlignment;
            startPos.y = halfsize.y * (int)verticalAlignment;

            startPos.y -= font.characterSize.y * fontScale;

            return startPos;
        }

        private void Dirty()
        {
            isDirty = true;
        }
    }
}