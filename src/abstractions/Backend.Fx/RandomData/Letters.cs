namespace Backend.Fx.RandomData
{
    public static class Letters
    {
        public static string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string LowerCase = "abcdefghijklmnopqrstuvwxyz";

        public static string RandomUpperCase(int length)
        {
            string random = string.Empty;
            for (int i = 0; i < length; i++)
            {
                random += UpperCase.Random();
            }
            return random;
        }

        public static string RandomLowerCase(int length)
        {
            string random = string.Empty;
            for (int i = 0; i < length; i++)
            {
                random += LowerCase.Random();
            }
            return random;
        }

        public static string RandomNormalCase(int length)
        {
            string random = string.Empty;
            for (int i = 0; i < length; i++)
            {
                random += i == 0 ? UpperCase.Random() : LowerCase.Random();
            }
            return random;
        }
    }
}
