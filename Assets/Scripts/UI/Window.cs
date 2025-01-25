using System;
using UnityEngine;

namespace InGame
{
    public class Window : MonoBehaviour
    {
        public bool IsShowed => isShowed;

        public SmartAction onShowed = new();

        [Require] private CanvasGroup group;
        [Require] private Draggable draggable;

        [SerializeField] private bool doSwitch;

        private bool isShowed;

        private void OnValidate()
        {
            if (doSwitch)
            {
                doSwitch = false;

                if (group == null) CustomAttributesResolver.Resolve(this);

                Switch();
            }
        }

        public void Show()
        {
            isShowed = true;

            group.alpha = 1;
            group.blocksRaycasts = true;

            onShowed.Fire();
        }

        public void Hide()
        {
            isShowed = false;

            group.alpha = 0;
            group.blocksRaycasts = false;
        }

        public void Switch()
        {
            if (isShowed) Hide();
            else Show();
        }
    }
}