using System;
using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    public class DropTarget : MonoBehaviour
    {
        public Func<Draggable, bool> canReceiveDrop;
        public Action<Draggable> onDrop;

        private void OnValidate()
        {
            if (GetComponent<Graphic>() == null)
            {
                Debug.LogError($"DropTarget on {transform.name} has no Graphic component");
            }
        }
    }
}