using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace InGame
{
    public class BinaryFileLot : UILot<BinaryFile>, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI filenameText;

        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite buttonDefault, buttonSelected;

        [SerializeField] private GameObject indicatorGroup;
        [SerializeField] private Image indicator;

        [SerializeField] private Color inAppChange, diskChange, deleted;

        [Inject] private ViewController view;
        [Inject] private FilesController files;
        [Inject] private ContextMenuController ctx;

        protected override void Refresh()
        {
            filenameText.text = Path.GetFileName(model.filepath);
        }

        private void Update()
        {
            indicatorGroup.SetActive(model.status != FileStatus.NotChanged);

            indicator.color =
                model.status == FileStatus.InAppChanged ? inAppChange :
                model.status == FileStatus.DiskChanged ? diskChange :
                deleted;

            buttonImage.sprite = view.File == model ? buttonSelected : buttonDefault;
        }

        public void OnOpenClicked()
        {
            view.Show(model);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ctx.Show(EnumerateContextItems());
            }
        }

        private IEnumerable<ContextMenuItem> EnumerateContextItems()
        {
            yield return new ContextMenuItem()
            {
                text = "Disconnect",
                action = () => files.Disconnect(model)
            };
            yield return new ContextMenuItem()
            {
                text = "Reload",
                action = () => files.Reload(model)
            };
            yield return new ContextMenuItem()
            {
                text = "Save to disk",
                action = () => files.SaveToDisk(model)
            };
            yield return new ContextMenuItem()
            {
                text = "Save to disk as ...",
                action = () => files.SaveToDiskAs(model)
            };
        }
    }
}