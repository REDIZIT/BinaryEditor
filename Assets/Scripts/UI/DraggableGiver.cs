using System;
using UnityEngine;
using Zenject;

namespace InGame
{
    public class DraggableGiver : MonoBehaviour
    {
        public Action onDragOut;
        public Action<GameObject> onSpawnedNew;

        [SerializeField] private Draggable prefab;
        [SerializeField] private Transform container;

        [Inject] private DiContainer diContainer;

        private Draggable currentDraggable;
        private bool isInited;

        public void TryInit()
        {
            if (isInited) return;
            isInited = true;

            SpawnNew();
        }

        private void OnDragBegin()
        {
            SpawnNew();
            onDragOut?.Invoke();
        }

        private void SpawnNew()
        {
            currentDraggable = diContainer.InstantiatePrefabForComponent<Draggable>(prefab, container);

            // Subscribe with higher order than InstructionLot has (InstructionLot = 1, DraggableGiver = 10)
            // That fixes fucking floating bug: If executing order will be backward, then on SymbolInventoryLot drag out (when only x1 item left) you will get destroyed gameobject access exception.
            // I have no will and time for fixing that, so...
            currentDraggable.onDragBegin.Subscribe(OnDragBegin, 10);

            onSpawnedNew?.Invoke(currentDraggable.gameObject);
        }
    }
}