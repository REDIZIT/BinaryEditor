using UnityEngine;

namespace InGame
{
    [ExecuteAlways]
    public class PixelCatcher : MonoBehaviour
    {
        public Vector2 localPosition;

        private RectTransform rect, parentRect;

        private void Start()
        {
            rect = GetComponent<RectTransform>();
            parentRect = transform.parent.GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            rect.anchoredPosition = GetWorldPosCorrection(localPosition);
        }

        private Vector2 GetWorldPosCorrection(Vector2 floatingLocalPos)
        {
            Vector2 rectWorldPos = parentRect.localToWorldMatrix.MultiplyPoint(floatingLocalPos);
            Vector2 rectWorldPosRounded = rectWorldPos.Round();

            Vector2 delta = rectWorldPos - rectWorldPosRounded;

            Vector2 correctedPos = floatingLocalPos - delta / 2f;

            return correctedPos;
        }
    }
}