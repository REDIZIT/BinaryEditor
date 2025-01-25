using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class FilesController : MonoBehaviour
    {
        [SerializeField] private BinaryFileLot prefab;
        [SerializeField] private Transform container;

        [Inject] private ViewController view;
        [Inject] private UIHelper helper;

        private List<BinaryFile> files = new();

        public void OnOpenClicked()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Open binary file", "", "", false);
            if (paths.Length > 0)
            {
                LoadFile(paths[0]);
            }
        }

        private void Refresh()
        {
            helper.Refresh(container, prefab, files);
        }

        public void Disconnect(BinaryFile file)
        {
            files.Remove(file);
            Refresh();

            if (view.File == file)
            {
                view.Show(null);
            }
        }

        private void LoadFile(string filepath)
        {
            BinaryFile file = new()
            {
                filename = Path.GetFileName(filepath)
            };

            file.data = File.ReadAllBytes(filepath).ToList();
            files.Add(file);

            Refresh();
            view.Show(file);
        }
    }

    public class BinaryFile
    {
        public string filename;
        public List<byte> data;
    }
}