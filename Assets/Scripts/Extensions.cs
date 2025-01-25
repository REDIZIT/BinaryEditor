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
    }
}