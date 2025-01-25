using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace InGame
{
    public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsDragging => isDragging;

        public SmartAction onDragBegin = new();
        public SmartAction onDragEnd = new();
        public SmartAction onDragCancel = new();

        [SerializeField] private bool moveOnTopOnDragBegin;

        [Require] private RectTransform rect;

        [Inject] private Canvas canvas;

        private Vector2 prevScreenPos;
        private bool isDragging;


        private void Update()
        {
            if (isDragging)
            {
                Vector2 delta = (Vector2)Input.mousePosition - prevScreenPos;
                prevScreenPos = Input.mousePosition;
                rect.anchoredPosition += delta / canvas.scaleFactor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                prevScreenPos = eventData.position;
                isDragging = true;

                onDragBegin.Fire();
                DragSystem.OnDragBegin(this);

                if (moveOnTopOnDragBegin)
                {
                    transform.SetParent(canvas.transform);
                    transform.SetAsLastSibling();
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                isDragging = false;
                onDragEnd.Fire();
                DragSystem.OnDragEnd();
            }
            else if (eventData.button == PointerEventData.InputButton.Right && isDragging)
            {
                isDragging = false;
                onDragCancel.Fire();
                DragSystem.OnDragCancel();
            }
        }
    }
}