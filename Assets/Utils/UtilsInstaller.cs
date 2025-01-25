using UnityEngine;
using Zenject;

namespace InGame
{
    public class UtilsInstaller : MonoInstaller
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private FilesController files;
        [SerializeField] private ViewController view;

        public override void InstallBindings()
        {
            Container.Bind<UIHelper>().AsSingle();

            Container.BindInstance(canvas);
            Container.BindInstance(files);
            Container.BindInstance(view);
        }
    }
}