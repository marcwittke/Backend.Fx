namespace Backend.Fx.Extensions
{
    public static class StringEx
    {
        public static string Cut(this string s, int length)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            if (s.Length > length)
            {
                s = s.Substring(0, length - 1) + "…";
            }

            return s;
        }
    }
}
