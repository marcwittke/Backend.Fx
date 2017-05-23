namespace Backend.Fx.RandomData
{
    using System;
    using System.Collections.Generic;

    public class TestAddressGenerator
    {
        public int GenerateCount { get; set; } = 100;

        public Random Random { get; set; } = TestRandom.Instance;

        public IEnumerable<TestAddress> GenerateTestAddresses()
        {
            var distinctCheck = new HashSet<string>();
            for (var i = 0; i < GenerateCount; i++)
            {
                TestAddress generated = null;

                while (generated == null || distinctCheck.Contains(generated.Street + generated.Number))
                {
                    generated = new TestAddress(
                        Names.Streets.Random(),
                        Numbers.RandomHouseNumber(),
                        Numbers.RandomPostalCode(),
                        Names.Cities.Random());
                }

                distinctCheck.Add(generated.Street + generated.Number);
                yield return generated;
            }
        }
    }
}