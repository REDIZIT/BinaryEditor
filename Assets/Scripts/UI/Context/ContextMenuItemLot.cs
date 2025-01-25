using System;
using TMPro;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class ContextMenuItemLot : UILot<ContextMenuItem>
    {
        [SerializeField] private TextMeshProUGUI text;

        [Inject] private ContextMenuController menu;

        protected override void Refresh()
        {
            text.text = model.text;
        }

        public void OnClick()
        {
            menu.OnClicked(model);
        }
    }
    public class ContextMenuItem
    {
        public string text;
        public Action action;
    }
}