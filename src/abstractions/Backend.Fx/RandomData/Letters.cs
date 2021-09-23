namespace Backend.Fx.RandomData
{
    public static class Letters
    {
        // ReSharper disable StringLiteralTypo
        public static string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        // ReSharper restore StringLiteralTypo

        public static string RandomUpperCase(int length)
        {
            var random = string.Empty;
            for (var i = 0; i < length; i++)
            {
                random += UpperCase.Random();
            }

            return random;
        }

        public static string RandomLowerCase(int length)
        {
            var random = string.Empty;
            for (var i = 0; i < length; i++)
            {
                random += LowerCase.Random();
            }

            return random;
        }

        public static string RandomNormalCase(int length)
        {
            var random = string.Empty;
            for (var i = 0; i < length; i++)
            {
                random += i == 0 ? UpperCase.Random() : LowerCase.Random();
            }

            return random;
        }

        public static string RandomPassword(int length = 10, int numberCount = 2, int specialCharCount = 2)
        {
            // ReSharper disable StringLiteralTypo
            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";
            const string numbers = "23456789";
            const string specials = "§$%&#+*-<>";
            // ReSharper restore StringLiteralTypo
            var password = new char[length];

            for (var i = 0; i < password.Length; i++)
            {
                password[i] = letters.Random();
            }

            for (var i = 0; i < numberCount; i++)
            {
                password[TestRandom.Next(length)] = numbers.Random();
            }

            for (var i = 0; i < specialCharCount; i++)
            {
                password[TestRandom.Next(length)] = specials.Random();
            }

            return new string(password);
        }
    }
}
