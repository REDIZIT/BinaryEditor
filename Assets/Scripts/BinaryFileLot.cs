using TMPro;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class BinaryFileLot : UILot<BinaryFile>
    {
        [SerializeField] private TextMeshProUGUI filenameText;

        [Inject] private ViewController view;
        [Inject] private FilesController files;

        protected override void Refresh()
        {
            filenameText.text = model.filename;
        }

        public void OnOpenClicked()
        {
            view.Show(model);
        }

        public void OnCloseClicked()
        {
            files.Disconnect(model);
        }
    }
}