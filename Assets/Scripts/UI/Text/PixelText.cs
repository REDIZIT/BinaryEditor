using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class PixelText : MonoBehaviour
    {
        [TextArea(3, 12)]
        [SerializeField] private string text;

        [SerializeField] private PixelFont font;
        [SerializeField] private int fontScale = 1;

        private CanvasRenderer rend;
        private Mesh mesh;

        private List<Vector3> verts = new();
        private List<Vector2> uvs = new();

        private void Awake()
        {
            rend = GetComponent<CanvasRenderer>();
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

            Vector3 offset = Vector3.zero;
            int charactersBuilt = 0;
            for (int i = 0; i < message.Length; i++)
            {
                char character = message[i];

                if (character == ' ')
                {
                    offset.x += font.spaceSize;
                }
                else if (character == '\n')
                {
                    offset.x = 0;
                    offset.y -= font.characterSize.y;
                }
                else
                {
                    AppendCharacter(verts, uvs, character, offset * fontScale);
                    charactersBuilt++;

                    Margin margin = font.GetMargin(character);
                    offset.x += font.characterSize.x + margin.right;
                }
            }


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

            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
        }

        private void AppendCharacter(List<Vector3> verts, List<Vector2> uvs, char character, Vector3 offset)
        {
            Vector2Int meshSize = font.characterSize * fontScale;

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
    }
}