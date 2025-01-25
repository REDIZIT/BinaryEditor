using System.Collections.Generic;
using System;
using UnityEngine;
using Zenject;
using System.Linq;
using Object = UnityEngine.Object;

namespace InGame
{
    public class UIHelper
    {
        [Inject] private DiContainer container;

        private void RemoveChildrenWithoutLot<TUILot>(Transform parent)
        {
            // Remove children without TUILot
            int childCount = parent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.GetComponent<TUILot>() == null)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                    childCount--;
                }
            }
        }

        private void InstantiateNewLotsIfNeeded<TUILot>(Transform parent, TUILot prefab, int diff, DiContainer container) where TUILot : MonoBehaviour
        {
            // Add new lots if needed
            for (int i = 0; i < diff; i++)
            {
                container.InstantiatePrefab(prefab, parent);
            }
        }

        private void RemoveLotsIfNeeded(Transform parent, int diff)
        {
            // Remove lots if needed
            for (int i = 0; i < -diff; i++)
            {
                GameObject.DestroyImmediate(parent.GetChild(0).gameObject);
            }
        }

        private void RefreshLotsWithModels<TUILot, TModel>(Transform parent, IEnumerable<TModel> models) where TUILot : UILot<TModel>
        {
            int index = 0;
            foreach (TModel model in models)
            {
                Transform child = parent.GetChild(index);
                TUILot lot = child.GetComponent<TUILot>();
                index++;

                if (lot == null)
                {
                    Debug.LogError($"Failed to Refresh due to child GameObject '{child.name}' of parent '{parent.name}' does not have {typeof(TUILot)} component.", parent);
                    return;
                }
                lot.Refresh(model);
            }
        }

        public void Refresh<TUILot, TModel>(Transform parent, TUILot prefab, IEnumerable<TModel> models, params object[] toInject) where TUILot : UILot<TModel>
        {
            DiContainer sub = container.CreateSubContainer();
            sub.BindInstances(toInject);

            RemoveChildrenWithoutLot<TUILot>(parent);
            int diff = models.Count() - parent.childCount;

            InstantiateNewLotsIfNeeded(parent, prefab, diff, sub);
            RemoveLotsIfNeeded(parent, diff);
            RefreshLotsWithModels<TUILot, TModel>(parent, models);
        }

        public void Refresh<TUILot, TModel>(Transform parent, TUILot prefab, IEnumerable<TModel> models, IEnumerable<DiContainer> perLotContainers) where TUILot : UILot<TModel>
        {

        }

        public IEnumerable<TModel> GetModels<TModel>(Transform parent)
        {
            foreach (Transform child in parent)
            {
                yield return child.GetComponent<UILot<TModel>>().ActiveModel;
            }
        }
        public IEnumerable<UILot<TModel>> GetLots<TModel>(Transform parent)
        {
            foreach (Transform child in parent)
            {
                yield return child.GetComponent<UILot<TModel>>();
            }
        }
        public IEnumerable<TLot> EnumerateLots<TLot>(Transform parent)
        {
            foreach (Transform child in parent)
            {
                yield return child.GetComponent<TLot>();
            }
        }
        public void Foreach<TLot>(Transform parent, Action<TLot> func)
        {
            foreach (Transform child in parent)
            {
                func(child.GetComponent<TLot>());
            }
        }
        public void FillWithInstance<TModel, TInstance>(Transform parent, IEnumerable<TModel> models, TInstance activeInstance, Action<TInstance, TModel> refreshFunc) where TInstance : MonoBehaviour
        {
            activeInstance.gameObject.SetActive(true);

            foreach (TModel model in models)
            {
                GameObject inst = container.InstantiatePrefab(activeInstance.gameObject, parent);
                ResolveCustom(inst);
                refreshFunc(inst.GetComponent<TInstance>(), model);
            }

            activeInstance.gameObject.SetActive(false);
        }
        public void AppendWithInstance<TModel, TInstance>(Transform parent, TModel model, TInstance activeInstance, Action<TInstance, TModel> refreshFunc) where TInstance : MonoBehaviour
        {
            activeInstance.gameObject.SetActive(true);

            GameObject inst = container.InstantiatePrefab(activeInstance.gameObject, parent);
            ResolveCustom(inst);
            refreshFunc(inst.GetComponent<TInstance>(), model);

            activeInstance.gameObject.SetActive(false);
        }
        public TUILot AppendAsLast<TUILot, TModel>(Transform parent, TUILot prefab, TModel model) where TUILot : UILot<TModel>
        {
            GameObject inst = container.InstantiatePrefab(prefab, parent);
            ResolveCustom(inst);
            inst.GetComponent<TUILot>().Refresh(model);

            inst.transform.SetAsLastSibling();
            return inst.GetComponent<TUILot>();
        }

        public void FillBySelector<TModel, TLot>(Transform parent, IEnumerable<TModel> models, Func<TModel, TLot> prefabByModel) where TLot : UILot<TModel>
        {
            ClearChildren(parent);

            DiContainer sub = container.CreateSubContainer();

            foreach (TModel model in models)
            {
                TLot prefab = prefabByModel(model);
                TLot inst = sub.InstantiatePrefab(prefab, parent).GetComponent<TLot>();
                ResolveCustom(inst.gameObject);
                inst.Refresh(model);
            }
        }
        public void FillBySelector(Transform parent, IEnumerable<object> models, Func<object, KeyValuePair<GameObject, object>> kvByModel)
        {
            ClearChildren(parent);

            foreach (object model in models)
            {
                KeyValuePair<GameObject, object> kv = kvByModel(model);

                DiContainer sub = container.CreateSubContainer();
                if (kv.Value != null) sub.Bind(kv.Value.GetType()).FromInstance(kv.Value);

                GameObject inst = sub.InstantiatePrefab(kv.Key, parent);
                ResolveCustom(inst);
                inst.GetComponent<IUILot>().OnConstruct();
            }
        }

        public void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private void ResolveCustom(GameObject inst)
        {
        }
    }
}