using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    public class Zoom : MonoBehaviour
    {
        [SerializeField] private float zoom = 2;

        [SerializeField] private RectTransform target;
        [SerializeField] private RawImage rawImage;

        [SerializeField] private Camera cam;
        [SerializeField] private Canvas canvas;

        private RectTransform canvasRect;
        private Texture2D zoomTex;
        [SerializeField] private RenderTexture renderTexture;

        private void Awake()
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            var imageRect = rawImage.GetComponent<RectTransform>();

            zoomTex = new Texture2D((int)(imageRect.rect.width / zoom), (int)(imageRect.rect.height / zoom), TextureFormat.RGBA32, false);
            zoomTex.filterMode = FilterMode.Point;

            rawImage.texture = zoomTex;


            Vector2 canvasSize = new Vector2((int)canvasRect.rect.width, (int)canvasRect.rect.height) ;
            renderTexture = new((int)canvasSize.x, (int)canvasSize.y, 24);
        }

        private void LateUpdate()
        {
            Vector2 canvasSize = new Vector2((int)canvasRect.rect.width, (int)canvasRect.rect.height) ;
            float scale = 1f / canvas.transform.localScale.x;

            Vector2 pos = (Vector2)((target.transform.position * scale)) + (canvasSize) / 2;

            // RenderTexture renderTexture = new((int)canvasSize.x, (int)canvasSize.y, 24);

            cam.targetTexture = renderTexture;
            cam.Render();

            RenderTexture.active = renderTexture;

            float x = pos.x - zoomTex.width / 2f;
            float y = canvasSize.y - pos.y - zoomTex.height / 2f;
            float width = zoomTex.width;
            float height = zoomTex.height;


            x *= canvas.scaleFactor;
            y *= canvas.scaleFactor;

            // Magic numbers
            // These numbers fixes zoom window offset if scaleFactor != 1
            x += 50 * (canvas.scaleFactor - 1);
            y += 30 * (canvas.scaleFactor - 1);

            // width *= canvas.scaleFactor;
            // height *= canvas.scaleFactor;

            x = Mathf.Round(x);
            y = Mathf.Floor(y);

            Rect renderTextureRect = new Rect(x, y, width, height);
            // Debug.Log(renderTextureRect);

            zoomTex.ReadPixels(renderTextureRect, 0, 0);
            zoomTex.Apply();



            cam.targetTexture = null;
            RenderTexture.active = null;

            // Destroy(renderTexture);
        }
    }
}