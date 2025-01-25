using UnityEngine;
using Zenject;

namespace InGame
{
    public class UtilsInstaller : MonoInstaller
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private FilesController files;
        [SerializeField] private ViewController view;
        [SerializeField] private SelectionController selection;
        [SerializeField] private ContextMenuController ctx;

        [SerializeField] private SelectionBlock selectionBlock;
        [SerializeField] private Transform selectionBlockContainer;

        public override void InstallBindings()
        {
            Container.Bind<UIHelper>().AsSingle();

            Container.BindInstance(canvas);
            Container.BindInstance(files);
            Container.BindInstance(view);
            Container.BindInstance(selection);
            Container.BindInstance(ctx);

            Container.BindMemoryPool<SelectionBlock, SelectionBlock.Pool>()
                .WithInitialSize(2)
                .FromComponentInNewPrefab(selectionBlock)
                .UnderTransform(selectionBlockContainer);

            CustomAttributesResolver.instance.Init();
        }
    }
}