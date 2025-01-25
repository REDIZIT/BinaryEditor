using System;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class SmartAction<T>
    {
        private SortedList<int, List<Action<T>>> callbacks = new();

        public void Subscribe(Action<T> callback, int order = 0)
        {
            if (callbacks.ContainsKey(order) == false)
            {
                callbacks.Add(order, new());
            }

            callbacks[order].Add(callback);
        }

        public void UnsubscribeAll(object owner)
        {
            foreach (KeyValuePair<int, List<Action<T>>> kv in callbacks)
            {
                for (int i = kv.Value.Count - 1; i >= 0; i--)
                {
                    Action<T> callback = kv.Value[i];
                    if (callback.Target == owner)
                    {
                        kv.Value.RemoveAt(i);
                    }
                }
            }
        }

        public void Fire(T argument)
        {
            foreach (KeyValuePair<int, List<Action<T>>> kv in callbacks)
            {
                foreach (Action<T> callback in kv.Value)
                {
                    callback.Invoke(argument);
                }
            }
        }
    }
    public class SmartAction
    {
        private SortedList<int, List<Action>> callbacks = new();

        public void Subscribe(Action callback, int order = 0)
        {
            if (callbacks.ContainsKey(order) == false)
            {
                callbacks.Add(order, new());
            }

            callbacks[order].Add(callback);
        }

        public void UnsubscribeAll(object owner)
        {
            foreach (KeyValuePair<int, List<Action>> kv in callbacks)
            {
                for (int i = kv.Value.Count - 1; i >= 0; i--)
                {
                    Action callback = kv.Value[i];
                    if (callback.Target == owner)
                    {
                        kv.Value.RemoveAt(i);
                    }
                }
            }
        }

        public void Fire()
        {
            foreach (KeyValuePair<int, List<Action>> kv in callbacks)
            {
                foreach (Action callback in kv.Value)
                {
                    callback.Invoke();
                }
            }
        }
    }
}