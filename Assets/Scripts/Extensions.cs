using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InGame
{
    public static class Extensions
    {
        public static string Multiply(this char character, int times)
        {
            if (times <= 0) return "";

            char[] chars = new char[times];
            for (int i = 0; i < times; i++)
            {
                chars[i] = character;
            }

            return new string(chars);
        }

        public static byte ToHexByte(this string hex)
        {
            return byte.Parse(hex, NumberStyles.HexNumber);
        }
        public static byte[] ToHexBytes(this string hex)
        {
            byte[] bytes = new byte[Mathf.CeilToInt(hex.Length / 2f)];
            for (int i = 0; i < hex.Length; i += 2)
            {
                string byteChar;
                if (i == hex.Length - 1) byteChar = hex[i].ToString();
                else byteChar = string.Concat(hex[i], hex[i + 1]);

                bytes[i / 2] = byte.Parse(byteChar, NumberStyles.HexNumber);
            }
            return bytes;
        }

        public static IEnumerable<A> Slice<A> (this IEnumerable<A> e, int from, int to)
        {
            return e.Take(to).Skip(from);
        }
        public static IEnumerable<A> Slice<A> (this IEnumerable<A> e, int from)
        {
            return e.Skip(from);
        }


        public static bool IsInputFieldFocused()
        {
            GameObject go = EventSystem.current.currentSelectedGameObject;
            if (go == null) return false;

            return go.GetComponent<TMP_InputField>() != null;
        }

        public static bool IsAnythingBlocksRay(Transform asker)
        {
            List<RaycastResult> results = new();

            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            }, results);

            foreach (RaycastResult result in results)
            {
                GameObject hit = result.gameObject;
                if (hit.transform.IsChildOf(asker) == false)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsChildOf(this Transform ask, Transform parent)
        {
            if (ask.parent == null) return false;
            if (ask.parent == parent) return true;

            return ask.parent.IsChildOf(parent);
        }
    }
}