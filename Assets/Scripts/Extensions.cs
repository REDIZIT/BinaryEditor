using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        public static IEnumerable<A> Slice<A> (this IEnumerable<A> e, int from, int to)
        {
            return e.Take(to).Skip(from);
        }
        public static IEnumerable<A> Slice<A> (this IEnumerable<A> e, int from)
        {
            return e.Skip(from);
        }
    }
}