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

        public void Reload(BinaryFile file)
        {
            file.data = File.ReadAllBytes(file.filepath).ToList();
            file.status = FileStatus.NotChanged;

            Refresh();
            view.Show(file);
        }

        public void SaveToDisk(BinaryFile file)
        {
            file.watcher.EnableRaisingEvents = false;

            File.WriteAllBytes(file.filepath, file.data.ToArray());

            file.watcher.EnableRaisingEvents = true;
            file.status = FileStatus.NotChanged;

            Refresh();
        }
        public void SaveToDiskAs(BinaryFile file)
        {
            string filepath = StandaloneFileBrowser.SaveFilePanel("Save bin as", Path.GetDirectoryName(file.filepath), Path.GetFileName(file.filepath), "");
            if (string.IsNullOrEmpty(filepath)) return;


            file.watcher.EnableRaisingEvents = false;

            File.WriteAllBytes(filepath, file.data.ToArray());

            file.watcher.EnableRaisingEvents = true;
            file.status = FileStatus.NotChanged;

            Refresh();
        }

        private void LoadFile(string filepath)
        {
            BinaryFile file = new()
            {
                filepath = filepath
            };

            file.data = File.ReadAllBytes(filepath).ToList();
            files.Add(file);

            file.SetupWatcher();

            Refresh();
            view.Show(file);
        }
    }

    public class BinaryFile
    {
        public string filepath;
        public List<byte> data;
        public FileSystemWatcher watcher;
        public FileStatus status;

        public void SetupWatcher()
        {
            status = FileStatus.NotChanged;

            watcher = new FileSystemWatcher(Path.GetDirectoryName(filepath));
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.EnableRaisingEvents = true;
            watcher.Changed += (s, e) => { CheckForStatus(e.Name, FileStatus.DiskChanged); };
            watcher.Renamed += (s, e) => { CheckForStatus(e.Name, FileStatus.Deleted); CheckForStatus(e.OldName, FileStatus.Deleted); };
            watcher.Deleted += (s, e) => { CheckForStatus(e.Name, FileStatus.Deleted); };
        }

        private void CheckForStatus(string filename, FileStatus status)
        {
            if (filename == Path.GetFileName(filepath))
            {
                this.status = status;
            }
        }
    }

    public enum FileStatus
    {
        NotChanged,
        InAppChanged,
        DiskChanged,
        Deleted
    }
}