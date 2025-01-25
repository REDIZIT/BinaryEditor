using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InGame
{
    public class CustomAttributesResolver : MonoBehaviour
    {
        public static CustomAttributesResolver instance;

        private HashSet<Type> componentsWithoutAttrs = new();
        private List<Component> temp = new();

        public CustomAttributesResolver()
        {
            instance = this;
        }

        public void Init()
        {
            instance = this;

            Stopwatch w = Stopwatch.StartNew();

            foreach (GameObject go in FindObjectsOfType<GameObject>())
            {
                Resolve(go);
            }

            w.Stop();

            if (w.ElapsedMilliseconds >= 10)
            {
                Debug.Log("Warning. Custom attributes resolve is too long. (" + w.ElapsedMilliseconds + " ms)");
            }
        }

        public static void Resolve(GameObject go)
        {
            instance.ResolveInternal(go);
        }

        public static void Resolve(Component component)
        {
            instance.ResolveInternal(component);
        }

        private void ResolveInternal(GameObject go)
        {
            go.GetComponents(temp);
            foreach (Component component in temp)
            {
                Resolve(component);
            }
        }
        private void ResolveInternal(Component component)
        {
            Type type = component.GetType();
            if (componentsWithoutAttrs.Contains(type)) return;


            bool hasAttrs = false;

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                Require attr = field.GetCustomAttribute<Require>();
                if (attr == null) continue;

                object value = GetObjectForRequire(component.gameObject, field.FieldType);
                if (value == null)
                {
                    Debug.LogError(@$"Failed to resolve Require attribute. GameObject '{component.gameObject.name}' does not have {field.FieldType} component on it or it's children.
Failed component: {type}, field: {field.FieldType} '{field.Name}'");
                    continue;
                }

                field.SetValue(component, value);

                hasAttrs = true;
            }

            if (hasAttrs == false)
            {
                componentsWithoutAttrs.Add(type);
            }
        }

        private object GetObjectForRequire(GameObject inst, Type target)
        {
            return inst.GetComponentInChildren(target, true);
        }
    }
}