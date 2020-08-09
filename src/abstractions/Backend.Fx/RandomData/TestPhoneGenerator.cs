using System.Collections.Generic;
using System.Linq;

namespace Backend.Fx.RandomData
{
    public class LandLineGenerator : Generator<string>
    {
        public static string Generate()
        {
            return new LandLineGenerator().First();
        }
        
        public override IEnumerator<string> GetEnumerator()
        {
            return new Enumerator();
        }

        private class Enumerator : GeneratingEnumerator<string>
        {
            protected override string Next()
            {
                var generated = Numbers.LandLineNetworks.Random();
                while (generated.Length < TestRandom.Instance.Next(8, 11))
                {
                    generated += Numbers.Ciphers.Random();
                }
                return generated;
            }
        }
    }
    
    public class MobileLineGenerator : Generator<string>
    {
        public static string Generate()
        {
            return new MobileLineGenerator().First();
        }
        
        public override IEnumerator<string> GetEnumerator()
        {
            return new Enumerator();
        }

        private class Enumerator : GeneratingEnumerator<string>
        {
            protected override string Next()
            {
                var generated = Numbers.MobileNetworks.Random();
                while (generated.Length < TestRandom.Instance.Next(11))
                {
                    generated += Numbers.Ciphers.Random();
                }
                return generated;
            }
        }
    }
}