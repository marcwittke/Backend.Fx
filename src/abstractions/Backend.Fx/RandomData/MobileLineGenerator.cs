﻿using System.Linq;
using JetBrains.Annotations;

namespace Backend.Fx.RandomData
{
    [PublicAPI]
    public class MobileLineGenerator : Generator<string>
    {
        public static string Generate()
        {
            return new MobileLineGenerator().First();
        }


        protected override string Next()
        {
            var generated = Numbers.MobileNetworks.Random();
            while (generated.Length < TestRandom.Instance.Next(11)) generated += Numbers.Ciphers.Random();

            return generated;
        }
    }
}