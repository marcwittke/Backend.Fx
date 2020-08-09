using System;
using System.Collections.Generic;

namespace Backend.Fx.RandomData
{
    public static class TestRandom
    {
        public static Random Instance { get; set; } = new Random(429756);

        public static IEnumerable<int> Next(int amount, Func<int> generate)
        {
            for (var i = 0; i < amount; i++)
            {
                yield return generate();
            }
        }

        public static int Next()
        {
            return Instance.Next();
        }

        public static int Next(int max)
        {
            return Instance.Next(max);
        }

        public static int Next(int min, int max)
        {
            return Instance.Next(min, max);
        }

        public static bool NextBool()
        {
            return Instance.Next(2) == 1;
        }

        public static double NextDouble()
        {
            return Instance.NextDouble();
        }

        public static DateTime RandomDateTime(int rangeDays)
        {
            return rangeDays < 0
                       ? DateTime.Now.AddDays(-Next(-rangeDays)).AddSeconds(-Next(100000))
                       : DateTime.Now.AddDays(Next(rangeDays)).AddSeconds(-Next(100000));
        }

        public static decimal RandomDecimal(int min = 0, int max = 999999)
        {
            var abs = Next(min, max);
            return abs + Math.Round(Next(100) / 100m, 2);
        }

        public static string NextPassword(int length = 10)
        {
            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";
            const string numbers = "23456789";
            const string specials = "§$%&#+*-<>";
            var password = new char[length];

            for (var i = 0; i < password.Length; i++)
            {
                var rnd = Next(100);
                password[i] = rnd < 60
                                  ? letters.Random()
                                  : rnd < 90
                                      ? numbers.Random()
                                      : specials.Random();
            }

            return new string(password);
        }

        public static decimal NextDecimal(decimal minimum, decimal maximum)
        {
            return (decimal) Instance.NextDouble() * (maximum - minimum) + minimum;
        }

        public static bool NextProbability(int p)
        {
            return Instance.Next(0, 100) < p;
        }
    }
}