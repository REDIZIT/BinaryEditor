using System;
using TMPro;
using UnityEngine;

namespace InGame
{
    public class TestScene : MonoBehaviour
    {
        [SerializeField] private bool playAnimation;
        [SerializeField] private float animationSpeed = 1;

        [Range(-1, 1)]
        [SerializeField] private float progress;

        [SerializeField] private float radius = 20;
        [SerializeField] private bool allowX = true, allowY = true;
        [SerializeField] private int fontScale = 1;
        [SerializeField] private float canvasScale = 1;


        [Space]

        [SerializeField] private RectTransform container;
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI proText;
        [SerializeField] private PixelText pixelText;

        private void Update()
        {
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            Vector2 pos = Vector2.zero;

            if (playAnimation && Application.isPlaying)
            {
                progress = Mathf.Repeat(progress + animationSpeed * Time.deltaTime / radius, 1);
            }


            pos.x = allowX ? Mathf.Cos(2 * Mathf.PI * progress) : 0;
            pos.y = allowY ? Mathf.Sin(2 * Mathf.PI * progress) : 0;

            container.anchoredPosition = pos * radius;


            canvas.scaleFactor = canvasScale;
            proText.fontSize = 8 * fontScale;
            pixelText.FontScale = fontScale;
        }
    }
}