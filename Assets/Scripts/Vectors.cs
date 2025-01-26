using UnityEngine;

namespace InGame
{
    public static class Vectors
    {
        public static Vector3 Round(this Vector3 round)
        {
            return new(Mathf.Round(round.x), Mathf.Round(round.y), Mathf.Round(round.z));
        }
        public static Vector2 Round(this Vector2 round)
        {
            return new(Mathf.Round(round.x), Mathf.Round(round.y));
        }
        public static Vector2 Floor(this Vector2 round)
        {
            return new(Mathf.Floor(round.x), Mathf.Floor(round.y));
        }
        public static Vector2 Ceil(this Vector2 round)
        {
            return new(Mathf.Ceil(round.x), Mathf.Ceil(round.y));
        }
    }
}