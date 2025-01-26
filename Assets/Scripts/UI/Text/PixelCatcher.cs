using UnityEngine;

namespace InGame
{
    [ExecuteAlways]
    public class PixelCatcher : MonoBehaviour
    {
        private RectTransform rect, parentRect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            parentRect = transform.parent.GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            rect.anchoredPosition = GetWorldPosCorrection(Vector2.zero);
        }

        private Vector2 GetWorldPosCorrection(Vector2 floatingLocalPos)
        {
            Vector2 rectWorldPos = parentRect.localToWorldMatrix.MultiplyPoint(floatingLocalPos);
            Vector2 rectWorldPosRounded = rectWorldPos.Round();

            Vector2 delta = rectWorldPos - rectWorldPosRounded;

            return -delta / 2f;
        }
    }
}