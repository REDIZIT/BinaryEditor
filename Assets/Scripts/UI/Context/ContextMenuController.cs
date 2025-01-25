using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class ContextMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private ContextMenuItemLot prefab;

        [Require] private Window window;

        [Inject] private Canvas canvas;
        [Inject] private UIHelper helper;

        public void Show(IEnumerable<ContextMenuItem> items)
        {
            container.anchoredPosition = Input.mousePosition / canvas.scaleFactor;

            helper.Refresh(container, prefab, items, this);
            window.Show();
        }

        public void Hide()
        {
            window.Hide();
        }

        public void OnClicked(ContextMenuItem item)
        {
            item.action?.Invoke();
            window.Hide();
        }
    }
}