using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InGame
{
    public class DragSystem : MonoBehaviour
    {
        public static DragSystem instance;

        public Draggable activeDragging;
        public Action onDragBegin, onDragEnd, onDragCancel;

        private void Awake()
        {
            instance = this;
        }

        public static void OnDragBegin(Draggable draggable)
        {
            instance.activeDragging = draggable;
            instance.onDragBegin?.Invoke();
        }

        public static void OnDragEnd()
        {
            instance.onDragEnd?.Invoke();

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            }, results);

            foreach (RaycastResult result in results)
            {
                DropTarget dropTarget = result.gameObject.GetComponent<DropTarget>();
                if (dropTarget == null) continue;

                if (dropTarget.canReceiveDrop(instance.activeDragging))
                {
                    dropTarget.onDrop(instance.activeDragging);
                    break;
                }
            }
        }

        public static void OnDragCancel()
        {
            instance.onDragCancel?.Invoke();
        }
    }
}