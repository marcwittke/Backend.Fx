using System.Linq;
using JetBrains.Annotations;

namespace Backend.Fx.RandomData
{
    [PublicAPI]
    public class LandLineGenerator : Generator<string>
    {
        public static string Generate()
        {
            return new LandLineGenerator().Take(1).Single();
        }

        protected override string Next()
        {
            var generated = Numbers.LandLineNetworks.Random();
            while (generated.Length < TestRandom.Instance.Next(8, 11)) generated += Numbers.Ciphers.Random();
            return generated;
        }
    }
}